using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Moq;
using SupportToolsServerCore.Domain.DeploymentEnvironments;
using SupportToolsServerCore.Domain.EditorConfigFileTypes;
using SupportToolsServerCore.Domain.GitIgnoreFileTypes;
using SupportToolsServerCore.Domain.GitRepos;
using SupportToolsServerCore.Domain.Primitives;
using SupportToolsServerDbPart.Db;
using SystemTools.SharedKernel;
using Xunit;

namespace SupportToolsServerDbPart.Tests;

//The entities are attached unchanged, so SaveChangesAsync has nothing to write and never connects to the database,
//while the domain events of the tracked entities are still published
public sealed class SupportToolsServerDbContextTests
{
    //A string that is valid only in format: the context never connects to a database
    private const string ConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=SupportToolsServerTests;Trusted_Connection=True";

    private static readonly DbContextOptions<SupportToolsServerDbContext> Options =
        new DbContextOptionsBuilder<SupportToolsServerDbContext>().UseSqlServer(ConnectionString).Options;

    private readonly List<IDomainEvent> _dispatched = [];
    private readonly Mock<IDomainEventsDispatcher> _dispatcher = new();
    private CancellationToken _dispatchToken;

    public SupportToolsServerDbContextTests()
    {
        _dispatcher.Setup(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<IDomainEvent>, CancellationToken>((events, token) =>
            {
                _dispatched.AddRange(events);
                _dispatchToken = token;
            }).Returns(Task.CompletedTask);
    }

    private static GitRepo NewGitRepo(string name)
    {
        return GitRepo.Create(name, $"address{name}", name, GitIgnoreFileTypeId.CreateUnique());
    }

    [Fact]
    public async Task SaveChangesAsync_PublishesTheEventsOfTheTrackedEntities_AndClearsThem()
    {
        await using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);
        GitRepo added = NewGitRepo("RepoA");
        var updated = new GitRepo(GitRepoId.CreateUnique(), "RepoB", "addressB", "RepoB",
            GitIgnoreFileTypeId.CreateUnique(), 1);
        updated.Update("RepoB", "addressB2", "RepoB2", updated.GitIgnoreFileTypeId);
        context.Attach(added);
        context.Attach(updated);
        using var cancellationTokenSource = new CancellationTokenSource();

        int written = await context.SaveChangesAsync(cancellationTokenSource.Token);

        Assert.Equal(0, written);
        Assert.Equal(2, _dispatched.Count);
        Assert.Contains(new GitRepoAddedDomainEvent(added.Id, "RepoA", "addressRepoA", "RepoA"), _dispatched);
        Assert.Contains(new GitRepoUpdatedDomainEvent(updated.Id, "RepoB", "addressB2", "RepoB2"), _dispatched);
        Assert.Equal(cancellationTokenSource.Token, _dispatchToken);
        Assert.Empty(added.DomainEvents);
        Assert.Empty(updated.DomainEvents);
    }

    [Fact]
    public async Task SaveChangesAsync_PublishesTheEventsOnlyOnce()
    {
        await using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);
        context.Attach(NewGitRepo("RepoA"));

        await context.SaveChangesAsync(CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        Assert.Single(_dispatched);
        _dispatcher.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task SaveChangesAsync_DoesNotPublish_WithTheDesignTimeConstructors()
    {
        GitRepo first = NewGitRepo("RepoA");
        GitRepo second = NewGitRepo("RepoB");
        await using (var context = new SupportToolsServerDbContext(Options, true))
        {
            context.Attach(first);
            Assert.Equal(0, await context.SaveChangesAsync(CancellationToken.None));
        }

        await using (var context = new SupportToolsServerDbContext(Options, 1))
        {
            context.Attach(second);
            Assert.Equal(0, await context.SaveChangesAsync(CancellationToken.None));
        }

        Assert.Single(first.DomainEvents);
        Assert.Single(second.DomainEvents);
    }

    [Fact]
    public void Model_ContainsTheConfiguredEntities()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        Assert.NotNull(context.Model.FindEntityType(typeof(EditorConfigFileType)));
        Assert.NotNull(context.Model.FindEntityType(typeof(GitIgnoreFileType)));
        Assert.NotNull(context.Model.FindEntityType(typeof(DeploymentEnvironment)));
        IEntityType gitRepo = Assert.IsType<IEntityType>(context.Model.FindEntityType(typeof(GitRepo)),
            exactMatch: false);
        Assert.Null(gitRepo.FindProperty(nameof(GitRepo.DomainEvents)));
        Assert.Contains(gitRepo.GetIndexes(), i => i.IsUnique && i.Properties.Single().Name == nameof(GitRepo.Name));
    }

    //The repositories rely on it: the Version of every registry aggregate root is the concurrency token of its table,
    //and the first version is the default that the existing rows get when the column is added
    [Fact]
    public void Model_MakesTheVersionOfEveryVersionedEntityAConcurrencyTokenWithTheFirstVersionAsDefault()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        List<IEntityType> versionedEntityTypes =
            [.. context.Model.GetEntityTypes().Where(x => IsVersionedEntity(x.ClrType))];

        Assert.Superset(
            new HashSet<Type>
            {
                typeof(DeploymentEnvironment), typeof(EditorConfigFileType), typeof(GitIgnoreFileType), typeof(GitRepo)
            }, versionedEntityTypes.Select(x => x.ClrType).ToHashSet());
        Assert.All(versionedEntityTypes, entityType =>
        {
            IProperty version = Assert.IsType<IProperty>(entityType.FindProperty(nameof(GitRepo.Version)),
                exactMatch: false);
            Assert.True(version.IsConcurrencyToken);
            Assert.False(version.IsNullable);
            Assert.Equal(EntityVersion.Initial, version.GetDefaultValue());
        });
    }

    [Fact]
    public void Model_MapsDeploymentEnvironmentToTheEnvironmentsTable()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType environment = Assert.IsType<IEntityType>(
            context.Model.FindEntityType(typeof(DeploymentEnvironment)), exactMatch: false);

        Assert.Equal("Environments", environment.GetTableName());
        Assert.Contains(environment.GetIndexes(),
            i => i.IsUnique && i.Properties.Single().Name == nameof(DeploymentEnvironment.Name));
        IProperty name = environment.GetProperty(nameof(DeploymentEnvironment.Name));
        Assert.False(name.IsNullable);
        Assert.Equal(DeploymentEnvironment.NameMaxLength, name.GetMaxLength());
        IProperty description = environment.GetProperty(nameof(DeploymentEnvironment.Description));
        Assert.True(description.IsNullable);
        Assert.Equal(DeploymentEnvironment.DescriptionMaxLength, description.GetMaxLength());
        Assert.Null(environment.FindProperty(nameof(DeploymentEnvironment.DomainEvents)));
    }

    [Fact]
    public void DbSets_ExposeTheTables()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        Assert.NotNull(context.GitRepos);
        Assert.NotNull(context.GitIgnoreFileTypes);
        Assert.NotNull(context.EditorConfigFileTypes);
        Assert.NotNull(context.Environments);
    }

    private static bool IsVersionedEntity(Type type)
    {
        for (Type? current = type; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(VersionedEntity<>))
            {
                return true;
            }
        }

        return false;
    }
}
