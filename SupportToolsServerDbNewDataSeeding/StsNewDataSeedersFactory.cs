using SupportToolsServerDbDataSeeding;
using SystemTools.SystemToolsShared;

namespace SupportToolsServerDbNewDataSeeding;

public sealed class StsNewDataSeedersFactory : StsDataSeedersFactory
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public StsNewDataSeedersFactory(string secretDataFolder, IStsDataSeederRepository repo,
        IDatabaseAbstraction databaseAbstraction) : base(secretDataFolder, repo, databaseAbstraction)
    {
    }
}