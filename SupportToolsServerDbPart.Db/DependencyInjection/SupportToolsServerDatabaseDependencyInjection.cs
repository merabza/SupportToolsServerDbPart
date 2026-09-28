using System;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SupportToolsServerCore.Application.Abstractions;
using SystemTools.Domain.Abstractions;

namespace SupportToolsServerDbPart.Db.DependencyInjection;

// ReSharper disable once UnusedType.Global
public static class SupportToolsServerDatabaseDependencyInjection
{
    public static IServiceCollection AddSupportToolsServerDatabase(this IServiceCollection services,
        ILogger? debugLogger, IConfiguration configuration)
    {
        const string connectionStringConfigurationKey = "Data:SupportToolsServerDatabase:ConnectionString";

        debugLogger?.Information("{MethodName} Started", nameof(AddSupportToolsServerDatabase));

        string? connectionString = configuration[connectionStringConfigurationKey];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"parameter {connectionStringConfigurationKey} is empty");
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

        services.AddScoped<ISupportToolsServerDbContext>(sp => sp.GetRequiredService<SupportToolsServerDbContext>());
        services.AddScoped<IUnitOfWork, SupportToolsServerUnitOfWork>();

        debugLogger?.Information("{MethodName} Finished", nameof(AddSupportToolsServerDatabase));

        return services;
    }
}
