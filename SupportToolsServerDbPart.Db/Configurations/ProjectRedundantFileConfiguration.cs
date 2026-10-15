using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.Projects;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class ProjectRedundantFileConfiguration : IEntityTypeConfiguration<ProjectRedundantFile>
{
    public void Configure(EntityTypeBuilder<ProjectRedundantFile> builder)
    {
        //შვილს DbSet არ აქვს, ამიტომ ცხრილის სახელი აქ იწერება
        builder.ToTable("ProjectRedundantFiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(projectRedundantFileId => projectRedundantFileId.Value,
            guidValue => new ProjectRedundantFileId(guidValue)).IsRequired();

        builder.Property(x => x.FileName).IsRequired().HasMaxLength(ProjectRedundantFile.FileNameMaxLength);

        //შვილები პროექტთან ერთად იშლება. კავშირი სავალდებულოა, ამიტომ განახლებისას ჩანაცვლებული შვილიც იშლება
        builder.HasOne<Project>().WithMany(x => x.RedundantFiles).HasForeignKey(ProjectConfiguration.ProjectIdColumn)
            .IsRequired().OnDelete(DeleteBehavior.Cascade);

        //ფაილის სახელი პროექტში ერთხელ გვხვდება
        builder.HasIndex(ProjectConfiguration.ProjectIdColumn, nameof(ProjectRedundantFile.FileName)).IsUnique();
    }
}
