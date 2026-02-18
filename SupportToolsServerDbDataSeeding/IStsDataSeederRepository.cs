using SupportToolsServerDb.Models;
using SystemTools.DatabaseToolsShared;

namespace SupportToolsServerDbDataSeeding;

public interface IStsDataSeederRepository : IDataSeederRepository
{
    bool CreateApiKey(ApiKeyByRemoteIpAddress apiKey);
}