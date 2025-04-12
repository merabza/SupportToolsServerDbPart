using DatabaseToolsShared;
using SupportToolsServerDbDataSeeding.Seeders;

namespace SupportToolsServerDbDataSeeding;

public /*open*/ class StsDataSeedersFabric
{
    protected IStsDataSeederRepository Repo { get; }
    protected readonly string SecretDataFolder;


    protected StsDataSeedersFabric(string secretDataFolder, IStsDataSeederRepository repo)
    {
        SecretDataFolder = secretDataFolder;
        Repo = repo;
    }

    public ITableDataSeeder CreateApiKeysSeeder()
    {
        return new StsApiKeysSeeder(SecretDataFolder, Repo);
    }
}