# FluentMigrator with .NET Aspire sample

This sample demonstrates how to use [FluentMigrator](https://fluentmigrator.github.io/) with [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview) to run database migrations as part of your distributed application.

The sample has three important projects:

- `AspireFluentMigrator.ApiService` — A web API that queries the database.
- `AspireFluentMigrator.MigrationService` — A background worker that applies FluentMigrator migrations on startup, then exits.
- `AspireFluentMigrator.ServiceDefaults` — Shared Aspire service defaults (OpenTelemetry, health checks, service discovery).

The migration classes themselves are **not** duplicated in this sample. `AspireFluentMigrator.MigrationService` references the shared [`FluentMigrator.Example.Migrations`](../FluentMigrator.Example.Migrations) project and scans it for migrations, so the same migrations are reused across the FluentMigrator samples.

`AspireFluentMigrator.ApiService` and `AspireFluentMigrator.MigrationService` each connect to a PostgreSQL database resource. During local development, the PostgreSQL resource is launched in a container by Aspire.

## How it works

The `FluentMigrator.Example.Host.Aspire` project orchestrates everything:

1. A PostgreSQL container is started.
2. `AspireFluentMigrator.MigrationService` waits for the database to be ready, then runs all pending FluentMigrator migrations and calls `IHostApplicationLifetime.StopApplication()` to signal completion.
3. `AspireFluentMigrator.ApiService` waits for the migration service to **complete** (using `WaitForCompletion`) before it starts accepting requests. This guarantees the database schema is up-to-date before any API traffic arrives.

The AppHost uses a dedicated `AddFluentMigratorMigrations` helper to keep the orchestration code concise, similar to how EF Core's `AddEFMigrations` keeps migration wiring centralized.

## Resource commands

The migration resource appears in the Aspire Dashboard with a custom command:

| Command | Description |
|---------|-------------|
| Update Database | Re-runs the migration worker so pending FluentMigrator migrations are applied. |

## Adding a new migration

Migrations live in the shared [`FluentMigrator.Example.Migrations`](../FluentMigrator.Example.Migrations) project as classes that inherit from `FluentMigrator.Migration`.

1. Add a new migration class to that project with an incremented `[Migration(N)]` attribute:

    ```csharp
    using FluentMigrator;

    namespace FluentMigrator.Example.Migrations
    {
        [Migration(20240101000000)]
        public class AddArchivedToNotes : Migration
        {
            public override void Up()
            {
                Alter.Table("Notes")
                    .AddColumn("Archived").AsBoolean().NotNullable().WithDefaultValue(false);
            }

            public override void Down()
            {
                Delete.Column("Archived").FromTable("Notes");
            }
        }
    }
    ```

2. Run the app — the migration service picks up and applies the new migration automatically.

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- [.NET Aspire prerequisites](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling) (Docker Desktop or Podman, and the Aspire workload)

Install the Aspire workload if you haven't already:

```shell
dotnet workload install aspire
```

## Run the sample

**Using the .NET CLI:**

```shell
dotnet run --project FluentMigrator.Example.Host.Aspire
```

**Using Visual Studio:**

Open `FluentMigrator.Example.Aspire.slnx` and set `FluentMigrator.Example.Host.Aspire` as the startup project.

When the app starts, the Aspire dashboard opens in your browser. You will see:

- The PostgreSQL container starting up.
- The `migration` service running, applying the schema migrations, and then completing.
- The `api` service starting after the migration completes.

Navigate to the `api` service endpoint (shown in the Aspire dashboard) and browse to `/` to see context rows being inserted and returned.

## Next step for Aspire community plug-in acceptance

To publish the AppHost integration as an Aspire community plug-in (for example `Aspire.Hosting.FluentMigrator`) outside the sample, the next step is to follow the Aspire community contribution process and propose the package in the community toolkit with documentation, tests, and API review, rather than requesting ad-hoc approval through social media mentions.
