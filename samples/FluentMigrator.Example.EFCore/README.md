# EF Core Scaffolding with FluentMigrator Example with Code First Models

This example demonstrates how to use FluentMigrator in conjunction with Entity Framework Core (EF Core) to scaffold a
database schema, from code first models.

It showcases how to manage database migrations using FluentMigrator while leveraging EF Core for data access.

## How to Run the Example

This will generate the initial migration files for the database schema, with the model snapshot, in a directory named
`Migrations`.

```bash
dotnet ef migrations add InitialCreate --output-dir Migrations
```
