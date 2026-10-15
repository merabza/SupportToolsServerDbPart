using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.NpmPackages;
using SupportToolsServerCore.Domain.Projects;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class ProjectNpmPackageConfiguration : IEntityTypeConfiguration<ProjectNpmPackage>
{
    public void Configure(EntityTypeBuilder<ProjectNpmPackage> builder)
    {
        //შვილს DbSet არ აქვს, ამიტომ ცხრილის სახელი აქ იწერება
        builder.ToTable("ProjectNpmPackages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(projectNpmPackageId => projectNpmPackageId.Value,
            guidValue => new ProjectNpmPackageId(guidValue)).IsRequired();

        builder.Property(x => x.NpmPackageId)
            .HasConversion(npmPackageId => npmPackageId.Value, guidValue => new NpmPackageId(guidValue))
            .IsRequired();

        //შვილები პროექტთან ერთად იშლება. კავშირი სავალდებულოა, ამიტომ განახლებისას ჩანაცვლებული შვილიც იშლება
        builder.HasOne<Project>().WithMany(x => x.NpmPackages).HasForeignKey(ProjectConfiguration.ProjectIdColumn)
            .IsRequired().OnDelete(DeleteBehavior.Cascade);

        //პროექტის მიერ გამოყენებული npm პაკეტის წაშლა ბაზაშიც იკრძალება (README §4.2)
        builder.HasOne<NpmPackage>().WithMany().HasForeignKey(x => x.NpmPackageId).OnDelete(DeleteBehavior.Restrict);

        //პაკეტი პროექტში ერთხელ გვხვდება
        builder.HasIndex(ProjectConfiguration.ProjectIdColumn, nameof(ProjectNpmPackage.NpmPackageId)).IsUnique();
    }
}
