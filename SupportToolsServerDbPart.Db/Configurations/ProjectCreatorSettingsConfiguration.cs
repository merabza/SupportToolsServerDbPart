using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.DatabaseServerConnections;
using SupportToolsServerCore.Domain.DeploymentEnvironments;
using SupportToolsServerCore.Domain.FileStorages;
using SupportToolsServerCore.Domain.Primitives;
using SupportToolsServerCore.Domain.Servers;
using SupportToolsServerCore.Domain.Settings;
using SupportToolsServerCore.Domain.SmartSchemas;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class ProjectCreatorSettingsConfiguration : IEntityTypeConfiguration<ProjectCreatorSettings>
{
    public void Configure(EntityTypeBuilder<ProjectCreatorSettings> builder)
    {
        //ჩანაწერი ერთადერთია (singleton): გასაღები ფიქსირებულია და სხვა გასაღებით ჩაწერას ბაზაც კრძალავს
        builder.ToTable(table => table.HasCheckConstraint("CK_ProjectCreatorSettings_Singleton",
            $"[Id] = '{ProjectCreatorSettingsId.Singleton.Value}'"));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(projectCreatorSettingsId => projectCreatorSettingsId.Value,
            guidValue => new ProjectCreatorSettingsId(guidValue)).IsRequired();

        builder.Property(x => x.FakeHostProjectName).HasMaxLength(ProjectCreatorSettings.FakeHostProjectNameMaxLength);
        builder.Property(x => x.ProjectsFolderPathReal)
            .HasMaxLength(ProjectCreatorSettings.ProjectsFolderPathRealMaxLength);
        builder.Property(x => x.SecretsFolderPathReal)
            .HasMaxLength(ProjectCreatorSettings.SecretsFolderPathRealMaxLength);

        //optimistic concurrency-ის token-ი (CLAUDE.md, Registry conventions)
        builder.Property(x => x.Version).IsConcurrencyToken().HasDefaultValue(EntityVersion.Initial);

        //null-ს კონვერტერები არ ეხება: მითითების გარეშე სვეტი NULL-ია
        builder.Property(x => x.ProductionServerId)
            .HasConversion(serverId => serverId!.Value, guidValue => new ServerId(guidValue));
        builder.Property(x => x.ProductionEnvironmentId).HasConversion(
            deploymentEnvironmentId => deploymentEnvironmentId!.Value,
            guidValue => new DeploymentEnvironmentId(guidValue));
        builder.Property(x => x.DeveloperDbConnectionId).HasConversion(
            databaseServerConnectionId => databaseServerConnectionId!.Value,
            guidValue => new DatabaseServerConnectionId(guidValue));
        builder.Property(x => x.DatabaseExchangeFileStorageId)
            .HasConversion(fileStorageId => fileStorageId!.Value, guidValue => new FileStorageId(guidValue));
        builder.Property(x => x.UseSmartSchemaId)
            .HasConversion(smartSchemaId => smartSchemaId!.Value, guidValue => new SmartSchemaId(guidValue));

        //გამოყენებული სერვერის, გარემოს, ბაზის კავშირის, ფაილსაცავისა და ჭკვიანი სქემის წაშლა ბაზაშიც იკრძალება
        //(README §4.2)
        builder.HasOne<Server>().WithMany().HasForeignKey(x => x.ProductionServerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<DeploymentEnvironment>().WithMany().HasForeignKey(x => x.ProductionEnvironmentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<DatabaseServerConnection>().WithMany().HasForeignKey(x => x.DeveloperDbConnectionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<FileStorage>().WithMany().HasForeignKey(x => x.DatabaseExchangeFileStorageId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SmartSchema>().WithMany().HasForeignKey(x => x.UseSmartSchemaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
