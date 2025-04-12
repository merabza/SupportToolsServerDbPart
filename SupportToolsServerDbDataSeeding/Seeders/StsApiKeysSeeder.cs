using DatabaseToolsShared;
using System.Collections.Generic;

namespace SupportToolsServerDbDataSeeding.Seeders;

public class StsApiKeysSeeder : ITableDataSeeder
{
    protected IStsDataSeederRepository Repo { get; }
    private readonly string _secretDataFolder;

    public StsApiKeysSeeder(string secretDataFolder, IStsDataSeederRepository repo)
    {
        _secretDataFolder = secretDataFolder;
        Repo = repo;
    }

    public bool Create(bool checkOnly)
    {
        throw new System.NotImplementedException();
    }

    private List<ApiKeyModel> GetApiKeyModels()
    {
        return [.. LoadFromJsonFile<UserModel>(_secretDataFolder, "Users.json")];
    }
}