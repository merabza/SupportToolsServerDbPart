using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.Projects;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class ProjectAllowedToolConfiguration : IEntityTypeConfiguration<ProjectAllowedTool>
{
    public void Configure(EntityTypeBuilder<ProjectAllowedTool> builder)
    {
        //შვილს DbSet არ აქვს, ამიტომ ცხრილის სახელი აქ იწერება
        builder.ToTable("ProjectAllowedTools");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(projectAllowedToolId => projectAllowedToolId.Value,
            guidValue => new ProjectAllowedToolId(guidValue)).IsRequired();

        builder.Property(x => x.ToolName).IsRequired().HasMaxLength(ProjectAllowedTool.ToolNameMaxLength);

        //შვილები პროექტთან ერთად იშლება. კავშირი სავალდებულოა, ამიტომ განახლებისას ჩანაცვლებული შვილიც იშლება
        builder.HasOne<Project>().WithMany(x => x.AllowedTools).HasForeignKey(ProjectConfiguration.ProjectIdColumn)
            .IsRequired().OnDelete(DeleteBehavior.Cascade);

        //ინსტრუმენტი პროექტში ერთხელ გვხვდება
        builder.HasIndex(ProjectConfiguration.ProjectIdColumn, nameof(ProjectAllowedTool.ToolName)).IsUnique();
    }
}
