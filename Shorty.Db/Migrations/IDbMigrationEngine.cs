using System.Reflection;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shorty.Db.Migrations;

public interface IDbMigrationEngine
{
    void Migrate();
}

public class DbMigrationEngine : IDbMigrationEngine
{
    private readonly string _connectionString;

    public DbMigrationEngine(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Migrate()
    {
        IServiceProvider serviceProvider = CreateServices(_connectionString, GetType().Assembly);

        using IServiceScope scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    private IServiceProvider CreateServices(string dbConnectionString, Assembly assembly)
    {
        IServiceCollection serviceCollection = new ServiceCollection()
            .AddFluentMigratorCore()
            .AddLogging(lb =>
                {
                    lb.AddFluentMigratorConsole()
                        .SetMinimumLevel(LogLevel.Warning);
                });

        serviceCollection.ConfigureRunner(rb =>
            {
                rb
                    .AddPostgres()
                    .WithGlobalConnectionString(dbConnectionString)
                    .ScanIn(assembly).For.All();
            });

        return serviceCollection.BuildServiceProvider(false);
    }
}