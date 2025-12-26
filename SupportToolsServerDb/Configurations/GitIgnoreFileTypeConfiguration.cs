using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerDb.Models;

namespace SupportToolsServerDb.Configurations;

public sealed class GitIgnoreFileTypeConfiguration : IEntityTypeConfiguration<GitIgnoreFileType>
{
    public void Configure(EntityTypeBuilder<GitIgnoreFileType> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RowId).IsRequired().ValueGeneratedOnAdd();
        //.HasDefaultValueSql("NEWID()")
        //.HasColumnType("uniqueidentifier")
        //.HasAnnotation("SqlServer:ValueGenerationStrategy", "None")
        //.HasComment("ROWGUIDCOL");
        builder.HasIndex(e => e.RowId);
        builder.HasIndex(e => e.Name).IsUnique();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Content).IsRequired().HasMaxLength(16384);
    }
}