# FluentMigrator migration creation from EF Core Code First DbContext

This example demonstrates how to use FluentMigrator in conjunction with Entity Framework Core (EF Core) to generate
FluentMigrator migration files based on an EF Core Code First DbContext.

## How to Run the Example

This will generate the initial migration files for the database schema, with the model snapshot, in a directory named
`Migrations`.

```bash
dotnet ef migrations add InitialCreate --output-dir Migrations
```

Any subsequent changes to the DbContext model can be captured by creating additional migrations:

```bash
dotnet ef migrations add AddNewColumnToTable --output-dir Migrations
```

## Supported EF Core commands

The following EF Core commands are supported:

- `dotnet ef migrations add <MigrationName> --output-dir Migrations` - Creates a new migration file in the `Migrations` directory. ([See docs](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-database-drop))
- `dotnet ef migrations has-pending-model-changes` - Checks if there are any pending model changes that need to be migrated. ([See docs](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-migrations-has-pending-model-changes))

These commands are not related to migrations and are supported as usual:

- `dotnet ef dbcontext info` - Displays information about the DbContext. ([See docs](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-dbcontext-info))
- `dotnet ef dbcontext list` - Lists all available DbContext types in the project. ([See docs](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-dbcontext-list))
- `dotnet ef dbcontext scaffold` - Scaffolds a DbContext and entity types for a specified database. ([See docs](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-dbcontext-scaffold))
- `dotnet ef dbcontext optimize` - Optimizes the DbContext for better performance. ([See docs](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-dbcontext-optimize))
- `dotnet ef dbcontext script` - Generates a SQL script to create all tables for the DbContext. ([See docs](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-dbcontext-script))

These commands are NOT supported, because the migrations are handled by FluentMigrator:

- `dotnet ef database update` - Use FluentMigrator to apply migrations to the database. ([See docs](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-database-update))
- `dotnet ef database drop` - Use FluentMigrator to drop the database if needed. ([See docs](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-database-drop))
- `dotnet ef migrations list` - Use FluentMigrator to list available migrations.  ([See docs](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-migrations-list))
- `dotnet ef migrations remove` - Use FluentMigrator to remove migrations. ([See docs](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-migrations-remove))
- `dotnet ef migrations script` - Use FluentMigrator to generate SQL scripts for migrations. ([See docs](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-migrations-script))
- `dotnet ef migrations bundle` - Use FluentMigrator to create migration bundles. ([See docs](https://learn.microsoft.com/ef/core/cli/dotnet#dotnet-ef-migrations-bundle))
