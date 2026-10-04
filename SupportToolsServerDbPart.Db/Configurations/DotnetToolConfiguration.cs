using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.DotnetTools;
using SupportToolsServerCore.Domain.Primitives;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class DotnetToolConfiguration : IEntityTypeConfiguration<DotnetTool>
{
    public void Configure(EntityTypeBuilder<DotnetTool> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Id).HasConversion(dotnetToolId => dotnetToolId.Value,
            guidValue => new DotnetToolId(guidValue)).IsRequired();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(DotnetTool.NameMaxLength);
        builder.Property(x => x.PackageId).IsRequired().HasMaxLength(DotnetTool.PackageIdMaxLength);
        builder.Property(x => x.MaxVersion).HasMaxLength(DotnetTool.MaxVersionMaxLength);
        builder.Property(x => x.Description).HasMaxLength(DotnetTool.DescriptionMaxLength);

        //optimistic concurrency-ის token-ი (CLAUDE.md, Registry conventions)
        builder.Property(x => x.Version).IsConcurrencyToken().HasDefaultValue(EntityVersion.Initial);
    }
}
