using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.ApiClients;
using SupportToolsServerCore.Domain.FileStorages;
using SupportToolsServerCore.Domain.Primitives;
using SupportToolsServerCore.Domain.Settings;
using SupportToolsServerCore.Domain.SmartSchemas;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class GlobalSettingsConfiguration : IEntityTypeConfiguration<GlobalSettings>
{
    public void Configure(EntityTypeBuilder<GlobalSettings> builder)
    {
        //ჩანაწერი ერთადერთია (singleton): გასაღები ფიქსირებულია და სხვა გასაღებით ჩაწერას ბაზაც კრძალავს
        builder.ToTable(table =>
            table.HasCheckConstraint("CK_GlobalSettings_Singleton", $"[Id] = '{GlobalSettingsId.Singleton.Value}'"));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(globalSettingsId => globalSettingsId.Value,
            guidValue => new GlobalSettingsId(guidValue)).IsRequired();

        builder.Property(x => x.ServiceDescriptionSignature)
            .HasMaxLength(GlobalSettings.ServiceDescriptionSignatureMaxLength);
        builder.Property(x => x.UploadTempExtension).HasMaxLength(GlobalSettings.UploadTempExtensionMaxLength);
        builder.Property(x => x.ProgramArchiveDateMask).HasMaxLength(GlobalSettings.ProgramArchiveDateMaskMaxLength);
        builder.Property(x => x.ProgramArchiveExtension).HasMaxLength(GlobalSettings.ProgramArchiveExtensionMaxLength);
        builder.Property(x => x.ParametersFileDateMask).HasMaxLength(GlobalSettings.ParametersFileDateMaskMaxLength);
        builder.Property(x => x.ParametersFileExtension).HasMaxLength(GlobalSettings.ParametersFileExtensionMaxLength);
        builder.Property(x => x.MediatRLicenseKey).HasMaxLength(GlobalSettings.MediatRLicenseKeyMaxLength);

        //optimistic concurrency-ის token-ი (CLAUDE.md, Registry conventions)
        builder.Property(x => x.Version).IsConcurrencyToken().HasDefaultValue(EntityVersion.Initial);

        //null-ს კონვერტერები არ ეხება: მითითების გარეშე სვეტი NULL-ია
        builder.Property(x => x.FileStorageForExchangeId)
            .HasConversion(fileStorageId => fileStorageId!.Value, guidValue => new FileStorageId(guidValue));
        builder.Property(x => x.SmartSchemaForExchangeId)
            .HasConversion(smartSchemaId => smartSchemaId!.Value, guidValue => new SmartSchemaId(guidValue));
        builder.Property(x => x.SmartSchemaForLocalId)
            .HasConversion(smartSchemaId => smartSchemaId!.Value, guidValue => new SmartSchemaId(guidValue));
        builder.Property(x => x.LocalPackageManagerWebApiClientId)
            .HasConversion(apiClientId => apiClientId!.Value, guidValue => new ApiClientId(guidValue));

        //გამოყენებული ფაილსაცავის, ჭკვიანი სქემისა და ApiClient-ის წაშლა ბაზაშიც იკრძალება (README §4.2)
        builder.HasOne<FileStorage>().WithMany().HasForeignKey(x => x.FileStorageForExchangeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SmartSchema>().WithMany().HasForeignKey(x => x.SmartSchemaForExchangeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SmartSchema>().WithMany().HasForeignKey(x => x.SmartSchemaForLocalId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApiClient>().WithMany().HasForeignKey(x => x.LocalPackageManagerWebApiClientId)
            .OnDelete(DeleteBehavior.Restrict);

        //ბაზების backup ფაილების გაცვლის პარამეტრები იმავე სტრიქონშია (owned type), სვეტები
        //DatabasesBackupFilesExchange_ პრეფიქსით
        builder.OwnsOne(x => x.DatabasesBackupFilesExchange, exchange =>
        {
            exchange.Property(x => x.DownloadTempExtension)
                .HasMaxLength(DatabasesBackupFilesExchange.DownloadTempExtensionMaxLength);
            exchange.Property(x => x.UploadTempExtension)
                .HasMaxLength(DatabasesBackupFilesExchange.UploadTempExtensionMaxLength);

            exchange.Property(x => x.ExchangeFileStorageId)
                .HasConversion(fileStorageId => fileStorageId!.Value, guidValue => new FileStorageId(guidValue));
            exchange.Property(x => x.ExchangeSmartSchemaId)
                .HasConversion(smartSchemaId => smartSchemaId!.Value, guidValue => new SmartSchemaId(guidValue));
            exchange.Property(x => x.LocalSmartSchemaId)
                .HasConversion(smartSchemaId => smartSchemaId!.Value, guidValue => new SmartSchemaId(guidValue));

            exchange.HasOne<FileStorage>().WithMany().HasForeignKey(x => x.ExchangeFileStorageId)
                .OnDelete(DeleteBehavior.Restrict);
            exchange.HasOne<SmartSchema>().WithMany().HasForeignKey(x => x.ExchangeSmartSchemaId)
                .OnDelete(DeleteBehavior.Restrict);
            exchange.HasOne<SmartSchema>().WithMany().HasForeignKey(x => x.LocalSmartSchemaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        //ნაწილის ყველა სვეტი შეიძლება NULL იყოს, ამიტომ EF ობიექტს მაშინაც ქმნის, როცა ყველა სვეტი NULL-ია
        builder.Navigation(x => x.DatabasesBackupFilesExchange).IsRequired();
    }
}
