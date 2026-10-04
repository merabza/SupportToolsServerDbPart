using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.SmartSchemas;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class SmartSchemaDetailConfiguration : IEntityTypeConfiguration<SmartSchemaDetail>
{
    //სქემის Id-ის სვეტი. დომენის დეტალმა თავისი სქემა არ იცის, ამიტომ ეს shadow property-ა
    public const string SmartSchemaIdColumn = "SmartSchemaId";

    public void Configure(EntityTypeBuilder<SmartSchemaDetail> builder)
    {
        //დეტალს DbSet არ აქვს (აგრეგატის შვილია), ამიტომ ცხრილის სახელი აქ იწერება
        builder.ToTable("SmartSchemaDetails");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(smartSchemaDetailId => smartSchemaDetailId.Value,
            guidValue => new SmartSchemaDetailId(guidValue)).IsRequired();

        builder.Property(x => x.PeriodType).IsRequired().HasMaxLength(SmartSchemaDetail.PeriodTypeMaxLength);

        //დეტალები სქემასთან ერთად იშლება. კავშირი სავალდებულოა, ამიტომ განახლებისას ჩანაცვლებული დეტალიც იშლება
        builder.HasOne<SmartSchema>().WithMany(x => x.Details).HasForeignKey(SmartSchemaIdColumn).IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        //პერიოდის ტიპი სქემაში ერთხელ გვხვდება
        builder.HasIndex(SmartSchemaIdColumn, nameof(SmartSchemaDetail.PeriodType)).IsUnique();
    }
}
