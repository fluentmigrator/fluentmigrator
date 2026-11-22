using System.Diagnostics;
using FluentMigrator.Runner;

namespace AspireFluentMigrator.MigrationService;

public class Worker(
    IMigrationRunner migrationRunner,
    IHostEnvironment hostEnvironment,
    IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    private readonly ActivitySource _activitySource = new(hostEnvironment.ApplicationName);

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = _activitySource.StartActivity(hostEnvironment.ApplicationName, ActivityKind.Client);

        try
        {
            migrationRunner.MigrateUp();
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
        return Task.CompletedTask;
    }
}
