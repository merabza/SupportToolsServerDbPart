using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.Projects;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class ServerInfoAllowedToolConfiguration : IEntityTypeConfiguration<ServerInfoAllowedTool>
{
    public void Configure(EntityTypeBuilder<ServerInfoAllowedTool> builder)
    {
        //შვილს DbSet არ აქვს, ამიტომ ცხრილის სახელი აქ იწერება
        builder.ToTable("ServerInfoAllowedTools");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(serverInfoAllowedToolId => serverInfoAllowedToolId.Value,
            guidValue => new ServerInfoAllowedToolId(guidValue)).IsRequired();

        builder.Property(x => x.ToolName).IsRequired().HasMaxLength(ServerInfoAllowedTool.ToolNameMaxLength);

        //ინსტრუმენტები ServerInfo-სთან ერთად იშლება. კავშირი სავალდებულოა, ამიტომ ჩანაცვლებული ServerInfo-ს
        //ინსტრუმენტებიც იშლება
        builder.HasOne<ServerInfo>().WithMany(x => x.AllowedTools)
            .HasForeignKey(ServerInfoConfiguration.ServerInfoIdColumn).IsRequired().OnDelete(DeleteBehavior.Cascade);

        //ინსტრუმენტი ServerInfo-ში ერთხელ გვხვდება
        builder.HasIndex(ServerInfoConfiguration.ServerInfoIdColumn, nameof(ServerInfoAllowedTool.ToolName))
            .IsUnique();
    }
}
