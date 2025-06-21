using System;
using DatabaseToolsShared;
using Microsoft.Extensions.Logging;

namespace SupportToolsServerDbDataSeeding;

public /*open*/ class ProjectDataSeeder : DataSeederBase
{
    private readonly bool _checkOnly;
    private readonly StsDataSeedersFactory _dataSeedersFactory;
    protected readonly ILogger Logger;

    protected ProjectDataSeeder(ILogger logger, StsDataSeedersFactory dataSeedersFactory, bool checkOnly) :
        base(checkOnly)
    {
        Logger = logger;
        _dataSeedersFactory = dataSeedersFactory;
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
        if (!Use(_dataSeedersFactory.CreateApiKeysSeeder())) 
            return false;

        Console.WriteLine("DataSeederCreator.Run Finished");
        return true;
    }
}