using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.ApiClients;
using SupportToolsServerCore.Domain.DatabaseServerConnections;
using SupportToolsServerCore.Domain.Primitives;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class DatabaseServerConnectionConfiguration : IEntityTypeConfiguration<DatabaseServerConnection>
{
    public void Configure(EntityTypeBuilder<DatabaseServerConnection> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Id).HasConversion(databaseServerConnectionId => databaseServerConnectionId.Value,
            guidValue => new DatabaseServerConnectionId(guidValue)).IsRequired();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(DatabaseServerConnection.NameMaxLength);
        builder.Property(x => x.DatabaseServerProvider).IsRequired()
            .HasMaxLength(DatabaseServerConnection.DatabaseServerProviderMaxLength);
        builder.Property(x => x.RemoteDbConnectionName)
            .HasMaxLength(DatabaseServerConnection.RemoteDbConnectionNameMaxLength);
        builder.Property(x => x.ServerAddress).HasMaxLength(DatabaseServerConnection.ServerAddressMaxLength);
        builder.Property(x => x.ServerUser).HasMaxLength(DatabaseServerConnection.ServerUserMaxLength);
        builder.Property(x => x.ServerPass).HasMaxLength(DatabaseServerConnection.ServerPassMaxLength);

        //optimistic concurrency-ის token-ი (CLAUDE.md, Registry conventions)
        builder.Property(x => x.Version).IsConcurrencyToken().HasDefaultValue(EntityVersion.Initial);

        //null-ს კონვერტერი არ ეხება: ვებაგენტის გარეშე სვეტი NULL-ია
        builder.Property(x => x.DbWebAgentId)
            .HasConversion(apiClientId => apiClientId!.Value, guidValue => new ApiClientId(guidValue));

        //ვებაგენტად გამოყენებული ApiClient-ის წაშლა ბაზაშიც იკრძალება (README §4.2)
        builder.HasOne<ApiClient>().WithMany().HasForeignKey(x => x.DbWebAgentId).OnDelete(DeleteBehavior.Restrict);
    }
}
