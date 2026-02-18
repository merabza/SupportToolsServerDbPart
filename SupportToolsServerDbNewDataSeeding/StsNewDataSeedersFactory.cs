using SupportToolsServerDbDataSeeding;
using SystemTools.DomainShared.Repositories;

namespace SupportToolsServerDbNewDataSeeding;

public sealed class StsNewDataSeedersFactory : StsDataSeedersFactory
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public StsNewDataSeedersFactory(string secretDataFolder, IStsDataSeederRepository repo, IUnitOfWork unitOfWork) :
        base(secretDataFolder, repo, unitOfWork)
    {
    }
}