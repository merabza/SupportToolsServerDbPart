using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.GitRepos;
using SupportToolsServerCore.Domain.Projects;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class ProjectGitRepoConfiguration : IEntityTypeConfiguration<ProjectGitRepo>
{
    public void Configure(EntityTypeBuilder<ProjectGitRepo> builder)
    {
        //შვილს DbSet არ აქვს, ამიტომ ცხრილის სახელი აქ იწერება
        builder.ToTable("ProjectGitRepos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(projectGitRepoId => projectGitRepoId.Value,
            guidValue => new ProjectGitRepoId(guidValue)).IsRequired();

        builder.Property(x => x.GitRepoId)
            .HasConversion(gitRepoId => gitRepoId.Value, guidValue => new GitRepoId(guidValue)).IsRequired();

        //შვილები პროექტთან ერთად იშლება. კავშირი სავალდებულოა, ამიტომ განახლებისას ჩანაცვლებული შვილიც იშლება
        builder.HasOne<Project>().WithMany(x => x.GitRepos).HasForeignKey(ProjectConfiguration.ProjectIdColumn)
            .IsRequired().OnDelete(DeleteBehavior.Cascade);

        //პროექტის მიერ გამოყენებული git-ის წაშლა ბაზაშიც იკრძალება (README §4.2)
        builder.HasOne<GitRepo>().WithMany().HasForeignKey(x => x.GitRepoId).OnDelete(DeleteBehavior.Restrict);

        //ერთი git პროექტში ერთხელ გვხვდება
        builder.HasIndex(ProjectConfiguration.ProjectIdColumn, nameof(ProjectGitRepo.GitRepoId)).IsUnique();
    }
}
