using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.EditorConfigFileTypes;
using SupportToolsServerCore.Domain.Primitives;

namespace SupportToolsServerDbPart.Db.Configurations;

public class EditorConfigFileTypeConfiguration : IEntityTypeConfiguration<EditorConfigFileType>
{
    public void Configure(EntityTypeBuilder<EditorConfigFileType> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(c => c.Name).IsUnique();

        builder.Property(x => x.Id).HasConversion(editorConfigFileTypeId => editorConfigFileTypeId.Value,
            guidValue => new EditorConfigFileTypeId(guidValue)).IsRequired();

        builder.Property(e => e.Name).IsRequired().HasMaxLength(EditorConfigFileType.NameMaxLength);
        builder.Property(e => e.Content).IsRequired().HasMaxLength(EditorConfigFileType.ContentMaxLength);

        //optimistic concurrency-ის token-ი. DEFAULT სვეტის დამატებისას არსებულ ჩანაწერებს პირველ ვერსიას აძლევს
        builder.Property(e => e.Version).IsConcurrencyToken().HasDefaultValue(EntityVersion.Initial);
    }
}
