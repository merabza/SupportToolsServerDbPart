using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.Primitives;
using SupportToolsServerCore.Domain.SmartSchemas;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class SmartSchemaConfiguration : IEntityTypeConfiguration<SmartSchema>
{
    public void Configure(EntityTypeBuilder<SmartSchema> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Id).HasConversion(smartSchemaId => smartSchemaId.Value,
            guidValue => new SmartSchemaId(guidValue)).IsRequired();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(SmartSchema.NameMaxLength);

        //optimistic concurrency-ის token-ი (CLAUDE.md, Registry conventions)
        builder.Property(x => x.Version).IsConcurrencyToken().HasDefaultValue(EntityVersion.Initial);
    }
}
