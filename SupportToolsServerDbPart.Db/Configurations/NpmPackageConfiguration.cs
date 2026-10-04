using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.NpmPackages;
using SupportToolsServerCore.Domain.Primitives;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class NpmPackageConfiguration : IEntityTypeConfiguration<NpmPackage>
{
    public void Configure(EntityTypeBuilder<NpmPackage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Id).HasConversion(npmPackageId => npmPackageId.Value,
            guidValue => new NpmPackageId(guidValue)).IsRequired();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(NpmPackage.NameMaxLength);
        builder.Property(x => x.Description).HasMaxLength(NpmPackage.DescriptionMaxLength);

        //optimistic concurrency-ის token-ი (CLAUDE.md, Registry conventions)
        builder.Property(x => x.Version).IsConcurrencyToken().HasDefaultValue(EntityVersion.Initial);
    }
}
