using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.Primitives;
using SupportToolsServerCore.Domain.Runtimes;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class RuntimeConfiguration : IEntityTypeConfiguration<Runtime>
{
    public void Configure(EntityTypeBuilder<Runtime> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Id).HasConversion(runtimeId => runtimeId.Value, guidValue => new RuntimeId(guidValue))
            .IsRequired();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(Runtime.NameMaxLength);
        builder.Property(x => x.Description).HasMaxLength(Runtime.DescriptionMaxLength);

        //optimistic concurrency-ის token-ი (CLAUDE.md, Registry conventions)
        builder.Property(x => x.Version).IsConcurrencyToken().HasDefaultValue(EntityVersion.Initial);
    }
}
