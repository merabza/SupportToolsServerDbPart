using DatabaseToolsShared;
using SupportToolsServerDb.Models;

namespace SupportToolsServerDbDataSeeding;

public interface IStsDataSeederRepository : IDataSeederRepository
{
    bool CreateApiKey(ApiKeyByRemoteIpAddress apiKey);
}