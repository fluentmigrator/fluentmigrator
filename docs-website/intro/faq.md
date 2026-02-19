# Frequently Asked Questions

This FAQ answers the most common questions about FluentMigrator. If your question isn't covered here, please [open an issue](https://github.com/fluentmigrator/fluentmigrator/issues) on GitHub.

## Migration Discovery Issues

### Why does the migration tool say "No migrations found"?

**Possible reasons:**
- Migration class isn't **public**
- Migration class doesn't inherit from `IMigration` (or `Migration` base class)
- Migration class isn't attributed with `[Migration(version)]`
- The versions of your migration tool and FluentMigrator packages are different
- Assembly isn't being scanned correctly

**Solutions:**
```csharp
// ✅ Correct migration structure
[Migration(20231201120000)]
public class AddUserTable : Migration  // Must be public and inherit from Migration
{
    public override void Up()
    {
        Create.Table("Users")
            .WithIdColumn()
            .WithColumn("Name").AsString(100).NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Users");
    }
}
```

### Why aren't my Maintenance Migrations found by the In-Process Runner?

**Solution**: Use `.For.All()` instead of `.For.Migrations()`:

```csharp
// ❌ Won't find maintenance migrations
.ConfigureRunner(rb => rb
    .AddSqlServer()
    .WithGlobalConnectionString(connectionString)
    .ScanIn(typeof(MyMigration).Assembly).For.Migrations())

// ✅ Finds all migration types
.ConfigureRunner(rb => rb
    .AddSqlServer()
    .WithGlobalConnectionString(connectionString)
    .ScanIn(typeof(MyMigration).Assembly).For.All())
```

## Assembly Loading Issues

### FileLoadException: Could not load assembly 'FluentMigrator...'

**Common scenario**: You installed FluentMigrator.DotNet.Cli globally with one version, but your assembly references a different version.

**Stack trace example:**
```
System.IO.FileLoadException: Could not load file or assembly 'FluentMigrator, Version=3.2.1.0...'
```

**Solutions:**

1. **Use local tool installation** (recommended):
```bash
# Instead of global installation
dotnet tool install FluentMigrator.DotNet.Cli
dotnet tool run dotnet-fm migrate -p sqlserver -c "..." -a "MyApp.dll"
```

2. **Allow dirty assemblies** (temporary fix):
```bash
dotnet fm migrate --allowDirtyAssemblies -p sqlserver -c "..." -a "MyApp.dll"
```

3. **Match versions exactly**:
   - Ensure FluentMigrator NuGet packages and tools have the same version

### How can I run FluentMigrator.DotNet.Cli with different .NET runtime targets?

Use the `--allowDirtyAssemblies` flag:
```bash
dotnet fm migrate --allowDirtyAssemblies -p sqlserver -c "..." -a "MyApp.dll"
```

This allows loading migration assemblies (e.g., .NET 6.0) in a different runtime context (e.g., .NET 5.0).

## Database Support

### What databases are supported?

| Database                | Identifier         | Alternative Identifiers |
|-------------------------|--------------------|-------------------------|
| **SQL Server 2022**     | `SqlServer2016`¹   | `SqlServer`             |
| **SQL Server 2019**     | `SqlServer2016`²   | `SqlServer`             |
| **SQL Server 2017**     | `SqlServer2016`³   | `SqlServer`             |
| **SQL Server 2016**     | `SqlServer2016`    | `SqlServer`             |
| **SQL Server 2014**     | `SqlServer2014`    | `SqlServer`             |
| **SQL Server 2012**     | `SqlServer2012`    | `SqlServer`             |
| **SQL Server 2008**     | `SqlServer2008`    | `SqlServer`             |
| **SQL Server 2005**     | `SqlServer2005`    | `SqlServer`             |
| **PostgreSQL**          | `Postgres`         | `PostgreSQL`            |
| **PostgreSQL 15.0**     | `PostgreSQL15_0`   | `PostgreSQL`            |
| **PostgreSQL 11.0**     | `PostgreSQL11_0`   | `PostgreSQL`            |
| **PostgreSQL 10.0**     | `PostgreSQL10_0`   | `PostgreSQL`            |
| **PostgreSQL 9.2**      | `Postgres92`       | `PostgreSQL92`          |
| **MySQL 8**             | `MySQL8`           | `MySql`, `MariaDB`      |
| **MySQL 5**             | `MySql5`           | `MySql`, `MariaDB`      |
| **MySQL 4**             | `MySql4`           | `MySql`                 |
| **Oracle**              | `Oracle`           |                         |
| **Oracle (managed)**    | `OracleManaged`    | `Oracle`                |
| **Oracle (DotConnect)** | `OracleDotConnect` | `Oracle`                |
| **SQLite**              | `Sqlite`           |                         |
| **Firebird**            | `Firebird`         |                         |
| **Amazon Redshift**     | `Redshift`         |                         |
| **SAP HANA**            | `Hana`             |                         |
| **DB2**                 | `DB2`              |                         |
| **DB2 iSeries**         | `DB2 iSeries`      | `DB2`                   |

**Notes:**
1. ¹² ³ All integration tests pass using SqlServer2016 dialect
2. SQL Server Compact Edition support dropped (end-of-life)
3. SAP SQL Anywhere support dropped (no .NET Core driver)

## Multi-Server Deployments

### How can I run migrations safely from multiple application servers?

When running multiple instances of your application (load-balanced scenarios), you need to prevent concurrent migration execution.

#### Database-Dependent Application Locking (SQL Server)

**Acquire lock before all migrations:**
```csharp
[Maintenance(MigrationStage.BeforeAll, TransactionBehavior.None)]
public class DbMigrationLockBefore : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            DECLARE @result INT
            EXEC @result = sp_getapplock 'MyApp', 'Exclusive', 'Session'

            IF @result < 0
            BEGIN
                DECLARE @msg NVARCHAR(1000) = 'Received error code ' +
                    CAST(@result AS VARCHAR(10)) + ' from sp_getapplock during migrations'
                THROW 99999, @msg, 1
            END
        ");
    }

    public override void Down()
    {
        throw new NotImplementedException("Down migrations not supported for sp_getapplock");
    }
}
```

**Release lock after all migrations:**
```csharp
[Maintenance(MigrationStage.AfterAll, TransactionBehavior.None)]
public class DbMigrationUnlockAfter : Migration
{
    public override void Up()
    {
        Execute.Sql("EXEC sp_releaseapplock 'MyApp', 'Session'");
    }

    public override void Down()
    {
        throw new NotImplementedException("Down migrations not supported for sp_releaseapplock");
    }
}
```

#### External Distributed Lock (Redis Example)

```csharp
async Task RunMigrationsWithDistributedLock(IMigrationRunner runner)
{
    var resource = "my-app-migrations";
    var expiry = TimeSpan.FromMinutes(5);

    using var redLock = await redlockFactory.CreateLockAsync(resource, expiry);

    if (redLock.IsAcquired)
    {
        runner.MigrateUp();
    }
    else
    {
        throw new InvalidOperationException("Could not acquire migration lock");
    }
}
```

## Database-Specific Issues

### SQL Server Certificate Errors

**Error**: `The certificate chain was issued by an authority that is not trusted`

Since `Microsoft.Data.SqlClient` 4.0.0, connections are encrypted by default.

**Solutions:**

1. **Disable encryption** (for development):
```
Server=.;Database=MyDb;Integrated Security=true;Encrypt=False
```

2. **Trust server certificate**:
```
Server=.;Database=MyDb;Integrated Security=true;TrustServerCertificate=True
```

3. **Fix certificate** (recommended for production):
   - Install proper SSL certificate on SQL Server

### Oracle Stored Procedure Execution

**Error**: `ORA-00900: Invalid SQL Statement`

**Problem**: Oracle requires stored procedures to be wrapped in PL/SQL blocks.

**Solution:**
```csharp
// ❌ Won't work
Execute.Sql("DBMS_UTILITY.EXEC_DDL_STATEMENT('Create Index Member_AddrId On Member(AddrId)');");

// ✅ Correct approach
Execute.Sql(@"
BEGIN
    DBMS_UTILITY.EXEC_DDL_STATEMENT('Create Index Member_AddrId On Member(AddrId)');
END;");
```

### How do I get the SQL Server database name?

**Use case**: Performing `ALTER DATABASE` operations.

::: warning Important
To ALTER DATABASE, you must switch to `master` database first, then switch back to avoid running subsequent migrations in the wrong database.
:::

```csharp
public override void Up()
{
    Execute.Sql(@"
        DECLARE @DbName sysname = DB_NAME();
        DECLARE @SqlCommand NVARCHAR(MAX) = '
USE [master];
SET DEADLOCK_PRIORITY 10;

-- Your ALTER DATABASE commands here
ALTER DATABASE [' + @DbName + '] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

-- Maintenance operations here

ALTER DATABASE [' + @DbName + '] SET MULTI_USER;
';

        EXEC(@SqlCommand);

        -- Switch back to original database
        SET @SqlCommand = 'USE [' + @DbName + ']';
        EXEC(@SqlCommand);
    ");
}
```

## SQLite-Specific Issues

### Connection Pooling and File Locks

**Issue**: SQLite keeps database file locked after migration due to connection pooling.

**Problem**: Cannot delete or move database file after migration.

**Solution**: Disable connection pooling:
```
Data Source=mydb.db;Pooling=False;
```

With pooling disabled, you can safely delete or move the database file after the FluentMigrator processor is disposed.

## Performance and Optimization

### Large Dataset Migrations

**Issue**: Migration times out on large datasets.

**Solutions:**

* **Increase timeout**:
.ConfigureGlobalProcessorOptions(opt => {
    opt.Timeout = TimeSpan.FromMinutes(30);
})
```

### Index Creation on Large Tables

**Best practice**: Create indexes `ONLINE` when possible (SQL Server):

```csharp
public override void Up()
{
    // For large tables, create indexes online to avoid blocking
    IfDatabase(ProcessorIdConstants.SqlServer)
        .Execute.Sql("CREATE INDEX IX_Users_Email ON Users(Email) WITH (ONLINE=ON)");

    // Fallback for other databases
    IfDatabase(ProcessorIdConstants.Postgres, ProcessorIdConstants.MySql)
        .Create.Index("IX_Users_Email").OnTable("Users").OnColumn("Email");
}
```

## Development and Testing

### In-Memory Testing

**Use case**: Unit testing migrations without a real database.

```csharp
[Test]
public void TestMigration()
{
    var serviceProvider = new ServiceCollection()
        .AddFluentMigratorCore()
        .ConfigureRunner(rb => rb
            .AddSQLite()
            .WithGlobalConnectionString("Data Source=:memory:")
            .ScanIn(typeof(AddUserTable).Assembly).For.Migrations())
        .AddLogging(lb => lb.AddFluentMigratorConsole())
        .BuildServiceProvider(false);

    using var scope = serviceProvider.CreateScope();
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

    runner.MigrateUp();

    // Assert migration results
    Assert.That(runner.HasMigrationsToApplyUp(), Is.False);
}
```

### Migration Rollback Testing

**Best practice**: Always test Down() methods:

```csharp
[Test]
public void TestMigrationRollback()
{
    // Migrate up
    runner.MigrateUp(20231201120000);

    // Verify up migration
    Assert.That(Schema.Table("Users").Exists(), Is.True);

    // Migrate down
    runner.MigrateDown(20231201120000);

    // Verify down migration
    Assert.That(Schema.Table("Users").Exists(), Is.False);
}
```

## Troubleshooting Tools

### Enable Verbose Logging

```csharp
.AddLogging(lb => lb
    .AddConsole()
    .SetMinimumLevel(LogLevel.Debug))
```

### Preview Migrations Without Execution

```bash
# Console runner
Migrate.exe -p sqlserver -c "..." -a "MyApp.dll" --preview

# dotnet-fm
dotnet fm migrate -p sqlserver -c "..." -a "MyApp.dll" --preview
```

### Generate SQL Scripts

```bash
# Output SQL to file without executing
dotnet fm migrate -p sqlserver -c "..." -a "MyApp.dll" --output --outputFileName "migration.sql"
```

### Validate Migrations

```bash
# Validate without connecting to database
dotnet fm validate -p sqlserver -c "..." -a "MyApp.dll" --noConnection
```

## Common Error Messages

### "Migration XYZ has already been applied"
This usually indicates:
- Migration version conflicts
- Version table corruption
- Manual database changes

**Solution**: Check the VersionInfo table and resolve conflicts.

### "Syntax error near 'GO'"
FluentMigrator doesn't support SQL batch separators like `GO`.

**Solution**: Split statements or use `Execute.Sql()` for each batch.

### "Object name 'dbo.VersionInfo' is invalid"
The version tracking table hasn't been created.

**Solution**: Ensure the database user has CREATE TABLE permissions.

## Edge Cases and Advanced Troubleshooting

This section covers common edge cases you might encounter when using FluentMigrator and how to handle them effectively.

### Migration Version Conflicts

#### Problem: Duplicate Migration Versions
```csharp
[Migration(20240101120000)]
public class CreateUsersTable : Migration { /* ... */ }

[Migration(20240101120000)] // Same version!
public class CreateProductsTable : Migration { /* ... */ }
```

**Error**: FluentMigrator will throw an exception about duplicate migration versions.

**Solution**: Use unique version numbers, preferably timestamps:
```csharp
[Migration(20240101120000)]
public class CreateUsersTable : Migration { /* ... */ }

[Migration(20240101120100)] // Different timestamp
public class CreateProductsTable : Migration { /* ... */ }
```

#### Problem: Out-of-Order Migration Versions
```csharp
// Already applied: 20240101120000, 20240101130000
[Migration(20240101125000)] // This is between already applied migrations
public class LateAddition : Migration { /* ... */ }
```

**Solution**: FluentMigrator will still apply this migration, but it's better to use a newer timestamp:
```csharp
[Migration(20240101140000)] // Use a timestamp after the last applied migration
public class LateAddition : Migration { /* ... */ }
```

### Schema and Table Issues

#### Problem: Column Dependencies
```csharp
public override void Down()
{
    // This will fail if there are constraints referencing this column
    Delete.Column("UserId").FromTable("Orders");
}
```

**Solution**: Drop dependent constraints first:
```csharp
public override void Down()
{
    Delete.ForeignKey("FK_Orders_Users").OnTable("Orders");
    Delete.Column("UserId").FromTable("Orders");
}
```

#### Problem: Default Constraints (SQL Server)
```csharp
public override void Down()
{
    // This may fail if there's a default constraint
    Delete.Column("IsActive").FromTable("Users");
}
```

**Solution**: Drop the default constraint first:
```csharp
public override void Down()
{
    Delete.DefaultConstraint().OnTable("Users").OnColumn("IsActive");
    Delete.Column("IsActive").FromTable("Users");
}
```

### Data Migration Issues

#### Problem: Large Dataset Migrations
```csharp
// This can cause timeouts or memory issues
Execute.Sql("UPDATE Users SET Status = 'Active' WHERE Status IS NULL");
```

**Solution**: Use batched operations:
```csharp
[Migration(1)]
public class BatchedUpdate : Migration
{
    public override void Up()
    {
        // SQL Server batch update
        Execute.Sql(@"
            WHILE @@ROWCOUNT > 0
            BEGIN
                UPDATE TOP (1000) Users
                SET Status = 'Active'
                WHERE Status IS NULL
            END");
    }

    public override void Down()
    {
        Execute.Sql(@"
            WHILE @@ROWCOUNT > 0
            BEGIN
                UPDATE TOP (1000) Users
                SET Status = NULL
                WHERE Status = 'Active'
            END");
    }
}
```

#### Problem: Data Loss in Down Migrations
```csharp
public override void Up()
{
    Alter.Table("Users").AddColumn("NewField").AsString(100).Nullable();
    Execute.Sql("UPDATE Users SET NewField = 'Default Value'");
}

public override void Down()
{
    // This loses all data in NewField!
    Delete.Column("NewField").FromTable("Users");
}
```

**Solution**: Consider data preservation strategies:
```csharp
public override void Down()
{
    // Option 1: Warn about data loss
    Execute.Sql("-- WARNING: This will lose data in NewField column");
    Delete.Column("NewField").FromTable("Users");

    // Option 2: Create backup table (if feasible)
    // Execute.Sql("SELECT * INTO Users_NewField_Backup FROM Users WHERE NewField IS NOT NULL");
    // Delete.Column("NewField").FromTable("Users");
}
```

### Cross-Database Compatibility Issues

#### Problem: Database-Specific SQL in Migrations
```csharp
public override void Up()
{
    Execute.Sql("SELECT TOP 10 * FROM Users"); // SQL Server specific
}
```

**Solution**: Use conditional logic:
```csharp
public override void Up()
{
    IfDatabase(ProcessorIdConstants.SqlServer)
        .Execute.Sql("SELECT TOP 10 * FROM Users");

    IfDatabase(ProcessorIdConstants.Postgres, ProcessorIdConstants.MySql)
        .Execute.Sql("SELECT * FROM Users LIMIT 10");
}
```

### Circular Dependencies

#### Problem: Foreign Key Circular References
```csharp
// This creates a circular dependency
Create.Table("Users")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("ManagerId").AsInt32().Nullable()
        .ForeignKey("FK_Users_Manager", "Users", "Id");

Create.Table("Departments")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("ManagerId").AsInt32().NotNullable()
        .ForeignKey("FK_Departments_Users", "Users", "Id");

Alter.Table("Users")
    .AddColumn("DepartmentId").AsInt32().Nullable()
        .ForeignKey("FK_Users_Departments", "Departments", "Id");
```

**Solution**: Create tables without foreign keys first, then add constraints:
```csharp
public override void Up()
{
    // Create tables without foreign keys
    Create.Table("Users")
        .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
        .WithColumn("ManagerId").AsInt32().Nullable()
        .WithColumn("DepartmentId").AsInt32().Nullable();

    Create.Table("Departments")
        .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
        .WithColumn("ManagerId").AsInt32().NotNullable();

    // Add foreign keys after tables exist
    Create.ForeignKey("FK_Users_Manager")
        .FromTable("Users").ForeignColumn("ManagerId")
        .ToTable("Users").PrimaryColumn("Id");

    Create.ForeignKey("FK_Departments_Users")
        .FromTable("Departments").ForeignColumn("ManagerId")
        .ToTable("Users").PrimaryColumn("Id");

    Create.ForeignKey("FK_Users_Departments")
        .FromTable("Users").ForeignColumn("DepartmentId")
        .ToTable("Departments").PrimaryColumn("Id");
}
```

### Identity Column Edge Cases

#### Problem: Identity Insert Issues
```csharp
public override void Up()
{
    Create.Table("Users")
        .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
        .WithColumn("Username").AsString(50).NotNullable();

    // This will fail because of IDENTITY column
    Insert.IntoTable("Users").Row(new { Id = 1, Username = "admin" });
}
```

**Solution**: Handle identity inserts properly:
```csharp
public override void Up()
{
    Create.Table("Users")
        .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
        .WithColumn("Username").AsString(50).NotNullable();

    IfDatabase(ProcessorIdConstants.SqlServer).Execute.Sql("SET IDENTITY_INSERT Users ON");
    Insert.IntoTable("Users").Row(new { Id = 1, Username = "admin" });
    IfDatabase(ProcessorIdConstants.SqlServer).Execute.Sql("SET IDENTITY_INSERT Users OFF");
}
```

### Transaction and Concurrency Issues

#### Problem: Migrations in Transactions
Some database operations can't be performed within transactions:
```csharp
// This might fail in some databases
public override void Up()
{
    Execute.Sql("CREATE INDEX CONCURRENTLY IX_Users_Email ON Users (Email)"); // PostgreSQL
}
```

**Solution**: Check if your database supports the operation within transactions:
```csharp
public override void Up()
{
    IfDatabase(ProcessorIdConstants.Postgres)
        .Execute.Sql("CREATE INDEX CONCURRENTLY IX_Users_Email ON Users (Email)");

    IfDatabase(ProcessorIdConstants.SqlServer)
        .Create.Index("IX_Users_Email").OnTable("Users").OnColumn("Email");
}
```

## Getting Help

If you encounter issues not covered in this FAQ:

1. **Search existing issues**: [GitHub Issues](https://github.com/fluentmigrator/fluentmigrator/issues)
2. **Check discussions**: [GitHub Discussions](https://github.com/fluentmigrator/fluentmigrator/discussions)
3. **Ask on Stack Overflow**: Use the `fluentmigrator` tag
4. **Create a new issue**: Include:
   - FluentMigrator version
   - Database provider and version
   - Complete error message and stack trace
   - Minimal reproducible example
