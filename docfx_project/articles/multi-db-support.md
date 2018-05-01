---
uid: multi-db-support.md
title: Dealing with Multiple Database Types
---

# Dealing with multiple database types

FluentMigrator allows you to target multiple database types from the same migration project. Every FluentMigrator project is database agnostic and can be run against any of the supported database types.

However, there are times when only some migrations in a project need to be executed against one of the database types. For supporting that scenario **FluentMigrator** includes the **IfDatabase** expression.

# Using IfDatabase

Suppose you have a migration that executes a script file to create a view:

```cs
public class CreateViewsMigration : Migration
{
    public override void Up()
    {
        Execute.Script("CreateViewsMigrationUp.sql");
    }

    public override void Down()
    {
        Execute.Script("CreateViewsMigrationDown.sql");
    }
}
```
However the project needs to **create some views in an SqlServer database and others in an Oracle database**, but you want both tasks to be part of the same migration, **sharing the same migration number** in both databases. You handle this by creating scripts for each database and specifying which one needs to be executed:

```cs
public class CreateViewsMigration : Migration
{
     public override void Up()
     {
        IfDatabase("oracle").Execute.Script("CreateViewsOracleMigrationUp.sql");
        IfDatabase("sqlserver").Execute.Script("CreateViewsSqlServerMigrationUp.sql");
     }

     public override void Down()
     {
         IfDatabase("oracle").Execute.Script("CreateViewsOracleMigrationDown.sql");
         IfDatabase("sqlserver").Execute.Script("CreateViewsSqlServerMigrationDown.sql");
     }
}
```

# Supported databases

[!include[Supported databases](../snippets/supported-databases.md)]
