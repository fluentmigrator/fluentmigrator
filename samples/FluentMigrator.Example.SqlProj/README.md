# FluentMigrator SqlProj Example

This example demonstrates how to generate FluentMigrator migrations from a SQL Server Database Project (.sqlproj).

## Overview

Instead of manually writing migration classes, you can maintain your database schema in SQL scripts organized in a .sqlproj file, and automatically generate FluentMigrator migration classes from them.

## Structure

```
FluentMigrator.Example.SqlProj/
├── SampleDatabase.sqlproj       # SQL Server Database Project file
├── Tables/
│   ├── Users.sql                # User table definition
│   └── Orders.sql               # Orders table definition
└── README.md                    # This file
```

## Usage

### Using the CLI tool

1. Install the FluentMigrator Migration Generator CLI tool:
   ```bash
   dotnet tool install -g FluentMigrator.MigrationGenerator
   ```

2. Generate a migration from the .sqlproj file:
   ```bash
   fm-generator from-sqlproj \
     --project ./SampleDatabase.sqlproj \
     --output ./GeneratedMigrations \
     --name InitialDatabaseSchema \
     --namespace MyApp.Migrations
   ```

### Command Options

- `-p, --project <PATH>`: Path to the .sqlproj file (required)
- `-o, --output <PATH>`: Output directory for generated migration file (required)
- `-n, --name <NAME>`: Name for the migration class (required)
- `--namespace <NAMESPACE>`: Namespace for the migration class (default: Migrations)
- `-v, --version <VERSION>`: Migration version number (default: timestamp in format YYYYMMDDHHmmss)
- `--verbose`: Show verbose output

### Output

The tool will generate a C# migration file like this:

```csharp
[Migration(20241120235959)]
public class InitialDatabaseSchema : Migration
{
    public override void Up()
    {
        Create.Table("Users")
            .WithColumn("Id").AsInt32().NotNullable().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Email").AsString(255).NotNullable()
            .WithColumn("CreatedDate").AsDateTime2().NotNullable()
            .WithColumn("IsActive").AsBoolean().NotNullable()
            ;

        Create.Table("Orders")
            .WithColumn("Id").AsInt32().NotNullable().Identity()
            .WithColumn("UserId").AsInt32().NotNullable()
            .WithColumn("OrderNumber").AsString(50).NotNullable()
            .WithColumn("TotalAmount").AsDecimal(18, 2).NotNullable()
            .WithColumn("OrderDate").AsDateTime2().NotNullable()
            .WithColumn("Status").AsString(20).NotNullable()
            ;
    }

    public override void Down()
    {
        Delete.Table("Orders");
        Delete.Table("Users");
    }
}
```

## Benefits

1. **Single Source of Truth**: Maintain your database schema in SQL scripts
2. **Tool Compatibility**: Work with existing SQL Server Database Project tools
3. **Code Generation**: Automatically generate type-safe FluentMigrator code
4. **Version Control**: Easily track changes in both SQL scripts and generated migrations
5. **Team Collaboration**: Leverage familiar SQL Server tooling while using FluentMigrator

## Supported Features

- Table creation with all standard SQL Server data types
- Column properties (nullable, identity, length, precision, scale)
- Schema support (dbo and custom schemas)
- Foreign key constraints (extracted from SQL DDL)

## Notes

- The tool uses Microsoft.SqlServer.DacFx to parse the .sqlproj file and extract schema information
- Only tables and columns are currently supported in the generated migrations
- Indexes, stored procedures, views, and other database objects can be added manually to the generated migration
- For incremental migrations, run the tool after making changes to your SQL scripts to generate a new migration with the differences
