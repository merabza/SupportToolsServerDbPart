using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupportToolsServerCore.Application.Abstractions;
using SupportToolsServerCore.Domain.ApiClients;
using SupportToolsServerCore.Domain.DatabaseServerConnections;
using SupportToolsServerCore.Domain.DeploymentEnvironments;
using SupportToolsServerCore.Domain.DotnetTools;
using SupportToolsServerCore.Domain.EditorConfigFileTypes;
using SupportToolsServerCore.Domain.FileStorages;
using SupportToolsServerCore.Domain.GitIgnoreFileTypes;
using SupportToolsServerCore.Domain.GitRepoProjects;
using SupportToolsServerCore.Domain.GitRepos;
using SupportToolsServerCore.Domain.NpmPackages;
using SupportToolsServerCore.Domain.Projects;
using SupportToolsServerCore.Domain.ProjectTemplates;
using SupportToolsServerCore.Domain.ReactAppTemplates;
using SupportToolsServerCore.Domain.Runtimes;
using SupportToolsServerCore.Domain.Servers;
using SupportToolsServerCore.Domain.Settings;
using SupportToolsServerCore.Domain.SmartSchemas;
using SupportToolsServerCore.Domain.StoredFiles;
using SystemTools.DatabaseToolsShared;
using SystemTools.SharedKernel;

namespace SupportToolsServerDbPart.Db;

public sealed class SupportToolsServerDbContext : DbContext, ISupportToolsServerDbContext
{
    //დიზაინის დროის კონსტრუქტორებს დისპეტჩერი არ აქვთ და დომენის მოვლენებს არ აგზავნიან
    private readonly IDomainEventsDispatcher? _domainEventsDispatcher;

    public SupportToolsServerDbContext(DbContextOptions<SupportToolsServerDbContext> options, bool isDesignTime) :
        base(options)
    {
        //Console.WriteLine("SupportToolsServerDbContext Constructor 2...");
    }

    public SupportToolsServerDbContext(DbContextOptions<SupportToolsServerDbContext> options, int int1) : base(options)
    {
        //Console.WriteLine("SupportToolsServerDbContext Constructor 3...");
    }

    public SupportToolsServerDbContext(DbContextOptions<SupportToolsServerDbContext> options,
        IDomainEventsDispatcher domainEventsDispatcher) : base(options)
    {
        //Console.WriteLine("SupportToolsServerDbContext Constructor 4...");
        _domainEventsDispatcher = domainEventsDispatcher;
    }

    //ბაზაში არსებული ცხრილები წარმოდგენილი DbSet-ების სახით
    //public DbSet<GitData> GitData => Set<GitData>();
    public DbSet<ApiClient> ApiClients { get; set; }
    public DbSet<DatabaseServerConnection> DatabaseServerConnections { get; set; }
    public DbSet<DotnetTool> DotnetTools { get; set; }
    public DbSet<EditorConfigFileType> EditorConfigFileTypes { get; set; }
    public DbSet<DeploymentEnvironment> Environments { get; set; }
    public DbSet<FileStorage> FileStorages { get; set; }
    public DbSet<GitIgnoreFileType> GitIgnoreFileTypes { get; set; }
    public DbSet<GitRepoProject> GitRepoProjects { get; set; }

    public DbSet<GitRepo> GitRepos { get; set; }
    public DbSet<GlobalSettings> GlobalSettings { get; set; }
    public DbSet<NpmPackage> NpmPackages { get; set; }
    public DbSet<ProjectCreatorSettings> ProjectCreatorSettings { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectTemplate> ProjectTemplates { get; set; }
    public DbSet<ReactAppTemplate> ReactAppTemplates { get; set; }

    public DbSet<Runtime> Runtimes { get; set; }

    public DbSet<Server> Servers { get; set; }

    public DbSet<SmartSchema> SmartSchemas { get; set; }
    public DbSet<StoredFile> StoredFiles { get; set; }
    //public DbSet<ApiKeyByRemoteIpAddress> ApiKeysByRemoteIpAddresses => Set<ApiKeyByRemoteIpAddress>();

    //დომენის მოვლენები შენახვის შემდეგ იგზავნება, ანუ ჰენდლერები უკვე შენახულ მონაცემებს ეხებიან
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(cancellationToken);

        await PublishDomainEventsAsync(cancellationToken);

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Console.WriteLine("AppGrammarGeDbContext OnModelCreating Start...");

        base.OnModelCreating(modelBuilder);

        //Console.WriteLine("AppGrammarGeDbContext OnModelCreating Pass 1...");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SupportToolsServerDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new DatabaseEntitiesDefaultConvention());
    }

    private async Task PublishDomainEventsAsync(CancellationToken cancellationToken)
    {
        if (_domainEventsDispatcher is null)
        {
            return;
        }

        List<IDomainEvent> domainEvents =
        [
            .. ChangeTracker.Entries<Entity>().Select(entry => entry.Entity).SelectMany(entity =>
            {
                List<IDomainEvent> entityDomainEvents = entity.DomainEvents;
                entity.ClearDomainEvents();
                return entityDomainEvents;
            })
        ];

        await _domainEventsDispatcher.DispatchAsync(domainEvents, cancellationToken);
    }
}
