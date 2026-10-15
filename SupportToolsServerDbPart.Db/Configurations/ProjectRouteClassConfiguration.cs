using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.Projects;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class ProjectRouteClassConfiguration : IEntityTypeConfiguration<ProjectRouteClass>
{
    public void Configure(EntityTypeBuilder<ProjectRouteClass> builder)
    {
        //შვილს DbSet არ აქვს, ამიტომ ცხრილის სახელი აქ იწერება
        builder.ToTable("ProjectRouteClasses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(projectRouteClassId => projectRouteClassId.Value,
            guidValue => new ProjectRouteClassId(guidValue)).IsRequired();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(ProjectRouteClass.NameMaxLength);
        builder.Property(x => x.Root).HasMaxLength(ProjectRouteClass.RootMaxLength);
        builder.Property(x => x.ApiVersion).HasMaxLength(ProjectRouteClass.ApiVersionMaxLength);
        builder.Property(x => x.Base).HasMaxLength(ProjectRouteClass.BaseMaxLength);

        //შვილები პროექტთან ერთად იშლება. კავშირი სავალდებულოა, ამიტომ განახლებისას ჩანაცვლებული შვილიც იშლება
        builder.HasOne<Project>().WithMany(x => x.RouteClasses).HasForeignKey(ProjectConfiguration.ProjectIdColumn)
            .IsRequired().OnDelete(DeleteBehavior.Cascade);

        //route კლასის key (კლიენტის dictionary-ის key) პროექტში ერთხელ გვხვდება
        builder.HasIndex(ProjectConfiguration.ProjectIdColumn, nameof(ProjectRouteClass.Name)).IsUnique();
    }
}
