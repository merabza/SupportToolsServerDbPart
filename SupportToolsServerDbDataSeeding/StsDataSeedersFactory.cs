using DatabaseToolsShared;
using SupportToolsServerDbDataSeeding.Seeders;

namespace SupportToolsServerDbDataSeeding;

public /*open*/ class StsDataSeedersFactory
{
    protected readonly string SecretDataFolder;

    protected StsDataSeedersFactory(string secretDataFolder, IStsDataSeederRepository repo)
    {
        SecretDataFolder = secretDataFolder;
        Repo = repo;
    }

    protected IStsDataSeederRepository Repo { get; }

    public ITableDataSeeder CreateApiKeysSeeder()
    {
        return new StsApiKeysSeeder(SecretDataFolder, Repo);
    }
}