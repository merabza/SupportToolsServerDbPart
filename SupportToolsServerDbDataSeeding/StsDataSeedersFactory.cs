using SupportToolsServerDbDataSeeding.Seeders;
using SystemTools.DatabaseToolsShared;
using SystemTools.DomainShared.Repositories;

namespace SupportToolsServerDbDataSeeding;

public /*open*/ class StsDataSeedersFactory
{
    protected readonly string SecretDataFolder;

    protected StsDataSeedersFactory(string secretDataFolder, IStsDataSeederRepository repo, IUnitOfWork unitOfWork)
    {
        SecretDataFolder = secretDataFolder;
        Repo = repo;
        UnitOfWork = unitOfWork;
    }

    protected IStsDataSeederRepository Repo { get; }
    protected IUnitOfWork UnitOfWork { get; }

    public ITableDataSeeder CreateApiKeysSeeder()
    {
        return new StsApiKeysSeeder(SecretDataFolder, Repo, UnitOfWork);
    }
}