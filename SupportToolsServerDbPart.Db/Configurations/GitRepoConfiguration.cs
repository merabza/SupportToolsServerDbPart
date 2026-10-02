using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportToolsServerCore.Domain.GitIgnoreFileTypes;
using SupportToolsServerCore.Domain.GitRepos;
using SupportToolsServerCore.Domain.Primitives;

namespace SupportToolsServerDbPart.Db.Configurations;

public class GitRepoConfiguration : IEntityTypeConfiguration<GitRepo>
{
    public void Configure(EntityTypeBuilder<GitRepo> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();

        //SupportTools-ის კლიენტი სერვერის რეპოზიტორიას მისამართით ეძებს და ერთზე მეტს ვერ გაუმკლავდება
        builder.HasIndex(x => x.Address).IsUnique();

        builder.Property(x => x.Id).HasConversion(gitRepoId => gitRepoId.Value, guidValue => new GitRepoId(guidValue))
            .IsRequired();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(GitRepo.NameMaxLength);
        builder.Property(x => x.Address).IsRequired().HasMaxLength(GitRepo.AddressMaxLength);
        builder.Property(x => x.FolderName).IsRequired().HasMaxLength(GitRepo.FolderNameMaxLength);

        //optimistic concurrency-ის token-ი. DEFAULT სვეტის დამატებისას არსებულ ჩანაწერებს პირველ ვერსიას აძლევს
        builder.Property(x => x.Version).IsConcurrencyToken().HasDefaultValue(EntityVersion.Initial);

        builder.Property(x => x.GitIgnoreFileTypeId).HasConversion(gitIgnoreFileTypeId => gitIgnoreFileTypeId.Value,
            guidValue => new GitIgnoreFileTypeId(guidValue)).IsRequired();

        //რეპოზიტორიის მიერ გამოყენებული gitignore ფაილის ტიპის წაშლა ბაზაშიც იკრძალება
        builder.HasOne<GitIgnoreFileType>().WithMany().HasForeignKey(x => x.GitIgnoreFileTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
