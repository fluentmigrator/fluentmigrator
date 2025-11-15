# Raw SQL Helper

FluentMigrator provides a RawSql helper class that allows you to embed SQL expressions within structured operations like Insert and Update.
Starting with [version 7.0.0](https://github.com/fluentmigrator/fluentmigrator/releases/tag/v7.0.0), RawSql.Insert can also be used in SET and WHERE clauses for Update and Delete operations.

## RawSql Helper for Insert Operations

### Basic RawSql.Insert Usage

When you need to use database functions in insert operations, `RawSql.Insert()` or `new RawSql()` allows you to embed SQL expressions:

```csharp
// Insert with database functions
Insert.IntoTable("AuditLog").Row(new
{
    Action = "UserLogin",
    Timestamp = RawSql.Insert("GETUTCDATE()"),     // SQL Server
    Username = RawSql.Insert("SUSER_SNAME()"),     // Current database user
    SessionId = RawSql.Insert("@@SPID")            // Session ID
});
```

### Database-Specific Functions

#### SQL Server Functions
```csharp
Insert.IntoTable("Events").Row(new
{
    EventId = RawSql.Insert("NEWID()"),            // UUID generation
    EventTime = RawSql.Insert("GETUTCDATE()"),     // UTC timestamp
    MachineName = RawSql.Insert("HOST_NAME()"),    // Machine name
    DatabaseName = RawSql.Insert("DB_NAME()"),     // Current database
    ProcessId = RawSql.Insert("@@SPID")            // Session/Process ID
});
```

#### PostgreSQL Functions
```csharp
Insert.IntoTable("Events").Row(new
{
    EventId = RawSql.Insert("gen_random_uuid()"),  // UUID generation
    EventTime = RawSql.Insert("NOW()"),            // Current timestamp
    Username = RawSql.Insert("current_user"),      // Current user
    DatabaseName = RawSql.Insert("current_database()"), // Current database
    ProcessId = RawSql.Insert("pg_backend_pid()")  // Process ID
});
```

#### MySQL Functions
```csharp
Insert.IntoTable("Events").Row(new
{
    EventId = RawSql.Insert("UUID()"),             // UUID generation
    EventTime = RawSql.Insert("UTC_TIMESTAMP()"),  // UTC timestamp
    Username = RawSql.Insert("USER()"),            // Current user
    DatabaseName = RawSql.Insert("DATABASE()"),    // Current database
    ProcessId = RawSql.Insert("CONNECTION_ID()")   // Connection ID
});
```

### Cross-Database Insert Examples

```csharp
public override void Up()
{
    // Create audit table first
    Create.Table("AuditLog")
        .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
        .WithColumn("Action").AsString(100).NotNullable()
        .WithColumn("Timestamp").AsDateTime().NotNullable()
        .WithColumn("Username").AsString(100).Nullable();

    // Insert initial record with database-specific functions
    IfDatabase(ProcessorIdConstants.SqlServer).Insert.IntoTable("SystemEvents").Row(new
    {
        EventTime = RawSql.Insert("GETUTCDATE()"),
        Username = RawSql.Insert("SUSER_SNAME()"),
        EventType = "SystemInitialized"
    });

    IfDatabase(ProcessorIdConstants.Postgres).Insert.IntoTable("SystemEvents").Row(new
    {
        EventTime = RawSql.Insert("NOW()"),
        Username = RawSql.Insert("current_user"),
        EventType = "SystemInitialized"
    });

    IfDatabase(ProcessorIdConstants.MySql).Insert.IntoTable("SystemEvents").Row(new
    {
        EventTime = RawSql.Insert("UTC_TIMESTAMP()"),
        Username = RawSql.Insert("USER()"),
        EventType = "SystemInitialized"
    });
}
```

## RawSql in Update and Delete Operations (v7.0.0+)

Starting with [FluentMigrator v7.0.0](https://github.com/fluentmigrator/fluentmigrator/releases/tag/v7.0.0), `RawSql.Insert()` can be used in SET and WHERE clauses for Update and Delete operations.

### Update Operations with RawSql

#### Using RawSql in SET clauses

```csharp
public override void Up()
{
    // Update with database functions in SET clause
    Update.Table("Users")
        .Set(new {
            LastLoginTime = RawSql.Insert("GETUTCDATE()"),
            LoginCount = RawSql.Insert("LoginCount + 1")
        })
        .Where(new { Username = "admin" });

    // Cross-database timestamp updates
    IfDatabase(ProcessorIdConstants.SqlServer)
        .Update.Table("Users")
        .Set(new { ModifiedAt = RawSql.Insert("GETUTCDATE()") })
        .Where(new { IsActive = true });

    IfDatabase(ProcessorIdConstants.Postgres)
        .Update.Table("Users")
        .Set(new { ModifiedAt = RawSql.Insert("NOW()") })
        .Where(new { IsActive = true });

    IfDatabase(ProcessorIdConstants.MySql)
        .Update.Table("Users")
        .Set(new { ModifiedAt = RawSql.Insert("UTC_TIMESTAMP()") })
        .Where(new { IsActive = true });
}
```

#### Using RawSql in WHERE clauses

```csharp
public override void Up()
{
    // Update records where date is older than 30 days
    IfDatabase(ProcessorIdConstants.SqlServer)
        .Update.Table("TempData")
        .Set(new { IsArchived = true })
        .Where(new { CreatedAt = RawSql.Insert("< DATEADD(day, -30, GETDATE())") });

    IfDatabase(ProcessorIdConstants.Postgres)
        .Update.Table("TempData")
        .Set(new { IsArchived = true })
        .Where(new { CreatedAt = RawSql.Insert("< NOW() - INTERVAL '30 days'") });

    // Update with complex conditions
    Update.Table("Orders")
        .Set(new { Status = "Expired" })
        .Where(new {
            Status = "Pending",
            CreatedAt = RawSql.Insert("< DATEADD(hour, -24, GETDATE())")
        });
}
```

### Delete Operations with RawSql

#### Using RawSql in WHERE clauses for Delete

```csharp
public override void Up()
{
    // Delete old log entries using database functions
    IfDatabase(ProcessorIdConstants.SqlServer)
        .Delete.FromTable("AuditLog")
        .Where(new { CreatedAt = RawSql.Insert("< DATEADD(month, -6, GETDATE())") });

    IfDatabase(ProcessorIdConstants.Postgres)
        .Delete.FromTable("AuditLog")
        .Where(new { CreatedAt = RawSql.Insert("< NOW() - INTERVAL '6 months'") });

    IfDatabase(ProcessorIdConstants.MySql)
        .Delete.FromTable("AuditLog")
        .Where(new { CreatedAt = RawSql.Insert("< DATE_SUB(NOW(), INTERVAL 6 MONTH)") });
}
```

#### Complex Delete Conditions

```csharp
public override void Up()
{
    // Delete with multiple RawSql conditions
    Delete.FromTable("Sessions")
        .Where(new {
            IsActive = false,
            LastAccessed = RawSql.Insert("< DATEADD(hour, -2, GETDATE())"),
            LoginAttempts = RawSql.Insert("> 5")
        });

    // Delete with subquery conditions
    IfDatabase(ProcessorIdConstants.SqlServer)
        .Delete.FromTable("UserPreferences")
        .Where(new {
            UserId = RawSql.Insert("IN (SELECT Id FROM Users WHERE IsDeleted = 1)")
        });
}
```

## Advanced RawSql Patterns

### Combining RawSql with Conditional Logic

```csharp
[Migration(1)]
public class AdvancedRawSqlMigration : Migration
{
    public override void Up()
    {
        // Insert default admin user with appropriate timestamps
        IfDatabase(ProcessorIdConstants.SqlServer).Delegate(() =>
        {
            Insert.IntoTable("Users").Row(new
            {
                Username = "admin",
                Email = "admin@company.com",
                CreatedAt = RawSql.Insert("GETUTCDATE()"),
                PasswordHash = RawSql.Insert("HASHBYTES('SHA2_256', 'TempPassword123')"),
                IsActive = true
            });
        });

        IfDatabase(ProcessorIdConstants.Postgres).Delegate(() =>
        {
            Insert.IntoTable("Users").Row(new
            {
                Username = "admin",
                Email = "admin@company.com",
                CreatedAt = RawSql.Insert("NOW()"),
                PasswordHash = RawSql.Insert("encode(digest('TempPassword123', 'sha256'), 'hex')"),
                IsActive = true
            });
        });
    }

    public override void Down()
    {
        Delete.FromTable("Users").Where(new { Username = "admin" });
    }
}
```

### Data Transformation with RawSql

```csharp
public override void Up()
{
    // Transform existing data using database functions
    Update.Table("Products")
        .Set(new {
            Slug = RawSql.Insert("LOWER(REPLACE(REPLACE(Name, ' ', '-'), '&', 'and'))"),
            UpdatedAt = RawSql.Insert("GETUTCDATE()")
        })
        .Where(new { Slug = RawSql.Insert("IS NULL") });

    // Calculate derived values
    Update.Table("Orders")
        .Set(new {
            TotalAmount = RawSql.Insert("Quantity * UnitPrice"),
            Tax = RawSql.Insert("(Quantity * UnitPrice) * 0.08")
        })
        .Where(new {
            TotalAmount = RawSql.Insert("IS NULL"),
            Status = "Pending"
        });
}
```

## Best Practices and Limitations

### When to Use RawSql

**✅ Good uses:**
- Database function calls (timestamps, GUIDs, user functions)
- Mathematical calculations
- String transformations
- Date/time operations
- Conditional expressions that can't be expressed in the fluent API

**❌ Avoid for:**
- Complex logic (use Execute.Sql instead)
- Multi-statement operations
- Operations that vary significantly between databases without conditional wrapping

### Cross-Database Compatibility

```csharp
public override void Up()
{
    // Handle database differences explicitly
    IfDatabase(ProcessorIdConstants.SqlServer)
        .Insert.IntoTable("Events").Row(new
        {
            Id = RawSql.Insert("NEWID()"),
            CreatedAt = RawSql.Insert("GETUTCDATE()")
        });

    IfDatabase(ProcessorIdConstants.Postgres)
        .Insert.IntoTable("Events").Row(new
        {
            Id = RawSql.Insert("gen_random_uuid()"),
            CreatedAt = RawSql.Insert("NOW()")
        });

    IfDatabase(ProcessorIdConstants.MySql)
        .Insert.IntoTable("Events").Row(new
        {
            Id = RawSql.Insert("UUID()"),
            CreatedAt = RawSql.Insert("UTC_TIMESTAMP()")
        });
}
```

### Performance Considerations

```csharp
public override void Up()
{
    // ✅ Good: Efficient bulk operations
    Update.Table("Users")
        .Set(new {
            LastLoginTime = RawSql.Insert("GETUTCDATE()"),
            IsOnline = true
        })
        .Where(new { Status = "LoggedIn" });

    // ❌ Less efficient: Multiple individual updates
    // (Use Execute.Sql with loops for this pattern instead)
}
```
