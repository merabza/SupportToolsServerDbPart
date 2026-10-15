using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.ApiClients;
using SupportToolsServerCore.Domain.DeploymentEnvironments;
using SupportToolsServerCore.Domain.Projects;
using SupportToolsServerCore.Domain.Servers;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class ServerInfoConfiguration : IEntityTypeConfiguration<ServerInfo>
{
    //ServerInfo-ს Id-ის სვეტი ინსტრუმენტების ცხრილში. დომენის ინსტრუმენტმა თავისი ServerInfo არ იცის, ამიტომ ეს shadow
    //property-ა
    public const string ServerInfoIdColumn = "ServerInfoId";

    public void Configure(EntityTypeBuilder<ServerInfo> builder)
    {
        //ServerInfo-ს DbSet არ აქვს (პროექტის აგრეგატის შვილია), ამიტომ ცხრილის სახელი აქ იწერება
        builder.ToTable("ServerInfos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(serverInfoId => serverInfoId.Value,
            guidValue => new ServerInfoId(guidValue)).IsRequired();

        builder.Property(x => x.ServerId)
            .HasConversion(serverId => serverId.Value, guidValue => new ServerId(guidValue)).IsRequired();
        builder.Property(x => x.EnvironmentId).HasConversion(environmentId => environmentId.Value,
            guidValue => new DeploymentEnvironmentId(guidValue)).IsRequired();

        //null-ს კონვერტერი არ ეხება: ვებაგენტის გარეშე სვეტი NULL-ია
        builder.Property(x => x.WebAgentForCheckId)
            .HasConversion(apiClientId => apiClientId!.Value, guidValue => new ApiClientId(guidValue));

        builder.Property(x => x.ApiVersionId).HasMaxLength(ServerInfo.ApiVersionIdMaxLength);
        builder.Property(x => x.AppSettingsJsonSourceFileName).HasMaxLength(ServerInfo.PathMaxLength);
        builder.Property(x => x.AppSettingsEncodedJsonFileName).HasMaxLength(ServerInfo.PathMaxLength);
        builder.Property(x => x.ServiceUserName).HasMaxLength(ServerInfo.ServiceUserNameMaxLength);

        //ServerInfo-ები პროექტთან ერთად იშლება. კავშირი სავალდებულოა, ამიტომ განახლებისას ჩანაცვლებული ServerInfo-ც
        //იშლება, თავის ინსტრუმენტებთან ერთად
        builder.HasOne<Project>().WithMany(x => x.ServerInfos).HasForeignKey(ProjectConfiguration.ProjectIdColumn)
            .IsRequired().OnDelete(DeleteBehavior.Cascade);

        //ServerInfo-ს მიერ გამოყენებული სერვერის, გარემოსა და ვებაგენტის წაშლა ბაზაშიც იკრძალება (README §4.2)
        builder.HasOne<Server>().WithMany().HasForeignKey(x => x.ServerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<DeploymentEnvironment>().WithMany().HasForeignKey(x => x.EnvironmentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApiClient>().WithMany().HasForeignKey(x => x.WebAgentForCheckId)
            .OnDelete(DeleteBehavior.Restrict);

        //სერვერისა და გარემოს წყვილი პროექტში ერთხელ გვხვდება (ServerInfo-ს ნატურალური გასაღები)
        builder.HasIndex(ProjectConfiguration.ProjectIdColumn, nameof(ServerInfo.ServerId),
            nameof(ServerInfo.EnvironmentId)).IsUnique();

        //ბაზის პარამეტრები იმავე სტრიქონშია (owned type), სვეტები CurrentDatabaseParameters_ და NewDatabaseParameters_
        //პრეფიქსებით
        builder.OwnsOne(x => x.CurrentDatabaseParameters, DatabaseParametersConfiguration.Configure);
        builder.OwnsOne(x => x.NewDatabaseParameters, DatabaseParametersConfiguration.Configure);
    }
}
