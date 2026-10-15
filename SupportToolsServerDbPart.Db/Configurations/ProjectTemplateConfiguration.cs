using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.Primitives;
using SupportToolsServerCore.Domain.ProjectTemplates;
using SupportToolsServerCore.Domain.ReactAppTemplates;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class ProjectTemplateConfiguration : IEntityTypeConfiguration<ProjectTemplate>
{
    public void Configure(EntityTypeBuilder<ProjectTemplate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Id).HasConversion(projectTemplateId => projectTemplateId.Value,
            guidValue => new ProjectTemplateId(guidValue)).IsRequired();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(ProjectTemplate.NameMaxLength);
        builder.Property(x => x.SupportProjectType).IsRequired()
            .HasMaxLength(ProjectTemplate.SupportProjectTypeMaxLength);
        builder.Property(x => x.TestProjectName).HasMaxLength(ProjectTemplate.TestProjectNameMaxLength);
        builder.Property(x => x.TestProjectShortName).HasMaxLength(ProjectTemplate.TestProjectShortNameMaxLength);

        //optimistic concurrency-ის token-ი (CLAUDE.md, Registry conventions)
        builder.Property(x => x.Version).IsConcurrencyToken().HasDefaultValue(EntityVersion.Initial);

        //null-ს კონვერტერი არ ეხება: React-ის შაბლონის გარეშე სვეტი NULL-ია
        builder.Property(x => x.ReactTemplateId).HasConversion(reactAppTemplateId => reactAppTemplateId!.Value,
            guidValue => new ReactAppTemplateId(guidValue));

        //შაბლონის მიერ გამოყენებული React-ის შაბლონის წაშლა ბაზაშიც იკრძალება (README §4.2)
        builder.HasOne<ReactAppTemplate>().WithMany().HasForeignKey(x => x.ReactTemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
