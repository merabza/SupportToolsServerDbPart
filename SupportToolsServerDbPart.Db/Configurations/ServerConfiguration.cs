using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.ApiClients;
using SupportToolsServerCore.Domain.Primitives;
using SupportToolsServerCore.Domain.Runtimes;
using SupportToolsServerCore.Domain.Servers;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class ServerConfiguration : IEntityTypeConfiguration<Server>
{
    public void Configure(EntityTypeBuilder<Server> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Id).HasConversion(serverId => serverId.Value, guidValue => new ServerId(guidValue))
            .IsRequired();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(Server.NameMaxLength);
        builder.Property(x => x.FilesUserName).HasMaxLength(Server.FilesUserNameMaxLength);
        builder.Property(x => x.FilesUsersGroupName).HasMaxLength(Server.FilesUsersGroupNameMaxLength);
        builder.Property(x => x.ServerSideDownloadFolder).HasMaxLength(Server.ServerSideDownloadFolderMaxLength);
        builder.Property(x => x.ServerSideDeployFolder).HasMaxLength(Server.ServerSideDeployFolderMaxLength);

        //optimistic concurrency-ის token-ი (CLAUDE.md, Registry conventions)
        builder.Property(x => x.Version).IsConcurrencyToken().HasDefaultValue(EntityVersion.Initial);

        //null-ს კონვერტერები არ ეხება: ვებაგენტის ან Runtime-ის გარეშე სვეტი NULL-ია
        builder.Property(x => x.WebAgentId)
            .HasConversion(apiClientId => apiClientId!.Value, guidValue => new ApiClientId(guidValue));
        builder.Property(x => x.WebAgentInstallerId)
            .HasConversion(apiClientId => apiClientId!.Value, guidValue => new ApiClientId(guidValue));
        builder.Property(x => x.RuntimeId)
            .HasConversion(runtimeId => runtimeId!.Value, guidValue => new RuntimeId(guidValue));

        //სერვერის მიერ გამოყენებული ApiClient-ისა და Runtime-ის წაშლა ბაზაშიც იკრძალება (README §4.2)
        builder.HasOne<ApiClient>().WithMany().HasForeignKey(x => x.WebAgentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ApiClient>().WithMany().HasForeignKey(x => x.WebAgentInstallerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Runtime>().WithMany().HasForeignKey(x => x.RuntimeId).OnDelete(DeleteBehavior.Restrict);
    }
}
