using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupportToolsServerCore.Application.Abstractions;
using SupportToolsServerCore.Domain.EditorConfigFileTypes;
using SupportToolsServerCore.Domain.GitIgnoreFileTypes;
using SupportToolsServerCore.Domain.GitRepos;
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
    public DbSet<EditorConfigFileType> EditorConfigFileTypes { get; set; }
    public DbSet<GitIgnoreFileType> GitIgnoreFileTypes { get; set; }

    public DbSet<GitRepo> GitRepos { get; set; }
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
