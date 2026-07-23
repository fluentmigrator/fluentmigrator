using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Example.Host.Aspire;

internal static class FluentMigratorMigrationResourceExtensions
{
    public static IResourceBuilder<ProjectResource> AddFluentMigratorMigrations(
        this IDistributedApplicationBuilder builder,
        string migrationResourceName,
        IResourceBuilder<IResourceWithConnectionString> database)
    {
        var migrationService = builder.AddProject<Projects.AspireFluentMigrator_MigrationService>(migrationResourceName)
            .WithReference(database)
            .WaitFor(database);

        migrationService.WithCommand(
            name: "update-database",
            displayName: "Update Database",
            executeCommand: async context =>
            {
                var commandService = context.ServiceProvider.GetRequiredService<ResourceCommandService>();

                var result = await commandService.ExecuteCommandAsync(
                    migrationResourceName,
                    KnownResourceCommands.RestartCommand,
                    context.CancellationToken).ConfigureAwait(false);

                return result.Success
                    ? result
                    : await commandService.ExecuteCommandAsync(
                        migrationResourceName,
                        KnownResourceCommands.StartCommand,
                        context.CancellationToken).ConfigureAwait(false);
            },
            commandOptions: new CommandOptions
            {
                Description = "Apply pending FluentMigrator migrations by rerunning the migration service."
            });

        return migrationService;
    }
}
