using Npgsql;
using Shorty.Db.Migrations;

namespace Shorty.RedirectApi.HostedServices;

public class DbMigratorHostedService : IHostedService
{
    private readonly IDbMigrationEngine _dbMigrationEngine;

    public DbMigratorHostedService(IDbMigrationEngine dbMigrationEngine)
    {
        _dbMigrationEngine = dbMigrationEngine;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        const int maxAttempts = 3;
        var attempts = 0;
        bool succeeded = false;
        do
        {
            attempts++;

            try
            {
                _dbMigrationEngine.Migrate();
                succeeded = true;
            }
            catch (NpgsqlException)
            {
                if (attempts >= maxAttempts) throw;
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
        while (!succeeded);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}