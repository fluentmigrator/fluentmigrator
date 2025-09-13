# SQLite

SQLite is a lightweight, file-based database engine that's perfect for development, testing, and applications requiring an embedded database. FluentMigrator provides comprehensive SQLite support with considerations for its unique characteristics and limitations.

## Getting Started with SQLite

### Installation

Install the SQLite provider package:

```bash
# For .NET CLI
dotnet add package FluentMigrator.Runner.SQLite

# For Package Manager Console
Install-Package FluentMigrator.Runner.SQLite
```

### Basic Configuration

```csharp
services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddSQLite()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());
```


## SQLite Data Types and Limitations

### SQLite Type System

Column types are specified in the [DBMS specific type map clashttps://github.com/fluentmigrator/fluentmigrator/blob/main/src/FluentMigrator.Runner.SQLite/Generators/SQLite/SQLiteTypeMap.cs).

### Understanding SQLite Limitations

```csharp
public class SQLiteLimitations : Migration
{
    public override void Up()
    {
        // SQLite limitations to be aware of:

        // 1. No ALTER COLUMN support - must recreate table
        Create.Table("LimitationExample")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(50).NotNullable()
            .WithColumn("Value").AsInt32().NotNullable();

        // 2. No DROP COLUMN support (pre SQLite 3.35.0)
        // This will work in FluentMigrator by recreating the table behind the scenes

        // 3. Limited foreign key constraint enforcement
        // Foreign keys must be enabled with PRAGMA foreign_keys = ON

        // 4. No RIGHT JOIN or FULL OUTER JOIN

        // 5. No stored procedures or user-defined functions (in standard SQLite)

        // 6. Limited ALTER TABLE support
        // Only ADD COLUMN and RENAME TABLE are supported natively

        Insert.IntoTable("LimitationExample")
            .Row(new { Name = "Example", Value = 100 });
    }

    public override void Down()
    {
        Delete.Table("LimitationExample");
    }
}
```

## Working Around SQLite Limitations

### Column Modifications (Recreate Table Pattern)

```csharp
public class SQLiteColumnModification : Migration
{
    public override void Up()
    {
        // Create initial table
        Create.Table("Users")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(50).NotNullable()
            .WithColumn("Email").AsString(100).NotNullable()
            .WithColumn("Age").AsInt32().NotNullable();

        Insert.IntoTable("Users")
            .Row(new { Name = "John Doe", Email = "john@example.com", Age = 30 })
            .Row(new { Name = "Jane Smith", Email = "jane@example.com", Age = 25 });
    }

    public override void Down()
    {
        Delete.Table("Users");
    }
}

public class SQLiteAlterColumn : Migration
{
    public override void Up()
    {
        // FluentMigrator handles this automatically by recreating the table
        // but you can also do it manually for better control

        // Method 1: Let FluentMigrator handle it (recommended)
        Alter.Column("Email").OnTable("Users")
            .AsString(255).NotNullable(); // Change from 100 to 255

        // Method 2: Manual table recreation (for complex scenarios)
        /*
        // Step 1: Create new table with desired structure
        Create.Table("Users_New")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(50).NotNullable()
            .WithColumn("Email").AsString(255).NotNullable() // New size
            .WithColumn("Age").AsInt32().NotNullable()
            .WithColumn("Status").AsString(20).NotNullable().WithDefaultValue("Active"); // New column

        // Step 2: Copy data
        Execute.Sql(@"
            INSERT INTO Users_New (Id, Name, Email, Age, Status)
            SELECT Id, Name, Email, Age, 'Active' FROM Users");

        // Step 3: Drop old table and rename new table
        Delete.Table("Users");
        Rename.Table("Users_New").To("Users");
        */
    }

    public override void Down()
    {
        Alter.Column("Email").OnTable("Users")
            .AsString(100).NotNullable();
    }
}
```

### Adding and Dropping Columns

```csharp
public class SQLiteAddDropColumns : Migration
{
    public override void Up()
    {
        // Adding columns is supported natively
        Alter.Table("Users")
            .AddColumn("CreatedAt").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime)
            .AddColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
            .AddColumn("LastLoginAt").AsDateTime().Nullable();

        // Update existing records
        Execute.Sql("UPDATE Users SET CreatedAt = datetime('now') WHERE CreatedAt IS NULL");

        // Dropping columns - FluentMigrator will recreate the table
        // Delete.Column("Age").FromTable("Users"); // This works but recreates table
    }

    public override void Down()
    {
        Delete.Column("LastLoginAt").FromTable("Users");
        Delete.Column("IsActive").FromTable("Users");
        Delete.Column("CreatedAt").FromTable("Users");
    }
}
```

## SQLite-Specific Features

### Working with SQLite Pragmas

```csharp
public class SQLitePragmas : Migration
{
    public override void Up()
    {
        // Enable foreign key constraints
        Execute.Sql("PRAGMA foreign_keys = ON");

        // Set journal mode for better concurrency
        Execute.Sql("PRAGMA journal_mode = WAL");

        // Set synchronous mode for better performance
        Execute.Sql("PRAGMA synchronous = NORMAL");

        // Set cache size (in pages, negative for KB)
        Execute.Sql("PRAGMA cache_size = -64000"); // 64MB cache

        // Enable recursive triggers
        Execute.Sql("PRAGMA recursive_triggers = ON");

        // Set temp store to memory for better performance
        Execute.Sql("PRAGMA temp_store = MEMORY");

        // Create tables after setting pragmas
        Create.Table("Orders")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("CustomerId").AsInt32().NotNullable()
            .WithColumn("OrderDate").AsDateTime().NotNullable()
            .WithColumn("TotalAmount").AsDecimal(10, 2).NotNullable();

        Create.Table("OrderItems")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("OrderId").AsInt32().NotNullable()
            .WithColumn("ProductName").AsString(100).NotNullable()
            .WithColumn("Quantity").AsInt32().NotNullable()
            .WithColumn("UnitPrice").AsDecimal(10, 2).NotNullable();

        // Foreign key will be enforced because we enabled foreign_keys pragma
        Create.ForeignKey("FK_OrderItems_Orders")
            .FromTable("OrderItems").ForeignColumn("OrderId")
            .ToTable("Orders").PrimaryColumn("Id")
            .OnDelete(Rule.Cascade);
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_OrderItems_Orders").OnTable("OrderItems");
        Delete.Table("OrderItems");
        Delete.Table("Orders");
    }
}
```

## Troubleshooting Common SQLite Issues

### Common Problems and Solutions

```csharp
public class SQLiteTroubleshooting : Migration
{
    public override void Up()
    {
        // Problem 1: Database locked errors
        Execute.Sql("PRAGMA busy_timeout = 30000"); // 30 seconds timeout
        Execute.Sql("PRAGMA journal_mode = WAL");   // Reduces locking issues

        // Problem 2: Foreign key constraint violations
        Execute.Sql("PRAGMA foreign_keys = ON");    // Enable FK checking

        Create.Table("TroubleshootingParent")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable();

        Create.Table("TroubleshootingChild")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("ParentId").AsInt32().NotNullable()
            .WithColumn("ChildName").AsString(100).NotNullable();

        // Insert parent record first to avoid FK violations
        Insert.IntoTable("TroubleshootingParent")
            .Row(new { Name = "Parent 1" });

        // Problem 3: Data type mismatches
        // SQLite is flexible but can cause issues - use proper types
        Insert.IntoTable("TroubleshootingChild")
            .Row(new { ParentId = 1, ChildName = "Child 1" }); // Correct: integer

        // Create FK after data exists
        Create.ForeignKey("FK_TroubleshootingChild_Parent")
            .FromTable("TroubleshootingChild").ForeignColumn("ParentId")
            .ToTable("TroubleshootingParent").PrimaryColumn("Id")
            .OnDelete(Rule.Cascade);

        // Problem 4: Performance issues with large datasets
        // Solution: Proper indexing and query optimization
        Create.Index("IX_TroubleshootingChild_ParentId")
            .OnTable("TroubleshootingChild")
            .OnColumn("ParentId");

        // Problem 5: Backup corruption
        // Solution: Regular integrity checks
        Execute.Sql(@"
            -- Regular maintenance queries:

            -- Check database integrity
            -- PRAGMA integrity_check;

            -- Quick check (faster)
            -- PRAGMA quick_check;

            -- Check foreign key constraints
            -- PRAGMA foreign_key_check;

            -- Optimize database
            -- VACUUM;

            -- Update statistics
            -- ANALYZE;
            ");
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_TroubleshootingChild_Parent").OnTable("TroubleshootingChild");
        Delete.Table("TroubleshootingChild");
        Delete.Table("TroubleshootingParent");
    }
}
```

## SQLite Best Practices

### Use Appropriate Data Types
SQLite has flexible typing, but be explicit for better performance:

```csharp
// Good - explicit types
Create.Table("OptimalUsers")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("Username").AsString(50).NotNullable()
    .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);
```

### Consider WAL Mode for Better Concurrency
```csharp
[Migration(1)]
public class EnableWalMode : Migration
{
    public override void Up()
    {
        Execute.Sql("PRAGMA journal_mode = WAL;");
        Execute.Sql("PRAGMA synchronous = NORMAL;");
    }

    public override void Down()
    {
        Execute.Sql("PRAGMA journal_mode = DELETE;");
        Execute.Sql("PRAGMA synchronous = FULL;");
    }
}
```
