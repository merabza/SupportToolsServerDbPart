using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.GitIgnoreFileTypes;
using SupportToolsServerCore.Domain.Primitives;

namespace SupportToolsServerDbPart.Db.Configurations;

public class GitIgnoreFileTypeConfiguration : IEntityTypeConfiguration<GitIgnoreFileType>
{
    public void Configure(EntityTypeBuilder<GitIgnoreFileType> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(c => c.Name).IsUnique();

        builder.Property(x => x.Id).HasConversion(gitIgnoreFileTypeId => gitIgnoreFileTypeId.Value,
            guidValue => new GitIgnoreFileTypeId(guidValue)).IsRequired();

        builder.Property(e => e.Name).IsRequired().HasMaxLength(GitIgnoreFileType.NameMaxLength);
        builder.Property(e => e.Content).IsRequired().HasMaxLength(GitIgnoreFileType.ContentMaxLength);

        //optimistic concurrency-ის token-ი. DEFAULT სვეტის დამატებისას არსებულ ჩანაწერებს პირველ ვერსიას აძლევს
        builder.Property(e => e.Version).IsConcurrencyToken().HasDefaultValue(EntityVersion.Initial);
    }
}
