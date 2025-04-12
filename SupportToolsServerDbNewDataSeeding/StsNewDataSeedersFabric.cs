using SupportToolsServerDbDataSeeding;

namespace SupportToolsServerDbNewDataSeeding;

public sealed class StsNewDataSeedersFabric : StsDataSeedersFabric
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public StsNewDataSeedersFabric(string secretDataFolder, IStsDataSeederRepository repo) : base(secretDataFolder,
        repo)
    {
    }
}