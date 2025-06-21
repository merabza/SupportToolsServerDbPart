using SupportToolsServerDbDataSeeding;

namespace SupportToolsServerDbNewDataSeeding;

public sealed class StsNewDataSeedersFactory : StsDataSeedersFactory
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public StsNewDataSeedersFactory(string secretDataFolder, IStsDataSeederRepository repo) : base(secretDataFolder,
        repo)
    {
    }
}