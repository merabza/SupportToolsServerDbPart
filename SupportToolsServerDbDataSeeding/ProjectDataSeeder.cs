using System;
using DatabaseToolsShared;
using Microsoft.Extensions.Logging;

namespace SupportToolsServerDbDataSeeding;

public /*open*/ class ProjectDataSeeder : DataSeederBase
{
    private readonly bool _checkOnly;
    private readonly StsDataSeedersFabric _dataSeedersFabric;
    protected readonly ILogger Logger;

    protected ProjectDataSeeder(ILogger logger, StsDataSeedersFabric dataSeedersFabric, bool checkOnly) :
        base(checkOnly)
    {
        Logger = logger;
        _dataSeedersFabric = dataSeedersFabric;
        _checkOnly = checkOnly;
    }

    public override bool SeedData()
    {
        return SeedProjectData();
    }

    private bool SeedProjectData()
    {
        Logger.LogInformation("Seed Project Data Started");

        Logger.LogInformation("Seeding ApiKeys");

        //1 ActantGrammarCases
        if (!Use(_dataSeedersFabric.CreateApiKeysSeeder())) 
            return false;

        Console.WriteLine("DataSeederCreator.Run Finished");
        return true;
    }
}