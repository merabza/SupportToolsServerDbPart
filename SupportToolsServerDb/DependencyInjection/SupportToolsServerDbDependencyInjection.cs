using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportToolsServer.Persistence;

namespace SupportToolsServerDb.DependencyInjection;

// ReSharper disable once UnusedType.Global
public static class SupportToolsServerDbDependencyInjection
{
    public static IServiceCollection AddSupportToolsServerDb(this IServiceCollection services,
        IConfiguration configuration, bool debugMode)
    {
        if (debugMode) Console.WriteLine($"{nameof(AddSupportToolsServerDb)} Started");

        const string connectionStringConfigKey = "Data:SupportToolsServerDatabase:ConnectionString";
        var connectionString = configuration[connectionStringConfigKey];

        if (string.IsNullOrWhiteSpace(connectionString) && !debugMode)
        {
            Console.WriteLine($"parameter {connectionStringConfigKey} is empty");
            return services;
        }

        services.AddDbContext<SupportToolsServerDbContext>(options => options.UseSqlServer(connectionString));
        if (debugMode) Console.WriteLine($"{nameof(AddSupportToolsServerDb)} Finished");

        return services;
    }
}