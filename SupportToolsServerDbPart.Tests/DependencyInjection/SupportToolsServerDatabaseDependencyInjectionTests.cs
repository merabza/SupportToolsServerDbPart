using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Serilog;
using SupportToolsServerCore.Application.Abstractions;
using SupportToolsServerDbPart.Db;
using SupportToolsServerDbPart.Db.DependencyInjection;
using SystemTools.Domain.Abstractions;
using SystemTools.SharedKernel;
using Xunit;

namespace SupportToolsServerDbPart.Tests.DependencyInjection;

public sealed class SupportToolsServerDatabaseDependencyInjectionTests
{
    private const string ConnectionStringKey = "Data:SupportToolsServerDatabase:ConnectionString";

    //A string that is valid only in format: the context never connects to a database
    private const string ConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=SupportToolsServerTests;Trusted_Connection=True";

    private static IConfiguration CreateConfiguration(string? connectionString)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [ConnectionStringKey] = connectionString })
            .Build();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddSupportToolsServerDatabase_Throws_WhenTheConnectionStringIsEmpty(string? connectionString)
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration(connectionString);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddSupportToolsServerDatabase(null, configuration));

        Assert.Equal("parameter Data:SupportToolsServerDatabase:ConnectionString is empty", exception.Message);
        Assert.Empty(services);
    }

    [Fact]
    public void AddSupportToolsServerDatabase_Throws_WhenTheConnectionStringIsNotValid()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = CreateConfiguration("not a connection string");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddSupportToolsServerDatabase(null, configuration));

        ArgumentException inner = Assert.IsType<ArgumentException>(exception.InnerException);
        Assert.Equal(
            "parameter Data:SupportToolsServerDatabase:ConnectionString is not a valid connection string: " +
            inner.Message, exception.Message);
        Assert.Empty(services);
    }

    [Fact]
    public void AddSupportToolsServerDatabase_ReturnsTheSameCollection()
    {
        var services = new ServiceCollection();

        IServiceCollection returned =
            services.AddSupportToolsServerDatabase(null, CreateConfiguration(ConnectionString));

        Assert.Same(services, returned);
    }

    [Fact]
    public void AddSupportToolsServerDatabase_RegistersTheContextForTheConfiguredSqlServerDatabase()
    {
        var services = new ServiceCollection();
        services.AddSupportToolsServerDatabase(null, CreateConfiguration(ConnectionString));
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<SupportToolsServerDbContext>();

        //EF keeps the connection string in its canonical form, so the parts are compared one by one
        var connectionStringBuilder = new SqlConnectionStringBuilder(context.Database.GetConnectionString());
        Assert.True(context.Database.IsSqlServer());
        Assert.Equal(@"(localdb)\MSSQLLocalDB", connectionStringBuilder.DataSource);
        Assert.Equal("SupportToolsServerTests", connectionStringBuilder.InitialCatalog);
    }

    [Fact]
    public void AddSupportToolsServerDatabase_ResolvesTheAbstractionToTheContextOfTheScope()
    {
        var services = new ServiceCollection();
        services.AddSupportToolsServerDatabase(null, CreateConfiguration(ConnectionString));
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        var abstraction = scope.ServiceProvider.GetRequiredService<ISupportToolsServerDbContext>();

        Assert.Same(scope.ServiceProvider.GetRequiredService<SupportToolsServerDbContext>(), abstraction);
    }

    [Fact]
    public void AddSupportToolsServerDatabase_RegistersTheUnitOfWork()
    {
        var services = new ServiceCollection();
        services.AddSupportToolsServerDatabase(null, CreateConfiguration(ConnectionString));
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        Assert.IsType<SupportToolsServerUnitOfWork>(unitOfWork);
    }

    [Fact]
    public void AddSupportToolsServerDatabase_RegistersTheDomainEventsDispatcherAsScoped()
    {
        var services = new ServiceCollection();

        services.AddSupportToolsServerDatabase(null, CreateConfiguration(ConnectionString));

        Assert.Contains(services,
            d => d.ServiceType == typeof(IDomainEventsDispatcher) &&
                 d.ImplementationType == typeof(DomainEventsDispatcher) && d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public async Task AddSupportToolsServerDatabase_CreatesTheContextWithTheDispatcherOfTheScope()
    {
        var services = new ServiceCollection();
        services.AddSupportToolsServerDatabase(null, CreateConfiguration(ConnectionString));
        var dispatcher = new Mock<IDomainEventsDispatcher>();
        services.AddScoped(_ => dispatcher.Object);
        await using ServiceProvider provider = services.BuildServiceProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<SupportToolsServerDbContext>();

        await context.SaveChangesAsync(CancellationToken.None);

        dispatcher.Verify(d => d.DispatchAsync(It.IsAny<IEnumerable<IDomainEvent>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void AddSupportToolsServerDatabase_LogsTheStartAndTheEnd()
    {
        var logger = new Mock<ILogger>();

        new ServiceCollection().AddSupportToolsServerDatabase(logger.Object, CreateConfiguration(ConnectionString));

        logger.Verify(l => l.Information("{MethodName} Started", "AddSupportToolsServerDatabase"), Times.Once);
        logger.Verify(l => l.Information("{MethodName} Finished", "AddSupportToolsServerDatabase"), Times.Once);
    }

    [Fact]
    public void AddSupportToolsServerDatabase_DoesNotLogTheEnd_WhenItThrows()
    {
        var logger = new Mock<ILogger>();
        IConfiguration configuration = CreateConfiguration(null);

        Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddSupportToolsServerDatabase(logger.Object, configuration));

        logger.Verify(l => l.Information("{MethodName} Started", "AddSupportToolsServerDatabase"), Times.Once);
        logger.Verify(l => l.Information("{MethodName} Finished", "AddSupportToolsServerDatabase"), Times.Never);
    }
}
