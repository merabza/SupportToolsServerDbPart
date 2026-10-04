using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.FileStorages;
using SupportToolsServerCore.Domain.Primitives;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class FileStorageConfiguration : IEntityTypeConfiguration<FileStorage>
{
    public void Configure(EntityTypeBuilder<FileStorage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Id).HasConversion(fileStorageId => fileStorageId.Value,
            guidValue => new FileStorageId(guidValue)).IsRequired();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(FileStorage.NameMaxLength);
        builder.Property(x => x.FileStoragePath).HasMaxLength(FileStorage.FileStoragePathMaxLength);
        builder.Property(x => x.UserName).HasMaxLength(FileStorage.UserNameMaxLength);
        builder.Property(x => x.Password).HasMaxLength(FileStorage.PasswordMaxLength);

        //optimistic concurrency-ის token-ი (CLAUDE.md, Registry conventions)
        builder.Property(x => x.Version).IsConcurrencyToken().HasDefaultValue(EntityVersion.Initial);
    }
}
