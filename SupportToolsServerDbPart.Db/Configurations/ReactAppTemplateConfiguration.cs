using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.Primitives;
using SupportToolsServerCore.Domain.ReactAppTemplates;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class ReactAppTemplateConfiguration : IEntityTypeConfiguration<ReactAppTemplate>
{
    public void Configure(EntityTypeBuilder<ReactAppTemplate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Id).HasConversion(reactAppTemplateId => reactAppTemplateId.Value,
            guidValue => new ReactAppTemplateId(guidValue)).IsRequired();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(ReactAppTemplate.NameMaxLength);
        builder.Property(x => x.Template).IsRequired().HasMaxLength(ReactAppTemplate.TemplateMaxLength);

        //optimistic concurrency-ის token-ი (CLAUDE.md, Registry conventions)
        builder.Property(x => x.Version).IsConcurrencyToken().HasDefaultValue(EntityVersion.Initial);
    }
}
