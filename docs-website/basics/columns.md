# Columns

This comprehensive guide covers all aspects of column management in FluentMigrator, including data types, modifiers, constraints, and best practices for working with database columns across different providers.

## Column Operations

### Adding Columns to Existing Tables
```csharp
public class AddColumns : Migration
{
    public override void Up()
    {
        Alter.Table("Users")
            .AddColumn("Email").AsString(255).NotNullable()
            .AddColumn("PhoneNumber").AsString(20).Nullable()
            .AddColumn("CreatedAt").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime);
    }

    public override void Down()
    {
        Delete.Column("Email").FromTable("Users");
        Delete.Column("PhoneNumber").FromTable("Users");
        Delete.Column("CreatedAt").FromTable("Users");
    }
}
```

### Modifying Existing Columns
```csharp
public class ModifyColumns : Migration
{
    public override void Up()
    {
        // Change data type
        Alter.Column("Price").OnTable("Products")
            .AsDecimal(10, 2).NotNullable();

        // Change nullability
        Alter.Column("Description").OnTable("Products")
            .AsString(500).Nullable();

        // Add default value
        Alter.Column("IsActive").OnTable("Users")
            .AsBoolean().NotNullable().WithDefaultValue(true);
    }

    public override void Down()
    {
        Alter.Column("Price").OnTable("Products")
            .AsString(50).Nullable();
        Alter.Column("Description").OnTable("Products")
            .AsString(500).NotNullable();
        Alter.Column("IsActive").OnTable("Users")
            .AsBoolean().Nullable();
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
    }

    public override void Down()
    {
        Alter.Table("Users")
            .AddColumn("ObsoleteField").AsString(100).Nullable();
    }
}
```

## Data Types

Each provider supports a variety of data types. Below are examples of commonly used types in FluentMigrator.

To see the full list of supported data types, refer to the provider specific documentation in the **Database Providers** section.

```csharp
.WithColumn("StringCol").AsString(100)
.WithColumn("AnsiStringCol").AsAnsiString(50)
.WithColumn("TextCol").AsString()
.WithColumn("IntCol").AsInt32()
.WithColumn("LongCol").AsInt64()
.WithColumn("ShortCol").AsInt16()
.WithColumn("ByteCol").AsByte()
.WithColumn("DecimalCol").AsDecimal(10, 2)
.WithColumn("MoneyCol").AsCurrency()
.WithColumn("FloatCol").AsFloat()
.WithColumn("DoubleCol").AsDouble()
.WithColumn("DateCol").AsDate()
.WithColumn("DateTimeCol").AsDateTime()
.WithColumn("DateTime2Col").AsDateTime2()
.WithColumn("TimeCol").AsTime()
.WithColumn("BoolCol").AsBoolean()
.WithColumn("GuidCol").AsGuid()
.WithColumn("BinaryCol").AsBinary(100)
.WithColumn("BlobCol").AsBinary()
.WithColumn("XmlCol").AsXml()
.WithColumn("JsonCol").AsCustom("ENUM('A','B','C')") // Custom type, depending on DB
```

## Nullability and Defaults
```csharp
.WithColumn("Name")
    .AsString(100)
    .NotNullable()                               // NOT NULL
    .WithDefaultValue("Unknown")                 // DEFAULT 'Unknown'

.WithColumn("CreatedAt")
    .AsDateTime()
    .NotNullable()
    .WithDefaultValue(SystemMethods.CurrentDateTime) // DEFAULT GETDATE()

.WithColumn("Count")
    .AsInt32()
    .Nullable()                                  // NULL (default)
    .WithDefaultValue(0)                        // DEFAULT 0
```

## Identity and Primary Keys
```csharp
.WithColumn("Id")
    .AsInt32()
    .NotNullable()
    .PrimaryKey()                               // PRIMARY KEY
    .Identity()                                 // IDENTITY/AUTO_INCREMENT

.WithColumn("Code")
    .AsString(10)
    .NotNullable()
    .PrimaryKey()                              // String primary key
    .Unique()                                  // UNIQUE constraint
```

## Indexing
```csharp
.WithColumn("Email")
    .AsString(255)
    .NotNullable()
    .Indexed()                                 // Creates index automatically
    .Unique()                                  // UNIQUE constraint
```

## Computed Columns

### Basic Computed Column
```csharp
Create.Table("Orders")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("Subtotal").AsDecimal(10, 2).NotNullable()
    .WithColumn("Tax").AsDecimal(10, 2).NotNullable()
    .WithColumn("Total").AsDecimal(10, 2).Computed("Subtotal + Tax");
```

### Persisted Computed Column
```csharp
Create.Table("Products")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("Name").AsString(255).NotNullable()
    .WithColumn("SearchName").AsString(255).Computed("UPPER(Name)").Persisted();
```

## Documentation and Descriptions

FluentMigrator provides several methods to document columns and tables, allowing you to add metadata that can be stored in the database (depending on provider support).

### Basic Column Description

Use `WithColumnDescription` to add a primary description for a column:

```csharp
Create.Table("Users")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("Email").AsString(255).NotNullable()
        .WithColumnDescription("User's primary email address for authentication")
    .WithColumn("CreatedAt").AsDateTime().NotNullable()
        .WithColumnDescription("Timestamp when the user account was created");

// When altering existing columns
Alter.Column("Status").OnTable("Orders")
    .AsString(50).NotNullable()
    .WithColumnDescription("Current order status: Pending, Processing, Shipped, or Delivered");
```

### Additional Column Descriptions

Use `WithColumnAdditionalDescription` to add named metadata beyond the primary description:

```csharp
Create.Table("Products")
    .WithColumn("Price").AsDecimal(10, 2).NotNullable()
        .WithColumnDescription("Product price in USD")
        .WithColumnAdditionalDescription("Currency", "USD")
        .WithColumnAdditionalDescription("LastUpdated", "2024-01-01")
        .WithColumnAdditionalDescription("ValidationRule", "Must be positive value");
```

### Multiple Additional Descriptions

Use `WithColumnAdditionalDescriptions` to add multiple metadata entries at once:

```csharp
var metadata = new Dictionary<string, string>
{
    {"DataSource", "External API"},
    {"RefreshFrequency", "Daily"},
    {"Owner", "DataTeam"},
    {"SensitivityLevel", "Internal"}
};

Create.Table("ExternalData")
    .WithColumn("ExternalId").AsString(100).NotNullable()
        .WithColumnDescription("Unique identifier from external system")
        .WithColumnAdditionalDescriptions(metadata);
```

### Database Provider Support

Column descriptions are supported by different databases with varying storage mechanisms:

| Provider   | Storage Method      | Notes                               |
|------------|---------------------|-------------------------------------|
| SQL Server | Extended Properties | Stored as MS_Description properties |
| PostgreSQL | COMMENT ON COLUMN   | Native comment support              |
| Oracle     | COMMENT ON COLUMN   | Native comment support              |
| MySQL      | Column comments     | Native comment support              |
| SQLite     | Not supported       | Descriptions are ignored            |

### Advanced Description Patterns

```csharp
// Environment-specific documentation
Create.Table("Configuration")
    .WithColumn("Setting").AsString(100).NotNullable()
        .WithColumnDescription("Application configuration setting")
        .WithColumnAdditionalDescription("Environment", "Production")
        .WithColumnAdditionalDescription("Example", "timeout=30000");

// Data lineage documentation
Create.Table("Analytics")
    .WithColumn("MetricValue").AsDecimal(18, 4).NotNullable()
        .WithColumnDescription("Calculated metric value")
        .WithColumnAdditionalDescription("Formula", "SUM(revenue) / COUNT(orders)")
        .WithColumnAdditionalDescription("DataSource", "orders.revenue")
        .WithColumnAdditionalDescription("UpdateFrequency", "Hourly");

// Business rule documentation
Create.Table("Employees")
    .WithColumn("Salary").AsCurrency().NotNullable()
        .WithColumnDescription("Employee annual salary")
        .WithColumnAdditionalDescription("Range", "30000-200000")
        .WithColumnAdditionalDescription("ReviewCycle", "Annual")
        .WithColumnAdditionalDescription("Approver", "HR Director");
```

## Setting Initial Values for Existing Rows

When adding new columns to existing tables with data, you may need to populate the new column with initial values for existing rows. The `SetExistingRowsTo` method provides this functionality.

### Basic Usage
```csharp
public class AddLastLoginDate : Migration
{
    public override void Up()
    {
        Alter.Table("Users")
            .AddColumn("LastLoginDate")
            .AsDateTime()
            .NotNullable()
            .SetExistingRowsTo(DateTime.Today);
    }

    public override void Down()
    {
        Delete.Column("LastLoginDate").FromTable("Users");
    }
}
```

### With Nullable Columns
```csharp
public class AddOptionalField : Migration
{
    public override void Up()
    {
        Alter.Table("Products")
            .AddColumn("Category")
            .AsString(50)
            .Nullable()
            .SetExistingRowsTo("Uncategorized");
    }

    public override void Down()
    {
        Delete.Column("Category").FromTable("Products");
    }
}
```

### Database-Specific Handling
```csharp
public class AddTimestampField : Migration
{
    public override void Up()
    {
        // SQLite doesn't support adding NOT NULL columns to existing tables
        // so we handle it differently
        IfDatabase(t => t != ProcessorIdConstants.SQLite)
            .Alter.Table("Orders")
            .AddColumn("CreatedAt")
            .AsDateTime()
            .NotNullable()
            .SetExistingRowsTo(SystemMethods.CurrentDateTime);

        IfDatabase(ProcessorIdConstants.SQLite)
            .Alter.Table("Orders")
            .AddColumn("CreatedAt")
            .AsDateTime()
            .Nullable()
            .SetExistingRowsTo(SystemMethods.CurrentDateTime);
    }

    public override void Down()
    {
        Delete.Column("CreatedAt").FromTable("Orders");
    }
}
```

### Complex Value Assignment
```csharp
public class AddCalculatedField : Migration
{
    public override void Up()
    {
        Alter.Table("Employees")
            .AddColumn("FullName")
            .AsString(255)
            .NotNullable()
            .SetExistingRowsTo("Unknown");

        // Use RawSql.Insert (FluentMigrator 7+)
        Update.Table("Employees")
            .Set(new
            {
                FullName = RawSql.Insert("CONCAT(FirstName, ' ', LastName)")
            })
            .Where(new
            {
                FirstName = RawSql.Insert("IS NOT NULL",
                LastName = RawSql.Insert("IS NOT NULL")
            });

        // Or before FluentMigrator 7, use Execute.Sql
        Execute.Sql(@"
            UPDATE Employees
            SET FullName = CONCAT(FirstName, ' ', LastName)
            WHERE FirstName IS NOT NULL AND LastName IS NOT NULL
        ");
    }

    public override void Down()
    {
        Delete.Column("FullName").FromTable("Employees");
    }
}
```

### Important Notes

- `SetExistingRowsTo` only works when **creating** new columns, not when altering existing ones
- The method automatically handles the sequence of operations: it first adds the column as nullable, updates existing rows with the specified value, then makes the column NOT NULL if specified
- For NOT NULL columns, FluentMigrator automatically creates the necessary UPDATE statement to populate existing rows before applying the NOT NULL constraint
- The value provided must be compatible with the column's data type
- Consider using database-specific logic when dealing with different database providers that have varying limitations

## Database-Specific Column Features

Different database providers offer specialized column features and extensions:

### SQL Server Specific
- **SPARSE columns**: `Sparse()` modifier for optimized storage - [SQL Server Provider](../providers/sql-server.md#sql-server-specific-features)
- **ROWGUID columns**: `RowGuid()` modifier for replication - [SQL Server Provider](../providers/sql-server.md#sql-server-specific-features)

### Oracle Specific
- **Sequences Integration**: Advanced sequence usage with columns - [Oracle Provider](../providers/oracle.md)

## See Also
- [Data Types Reference](../operations/create-tables.md#column-types)
- [Constraints](./constraints.md) - Column constraints and validation
- [Foreign Keys](./foreign-keys.md) - Referential integrity
- [Indexes](./indexes.md) - Column indexing strategies

## Best Practices

### Data Type Selection
- Use appropriate precision for `DECIMAL` types: `AsDecimal(10, 2)` for currency
- Consider `VARCHAR` length limits across different database providers
- Use `AsAnsiString()` for non-Unicode strings when appropriate
- Choose `AsInt32()` vs `AsInt64()` based on expected data range

### Nullability Guidelines
- Make columns `NotNullable()` when business logic requires values
- Specify explicitly `Nullable()` for optional fields
- Provide appropriate `WithDefaultValue()` for NOT NULL columns
- Use `SystemMethods.CurrentDateTime` for timestamp defaults
- Consider database-specific defaults with `IfDatabase()` conditionals

### Performance Considerations
- Use `Indexed()` for frequently queried columns
- Avoid over-indexing - each index has maintenance overhead
- Consider composite indexes for multi-column queries (see [Indexes](/basics/indexes.md))
- Use appropriate string lengths to optimize storage and performance

### Cross-Database Compatibility
- Test data types across all target database providers
- Use `AsCustom()` with `IfDatabase()` for provider-specific types
- Be aware of different NULL handling across providers, especially with unique constraints
- Consider maximum identifier lengths for different databases (30 for Oracle 12, etc)
