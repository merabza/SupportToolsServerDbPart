using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Moq;
using SupportToolsServerCore.Domain.ApiClients;
using SupportToolsServerCore.Domain.DatabaseServerConnections;
using SupportToolsServerCore.Domain.DeploymentEnvironments;
using SupportToolsServerCore.Domain.DotnetTools;
using SupportToolsServerCore.Domain.EditorConfigFileTypes;
using SupportToolsServerCore.Domain.FileStorages;
using SupportToolsServerCore.Domain.GitIgnoreFileTypes;
using SupportToolsServerCore.Domain.GitRepos;
using SupportToolsServerCore.Domain.NpmPackages;
using SupportToolsServerCore.Domain.Primitives;
using SupportToolsServerCore.Domain.Projects;
using SupportToolsServerCore.Domain.ProjectTemplates;
using SupportToolsServerCore.Domain.ReactAppTemplates;
using SupportToolsServerCore.Domain.Runtimes;
using SupportToolsServerCore.Domain.Servers;
using SupportToolsServerCore.Domain.Settings;
using SupportToolsServerCore.Domain.SmartSchemas;
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
        Assert.NotNull(context.Model.FindEntityType(typeof(Runtime)));
        Assert.NotNull(context.Model.FindEntityType(typeof(NpmPackage)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ReactAppTemplate)));
        Assert.NotNull(context.Model.FindEntityType(typeof(DotnetTool)));
        Assert.NotNull(context.Model.FindEntityType(typeof(SmartSchema)));
        Assert.NotNull(context.Model.FindEntityType(typeof(SmartSchemaDetail)));
        Assert.NotNull(context.Model.FindEntityType(typeof(FileStorage)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ApiClient)));
        Assert.NotNull(context.Model.FindEntityType(typeof(DatabaseServerConnection)));
        Assert.NotNull(context.Model.FindEntityType(typeof(DatabaseFoldersSet)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Server)));
        Assert.NotNull(context.Model.FindEntityType(typeof(GlobalSettings)));
        Assert.NotNull(context.Model.FindEntityType(typeof(DatabasesBackupFilesExchange)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ProjectCreatorSettings)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ProjectTemplate)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Project)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ProjectGitRepo)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ProjectNpmPackage)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ProjectRedundantFile)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ProjectAllowedTool)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ProjectEndpoint)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ProjectRouteClass)));
        IEntityType gitRepo =Assert.IsType<IEntityType>(context.Model.FindEntityType(typeof(GitRepo)),
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
                typeof(ApiClient),
                typeof(DatabaseServerConnection),
                typeof(DeploymentEnvironment),
                typeof(DotnetTool),
                typeof(EditorConfigFileType),
                typeof(FileStorage),
                typeof(GitIgnoreFileType),
                typeof(GitRepo),
                typeof(GlobalSettings),
                typeof(NpmPackage),
                typeof(Project),
                typeof(ProjectCreatorSettings),
                typeof(ProjectTemplate),
                typeof(ReactAppTemplate),
                typeof(Runtime),
                typeof(Server),
                typeof(SmartSchema)
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
    public void Model_MapsRuntimeToTheRuntimesTable()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType runtime = EntityTypeOf<Runtime>(context);

        Assert.Equal("Runtimes", runtime.GetTableName());
        AssertUniqueName(runtime);
        AssertText(runtime, nameof(Runtime.Name), false, Runtime.NameMaxLength);
        AssertText(runtime, nameof(Runtime.Description), true, Runtime.DescriptionMaxLength);
        Assert.Null(runtime.FindProperty(nameof(Runtime.DomainEvents)));
    }

    [Fact]
    public void Model_MapsNpmPackageToTheNpmPackagesTable()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType npmPackage = EntityTypeOf<NpmPackage>(context);

        Assert.Equal("NpmPackages", npmPackage.GetTableName());
        AssertUniqueName(npmPackage);
        AssertText(npmPackage, nameof(NpmPackage.Name), false, NpmPackage.NameMaxLength);
        AssertText(npmPackage, nameof(NpmPackage.Description), true, NpmPackage.DescriptionMaxLength);
        Assert.Null(npmPackage.FindProperty(nameof(NpmPackage.DomainEvents)));
    }

    [Fact]
    public void Model_MapsReactAppTemplateToTheReactAppTemplatesTable()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType reactAppTemplate = EntityTypeOf<ReactAppTemplate>(context);

        Assert.Equal("ReactAppTemplates", reactAppTemplate.GetTableName());
        AssertUniqueName(reactAppTemplate);
        AssertText(reactAppTemplate, nameof(ReactAppTemplate.Name), false, ReactAppTemplate.NameMaxLength);
        AssertText(reactAppTemplate, nameof(ReactAppTemplate.Template), false, ReactAppTemplate.TemplateMaxLength);
        Assert.Null(reactAppTemplate.FindProperty(nameof(ReactAppTemplate.DomainEvents)));
    }

    //InstalledVersion, LatestVersion and CommandName belong to the machine, so they have no columns
    [Fact]
    public void Model_MapsDotnetToolToTheDotnetToolsTable()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType dotnetTool = EntityTypeOf<DotnetTool>(context);

        Assert.Equal("DotnetTools", dotnetTool.GetTableName());
        AssertUniqueName(dotnetTool);
        AssertText(dotnetTool, nameof(DotnetTool.Name), false, DotnetTool.NameMaxLength);
        AssertText(dotnetTool, nameof(DotnetTool.PackageId), false, DotnetTool.PackageIdMaxLength);
        AssertText(dotnetTool, nameof(DotnetTool.MaxVersion), true, DotnetTool.MaxVersionMaxLength);
        AssertText(dotnetTool, nameof(DotnetTool.Description), true, DotnetTool.DescriptionMaxLength);
        Assert.Equal(
            [
                nameof(DotnetTool.Description), nameof(DotnetTool.Id), nameof(DotnetTool.MaxVersion),
                nameof(DotnetTool.Name), nameof(DotnetTool.PackageId), nameof(DotnetTool.Version)
            ], dotnetTool.GetProperties().Select(x => x.Name).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Model_MapsSmartSchemaToTheSmartSchemasTable()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType smartSchema = EntityTypeOf<SmartSchema>(context);

        Assert.Equal("SmartSchemas", smartSchema.GetTableName());
        AssertUniqueName(smartSchema);
        AssertText(smartSchema, nameof(SmartSchema.Name), false, SmartSchema.NameMaxLength);
        Assert.False(smartSchema.GetProperty(nameof(SmartSchema.LastPreserveCount)).IsNullable);
        Assert.Equal(
            [nameof(SmartSchema.Id), nameof(SmartSchema.LastPreserveCount), nameof(SmartSchema.Name),
                nameof(SmartSchema.Version)],
            smartSchema.GetProperties().Select(x => x.Name).Order(StringComparer.Ordinal));
    }

    //The details are children of the aggregate: a required foreign key that cascades, so the replaced details of an
    //update are deleted, and one detail of a period type per schema
    [Fact]
    public void Model_MapsSmartSchemaDetailToTheSmartSchemaDetailsTableAsAChildOfTheSchema()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType detail = EntityTypeOf<SmartSchemaDetail>(context);

        Assert.Equal("SmartSchemaDetails", detail.GetTableName());
        AssertText(detail, nameof(SmartSchemaDetail.PeriodType), false, SmartSchemaDetail.PeriodTypeMaxLength);
        Assert.False(detail.GetProperty(nameof(SmartSchemaDetail.PreserveCount)).IsNullable);
        AssertChildOf<SmartSchema>(detail, "SmartSchemaId", nameof(SmartSchema.Details));
        Assert.Contains(detail.GetIndexes(),
            i => i.IsUnique && i.Properties.Select(x => x.Name).SequenceEqual(["SmartSchemaId", "PeriodType"]));
        Assert.Equal(["Id", "PeriodType", "PreserveCount", "SmartSchemaId"],
            detail.GetProperties().Select(x => x.Name).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Model_MapsFileStorageToTheFileStoragesTable()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType fileStorage = EntityTypeOf<FileStorage>(context);

        Assert.Equal("FileStorages", fileStorage.GetTableName());
        AssertUniqueName(fileStorage);
        AssertText(fileStorage, nameof(FileStorage.Name), false, FileStorage.NameMaxLength);
        AssertText(fileStorage, nameof(FileStorage.FileStoragePath), true, FileStorage.FileStoragePathMaxLength);
        AssertText(fileStorage, nameof(FileStorage.UserName), true, FileStorage.UserNameMaxLength);
        AssertText(fileStorage, nameof(FileStorage.Password), true, FileStorage.PasswordMaxLength);
        Assert.Equal(
            [
                nameof(FileStorage.FileNameMaxLength), nameof(FileStorage.FileSizeSplitPositionInRow),
                nameof(FileStorage.FileStoragePath), nameof(FileStorage.FtpSiteLsFileOffset), nameof(FileStorage.Id),
                nameof(FileStorage.Name), nameof(FileStorage.Password), nameof(FileStorage.UserName),
                nameof(FileStorage.Version)
            ], fileStorage.GetProperties().Select(x => x.Name).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Model_MapsApiClientToTheApiClientsTable()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType apiClient = EntityTypeOf<ApiClient>(context);

        Assert.Equal("ApiClients", apiClient.GetTableName());
        AssertUniqueName(apiClient);
        AssertText(apiClient, nameof(ApiClient.Name), false, ApiClient.NameMaxLength);
        AssertText(apiClient, nameof(ApiClient.Server), true, ApiClient.ServerMaxLength);
        AssertText(apiClient, nameof(ApiClient.ApiKey), true, ApiClient.ApiKeyMaxLength);
        Assert.Equal(
            [
                nameof(ApiClient.ApiKey), nameof(ApiClient.Id), nameof(ApiClient.Name), nameof(ApiClient.Server),
                nameof(ApiClient.Version)
            ], apiClient.GetProperties().Select(x => x.Name).Order(StringComparer.Ordinal));
    }

    //The web agent is another aggregate: an optional foreign key that refuses to delete the ApiClient in use
    [Fact]
    public void Model_MapsDatabaseServerConnectionToItsTableWithAnOptionalRestrictedWebAgent()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType connection = EntityTypeOf<DatabaseServerConnection>(context);

        Assert.Equal("DatabaseServerConnections", connection.GetTableName());
        AssertUniqueName(connection);
        AssertText(connection, nameof(DatabaseServerConnection.Name), false, DatabaseServerConnection.NameMaxLength);
        AssertText(connection, nameof(DatabaseServerConnection.DatabaseServerProvider), false,
            DatabaseServerConnection.DatabaseServerProviderMaxLength);
        AssertText(connection, nameof(DatabaseServerConnection.RemoteDbConnectionName), true,
            DatabaseServerConnection.RemoteDbConnectionNameMaxLength);
        AssertText(connection, nameof(DatabaseServerConnection.ServerAddress), true,
            DatabaseServerConnection.ServerAddressMaxLength);
        AssertText(connection, nameof(DatabaseServerConnection.ServerUser), true,
            DatabaseServerConnection.ServerUserMaxLength);
        AssertText(connection, nameof(DatabaseServerConnection.ServerPass), true,
            DatabaseServerConnection.ServerPassMaxLength);
        IForeignKey webAgent = Assert.Single(connection.GetForeignKeys());
        Assert.Equal(typeof(ApiClient), webAgent.PrincipalEntityType.ClrType);
        Assert.Equal(nameof(DatabaseServerConnection.DbWebAgentId), Assert.Single(webAgent.Properties).Name);
        Assert.False(webAgent.IsRequired);
        Assert.Equal(DeleteBehavior.Restrict, webAgent.DeleteBehavior);
        Assert.Equal("uniqueidentifier", webAgent.Properties[0].GetColumnType());
        Assert.Equal(
            [
                nameof(DatabaseServerConnection.ConnectionTimeOut),
                nameof(DatabaseServerConnection.DatabaseServerProvider), nameof(DatabaseServerConnection.DbWebAgentId),
                nameof(DatabaseServerConnection.Encrypt), nameof(DatabaseServerConnection.Id),
                nameof(DatabaseServerConnection.Name), nameof(DatabaseServerConnection.RemoteDbConnectionName),
                nameof(DatabaseServerConnection.ServerAddress), nameof(DatabaseServerConnection.ServerPass),
                nameof(DatabaseServerConnection.ServerUser), nameof(DatabaseServerConnection.TrustServerCertificate),
                nameof(DatabaseServerConnection.Version), nameof(DatabaseServerConnection.WindowsNtIntegratedSecurity)
            ], connection.GetProperties().Select(x => x.Name).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Model_MapsDatabaseFoldersSetToTheDatabaseFoldersSetsTableAsAChildOfTheConnection()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType foldersSet = EntityTypeOf<DatabaseFoldersSet>(context);

        Assert.Equal("DatabaseFoldersSets", foldersSet.GetTableName());
        AssertText(foldersSet, nameof(DatabaseFoldersSet.Name), false, DatabaseFoldersSet.NameMaxLength);
        AssertText(foldersSet, nameof(DatabaseFoldersSet.Backup), true, DatabaseFoldersSet.FolderMaxLength);
        AssertText(foldersSet, nameof(DatabaseFoldersSet.Data), true, DatabaseFoldersSet.FolderMaxLength);
        AssertText(foldersSet, nameof(DatabaseFoldersSet.DataLog), true, DatabaseFoldersSet.FolderMaxLength);
        AssertChildOf<DatabaseServerConnection>(foldersSet, "DatabaseServerConnectionId",
            nameof(DatabaseServerConnection.DatabaseFoldersSets));
        Assert.Contains(foldersSet.GetIndexes(),
            i => i.IsUnique && i.Properties.Select(x => x.Name).SequenceEqual(["DatabaseServerConnectionId", "Name"]));
        Assert.Equal(["Backup", "Data", "DataLog", "DatabaseServerConnectionId", "Id", "Name"],
            foldersSet.GetProperties().Select(x => x.Name).Order(StringComparer.Ordinal));
    }

    //The web agents and the runtime are other aggregates: optional foreign keys that refuse to delete the records in
    //use. IsLocal belongs to the machine, so it has no column
    [Fact]
    public void Model_MapsServerToTheServersTableWithOptionalRestrictedReferences()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType server = EntityTypeOf<Server>(context);

        Assert.Equal("Servers", server.GetTableName());
        AssertUniqueName(server);
        AssertText(server, nameof(Server.Name), false, Server.NameMaxLength);
        AssertText(server, nameof(Server.FilesUserName), true, Server.FilesUserNameMaxLength);
        AssertText(server, nameof(Server.FilesUsersGroupName), true, Server.FilesUsersGroupNameMaxLength);
        AssertText(server, nameof(Server.ServerSideDownloadFolder), true, Server.ServerSideDownloadFolderMaxLength);
        AssertText(server, nameof(Server.ServerSideDeployFolder), true, Server.ServerSideDeployFolderMaxLength);
        Assert.Equal(
            [
                (nameof(Server.RuntimeId), typeof(Runtime)), (nameof(Server.WebAgentId), typeof(ApiClient)),
                (nameof(Server.WebAgentInstallerId), typeof(ApiClient))
            ],
            server.GetForeignKeys().Select(x => (Assert.Single(x.Properties).Name, x.PrincipalEntityType.ClrType))
                .OrderBy(x => x.Name, StringComparer.Ordinal));
        Assert.All(server.GetForeignKeys(), foreignKey =>
        {
            Assert.False(foreignKey.IsRequired);
            Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
            Assert.Equal("uniqueidentifier", foreignKey.Properties[0].GetColumnType());
        });
        Assert.Equal(
            [
                nameof(Server.FilesUserName), nameof(Server.FilesUsersGroupName), nameof(Server.Id),
                nameof(Server.Name), nameof(Server.RuntimeId), nameof(Server.ServerSideDeployFolder),
                nameof(Server.ServerSideDownloadFolder), nameof(Server.Version), nameof(Server.WebAgentId),
                nameof(Server.WebAgentInstallerId)
            ], server.GetProperties().Select(x => x.Name).Order(StringComparer.Ordinal));
    }

    //A singleton: the key is fixed and a check constraint refuses any other key. The exchange parameters are an owned
    //type in the same row, required so that EF creates them even when all their columns are NULL. Every reference is
    //an optional foreign key that refuses to delete the record in use
    [Fact]
    public void Model_MapsGlobalSettingsToASingletonRowWithTheExchangeParametersAndRestrictedReferences()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType globalSettings = EntityTypeOf<GlobalSettings>(context);

        Assert.Equal("GlobalSettings", globalSettings.GetTableName());
        AssertSingletonCheck<GlobalSettings>(context, "CK_GlobalSettings_Singleton");
        AssertText(globalSettings, nameof(GlobalSettings.ServiceDescriptionSignature), true,
            GlobalSettings.ServiceDescriptionSignatureMaxLength);
        AssertText(globalSettings, nameof(GlobalSettings.UploadTempExtension), true,
            GlobalSettings.UploadTempExtensionMaxLength);
        AssertText(globalSettings, nameof(GlobalSettings.ProgramArchiveDateMask), true,
            GlobalSettings.ProgramArchiveDateMaskMaxLength);
        AssertText(globalSettings, nameof(GlobalSettings.ProgramArchiveExtension), true,
            GlobalSettings.ProgramArchiveExtensionMaxLength);
        AssertText(globalSettings, nameof(GlobalSettings.ParametersFileDateMask), true,
            GlobalSettings.ParametersFileDateMaskMaxLength);
        AssertText(globalSettings, nameof(GlobalSettings.ParametersFileExtension), true,
            GlobalSettings.ParametersFileExtensionMaxLength);
        AssertText(globalSettings, nameof(GlobalSettings.MediatRLicenseKey), true,
            GlobalSettings.MediatRLicenseKeyMaxLength);
        AssertOptionalRestrictedReferences(globalSettings,
        [
            (nameof(GlobalSettings.FileStorageForExchangeId), typeof(FileStorage)),
            (nameof(GlobalSettings.LocalPackageManagerWebApiClientId), typeof(ApiClient)),
            (nameof(GlobalSettings.SmartSchemaForExchangeId), typeof(SmartSchema)),
            (nameof(GlobalSettings.SmartSchemaForLocalId), typeof(SmartSchema))
        ]);
        Assert.Equal(
            [
                nameof(GlobalSettings.FileStorageForExchangeId), nameof(GlobalSettings.Id),
                nameof(GlobalSettings.LocalPackageManagerWebApiClientId), nameof(GlobalSettings.MediatRLicenseKey),
                nameof(GlobalSettings.ParametersFileDateMask), nameof(GlobalSettings.ParametersFileExtension),
                nameof(GlobalSettings.ProgramArchiveDateMask), nameof(GlobalSettings.ProgramArchiveExtension),
                nameof(GlobalSettings.ServiceDescriptionSignature), nameof(GlobalSettings.SmartSchemaForExchangeId),
                nameof(GlobalSettings.SmartSchemaForLocalId), nameof(GlobalSettings.UploadTempExtension),
                nameof(GlobalSettings.Version)
            ], globalSettings.GetProperties().Select(x => x.Name).Order(StringComparer.Ordinal));

        INavigation exchangeNavigation =
            globalSettings.GetNavigations().Single(x => x.Name == nameof(GlobalSettings.DatabasesBackupFilesExchange));
        Assert.True(exchangeNavigation.ForeignKey.IsOwnership);
        Assert.True(exchangeNavigation.ForeignKey.IsRequiredDependent);
        IEntityType exchange = exchangeNavigation.TargetEntityType;
        Assert.Equal(typeof(DatabasesBackupFilesExchange), exchange.ClrType);
        Assert.Equal("GlobalSettings", exchange.GetTableName());
        AssertText(exchange, nameof(DatabasesBackupFilesExchange.DownloadTempExtension), true,
            DatabasesBackupFilesExchange.DownloadTempExtensionMaxLength);
        AssertText(exchange, nameof(DatabasesBackupFilesExchange.UploadTempExtension), true,
            DatabasesBackupFilesExchange.UploadTempExtensionMaxLength);
        AssertOptionalRestrictedReferences(exchange,
        [
            (nameof(DatabasesBackupFilesExchange.ExchangeFileStorageId), typeof(FileStorage)),
            (nameof(DatabasesBackupFilesExchange.ExchangeSmartSchemaId), typeof(SmartSchema)),
            (nameof(DatabasesBackupFilesExchange.LocalSmartSchemaId), typeof(SmartSchema))
        ]);
        Assert.Equal(
            [
                "DatabasesBackupFilesExchange_DownloadTempExtension",
                "DatabasesBackupFilesExchange_ExchangeFileStorageId",
                "DatabasesBackupFilesExchange_ExchangeSmartSchemaId",
                "DatabasesBackupFilesExchange_LocalSmartSchemaId", "DatabasesBackupFilesExchange_UploadTempExtension",
                "Id"
            ],
            exchange.GetProperties().Select(x => x.GetColumnName(StoreObjectIdentifier.Table("GlobalSettings")))
                .Order(StringComparer.Ordinal));
    }

    //A singleton with optional foreign keys that refuse to delete the records in use. The paths are canonical paths,
    //stored as they come
    [Fact]
    public void Model_MapsProjectCreatorSettingsToASingletonRowWithRestrictedReferences()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType projectCreatorSettings = EntityTypeOf<ProjectCreatorSettings>(context);

        Assert.Equal("ProjectCreatorSettings", projectCreatorSettings.GetTableName());
        AssertSingletonCheck<ProjectCreatorSettings>(context, "CK_ProjectCreatorSettings_Singleton");
        Assert.False(projectCreatorSettings.GetProperty(nameof(ProjectCreatorSettings.IndentSize)).IsNullable);
        AssertText(projectCreatorSettings, nameof(ProjectCreatorSettings.FakeHostProjectName), true,
            ProjectCreatorSettings.FakeHostProjectNameMaxLength);
        AssertText(projectCreatorSettings, nameof(ProjectCreatorSettings.ProjectsFolderPathReal), true,
            ProjectCreatorSettings.ProjectsFolderPathRealMaxLength);
        AssertText(projectCreatorSettings, nameof(ProjectCreatorSettings.SecretsFolderPathReal), true,
            ProjectCreatorSettings.SecretsFolderPathRealMaxLength);
        AssertOptionalRestrictedReferences(projectCreatorSettings,
        [
            (nameof(ProjectCreatorSettings.DatabaseExchangeFileStorageId), typeof(FileStorage)),
            (nameof(ProjectCreatorSettings.DeveloperDbConnectionId), typeof(DatabaseServerConnection)),
            (nameof(ProjectCreatorSettings.ProductionEnvironmentId), typeof(DeploymentEnvironment)),
            (nameof(ProjectCreatorSettings.ProductionServerId), typeof(Server)),
            (nameof(ProjectCreatorSettings.UseSmartSchemaId), typeof(SmartSchema))
        ]);
        Assert.Equal(
            [
                nameof(ProjectCreatorSettings.DatabaseExchangeFileStorageId),
                nameof(ProjectCreatorSettings.DeveloperDbConnectionId),
                nameof(ProjectCreatorSettings.FakeHostProjectName), nameof(ProjectCreatorSettings.Id),
                nameof(ProjectCreatorSettings.IndentSize), nameof(ProjectCreatorSettings.ProductionEnvironmentId),
                nameof(ProjectCreatorSettings.ProductionServerId),
                nameof(ProjectCreatorSettings.ProjectsFolderPathReal),
                nameof(ProjectCreatorSettings.SecretsFolderPathReal), nameof(ProjectCreatorSettings.UseSmartSchemaId),
                nameof(ProjectCreatorSettings.Version)
            ], projectCreatorSettings.GetProperties().Select(x => x.Name).Order(StringComparer.Ordinal));
    }

    //The React template is another aggregate: an optional foreign key that refuses to delete the template in use
    [Fact]
    public void Model_MapsProjectTemplateToTheProjectTemplatesTableWithAnOptionalRestrictedReactTemplate()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType projectTemplate = EntityTypeOf<ProjectTemplate>(context);

        Assert.Equal("ProjectTemplates", projectTemplate.GetTableName());
        AssertUniqueName(projectTemplate);
        AssertText(projectTemplate, nameof(ProjectTemplate.Name), false, ProjectTemplate.NameMaxLength);
        AssertText(projectTemplate, nameof(ProjectTemplate.SupportProjectType), false,
            ProjectTemplate.SupportProjectTypeMaxLength);
        AssertText(projectTemplate, nameof(ProjectTemplate.TestProjectName), true,
            ProjectTemplate.TestProjectNameMaxLength);
        AssertText(projectTemplate, nameof(ProjectTemplate.TestProjectShortName), true,
            ProjectTemplate.TestProjectShortNameMaxLength);
        AssertOptionalRestrictedReferences(projectTemplate,
            [(nameof(ProjectTemplate.ReactTemplateId), typeof(ReactAppTemplate))]);
        Assert.All(
            projectTemplate.GetProperties().Where(x => x.ClrType == typeof(bool)),
            flag => Assert.False(flag.IsNullable));
        Assert.Equal(
            [
                nameof(ProjectTemplate.Id), nameof(ProjectTemplate.Name), nameof(ProjectTemplate.ReactTemplateId),
                nameof(ProjectTemplate.SupportProjectType), nameof(ProjectTemplate.TestProjectName),
                nameof(ProjectTemplate.TestProjectShortName), nameof(ProjectTemplate.UseCarcass),
                nameof(ProjectTemplate.UseDatabase), nameof(ProjectTemplate.UseDbPartFolderForDatabaseProjects),
                nameof(ProjectTemplate.UseFluentValidation), nameof(ProjectTemplate.UseHttps),
                nameof(ProjectTemplate.UseIdentity), nameof(ProjectTemplate.UseMenu),
                nameof(ProjectTemplate.UseReCounter), nameof(ProjectTemplate.UseReact),
                nameof(ProjectTemplate.UseSignalR), nameof(ProjectTemplate.Version)
            ], projectTemplate.GetProperties().Select(x => x.Name).Order(StringComparer.Ordinal));
    }

    //The root of the Project aggregate. The .editorconfig template is another aggregate: an optional foreign key that
    //refuses to delete the template in use. The paths are canonical paths, stored as they come
    [Fact]
    public void Model_MapsProjectToTheProjectsTableWithAnOptionalRestrictedEditorConfigTemplate()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType project = EntityTypeOf<Project>(context);

        Assert.Equal("Projects", project.GetTableName());
        AssertUniqueName(project);
        AssertText(project, nameof(Project.Name), false, Project.NameMaxLength);
        AssertText(project, nameof(Project.ProjectType), false, Project.ProjectTypeMaxLength);
        AssertText(project, nameof(Project.ProjectGroupName), true, Project.ProjectGroupNameMaxLength);
        AssertText(project, nameof(Project.ProjectDescription), true, Project.ProjectDescriptionMaxLength);
        AssertText(project, nameof(Project.KeyGuidPart), true, Project.KeyGuidPartMaxLength);
        Assert.All(
            [
                nameof(Project.MainProjectName), nameof(Project.ApiContractsProjectName), nameof(Project.SpaProjectName),
                nameof(Project.DbContextName), nameof(Project.ProjectShortPrefix),
                nameof(Project.ScaffoldSeederProjectName), nameof(Project.DbContextProjectName),
                nameof(Project.NewDataSeedingClassLibProjectName)
            ], name => AssertText(project, name, true, Project.CodeNameMaxLength));
        Assert.All(
            [
                nameof(Project.ProgramArchiveDateMask), nameof(Project.ProgramArchiveExtension),
                nameof(Project.ParametersFileDateMask), nameof(Project.ParametersFileExtension)
            ], name => AssertText(project, name, true, Project.MaskMaxLength));
        Assert.All(
            [
                nameof(Project.ProjectFolderName), nameof(Project.SolutionFileName),
                nameof(Project.ProjectSecurityFolderPath), nameof(Project.MigrationStartupProjectFilePath),
                nameof(Project.MigrationProjectFilePath), nameof(Project.DataSeederRulesByTableStartupProjectFilePath),
                nameof(Project.OldDataConvertorForDataSeeder), nameof(Project.SeedProjectFilePath),
                nameof(Project.SeedProjectParametersFilePath), nameof(Project.ExcludesRulesParametersFilePath),
                nameof(Project.AppSetEnKeysJsonFileName), nameof(Project.MigrationSqlFilesFolder),
                nameof(Project.PrepareProdCopyDatabaseProjectFilePath),
                nameof(Project.PrepareProdCopyDatabaseProjectParametersFilePath),
                nameof(Project.PairedDbObjectsResultFileName)
            ], name => AssertText(project, name, true, Project.PathMaxLength));
        Assert.All(
            [nameof(Project.MajorVersion), nameof(Project.MinorVersion), nameof(Project.UseAlternativeWebAgent)],
            name => Assert.False(project.GetProperty(name).IsNullable));
        AssertOptionalRestrictedReferences(project,
            [(nameof(Project.EditorConfigFileTypeId), typeof(EditorConfigFileType))]);
        Assert.Equal(38, project.GetProperties().Count());
    }

    //Both database parameters are optional owned types in the same row, with the columns of the reusable
    //configuration. Their required fields are stored in nullable columns, so EF can tell a missing part from an empty
    //one, and every reference is an optional foreign key that refuses to delete the record in use
    [Theory]
    [InlineData(nameof(Project.DevDatabaseParameters))]
    [InlineData(nameof(Project.ProdCopyDatabaseParameters))]
    public void Model_MapsTheDatabaseParametersOfAProjectToOptionalOwnedColumnsWithRestrictedReferences(
        string navigationName)
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);
        var projectsTable = StoreObjectIdentifier.Table("Projects");

        INavigation navigation = EntityTypeOf<Project>(context).GetNavigations().Single(x => x.Name == navigationName);

        Assert.True(navigation.ForeignKey.IsOwnership);
        Assert.False(navigation.ForeignKey.IsRequiredDependent);
        IEntityType parameters = navigation.TargetEntityType;
        Assert.Equal(typeof(DatabaseParameters), parameters.ClrType);
        Assert.Equal("Projects", parameters.GetTableName());
        AssertText(parameters, nameof(DatabaseParameters.DatabaseRecoveryModel), true,
            DatabaseParameters.DatabaseRecoveryModelMaxLength);
        AssertText(parameters, nameof(DatabaseParameters.DbServerFoldersSetName), true,
            DatabaseFoldersSet.NameMaxLength);
        AssertText(parameters, nameof(DatabaseParameters.DatabaseName), true, DatabaseParameters.DatabaseNameMaxLength);
        AssertText(parameters, nameof(DatabaseParameters.BackupNamePrefix), true,
            DatabaseParameters.BackupNamePrefixMaxLength);
        AssertText(parameters, nameof(DatabaseParameters.DateMask), true, DatabaseParameters.DateMaskMaxLength);
        AssertText(parameters, nameof(DatabaseParameters.BackupFileExtension), true,
            DatabaseParameters.BackupFileExtensionMaxLength);
        AssertText(parameters, nameof(DatabaseParameters.BackupNameMiddlePart), true,
            DatabaseParameters.BackupNameMiddlePartMaxLength);
        AssertText(parameters, nameof(DatabaseParameters.BackupType), true, DatabaseParameters.BackupTypeMaxLength);
        Assert.All([nameof(DatabaseParameters.CommandTimeOut), nameof(DatabaseParameters.SkipBackupBeforeRestore)],
            name =>
            {
                IProperty property = parameters.GetProperty(name);
                Assert.False(property.IsNullable);
                Assert.True(property.IsColumnNullable(projectsTable));
            });
        AssertOptionalRestrictedReferences(parameters,
        [
            (nameof(DatabaseParameters.DbConnectionId), typeof(DatabaseServerConnection)),
            (nameof(DatabaseParameters.FileStorageId), typeof(FileStorage)),
            (nameof(DatabaseParameters.SmartSchemaId), typeof(SmartSchema))
        ]);
        string[] fields =
        [
            "BackupFileExtension", "BackupNameMiddlePart", "BackupNamePrefix", "BackupType", "CommandTimeOut",
            "Compress", "DatabaseName", "DatabaseRecoveryModel", "DateMask", "DbConnectionId",
            "DbServerFoldersSetName", "FileStorageId", "SkipBackupBeforeRestore", "SmartSchemaId", "Verify"
        ];
        Assert.Equal(["Id", .. fields.Select(x => $"{navigationName}_{x}")],
            parameters.GetProperties().Select(x => x.GetColumnName(projectsTable)).OrderBy(x => x != "Id")
                .ThenBy(x => x, StringComparer.Ordinal));
    }

    //The children of the aggregate: a required foreign key that cascades, so the replaced children of an update are
    //deleted. A git or npm package of another aggregate is a required foreign key that refuses to delete the record in
    //use, and every child appears once in its project
    [Fact]
    public void Model_MapsProjectGitRepoToTheProjectGitReposTableAsAChildOfTheProject()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType gitRepo = EntityTypeOf<ProjectGitRepo>(context);

        Assert.Equal("ProjectGitRepos", gitRepo.GetTableName());
        AssertChildOf<Project>(gitRepo, "ProjectId", nameof(Project.GitRepos));
        AssertRequiredRestrictedReference<GitRepo>(gitRepo, nameof(ProjectGitRepo.GitRepoId));
        IProperty kind = gitRepo.GetProperty(nameof(ProjectGitRepo.Kind));
        Assert.False(kind.IsNullable);
        Assert.Equal(ProjectGitRepo.KindMaxLength, kind.GetMaxLength());
        Assert.Equal(typeof(string), kind.GetTypeMapping().Converter?.ProviderClrType);
        AssertUniqueChildIndex(gitRepo, nameof(ProjectGitRepo.GitRepoId), nameof(ProjectGitRepo.Kind));
        Assert.Equal(["GitRepoId", "Id", "Kind", "ProjectId"],
            gitRepo.GetProperties().Select(x => x.Name).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Model_MapsProjectNpmPackageToTheProjectNpmPackagesTableAsAChildOfTheProject()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType npmPackage = EntityTypeOf<ProjectNpmPackage>(context);

        Assert.Equal("ProjectNpmPackages", npmPackage.GetTableName());
        AssertChildOf<Project>(npmPackage, "ProjectId", nameof(Project.NpmPackages));
        AssertRequiredRestrictedReference<NpmPackage>(npmPackage, nameof(ProjectNpmPackage.NpmPackageId));
        AssertUniqueChildIndex(npmPackage, nameof(ProjectNpmPackage.NpmPackageId));
        Assert.Equal(["Id", "NpmPackageId", "ProjectId"],
            npmPackage.GetProperties().Select(x => x.Name).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Model_MapsProjectRedundantFileToTheProjectRedundantFilesTableAsAChildOfTheProject()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType redundantFile = EntityTypeOf<ProjectRedundantFile>(context);

        Assert.Equal("ProjectRedundantFiles", redundantFile.GetTableName());
        AssertText(redundantFile, nameof(ProjectRedundantFile.FileName), false,
            ProjectRedundantFile.FileNameMaxLength);
        AssertChildOf<Project>(redundantFile, "ProjectId", nameof(Project.RedundantFiles));
        AssertUniqueChildIndex(redundantFile, nameof(ProjectRedundantFile.FileName));
        Assert.Equal(["FileName", "Id", "ProjectId"],
            redundantFile.GetProperties().Select(x => x.Name).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Model_MapsProjectAllowedToolToTheProjectAllowedToolsTableAsAChildOfTheProject()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType allowedTool = EntityTypeOf<ProjectAllowedTool>(context);

        Assert.Equal("ProjectAllowedTools", allowedTool.GetTableName());
        AssertText(allowedTool, nameof(ProjectAllowedTool.ToolName), false, ProjectAllowedTool.ToolNameMaxLength);
        AssertChildOf<Project>(allowedTool, "ProjectId", nameof(Project.AllowedTools));
        AssertUniqueChildIndex(allowedTool, nameof(ProjectAllowedTool.ToolName));
        Assert.Equal(["Id", "ProjectId", "ToolName"],
            allowedTool.GetProperties().Select(x => x.Name).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Model_MapsProjectEndpointToTheProjectEndpointsTableAsAChildOfTheProject()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType endpoint = EntityTypeOf<ProjectEndpoint>(context);

        Assert.Equal("ProjectEndpoints", endpoint.GetTableName());
        AssertText(endpoint, nameof(ProjectEndpoint.Name), false, ProjectEndpoint.NameMaxLength);
        AssertText(endpoint, nameof(ProjectEndpoint.EndpointName), true, ProjectEndpoint.EndpointNameMaxLength);
        AssertText(endpoint, nameof(ProjectEndpoint.EndpointRoute), true, ProjectEndpoint.EndpointRouteMaxLength);
        AssertText(endpoint, nameof(ProjectEndpoint.HttpMethod), false, ProjectEndpoint.HttpMethodMaxLength);
        AssertText(endpoint, nameof(ProjectEndpoint.EndpointType), false, ProjectEndpoint.EndpointTypeMaxLength);
        AssertText(endpoint, nameof(ProjectEndpoint.ReturnType), true, ProjectEndpoint.ReturnTypeMaxLength);
        AssertChildOf<Project>(endpoint, "ProjectId", nameof(Project.Endpoints));
        AssertUniqueChildIndex(endpoint, nameof(ProjectEndpoint.Name));
        Assert.Equal(
            [
                "EndpointName", "EndpointRoute", "EndpointType", "HttpMethod", "Id", "Name", "ProjectId",
                "RequireAuthorization", "ReturnType", "SendMessageToCurrentUser"
            ], endpoint.GetProperties().Select(x => x.Name).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Model_MapsProjectRouteClassToTheProjectRouteClassesTableAsAChildOfTheProject()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        IEntityType routeClass = EntityTypeOf<ProjectRouteClass>(context);

        Assert.Equal("ProjectRouteClasses", routeClass.GetTableName());
        AssertText(routeClass, nameof(ProjectRouteClass.Name), false, ProjectRouteClass.NameMaxLength);
        AssertText(routeClass, nameof(ProjectRouteClass.Root), true, ProjectRouteClass.RootMaxLength);
        AssertText(routeClass, nameof(ProjectRouteClass.ApiVersion), true, ProjectRouteClass.ApiVersionMaxLength);
        AssertText(routeClass, nameof(ProjectRouteClass.Base), true, ProjectRouteClass.BaseMaxLength);
        AssertChildOf<Project>(routeClass, "ProjectId", nameof(Project.RouteClasses));
        AssertUniqueChildIndex(routeClass, nameof(ProjectRouteClass.Name));
        Assert.Equal(["ApiVersion", "Base", "Id", "Name", "ProjectId", "Root"],
            routeClass.GetProperties().Select(x => x.Name).Order(StringComparer.Ordinal));
    }

    //The context registers the default convention of SystemTools, which gives a DateTime column the SQL Server type
    //datetime instead of EF's datetime2. No entity of the model has such a column, so a test entity is added to it
    [Fact]
    public void Model_GivesADateTimeColumnTheTypeOfTheDefaultConvention()
    {
        DbContextOptions<SupportToolsServerDbContext> options =
            new DbContextOptionsBuilder<SupportToolsServerDbContext>().UseSqlServer(ConnectionString)
                .ReplaceService<IModelCustomizer, DateTimeEntityModelCustomizer>().Options;
        using var context = new SupportToolsServerDbContext(options, _dispatcher.Object);

        IEntityType dateTimeEntity = EntityTypeOf<DateTimeEntity>(context);

        Assert.Equal("datetime", dateTimeEntity.GetProperty(nameof(DateTimeEntity.Moment)).GetColumnType());
    }

    [Fact]
    public void DbSets_ExposeTheTables()
    {
        using var context = new SupportToolsServerDbContext(Options, _dispatcher.Object);

        Assert.NotNull(context.GitRepos);
        Assert.NotNull(context.GitIgnoreFileTypes);
        Assert.NotNull(context.EditorConfigFileTypes);
        Assert.NotNull(context.Environments);
        Assert.NotNull(context.Runtimes);
        Assert.NotNull(context.NpmPackages);
        Assert.NotNull(context.ReactAppTemplates);
        Assert.NotNull(context.DotnetTools);
        Assert.NotNull(context.SmartSchemas);
        Assert.NotNull(context.FileStorages);
        Assert.NotNull(context.ApiClients);
        Assert.NotNull(context.DatabaseServerConnections);
        Assert.NotNull(context.Servers);
        Assert.NotNull(context.GlobalSettings);
        Assert.NotNull(context.ProjectCreatorSettings);
        Assert.NotNull(context.ProjectTemplates);
        Assert.NotNull(context.Projects);
    }

    //The child has a required shadow foreign key to the root, stored as the Guid of the root's id and deleted in
    //cascade, and the root reaches the children through the collection navigation. A child may also reference another
    //aggregate, so the foreign key is the one to the root
    private static void AssertChildOf<TRoot>(IEntityType child, string foreignKeyName, string navigationName)
    {
        IForeignKey foreignKey =
            Assert.Single(child.GetForeignKeys(), x => x.PrincipalEntityType.ClrType == typeof(TRoot));
        Assert.Equal(typeof(TRoot), foreignKey.PrincipalEntityType.ClrType);
        IProperty property = Assert.Single(foreignKey.Properties);
        Assert.Equal(foreignKeyName, property.Name);
        Assert.True(property.IsShadowProperty());
        Assert.False(property.IsNullable);
        Assert.Equal("uniqueidentifier", property.GetColumnType());
        Assert.True(foreignKey.IsRequired);
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
        Assert.Equal(navigationName, foreignKey.PrincipalToDependent?.Name);
    }

    //Optional foreign keys stored as the Guid of the referenced id, which refuse to delete the records in use
    private static void AssertOptionalRestrictedReferences(IEntityType entityType,
        (string Name, Type ClrType)[] expected)
    {
        List<IForeignKey> references = [.. entityType.GetForeignKeys().Where(x => !x.IsOwnership)];
        Assert.Equal(expected,
            references.Select(x => (Assert.Single(x.Properties).Name, x.PrincipalEntityType.ClrType))
                .OrderBy(x => x.Name, StringComparer.Ordinal));
        Assert.All(references, foreignKey =>
        {
            Assert.False(foreignKey.IsRequired);
            Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
            Assert.Equal("uniqueidentifier", foreignKey.Properties[0].GetColumnType());
        });
    }

    //A required foreign key to another aggregate, stored as the Guid of the referenced id, which refuses to delete the
    //record in use
    private static void AssertRequiredRestrictedReference<TReferenced>(IEntityType entityType, string propertyName)
    {
        IForeignKey foreignKey = Assert.Single(entityType.GetForeignKeys(),
            x => x.PrincipalEntityType.ClrType == typeof(TReferenced));
        IProperty property = Assert.Single(foreignKey.Properties);
        Assert.Equal(propertyName, property.Name);
        Assert.False(property.IsNullable);
        Assert.Equal("uniqueidentifier", property.GetColumnType());
        Assert.True(foreignKey.IsRequired);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }

    //A unique index on the project and the key of the child
    private static void AssertUniqueChildIndex(IEntityType child, params string[] keyPropertyNames)
    {
        Assert.Contains(child.GetIndexes(),
            i => i.IsUnique && i.Properties.Select(x => x.Name).SequenceEqual(["ProjectId", .. keyPropertyNames]));
    }

    //The only row has the fixed key of the singleton, and the check constraint refuses any other key. Check constraints
    //are kept only in the design-time model
    private static void AssertSingletonCheck<TEntity>(SupportToolsServerDbContext context, string constraintName)
    {
        IEntityType entityType = Assert.IsType<IEntityType>(
            context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(TEntity)), exactMatch: false);
        ICheckConstraint check = Assert.Single(entityType.GetCheckConstraints());
        Assert.Equal(constraintName, check.Name);
        Assert.Equal("[Id] = '00000000-0000-0000-0000-000000000001'", check.Sql);
        Assert.Equal("uniqueidentifier", entityType.GetProperty("Id").GetColumnType());
    }

    private static IEntityType EntityTypeOf<TEntity>(SupportToolsServerDbContext context)
    {
        return Assert.IsType<IEntityType>(context.Model.FindEntityType(typeof(TEntity)), exactMatch: false);
    }

    private static void AssertUniqueName(IEntityType entityType)
    {
        Assert.Contains(entityType.GetIndexes(), i => i.IsUnique && i.Properties.Single().Name == "Name");
    }

    private static void AssertText(IEntityType entityType, string propertyName, bool isNullable, int maxLength)
    {
        IProperty property = entityType.GetProperty(propertyName);
        Assert.Equal(isNullable, property.IsNullable);
        Assert.Equal(maxLength, property.GetMaxLength());
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

    private sealed class DateTimeEntity
    {
        public DateTime Moment { get; init; }
    }

    //Builds the model of the context and then adds the test entity, keyless because only its column matters
    private sealed class DateTimeEntityModelCustomizer : RelationalModelCustomizer
    {
        public DateTimeEntityModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies)
        {
        }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);
            modelBuilder.Entity<DateTimeEntity>().HasNoKey();
        }
    }
}
