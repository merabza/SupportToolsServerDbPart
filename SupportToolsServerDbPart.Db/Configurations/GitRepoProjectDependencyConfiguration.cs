using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.GitRepoProjects;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class GitRepoProjectDependencyConfiguration : IEntityTypeConfiguration<GitRepoProjectDependency>
{
    //პროექტის Id-ის სვეტი. დომენის დამოკიდებულებამ თავისი პროექტი არ იცის, ამიტომ ეს shadow property-ა
    public const string GitRepoProjectIdColumn = "GitRepoProjectId";

    public void Configure(EntityTypeBuilder<GitRepoProjectDependency> builder)
    {
        //დამოკიდებულებას DbSet არ აქვს (პროექტის შვილია), ამიტომ ცხრილის სახელი აქ იწერება
        builder.ToTable("GitRepoProjectDependencies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(gitRepoProjectDependencyId => gitRepoProjectDependencyId.Value,
            guidValue => new GitRepoProjectDependencyId(guidValue)).IsRequired();

        builder.Property(x => x.ProjectName).IsRequired().HasMaxLength(GitRepoProjectDependency.ProjectNameMaxLength);

        //დამოკიდებულებები პროექტთან ერთად იშლება
        builder.HasOne<GitRepoProject>().WithMany(x => x.Dependencies).HasForeignKey(GitRepoProjectIdColumn)
            .IsRequired().OnDelete(DeleteBehavior.Cascade);

        //პროექტი სხვა პროექტზე ერთხელ არის დამოკიდებული
        builder.HasIndex(GitRepoProjectIdColumn, nameof(GitRepoProjectDependency.ProjectName)).IsUnique();
    }
}
