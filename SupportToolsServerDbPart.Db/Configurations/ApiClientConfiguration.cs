using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.ApiClients;
using SupportToolsServerCore.Domain.Primitives;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class ApiClientConfiguration : IEntityTypeConfiguration<ApiClient>
{
    public void Configure(EntityTypeBuilder<ApiClient> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Id).HasConversion(apiClientId => apiClientId.Value,
            guidValue => new ApiClientId(guidValue)).IsRequired();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(ApiClient.NameMaxLength);
        builder.Property(x => x.Server).HasMaxLength(ApiClient.ServerMaxLength);
        builder.Property(x => x.ApiKey).HasMaxLength(ApiClient.ApiKeyMaxLength);

        //optimistic concurrency-ის token-ი (CLAUDE.md, Registry conventions)
        builder.Property(x => x.Version).IsConcurrencyToken().HasDefaultValue(EntityVersion.Initial);
    }
}
