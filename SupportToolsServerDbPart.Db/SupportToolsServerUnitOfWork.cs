using SystemTools.RepositoriesShared;

namespace SupportToolsServerDbPart.Db;

public class SupportToolsServerUnitOfWork : UnitOfWork
{
    public SupportToolsServerUnitOfWork(SupportToolsServerDbContext dbContext) : base(dbContext)
    {
    }
}
