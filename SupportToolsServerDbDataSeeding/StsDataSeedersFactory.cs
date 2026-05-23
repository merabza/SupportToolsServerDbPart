using SupportToolsServerDbDataSeeding.Seeders;
using SystemTools.DatabaseToolsShared;
using SystemTools.SystemToolsShared;

namespace SupportToolsServerDbDataSeeding;

public /*open*/ class StsDataSeedersFactory
{
    protected readonly IDatabaseAbstraction DatabaseAbstraction;
    protected readonly string SecretDataFolder;

    protected StsDataSeedersFactory(string secretDataFolder, IStsDataSeederRepository repo,
        IDatabaseAbstraction databaseAbstraction)
    {
        SecretDataFolder = secretDataFolder;
        Repo = repo;
        DatabaseAbstraction = databaseAbstraction;
    }

    protected IStsDataSeederRepository Repo { get; }

    public ITableDataSeeder CreateApiKeysSeeder()
    {
        return new StsApiKeysSeeder(SecretDataFolder, Repo, DatabaseAbstraction);
    }
}