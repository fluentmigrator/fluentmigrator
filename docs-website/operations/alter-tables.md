# Altering Tables

FluentMigrator provides comprehensive support for modifying existing database tables. This guide covers all aspects of table alterations, from adding and removing columns to renaming tables and changing table constraints.

## Basic Table Alterations

### Renaming Tables

```csharp
public class RenameTables : Migration
{
    public override void Up()
    {
        Rename.Table("OldTableName").To("NewTableName");
    }

    public override void Down()
    {
        Rename.Table("NewTableName").To("OldTableName");
    }
}
```

### Adding Columns to Existing Tables

```csharp
public class AddColumnsToTable : Migration
{
    public override void Up()
    {
        Alter.Table("Users")
            .AddColumn("Email").AsString(255).Nullable()
            .AddColumn("CreatedAt").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
            .AddColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true);
    }

    public override void Down()
    {
        Delete.Column("Email").FromTable("Users");
        Delete.Column("CreatedAt").FromTable("Users");
        Delete.Column("IsActive").FromTable("Users");
    }
}
```

### Removing Columns

```csharp
public class RemoveColumns : Migration
{
    public override void Up()
    {
        Delete.Column("ObsoleteField").FromTable("Users");

        // Remove multiple columns at once
        Delete.Column("TempField1")
            .Column("TempField2")
            .FromTable("Products");
    }

    public override void Down()
    {
        Alter.Table("Users")
            .AddColumn("ObsoleteField").AsString(50).Nullable();

        Alter.Table("Products")
            .AddColumn("TempField1").AsInt32().Nullable()
            .AddColumn("TempField2").AsString(100).Nullable();
    }
}
```

## Advanced Column Modifications

### Changing Column Properties

```csharp
public class ModifyColumnProperties : Migration
{
    public override void Up()
    {
        // Change column type and size
        Alter.Column("Description").OnTable("Products")
            .AsString(1000).NotNullable();

        // Make a nullable column not nullable with default value
        Alter.Column("Status").OnTable("Orders")
            .AsString(20).NotNullable().WithDefaultValue("Pending");

        // Change column to nullable
        Alter.Column("MiddleName").OnTable("Users")
            .AsString(50).Nullable();
    }

    public override void Down()
    {
        Alter.Column("Description").OnTable("Products")
            .AsString(500).Nullable();

        Alter.Column("Status").OnTable("Orders")
            .AsString(20).Nullable();

        Alter.Column("MiddleName").OnTable("Users")
            .AsString(50).NotNullable();
    }
}
```

### Renaming Columns

```csharp
public class RenameColumns : Migration
{
    public override void Up()
    {
        Rename.Column("FirstName").OnTable("Users").To("GivenName");
        Rename.Column("LastName").OnTable("Users").To("FamilyName");
    }

    public override void Down()
    {
        Rename.Column("GivenName").OnTable("Users").To("FirstName");
        Rename.Column("FamilyName").OnTable("Users").To("LastName");
    }
}
```

## Table Documentation

Use `WithDescription` to add or update documentation for existing tables. This is useful for adding context to tables created in earlier migrations.

### Adding Table Descriptions

```csharp
public class AddTableDocumentation : Migration
{
    public override void Up()
    {
        Alter.Table("Users")
            .WithDescription("Application users with authentication credentials and profile data. " +
                           "Central entity linking to user preferences, roles, and activity logs.");
    }

    public override void Down()
    {
        // Note: Description removal is not directly supported in all databases
        // Consider using Execute.Sql for provider-specific removal if needed
    }
}
```

### Provider-Specific Description Management

Different databases handle description updates differently:

```csharp
public class ProviderSpecificDocumentation : Migration
{
    public override void Up()
    {
        // Standard approach works for most providers
        Alter.Table("Analytics")
            .WithDescription("Performance metrics and usage statistics for business intelligence");

        // For complex description removal (if needed)
        IfDatabase(ProcessorIdConstants.SqlServer)
            .Execute.Sql(@"
                IF EXISTS (SELECT * FROM fn_listextendedproperty(N'MS_Description', N'SCHEMA', N'dbo', N'TABLE', N'OldTable', NULL, NULL))
                    EXEC sys.sp_dropextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name='dbo', @level1type=N'TABLE', @level1name='OldTable'
            ");

        IfDatabase(ProcessorIdConstants.PostgreSql)
            .Execute.Sql("COMMENT ON TABLE \"OldTable\" IS NULL");
    }

    public override void Down()
    {
        // Rollback if needed for your use case
    }
}
```

## Working with Table Constraints

### Adding Unique Constraints

```csharp
public class AddTableConstraints : Migration
{
    public override void Up()
    {
        // Add unique constraint
        Create.UniqueConstraint("UQ_Users_Email")
            .OnTable("Users")
            .Column("Email");
    }

    public override void Down()
    {
        Delete.UniqueConstraint("UQ_Users_Email").FromTable("Users");
    }
}
```

For comprehensive constraint management and cross-database compatibility, see [Constraints](/basics/constraints.md).

### Removing Table Constraints

```csharp
public class RemoveConstraints : Migration
{
    public override void Up()
    {
        Delete.UniqueConstraint("UQ_Products_SKU").FromTable("Products");
    }

    public override void Down()
    {
        Create.UniqueConstraint("UQ_Products_SKU")
            .OnTable("Products")
            .Column("SKU");
    }
}
```

## Best Practices for Table Alterations

### 1. Always Provide Rollback Logic

```csharp
public class BestPracticeExample : Migration
{
    public override void Up()
    {
        // Store original column definition for rollback
        Alter.Column("Price").OnTable("Products")
            .AsDecimal(10, 4).NotNullable(); // Changed from (8,2)
    }

    public override void Down()
    {
        // Restore original definition
        Alter.Column("Price").OnTable("Products")
            .AsDecimal(8, 2).NotNullable();
    }
}
```

### 2. Handle Data Migration

```csharp
public class DataMigrationExample : Migration
{
    public override void Up()
    {
        // Add new column
        Alter.Table("Users")
            .AddColumn("FullName").AsString(200).Nullable();

        // Migrate existing data
        Execute.Sql(@"
            UPDATE Users
            SET FullName = FirstName + ' ' + LastName
            WHERE FirstName IS NOT NULL AND LastName IS NOT NULL");

        // Make column not nullable after data migration
        Alter.Column("FullName").OnTable("Users")
            .AsString(200).NotNullable();
    }

    public override void Down()
    {
        Delete.Column("FullName").FromTable("Users");
    }
}
```

### 3. Test with Large Datasets

When altering tables with significant data:

- Consider performance implications
- Test on production-sized datasets
- Plan for potential downtime
- Use database-specific optimization techniques

### 4. Handle Dependencies

```csharp
public class HandleDependencies : Migration
{
    public override void Up()
    {
        // Drop dependent objects first
        Delete.Index("IX_Users_OldColumn").OnTable("Users");

        // Alter the column
        Alter.Column("OldColumn").OnTable("Users")
            .AsString(100).NotNullable();

        // Recreate dependent objects
        Create.Index("IX_Users_OldColumn")
            .OnTable("Users")
            .OnColumn("OldColumn");
    }

    public override void Down()
    {
        Delete.Index("IX_Users_OldColumn").OnTable("Users");

        Alter.Column("OldColumn").OnTable("Users")
            .AsString(50).Nullable();

        Create.Index("IX_Users_OldColumn")
            .OnTable("Users")
            .OnColumn("OldColumn");
    }
}
```

## Common Patterns

### Adding Audit Columns

```csharp
public class AddAuditColumns : Migration
{
    public override void Up()
    {
        var tables = new[] { "Users", "Products", "Orders" };

        foreach (var table in tables)
        {
            Alter.Table(table)
                .AddColumn("CreatedAt").AsDateTime().NotNullable()
                    .WithDefaultValue(SystemMethods.CurrentDateTime)
                .AddColumn("UpdatedAt").AsDateTime().NotNullable()
                    .WithDefaultValue(SystemMethods.CurrentDateTime)
                .AddColumn("CreatedBy").AsString(100).Nullable()
                .AddColumn("UpdatedBy").AsString(100).Nullable();
        }
    }

    public override void Down()
    {
        var tables = new[] { "Users", "Products", "Orders" };

        foreach (var table in tables)
        {
            Delete.Column("CreatedAt").FromTable(table);
            Delete.Column("UpdatedAt").FromTable(table);
            Delete.Column("CreatedBy").FromTable(table);
            Delete.Column("UpdatedBy").FromTable(table);
        }
    }
}
```

### Soft Delete Implementation

```csharp
public class AddSoftDelete : Migration
{
    public override void Up()
    {
        Alter.Table("Users")
            .AddColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
            .AddColumn("DeletedAt").AsDateTime().Nullable()
            .AddColumn("DeletedBy").AsString(100).Nullable();
    }

    public override void Down()
    {
        Delete.Column("IsDeleted").FromTable("Users");
        Delete.Column("DeletedAt").FromTable("Users");
        Delete.Column("DeletedBy").FromTable("Users");
    }
}
```

## Troubleshooting Common Issues

### Issue: Column Already Exists

```csharp
public class SafeColumnAddition : Migration
{
    public override void Up()
    {
        if (!Schema.Table("Users").Column("Email").Exists())
        {
            Alter.Table("Users")
                .AddColumn("Email").AsString(255).Nullable();
        }
    }

    public override void Down()
    {
        if (Schema.Table("Users").Column("Email").Exists())
        {
            Delete.Column("Email").FromTable("Users");
        }
    }
}
```

### Issue: Cannot Make Column Not Null

```csharp
public class SafeNotNullConversion : Migration
{
    public override void Up()
    {
        // First, update any null values
        Execute.Sql("UPDATE Users SET Status = 'Unknown' WHERE Status IS NULL");

        // Then make the column not nullable
        Alter.Column("Status").OnTable("Users")
            .AsString(20).NotNullable();
    }

    public override void Down()
    {
        Alter.Column("Status").OnTable("Users")
            .AsString(20).Nullable();
    }
}
```

## Database-Specific Table Alteration Features

Different database providers offer specialized table alteration capabilities and advanced modification options:

### SQL Server Specific
- **Identity Column Modifications**: Altering identity seed and increment - [SQL Server Provider](../providers/sql-server.md#identity-columns)
- **Sparse Column Conversion**: Converting to/from SPARSE columns - [SQL Server Provider](../providers/sql-server.md#sql-server-specific-features)

### PostgreSQL Specific
- **Identity Column Changes**: Modifying GENERATED ALWAYS/BY DEFAULT - [PostgreSQL Provider](../providers/postgresql.md#identity-columns-postgresql-10)

## See Also
- [Creating Tables](./create-tables.md) - Initial table creation patterns
- [Columns](../basics/columns.md) - Column-specific modification strategies
- [Constraints](../basics/constraints.md) - Constraint alteration patterns
- [Execute SQL](./execute-sql.md) - Complex alteration scenarios
