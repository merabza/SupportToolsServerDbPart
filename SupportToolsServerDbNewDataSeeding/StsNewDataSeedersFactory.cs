using DomainShared.Repositories;
using SupportToolsServerDbDataSeeding;

namespace SupportToolsServerDbNewDataSeeding;

public sealed class StsNewDataSeedersFactory : StsDataSeedersFactory
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public StsNewDataSeedersFactory(string secretDataFolder, IStsDataSeederRepository repo, IUnitOfWork unitOfWork) :
        base(secretDataFolder, repo, unitOfWork)
    {
    }
}