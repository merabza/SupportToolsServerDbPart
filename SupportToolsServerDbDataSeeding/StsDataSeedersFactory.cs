using DatabaseToolsShared;
using SupportToolsServerDbDataSeeding.Seeders;

namespace SupportToolsServerDbDataSeeding;

public /*open*/ class StsDataSeedersFactory
{
    protected IStsDataSeederRepository Repo { get; }
    protected readonly string SecretDataFolder;


    protected StsDataSeedersFactory(string secretDataFolder, IStsDataSeederRepository repo)
    {
        SecretDataFolder = secretDataFolder;
        Repo = repo;
    }

    public ITableDataSeeder CreateApiKeysSeeder()
    {
        return new StsApiKeysSeeder(SecretDataFolder, Repo);
    }
}