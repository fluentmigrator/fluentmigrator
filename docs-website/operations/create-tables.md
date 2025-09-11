# Creating Tables

Creating tables is one of the most common operations in FluentMigrator. This guide covers all aspects of table creation using the fluent API.

## Basic Table Creation

### Simple Table
```csharp
[Migration(1)]
public class CreateSimpleTable : Migration
{
    public override void Up()
    {
        Create.Table("Users")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Email").AsString(255).NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Users");
    }
}
```

## Column Types

FluentMigrator supports all common column types:

### Numeric Types
```csharp
Create.Table("Products")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("Price").AsDecimal(10, 2).NotNullable()
    .WithColumn("Weight").AsDouble().Nullable()
    .WithColumn("Quantity").AsInt16().NotNullable()
    .WithColumn("LongId").AsInt64().Nullable()
    .WithColumn("Rating").AsFloat().Nullable()
    .WithColumn("IsActive").AsBoolean().NotNullable();
```

### String Types
```csharp
Create.Table("Articles")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("Title").AsString(255).NotNullable()          // VARCHAR(255)
    .WithColumn("Slug").AsAnsiString(255).NotNullable()      // VARCHAR(255) - ANSI
    .WithColumn("Content").AsString().Nullable()              // VARCHAR(MAX) or TEXT
    .WithColumn("Summary").AsString(500).Nullable()          // VARCHAR(500)
    .WithColumn("Tags").AsString(int.MaxValue).Nullable();   // TEXT/CLOB
```

### Date and Time Types
```csharp
Create.Table("Events")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("CreatedAt").AsDateTime().NotNullable()
    .WithColumn("UpdatedAt").AsDateTime().Nullable()
    .WithColumn("EventDate").AsDate().NotNullable()
    .WithColumn("EventTime").AsTime().NotNullable()
    .WithColumn("Timestamp").AsDateTime2().NotNullable()
    .WithColumn("UtcOffset").AsDateTimeOffset().Nullable();
```

### Binary Types
```csharp
Create.Table("Files")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("Data").AsBinary().Nullable()                // VARBINARY(MAX)
    .WithColumn("Thumbnail").AsBinary(1000).Nullable()       // VARBINARY(1000)
    .WithColumn("Hash").AsFixedLengthString(32).NotNullable(); // CHAR(32)
```

### Special Types
```csharp
Create.Table("Metadata")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("UniqueId").AsGuid().NotNullable()
    .WithColumn("XmlData").AsXml().Nullable()                // XML (SQL Server)
    .WithColumn("JsonData").AsString().Nullable()            // Store JSON as string
    .WithColumn("Currency").AsCurrency().NotNullable();      // MONEY
```

## Column Constraints

### Nullable and Not Nullable
```csharp
Create.Table("Users")
    .WithColumn("Id").AsInt32().NotNullable()
    .WithColumn("Name").AsString(100).NotNullable()
    .WithColumn("MiddleName").AsString(50).Nullable()    // Explicitly nullable
    .WithColumn("Email").AsString(255);                  // Nullable by default
```

### Primary Keys
```csharp
// Single column primary key with identity
Create.Table("Users")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity();

// Single column primary key without identity
Create.Table("Countries")
    .WithColumn("Code").AsString(2).NotNullable().PrimaryKey()
    .WithColumn("Name").AsString(100).NotNullable();

// Composite primary key (defined separately)
Create.Table("OrderItems")
    .WithColumn("OrderId").AsInt32().NotNullable()
    .WithColumn("ProductId").AsInt32().NotNullable()
    .WithColumn("Quantity").AsInt32().NotNullable();

Create.PrimaryKey("PK_OrderItems")
    .OnTable("OrderItems")
    .Columns("OrderId", "ProductId");
```

### Unique Constraints
```csharp
Create.Table("Users")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("Username").AsString(50).NotNullable().Unique()
    .WithColumn("Email").AsString(255).NotNullable().Unique("UQ_Users_Email");
```

### Default Values
```csharp
Create.Table("Users")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("Name").AsString(100).NotNullable()
    .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
    .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
    .WithColumn("Status").AsString(20).NotNullable().WithDefaultValue("Pending")
    .WithColumn("Priority").AsInt32().NotNullable().WithDefaultValue(1);
```

### System Methods for Defaults
```csharp
Create.Table("AuditLog")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("Timestamp").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
    .WithColumn("UserId").AsGuid().NotNullable().WithDefault(SystemMethods.NewGuid)
    .WithColumn("Action").AsString(100).NotNullable();
```

## Identity Columns

### Auto-increment Identity
```csharp
// Standard identity (starts at 1, increments by 1)
Create.Table("Users")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity();

// Custom identity seed and increment
Create.Table("Orders")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity(1000, 1);
```

## Foreign Keys

### Basic Foreign Key
```csharp
Create.Table("Orders")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("UserId").AsInt32().NotNullable()
        .ForeignKey("FK_Orders_Users", "Users", "Id")
    .WithColumn("Total").AsDecimal(10, 2).NotNullable();
```

For comprehensive foreign key management, including cascading actions, self-referencing relationships, and complex constraint scenarios, see [Foreign Keys](/basics/foreign-keys.md).

## Table-Level Constraints

### Separate Constraint Definitions
```csharp
Create.Table("Users")
    .WithColumn("Id").AsInt32().NotNullable()
    .WithColumn("Username").AsString(50).NotNullable()
    .WithColumn("Email").AsString(255).NotNullable();

// Add primary key
Create.PrimaryKey("PK_Users").OnTable("Users").Column("Id");

// Add unique constraints
Create.UniqueConstraint("UQ_Users_Username").OnTable("Users").Column("Username");
Create.UniqueConstraint("UQ_Users_Email").OnTable("Users").Column("Email");

// Add foreign key
Alter.Table("Users")
    .AddColumn("DepartmentId").AsInt32().Nullable()
    .ForeignKey("FK_Users_Departments", "Departments", "Id");
```

## Indexes

### Basic Indexes
```csharp
Create.Table("Users")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("Username").AsString(50).NotNullable()
    .WithColumn("Email").AsString(255).NotNullable();

// Basic index example
Create.Index("IX_Users_Email").OnTable("Users").OnColumn("Email");
```

For comprehensive index management, including composite indexes, unique indexes, performance optimization, and database-specific features, see [Indexes](/basics/indexes.md).

## Schema Support

### Tables in Different Schemas
```csharp
Create.Schema("hr");

Create.Table("Employees").InSchema("hr")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("Name").AsString(100).NotNullable()
    .WithColumn("DepartmentId").AsInt32().NotNullable()
        .ForeignKey("FK_Employees_Departments", "hr", "Departments", "Id");
```

## Conditional Table Creation

### Database-Specific Tables
```csharp
[Migration(1)]
public class CreateConditionalTables : Migration
{
    public override void Up()
    {
        // Common table for all databases
        Create.Table("Users")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable();

        // SQL Server specific table
        IfDatabase(ProcessorIdConstants.SqlServer)
            .Create.Table("SqlServerSpecific")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("XmlData").AsXml().Nullable();

        // PostgreSQL specific table
        IfDatabase(ProcessorIdConstants.Postgres)
            .Create.Table("PostgresSpecific")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("JsonData").AsString().Nullable();
    }

    public override void Down()
    {
        Delete.Table("Users");

        IfDatabase(ProcessorIdConstants.SqlServer)
            .Delete.Table("SqlServerSpecific");

        IfDatabase(ProcessorIdConstants.Postgres)
            .Delete.Table("PostgresSpecific");
    }
}
```

## Table Documentation

Use `WithDescription` to add documentation metadata to tables. This helps document the purpose, usage, and business context of your database tables.

```csharp
Create.Table("Users")
    .WithDescription("System users with authentication and profile information")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("Username").AsString(50).NotNullable().Unique()
    .WithColumn("Email").AsString(255).NotNullable();
```

### Database Provider Support

Table descriptions are supported differently across database providers:

| Provider   | Storage Method      | Access Method                   |
|------------|---------------------|---------------------------------|
| SQL Server | Extended Properties | `sys.fn_listextendedproperty()` |
| PostgreSQL | Table Comments      | `pg_description` catalog        |
| Oracle     | Table Comments      | `USER_TAB_COMMENTS` view        |
| MySQL      | Table Comments      | `INFORMATION_SCHEMA.TABLES`     |
| SQLite     | Not supported       | Descriptions are ignored        |

### Querying Table Descriptions

Different databases store and expose table descriptions differently:

```sql
-- SQL Server
SELECT objname, value as description
FROM fn_listextendedproperty('MS_Description', 'SCHEMA', 'dbo', 'TABLE', 'Users', NULL, NULL);

-- PostgreSQL
SELECT description
FROM pg_description
JOIN pg_class ON pg_description.objoid = pg_class.oid
WHERE relname = 'Users';

-- MySQL
SELECT table_comment as description
FROM information_schema.tables
WHERE table_name = 'Users';

-- Oracle
SELECT comments as description
FROM user_tab_comments
WHERE table_name = 'USERS';
```

## Best Practices

### 1. Always Implement Down Method
```csharp
public override void Down()
{
    Delete.Table("Users");
}
```

### 2. Use Meaningful Names
```csharp
// Good
Create.Table("UserProfiles")
    .WithColumn("UserId").AsInt32().NotNullable().PrimaryKey()
    .WithColumn("DisplayName").AsString(100).NotNullable();

// Avoid
Create.Table("tbl_usr_prof")
    .WithColumn("id").AsInt32().NotNullable().PrimaryKey()
    .WithColumn("nm").AsString(100).NotNullable();
```

### 3. Specify String Lengths
```csharp
// Good - explicit length
.WithColumn("Email").AsString(255).NotNullable()

// Avoid - unlimited length can cause issues
.WithColumn("Email").AsString().NotNullable()
```

### 4. Consider Performance
```csharp
// Add indexes for frequently queried columns
Create.Table("Orders")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("UserId").AsInt32().NotNullable()
    .WithColumn("CreatedAt").AsDateTime().NotNullable()
    .WithColumn("Status").AsString(20).NotNullable();

// Index for user's orders
Create.Index("IX_Orders_UserId").OnTable("Orders").OnColumn("UserId");

// Index for date range queries
Create.Index("IX_Orders_CreatedAt").OnTable("Orders").OnColumn("CreatedAt");
```

## Common Patterns

### Audit Columns Pattern
```csharp
Create.Table("Products")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("Name").AsString(255).NotNullable()
    .WithColumn("Price").AsDecimal(10, 2).NotNullable()
    // Audit columns
    .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
    .WithColumn("CreatedBy").AsInt32().NotNullable()
    .WithColumn("UpdatedAt").AsDateTime().Nullable()
    .WithColumn("UpdatedBy").AsInt32().Nullable()
    .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false);
```

### Lookup Table Pattern
```csharp
Create.Table("OrderStatuses")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey()
    .WithColumn("Name").AsString(50).NotNullable().Unique()
    .WithColumn("Description").AsString(255).Nullable()
    .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true);

// Insert lookup values
Insert.IntoTable("OrderStatuses")
    .Row(new { Id = 1, Name = "Pending", Description = "Order is pending processing" })
    .Row(new { Id = 2, Name = "Processing", Description = "Order is being processed" })
    .Row(new { Id = 3, Name = "Shipped", Description = "Order has been shipped" })
    .Row(new { Id = 4, Name = "Delivered", Description = "Order has been delivered" })
    .Row(new { Id = 5, Name = "Cancelled", Description = "Order has been cancelled" });
```

## Database-Specific Table Features

Different database providers offer specialized table creation features and extensions:

### SQL Server Specific
- **Identity Columns**: Advanced identity seed and increment options - [SQL Server Provider](../providers/sql-server.md#identity-columns)

### PostgreSQL Specific
- **Identity Columns**: GENERATED ALWAYS and BY DEFAULT options - [PostgreSQL Provider](../providers/postgresql.md#overriding-identity-values-extensions)

## Next Steps

- [Altering Tables](./alter-tables.md) - Learn how to modify existing tables
- [Columns](/basics/columns.md) - Deep dive into column operations
- [Indexes](/basics/indexes.md) - Advanced indexing strategies
- [Foreign Keys](/basics/foreign-keys.md) - Relationship management
