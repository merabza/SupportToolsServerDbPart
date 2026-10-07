using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.GitRepoProjects;
using SupportToolsServerCore.Domain.GitRepos;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class GitRepoProjectConfiguration : IEntityTypeConfiguration<GitRepoProject>
{
    public void Configure(EntityTypeBuilder<GitRepoProject> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(gitRepoProjectId => gitRepoProjectId.Value,
            guidValue => new GitRepoProjectId(guidValue)).IsRequired();

        builder.Property(x => x.GitRepoId).HasConversion(gitRepoId => gitRepoId.Value,
            guidValue => new GitRepoId(guidValue)).IsRequired();

        //პროექტები რეპოზიტორიის სკანირების შედეგია, ამიტომ რეპოზიტორიასთან ერთად იშლება
        builder.HasOne<GitRepo>().WithMany().HasForeignKey(x => x.GitRepoId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.ProjectRelativePath).IsRequired()
            .HasMaxLength(GitRepoProject.ProjectRelativePathMaxLength);
        builder.Property(x => x.ProjectFileName).IsRequired().HasMaxLength(GitRepoProject.ProjectFileNameMaxLength);

        //პროექტის ფაილი რეპოზიტორიაში ერთხელ გვხვდება. ინდექსის გასაღები SQL Server-ის 900 ბაიტში ეტევა
        builder.HasIndex(x => new { x.GitRepoId, x.ProjectRelativePath, x.ProjectFileName }).IsUnique();
    }
}
