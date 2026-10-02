using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.DeploymentEnvironments;
using SupportToolsServerCore.Domain.Primitives;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class DeploymentEnvironmentConfiguration : IEntityTypeConfiguration<DeploymentEnvironment>
{
    public void Configure(EntityTypeBuilder<DeploymentEnvironment> builder)
    {
        //კლასი System.Environment-ს რომ არ დაემთხვეს, DeploymentEnvironment ჰქვია, ცხრილი კი Environments-ია
        builder.ToTable("Environments");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Id).HasConversion(deploymentEnvironmentId => deploymentEnvironmentId.Value,
            guidValue => new DeploymentEnvironmentId(guidValue)).IsRequired();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(DeploymentEnvironment.NameMaxLength);
        builder.Property(x => x.Description).HasMaxLength(DeploymentEnvironment.DescriptionMaxLength);

        //optimistic concurrency-ის token-ი (CLAUDE.md, Registry conventions)
        builder.Property(x => x.Version).IsConcurrencyToken().HasDefaultValue(EntityVersion.Initial);
    }
}
