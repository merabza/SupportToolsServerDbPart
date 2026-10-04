using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.DatabaseServerConnections;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class DatabaseFoldersSetConfiguration : IEntityTypeConfiguration<DatabaseFoldersSet>
{
    //კავშირის Id-ის სვეტი. დომენის ნაკრებმა თავისი კავშირი არ იცის, ამიტომ ეს shadow property-ა
    public const string DatabaseServerConnectionIdColumn = "DatabaseServerConnectionId";

    public void Configure(EntityTypeBuilder<DatabaseFoldersSet> builder)
    {
        //ნაკრებს DbSet არ აქვს (აგრეგატის შვილია), ამიტომ ცხრილის სახელი აქ იწერება
        builder.ToTable("DatabaseFoldersSets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(databaseFoldersSetId => databaseFoldersSetId.Value,
            guidValue => new DatabaseFoldersSetId(guidValue)).IsRequired();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(DatabaseFoldersSet.NameMaxLength);
        builder.Property(x => x.Backup).HasMaxLength(DatabaseFoldersSet.FolderMaxLength);
        builder.Property(x => x.Data).HasMaxLength(DatabaseFoldersSet.FolderMaxLength);
        builder.Property(x => x.DataLog).HasMaxLength(DatabaseFoldersSet.FolderMaxLength);

        //ნაკრებები კავშირთან ერთად იშლება. კავშირი სავალდებულოა, ამიტომ განახლებისას ჩანაცვლებული ნაკრებიც იშლება
        builder.HasOne<DatabaseServerConnection>().WithMany(x => x.DatabaseFoldersSets)
            .HasForeignKey(DatabaseServerConnectionIdColumn).IsRequired().OnDelete(DeleteBehavior.Cascade);

        //ნაკრების სახელი კავშირის შიგნით უნიკალურია
        builder.HasIndex(DatabaseServerConnectionIdColumn, nameof(DatabaseFoldersSet.Name)).IsUnique();
    }
}
