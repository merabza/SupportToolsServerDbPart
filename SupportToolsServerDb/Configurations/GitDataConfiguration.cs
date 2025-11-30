using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerDb.Models;

namespace SupportToolsServerDb.Configurations;

public sealed class GitDataConfiguration : IEntityTypeConfiguration<GitData>
{
    public void Configure(EntityTypeBuilder<GitData> builder)
    {
        builder.HasKey(e => e.GdId);
        builder.HasIndex(e => e.GdName).IsUnique();
        builder.HasIndex(e => e.GdGitAddress).IsUnique();

        builder.Property(e => e.GdName).IsRequired().HasMaxLength(50);
        builder.Property(e => e.GdGitAddress).IsRequired().HasMaxLength(128);
        builder.Property(e => e.GdFolderName).IsRequired().HasMaxLength(128);
        builder.Property(e => e.GitIgnoreFileTypeId).IsRequired();

        builder.HasOne(h => h.GitIgnoreFileTypeNavigation).WithMany(w => w.GitData)
            .HasForeignKey(f => f.GitIgnoreFileTypeId);
    }
}