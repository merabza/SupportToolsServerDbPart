using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.Projects;

namespace SupportToolsServerDbPart.Db.Configurations;

public sealed class ProjectEndpointConfiguration : IEntityTypeConfiguration<ProjectEndpoint>
{
    public void Configure(EntityTypeBuilder<ProjectEndpoint> builder)
    {
        //შვილს DbSet არ აქვს, ამიტომ ცხრილის სახელი აქ იწერება
        builder.ToTable("ProjectEndpoints");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(projectEndpointId => projectEndpointId.Value,
            guidValue => new ProjectEndpointId(guidValue)).IsRequired();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(ProjectEndpoint.NameMaxLength);
        builder.Property(x => x.EndpointName).HasMaxLength(ProjectEndpoint.EndpointNameMaxLength);
        builder.Property(x => x.EndpointRoute).HasMaxLength(ProjectEndpoint.EndpointRouteMaxLength);
        builder.Property(x => x.HttpMethod).IsRequired().HasMaxLength(ProjectEndpoint.HttpMethodMaxLength);
        builder.Property(x => x.EndpointType).IsRequired().HasMaxLength(ProjectEndpoint.EndpointTypeMaxLength);
        builder.Property(x => x.ReturnType).HasMaxLength(ProjectEndpoint.ReturnTypeMaxLength);

        //შვილები პროექტთან ერთად იშლება. კავშირი სავალდებულოა, ამიტომ განახლებისას ჩანაცვლებული შვილიც იშლება
        builder.HasOne<Project>().WithMany(x => x.Endpoints).HasForeignKey(ProjectConfiguration.ProjectIdColumn)
            .IsRequired().OnDelete(DeleteBehavior.Cascade);

        //endpoint-ის key (კლიენტის dictionary-ის key) პროექტში ერთხელ გვხვდება
        builder.HasIndex(ProjectConfiguration.ProjectIdColumn, nameof(ProjectEndpoint.Name)).IsUnique();
    }
}
