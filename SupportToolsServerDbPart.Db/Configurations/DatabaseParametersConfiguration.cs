using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.DatabaseServerConnections;
using SupportToolsServerCore.Domain.FileStorages;
using SupportToolsServerCore.Domain.Projects;
using SupportToolsServerCore.Domain.SmartSchemas;

namespace SupportToolsServerDbPart.Db.Configurations;

//ბაზის პარამეტრების owned type, მფლობელის ცხრილში (table splitting): სვეტები <ნავიგაცია>_<ველი> სახელებითაა. პროექტი
//მას DevDatabaseParameters-ისა და ProdCopyDatabaseParameters-ისთვის იყენებს, B7 კი ServerInfo-ს
//CurrentDatabaseParameters-ისა და NewDatabaseParameters-ისთვის. ნავიგაცია არასავალდებულოა: სავალდებულო CommandTimeOut და
//SkipBackupBeforeRestore ველების სვეტები NULL-ია მხოლოდ მაშინ, როცა ნაწილი არ არის, ამიტომ EF null-სა და ცარიელ ნაწილს
//ერთმანეთისგან არჩევს
public static class DatabaseParametersConfiguration
{
    public static void Configure<TOwner>(OwnedNavigationBuilder<TOwner, DatabaseParameters> parameters)
        where TOwner : class
    {
        parameters.Property(x => x.DatabaseRecoveryModel)
            .HasMaxLength(DatabaseParameters.DatabaseRecoveryModelMaxLength);
        parameters.Property(x => x.DbServerFoldersSetName)
            .HasMaxLength(DatabaseParameters.DbServerFoldersSetNameMaxLength);
        parameters.Property(x => x.DatabaseName).HasMaxLength(DatabaseParameters.DatabaseNameMaxLength);
        parameters.Property(x => x.BackupNamePrefix).HasMaxLength(DatabaseParameters.BackupNamePrefixMaxLength);
        parameters.Property(x => x.DateMask).HasMaxLength(DatabaseParameters.DateMaskMaxLength);
        parameters.Property(x => x.BackupFileExtension).HasMaxLength(DatabaseParameters.BackupFileExtensionMaxLength);
        parameters.Property(x => x.BackupNameMiddlePart)
            .HasMaxLength(DatabaseParameters.BackupNameMiddlePartMaxLength);
        parameters.Property(x => x.BackupType).HasMaxLength(DatabaseParameters.BackupTypeMaxLength);

        //null-ს კონვერტერები არ ეხება: მითითების გარეშე სვეტი NULL-ია
        parameters.Property(x => x.DbConnectionId).HasConversion(connectionId => connectionId!.Value,
            guidValue => new DatabaseServerConnectionId(guidValue));
        parameters.Property(x => x.SmartSchemaId)
            .HasConversion(smartSchemaId => smartSchemaId!.Value, guidValue => new SmartSchemaId(guidValue));
        parameters.Property(x => x.FileStorageId)
            .HasConversion(fileStorageId => fileStorageId!.Value, guidValue => new FileStorageId(guidValue));

        //გამოყენებული ბაზის კავშირის, ჭკვიანი სქემისა და ფაილსაცავის წაშლა ბაზაშიც იკრძალება (README §4.2)
        parameters.HasOne<DatabaseServerConnection>().WithMany().HasForeignKey(x => x.DbConnectionId)
            .OnDelete(DeleteBehavior.Restrict);
        parameters.HasOne<SmartSchema>().WithMany().HasForeignKey(x => x.SmartSchemaId)
            .OnDelete(DeleteBehavior.Restrict);
        parameters.HasOne<FileStorage>().WithMany().HasForeignKey(x => x.FileStorageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
