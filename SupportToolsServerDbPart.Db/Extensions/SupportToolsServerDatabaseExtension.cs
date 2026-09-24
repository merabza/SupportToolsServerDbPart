using System;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SystemTools.Domain.Abstractions;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

namespace SupportToolsServerDbPart.Db.Extensions;

// ReSharper disable once UnusedType.Global
public static class SupportToolsServerDatabaseExtension
{
    public static IServiceCollection AddGanmartebaGeDatabase(this IServiceCollection services, ILogger? debugLogger,
        IConfiguration? configuration, string? conStr = null)
    {
        const string connectionStringConfigurationKey = "Data:GanmartebaGeDatabase:ConnectionString";

        debugLogger?.Information("{MethodName} Started", nameof(AddGanmartebaGeDatabase));

        string? connectionString = conStr ?? configuration?[connectionStringConfigurationKey];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine($"parameter {connectionStringConfigurationKey} is empty");
            return services;
        }

        try
        {
            _ = new SqlConnectionStringBuilder(connectionString);
        }
        catch (ArgumentException e)
        {
            throw new InvalidOperationException(
                $"parameter {connectionStringConfigurationKey} is not a valid connection string: {e.Message}", e);
        }

        services.AddDbContext<SupportToolsServerDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IDomainEventsDispatcher, DomainEventsDispatcher>();
        services.AddScoped<IGanmartebaGeApplicationDbContext>(sp => sp.GetRequiredService<SupportToolsServerDbContext>());

        services.AddScoped<IUnitOfWork, GanmartebaGeUnitOfWork>();
        services.AddScoped<IDatabaseAbstraction, GanmartebaGeDatabaseAbstractionRepository>();

        debugLogger?.Information("{MethodName} Finished", nameof(AddGanmartebaGeDatabase));

        return services;
    }
}
