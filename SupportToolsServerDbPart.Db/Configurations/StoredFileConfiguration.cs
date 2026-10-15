using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.Primitives;
using SupportToolsServerCore.Domain.StoredFiles;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.HasKey(x => x.Id);

        //გზა ჩანაწერის გასაღებია
        builder.HasIndex(x => x.Path).IsUnique();

        builder.Property(x => x.Id).HasConversion(storedFileId => storedFileId.Value,
            guidValue => new StoredFileId(guidValue)).IsRequired();

        builder.Property(x => x.Path).IsRequired().HasMaxLength(StoredFile.PathMaxLength);

        //nvarchar(max). ზომას ვალიდატორი ზღუდავს (StsStoredFileDataModel.ContentMaxBytes)
        builder.Property(x => x.Content).IsRequired();

        builder.Property(x => x.Sha256).IsRequired().HasMaxLength(StoredFile.Sha256Length);

        //სვეტის ტიპს (datetime) SystemTools-ის DatabaseEntitiesDefaultConvention ადგენს. ბაზა DateTime-ის Kind-ს არ
        //ინახავს, ამიტომ წაკითხული დრო UTC-ად მოინიშნება და JSON-ში Z-ით გადაიცემა
        builder.Property(x => x.UpdatedAtUtc).HasConversion(dateTime => dateTime,
            dateTime => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));

        //optimistic concurrency-ის token-ი (CLAUDE.md, Registry conventions)
        builder.Property(x => x.Version).IsConcurrencyToken().HasDefaultValue(EntityVersion.Initial);
    }
}
