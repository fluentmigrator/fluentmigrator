# FluentMigrator with .NET Aspire sample

This sample demonstrates how to use [FluentMigrator](https://fluentmigrator.github.io/) with [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview) to run database migrations as part of your distributed application.

The sample has three important projects:

- `AspireFluentMigrator.ApiService` — A web API that queries the database.
- `AspireFluentMigrator.MigrationService` — A background worker that applies FluentMigrator migrations on startup, then exits.
- `AspireFluentMigrator.ServiceDefaults` — Shared Aspire service defaults (OpenTelemetry, health checks, service discovery).

`AspireFluentMigrator.ApiService` and `AspireFluentMigrator.MigrationService` each connect to a PostgreSQL database resource. During local development, the PostgreSQL resource is launched in a container by Aspire.

## How it works

The `AspireFluentMigrator.AppHost` project orchestrates everything:

1. A PostgreSQL container is started.
2. `AspireFluentMigrator.MigrationService` waits for the database to be ready, then runs all pending FluentMigrator migrations and calls `IHostApplicationLifetime.StopApplication()` to signal completion.
3. `AspireFluentMigrator.ApiService` waits for the migration service to **complete** (using `WaitForCompletion`) before it starts accepting requests. This guarantees the database schema is up-to-date before any API traffic arrives.

## Adding a new migration

Migrations are defined in the `AspireFluentMigrator.MigrationService/Migrations/` folder as classes that inherit from `FluentMigrator.Migration`.

1. Create a new migration class with an incremented `[Migration(N)]` attribute:

    ```csharp
    using FluentMigrator;

    namespace AspireFluentMigrator.MigrationService.Migrations;

    [Migration(2, "Add name column to entries")]
    public class M002_AddNameToEntries : Migration
    {
        public override void Up()
        {
            Alter.Table("entries")
                .AddColumn("name").AsString(200).Nullable();
        }

        public override void Down()
        {
            Delete.Column("name").FromTable("entries");
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
dotnet run --project AspireFluentMigrator.AppHost
```

**Using Visual Studio:**

Open `FluentMigrator.Example.Aspire.sln` and set `AspireFluentMigrator.AppHost` as the startup project.

When the app starts, the Aspire dashboard opens in your browser. You will see:

- The PostgreSQL container starting up.
- The `migration` service running, applying the schema migrations, and then completing.
- The `api` service starting after the migration completes.

Navigate to the `api` service endpoint (shown in the Aspire dashboard) and browse to `/` to see entries being inserted and returned.
