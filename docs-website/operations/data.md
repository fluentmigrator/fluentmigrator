# Data Operations

FluentMigrator provides powerful capabilities for manipulating data during migrations. This guide covers inserting, updating, deleting, and transforming data as part of your database schema evolution.

## Basic Data Operations

### Inserting Data

```csharp
public class BasicInsertOperations : Migration
{
    public override void Up()
    {
        // Create table first
        Create.Table("Users")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Email").AsString(255).NotNullable()
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("CreatedAt").AsDateTime().NotNullable();

        // Single row insert
        Insert.IntoTable("Users")
            .Row(new
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                IsActive = true,
                CreatedAt = DateTime.Now
            });

        // Multiple row insert
        Insert.IntoTable("Users")
            .Row(new { Name = "Jane Smith", Email = "jane.smith@example.com", IsActive = true, CreatedAt = DateTime.Now })
            .Row(new { Name = "Bob Johnson", Email = "bob.johnson@example.com", IsActive = true, CreatedAt = DateTime.Now })
            .Row(new { Name = "Alice Brown", Email = "alice.brown@example.com", IsActive = false, CreatedAt = DateTime.Now });
    }

    public override void Down()
    {
        Delete.FromTable("Users").AllRows();
        Delete.Table("Users");
    }
}
```

## Database-Specific Column Features

Different database providers offer specialized insert features and extensions:

### SQL Server Specific
- **Identity Insert Operations**: Use `WithIdentityInsert()` for explicit identity values - [SQL Server Provider](../providers/sql-server.md#identity-insert-operations)

### PostgreSQL Specific
- **Identity Column Overrides**: `WithOverridingSystemValue()` and `WithOverridingUserValue()` - [PostgreSQL Provider](../providers/postgresql.md#overriding-identity-values-extensions)

### Updating Data

```csharp
public class BasicUpdateOperations : Migration
{
    public override void Up()
    {
        // Update all rows using anonymous object
        Update.Table("Users")
            .Set(new { IsActive = true, UpdatedAt = DateTime.Now })
            .AllRows();

        // Update with WHERE condition using anonymous object
        Update.Table("Users")
            .Set(new { IsActive = false })
            .Where(new { Name = "John Doe" });

        // Update using dictionary (useful for dynamic scenarios)
        var updates = new Dictionary<string, object>
        {
            { "IsActive", true },
            { "UpdatedAt", DateTime.Now }
        };
        
        Update.Table("Users")
            .Set(updates)
            .AllRows();

        // WHERE clause with dictionary
        var criteria = new Dictionary<string, object>
        {
            { "Name", "John Doe" },
            { "Email", "john.doe@example.com" }
        };
        
        Update.Table("Users")
            .Set(new { IsActive = false })
            .Where(criteria);

        // For complex operations, using raw SQL helper (Fluent Migrator 7.0+):
        Update.Table("Users")
            .Set(new
            {
                LastLoginAt = RawSql.Insert("GETDATE()"),
            })
            .Where(new
            {
                Email = RawSql.Insert("LIKE '%@company.com'")
            });

        // Equivalent to :
        Execute.Sql("UPDATE Users SET LastLoginAt = GETDATE() WHERE Email LIKE '%@company.com'");
    }

    public override void Down()
    {
        // Restore original state if possible
        Update.Table("Users")
            .Set(new { IsActive = true })
            .AllRows();
    }
}
```

#### Using Dictionaries for Dynamic Updates

The `Set()` and `Where()` methods support `IDictionary<string, object>` in addition to anonymous objects. This is particularly useful when:

- Building updates dynamically at runtime
- Reading column values from configuration
- Creating reusable migration helpers
- Working with reflection-based scenarios

```csharp
public class DynamicUpdateOperations : Migration
{
    public override void Up()
    {
        // Dynamic update based on configuration
        var configValues = LoadConfigurationValues();
        var updates = new Dictionary<string, object>();
        
        foreach (var setting in configValues)
        {
            updates[setting.ColumnName] = setting.Value;
        }
        
        Update.Table("Configuration")
            .Set(updates)
            .Where(new Dictionary<string, object>
            {
                { "Environment", "Production" },
                { "IsActive", true }
            });
    }

    public override void Down() { }
    
    private IEnumerable<ConfigSetting> LoadConfigurationValues()
    {
        // Load from external source
        return new[]
        {
            new ConfigSetting { ColumnName = "MaxConnections", Value = 100 },
            new ConfigSetting { ColumnName = "Timeout", Value = 30 }
        };
    }
}

public class ConfigSetting
{
    public string ColumnName { get; set; }
    public object Value { get; set; }
}
```

#### Mixing Anonymous Objects and Dictionaries

You can use both approaches within the same migration:

```csharp
public class MixedUpdateOperations : Migration
{
    public override void Up()
    {
        // Use anonymous object for known, static values
        Update.Table("Users")
            .Set(new { Status = "Active" })
            .Where(BuildDynamicCriteria());  // Use dictionary for dynamic criteria
    }

    public override void Down() { }

    private Dictionary<string, object> BuildDynamicCriteria()
    {
        var criteria = new Dictionary<string, object>();
        
        // Add criteria based on runtime conditions
        if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
        {
            criteria["Department"] = "Sales";
        }
        else
        {
            criteria["Department"] = "Marketing";
        }
        
        return criteria;
    }
}
```

### Deleting Data

```csharp
public class BasicDeleteOperations : Migration
{
    public override void Up()
    {
        // Delete specific rows
        Delete.FromTable("Users")
            .Row(new { Name = "John Doe" });

        // Delete multiple rows with condition
        Delete.FromTable("Users")
            .Where(new { IsActive = false });

        // For complex operations, using raw SQL helper (Fluent Migrator 7.0+):
        Delete.FromTable("Users")
            .Where(new
            {
                CreatedAt = RawSql.Insert("< '2020-01-01'")
            });

        // Equivalent to :
        Execute.Sql("DELETE FROM Users WHERE CreatedAt < '2020-01-01'");
    }

    public override void Down()
    {
        // Re-insert deleted data if needed (typically not possible in real scenarios)
        Insert.IntoTable("Users")
            .Row(new
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                IsActive = true
            });
    }
}
```

## Advanced Data Operations

### Data Migration Between Columns

```csharp
public class DataColumnMigration : Migration
{
    public override void Up()
    {
        // Add new column
        Alter.Table("Users")
            .AddColumn("FullName").AsString(200).Nullable();

        // Migrate data from existing columns - see Raw SQL guide for complex data migration patterns
        Execute.Sql("UPDATE Users SET FullName = COALESCE(FirstName + ' ' + LastName, FirstName, LastName) WHERE FullName IS NULL");

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

### Data Type Conversions

```csharp
public class DataTypeConversions : Migration
{
    public override void Up()
    {
        // Add new column with correct data type
        Alter.Table("Products")
            .AddColumn("PriceDecimal").AsDecimal(10, 2).Nullable();

        // Convert string price to decimal, handling invalid values, also possible with Raw SQL helper (Fluent Migrator 7.0+):
        IfDatabase(ProcessorIdConstants.SqlServer).Execute.Sql(@"
            UPDATE Products
            SET PriceDecimal = TRY_CONVERT(DECIMAL(10,2), PriceString)
            WHERE ISNUMERIC(PriceString) = 1");

        IfDatabase(ProcessorIdConstants.Postgres).Execute.Sql(@"
            UPDATE Products
            SET PriceDecimal = CASE
                WHEN PriceString ~ '^[0-9]+\.?[0-9]*$' THEN PriceString::DECIMAL(10,2)
                ELSE NULL
            END");

        IfDatabase(ProcessorIdConstants.MySql).Execute.Sql(@"
            UPDATE Products
            SET PriceDecimal = CASE
                WHEN PriceString REGEXP '^[0-9]+\.?[0-9]*$' THEN CAST(PriceString AS DECIMAL(10,2))
                ELSE NULL
            END");

        // Make new column not nullable after successful conversion
        Alter.Column("PriceDecimal").OnTable("Products")
            .AsDecimal(10, 2).NotNullable().WithDefaultValue(0.00m);

        // Remove old column
        Delete.Column("PriceString").FromTable("Products");

        // Rename new column to original name
        Rename.Column("PriceDecimal").OnTable("Products").To("Price");
    }

    public override void Down()
    {
        Rename.Column("Price").OnTable("Products").To("PriceDecimal");

        Alter.Table("Products")
            .AddColumn("PriceString").AsString(50).Nullable();

        Execute.Sql("UPDATE Products SET PriceString = CAST(PriceDecimal AS VARCHAR(50))");

        Delete.Column("PriceDecimal").FromTable("Products");

        Rename.Column("PriceString").OnTable("Products").To("Price");
    }
}
```

### Bulk Data Operations

For comprehensive bulk operations examples, see [Execute code on connection](../operations/with-connection.md#bulk-data-operations).

```csharp
public class BulkDataOperations : Migration
{
    public override void Up()
    {
        // Basic bulk insert example - for complex bulk operations, use Execute.Sql
        Insert.IntoTable("Categories").Row(new { Name = "Electronics", IsActive = true });
        Insert.IntoTable("Categories").Row(new { Name = "Clothing", IsActive = true });

        // For complex bulk operations with joins and batch processing, using raw SQL helper (Fluent Migrator 7.0+):
        Update.Table("Products")
            .Set(new
            {
                CategoryId = RawSql.Insert("(SELECT Id FROM Categories WHERE Name = 'Electronics')")
            })
            .Where(new
            {
                CategoryName = "Electronics"
            });

        // Equivalent to :
        Execute.Sql("UPDATE Products SET CategoryId = (SELECT Id FROM Categories WHERE Name = 'Electronics') WHERE CategoryName = 'Electronics'");
    }

    public override void Down()
    {
        Delete.FromTable("Categories").AllRows();
    }
}
```

## Advanced SQL Operations

For comprehensive examples of advanced Execute.Sql operations including:
- Batch processing for large datasets
- Complex data transformations
- Database-specific optimizations
- Error handling and validation
- Transaction control

See: [Execute SQL](/operations/execute-sql.md) and [Execute code on connection](../operations/with-connection.md).
