using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.EditorConfigFileTypes;
using SupportToolsServerCore.Domain.Primitives;
using SupportToolsServerCore.Domain.Projects;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    //პროექტის Id-ის სვეტი შვილების ცხრილებში. დომენის შვილმა თავისი პროექტი არ იცის, ამიტომ ეს shadow property-ა
    public const string ProjectIdColumn = "ProjectId";

    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Id).HasConversion(projectId => projectId.Value, guidValue => new ProjectId(guidValue))
            .IsRequired();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(Project.NameMaxLength);
        builder.Property(x => x.ProjectType).IsRequired().HasMaxLength(Project.ProjectTypeMaxLength);
        builder.Property(x => x.ProjectGroupName).HasMaxLength(Project.ProjectGroupNameMaxLength);
        builder.Property(x => x.ProjectDescription).HasMaxLength(Project.ProjectDescriptionMaxLength);

        builder.Property(x => x.MainProjectName).HasMaxLength(Project.CodeNameMaxLength);
        builder.Property(x => x.ApiContractsProjectName).HasMaxLength(Project.CodeNameMaxLength);
        builder.Property(x => x.SpaProjectName).HasMaxLength(Project.CodeNameMaxLength);
        builder.Property(x => x.DbContextName).HasMaxLength(Project.CodeNameMaxLength);
        builder.Property(x => x.ProjectShortPrefix).HasMaxLength(Project.CodeNameMaxLength);
        builder.Property(x => x.ScaffoldSeederProjectName).HasMaxLength(Project.CodeNameMaxLength);
        builder.Property(x => x.DbContextProjectName).HasMaxLength(Project.CodeNameMaxLength);
        builder.Property(x => x.NewDataSeedingClassLibProjectName).HasMaxLength(Project.CodeNameMaxLength);

        builder.Property(x => x.ProgramArchiveDateMask).HasMaxLength(Project.MaskMaxLength);
        builder.Property(x => x.ProgramArchiveExtension).HasMaxLength(Project.MaskMaxLength);
        builder.Property(x => x.ParametersFileDateMask).HasMaxLength(Project.MaskMaxLength);
        builder.Property(x => x.ParametersFileExtension).HasMaxLength(Project.MaskMaxLength);

        builder.Property(x => x.ProjectFolderName).HasMaxLength(Project.PathMaxLength);
        builder.Property(x => x.SolutionFileName).HasMaxLength(Project.PathMaxLength);
        builder.Property(x => x.ProjectSecurityFolderPath).HasMaxLength(Project.PathMaxLength);
        builder.Property(x => x.MigrationStartupProjectFilePath).HasMaxLength(Project.PathMaxLength);
        builder.Property(x => x.MigrationProjectFilePath).HasMaxLength(Project.PathMaxLength);
        builder.Property(x => x.DataSeederRulesByTableStartupProjectFilePath).HasMaxLength(Project.PathMaxLength);
        builder.Property(x => x.OldDataConvertorForDataSeeder).HasMaxLength(Project.PathMaxLength);
        builder.Property(x => x.SeedProjectFilePath).HasMaxLength(Project.PathMaxLength);
        builder.Property(x => x.SeedProjectParametersFilePath).HasMaxLength(Project.PathMaxLength);
        builder.Property(x => x.ExcludesRulesParametersFilePath).HasMaxLength(Project.PathMaxLength);
        builder.Property(x => x.AppSetEnKeysJsonFileName).HasMaxLength(Project.PathMaxLength);
        builder.Property(x => x.MigrationSqlFilesFolder).HasMaxLength(Project.PathMaxLength);
        builder.Property(x => x.PrepareProdCopyDatabaseProjectFilePath).HasMaxLength(Project.PathMaxLength);
        builder.Property(x => x.PrepareProdCopyDatabaseProjectParametersFilePath).HasMaxLength(Project.PathMaxLength);
        builder.Property(x => x.PairedDbObjectsResultFileName).HasMaxLength(Project.PathMaxLength);

        builder.Property(x => x.KeyGuidPart).HasMaxLength(Project.KeyGuidPartMaxLength);

        //optimistic concurrency-ის token-ი (CLAUDE.md, Registry conventions)
        builder.Property(x => x.Version).IsConcurrencyToken().HasDefaultValue(EntityVersion.Initial);

        //null-ს კონვერტერი არ ეხება: შაბლონის გარეშე სვეტი NULL-ია
        builder.Property(x => x.EditorConfigFileTypeId).HasConversion(
            editorConfigFileTypeId => editorConfigFileTypeId!.Value, guidValue => new EditorConfigFileTypeId(guidValue));

        //პროექტის მიერ გამოყენებული .editorconfig შაბლონის წაშლა ბაზაშიც იკრძალება (README §4.2)
        builder.HasOne<EditorConfigFileType>().WithMany().HasForeignKey(x => x.EditorConfigFileTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        //ბაზის პარამეტრები იმავე სტრიქონშია (owned type), სვეტები DevDatabaseParameters_ და ProdCopyDatabaseParameters_
        //პრეფიქსებით
        builder.OwnsOne(x => x.DevDatabaseParameters, DatabaseParametersConfiguration.Configure);
        builder.OwnsOne(x => x.ProdCopyDatabaseParameters, DatabaseParametersConfiguration.Configure);
    }
}
