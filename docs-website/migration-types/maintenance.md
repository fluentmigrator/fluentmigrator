# Maintenance Migrations

Maintenance migrations are special migrations that run at specific stages during the migration process, regardless of the target version. Unlike regular migrations that run once based on version tracking, maintenance migrations execute every time migrations are run, but only at their designated stage.

::: info Key Characteristics
**Maintenance migrations**:
- Run at specific **lifecycle stages** during migration execution
- Execute **every time** migrations run (not version-tracked)
- Use the `[Maintenance(MigrationStage)]` attribute
- Can have custom transaction behavior
- Are perfect for **database maintenance**, **cleanup tasks**, and **validation**
:::

## Migration Stages

Maintenance migrations use the `MigrationStage` enum to determine when they execute:

| Stage            | Description                                    | Use Cases                                                |
|------------------|------------------------------------------------|----------------------------------------------------------|
| `BeforeAll`      | Before any standard migrations                 | Database setup, connection validation, permissions check |
| `BeforeEach`     | Before each standard migration                 | Backup creation, pre-migration validation                |
| `AfterEach`      | After each standard migration                  | Cleanup, index optimization, statistics update           |
| `BeforeProfiles` | After all standard migrations, before profiles | Data validation, constraint checks                       |
| `AfterAll`       | After all standard migrations and profiles     | Final cleanup, reporting, post-migration tasks           |

## Basic Maintenance Migration

```csharp
[Maintenance(MigrationStage.AfterAll)]
public class DatabaseCleanup : Migration
{
    public override void Up()
    {
        // Clean up temporary tables
        Execute.Sql("DROP TABLE IF EXISTS #TempMigrationData");

        // Update statistics
        Execute.Sql("UPDATE STATISTICS");

        // Log completion
        Insert.IntoTable("MigrationLog").Row(new
        {
            Timestamp = DateTime.UtcNow,
            Action = "Database cleanup completed"
        });
    }

    public override void Down()
    {
        // Maintenance migrations typically don't implement Down()
        // as they perform maintenance tasks rather than schema changes
    }
}
```

## Stage-Specific Examples

### BeforeAll - Initial Setup

```csharp
[Maintenance(MigrationStage.BeforeAll)]
public class DatabaseInitialization : Migration
{
    public override void Up()
    {
        // Ensure required extensions are available
        Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\"");

        // Create audit schema if it doesn't exist
        Create.Schema("audit").IfNotExists();

        // Set up migration logging
        if (!Schema.Table("MigrationLog").Exists())
        {
            Create.Table("MigrationLog")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Timestamp").AsDateTime().NotNullable()
                .WithColumn("Action").AsString(500).NotNullable();
        }
    }

    public override void Down() { }
}
```

### BeforeEach - Pre-Migration Tasks

```csharp
[Maintenance(MigrationStage.BeforeEach)]
public class PreMigrationBackup : Migration
{
    public override void Up()
    {
        // Create backup point
        Execute.Sql(@"
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MigrationBackups')
            BEGIN
                CREATE TABLE MigrationBackups (
                    Id int IDENTITY(1,1) PRIMARY KEY,
                    BackupTime datetime NOT NULL,
                    MigrationVersion bigint NOT NULL
                )
            END");

        // Log the backup
        Insert.IntoTable("MigrationBackups").Row(new
        {
            BackupTime = DateTime.UtcNow,
            MigrationVersion = 0  // Would need context to get actual version
        });
    }

    public override void Down() { }
}
```

### AfterEach - Post-Migration Cleanup

```csharp
[Maintenance(MigrationStage.AfterEach)]
public class PostMigrationOptimization : Migration
{
    public override void Up()
    {
        // Rebuild indexes for optimal performance
        Execute.Sql("EXEC sp_MSforeachtable 'DBCC DBREINDEX(''?'')'");

        // Update table statistics
        Execute.Sql("EXEC sp_updatestats");

        // Clear temporary migration data
        Execute.Sql("DELETE FROM TempMigrationData WHERE Created < DATEADD(hour, -1, GETDATE())");
    }

    public override void Down() { }
}
```

### BeforeProfiles - Pre-Profile Validation

```csharp
[Maintenance(MigrationStage.BeforeProfiles)]
public class PreProfileValidation : Migration
{
    public override void Up()
    {
        // Validate required tables exist before running profiles
        var tables = new[] { "Users", "Roles", "Permissions" };

        foreach (var table in tables)
        {
            if (!Schema.Table(table).Exists())
            {
                throw new Exception($"Required table '{table}' does not exist before profile execution");
            }
        }

        // Ensure required data exists
        Execute.Sql(@"
            IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Admin')
            BEGIN
                INSERT INTO Roles (Name, Description) VALUES ('Admin', 'System Administrator')
            END");
    }

    public override void Down() { }
}
```

### AfterAll - Final Tasks

```csharp
[Maintenance(MigrationStage.AfterAll)]
public class FinalDatabaseMaintenance : Migration
{
    public override void Up()
    {
        // Final index optimization
        Execute.Sql("REINDEX INDEX ALL");

        // Update database statistics
        Execute.Sql("ANALYZE");

        // Clean up migration artifacts
        Execute.Sql("DROP TABLE IF EXISTS __MigrationHistory_Temp");

        // Record migration completion
        Insert.IntoTable("SystemEvents").Row(new
        {
            EventType = "MigrationCompleted",
            Timestamp = DateTime.UtcNow,
            Details = "All migrations and maintenance tasks completed successfully"
        });
    }

    public override void Down() { }
}
```

## Custom Transaction Behavior

Maintenance migrations support custom transaction behavior:

```csharp
[Maintenance(MigrationStage.AfterAll, TransactionBehavior.None)]
public class NonTransactionalMaintenance : Migration
{
    public override void Up()
    {
        // Operations that can't run in a transaction
        Execute.Sql("BACKUP DATABASE MyApp TO DISK = 'C:\\Backups\\MyApp.bak'");
        Execute.Sql("DBCC SHRINKDATABASE(MyApp)");
    }

    public override void Down() { }
}
```

## Working with Tags

Maintenance migrations can be combined with tags for conditional execution:

```csharp
[Maintenance(MigrationStage.AfterAll)]
[Tags("Production")]
public class ProductionOnlyMaintenance : Migration
{
    public override void Up()
    {
        // Production-specific maintenance tasks
        Execute.Sql("EXEC sp_configure 'cost threshold for parallelism', 50");
        Execute.Sql("RECONFIGURE");

        // Enable query store in production
        Execute.Sql("ALTER DATABASE CURRENT SET QUERY_STORE = ON");
    }

    public override void Down() { }
}
```

## Command-Line Usage

Maintenance migrations run automatically with regular migrations, but you can control their execution:

### Console Tool (Migrate.exe)
```bash
# Run with maintenance migrations (default)
Migrate.exe -db SqlServer -conn "connection_string" -assembly "MyMigrations.dll"

# Disable maintenance migrations
Migrate.exe -db SqlServer -conn "connection_string" -assembly "MyMigrations.dll" --include ma-
```

### dotnet-fm CLI
```bash
# Run with maintenance migrations (default)
dotnet fm migrate -p SqlServer -c "connection_string" -a "MyMigrations.dll"

# Control maintenance migration inclusion
dotnet fm migrate -p SqlServer -c "connection_string" -a "MyMigrations.dll" --include-maintenance false
```

### In-Process Runner
```csharp
var serviceProvider = CreateServices()
    .ConfigureRunner(rb => rb
        .AddSqlServer()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations())
    .Configure<RunnerOptions>(opt => {
        opt.IncludeMaintenanceMigrations = true;  // Default is true
    })
    .BuildServiceProvider(false);

using (var scope = serviceProvider.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();  // Includes maintenance migrations
}
```

## Best Practices

### 1. Use Appropriate Stages
- **BeforeAll**: Database setup, extension creation, schema creation
- **BeforeEach**: Backups, validation, logging
- **AfterEach**: Cleanup, optimization, statistics
- **BeforeProfiles**: Data validation, required data setup
- **AfterAll**: Final cleanup, reporting, database maintenance

### 2. Keep Operations Idempotent
```csharp
[Maintenance(MigrationStage.BeforeAll)]
public class IdempotentSetup : Migration
{
    public override void Up()
    {
        // Always safe to run multiple times
        Execute.Sql("CREATE SCHEMA IF NOT EXISTS audit");

        if (!Schema.Table("SystemConfig").Exists())
        {
            Create.Table("SystemConfig")
                .WithColumn("Key").AsString(100).PrimaryKey()
                .WithColumn("Value").AsString(500).NotNullable();
        }
    }

    public override void Down() { }
}
```

### 3. Use Tags for Environment-Specific Maintenance
```csharp
[Maintenance(MigrationStage.AfterAll)]
[Tags("Development")]
public class DevelopmentMaintenance : Migration
{
    public override void Up()
    {
        // Create test data, enable debugging features
        Execute.Sql("ALTER DATABASE CURRENT SET QUERY_STORE = ON");
    }

    public override void Down() { }
}
```

## Comparison with Other Migration Types

| Feature              | Regular Migrations    | Maintenance Migrations | Profiles                 |
|----------------------|-----------------------|------------------------|--------------------------|
| **Execution**        | Once per version      | Every run              | Every run when specified |
| **Version Tracking** | Yes                   | No                     | No                       |
| **Timing**           | Sequential by version | At specific stages     | After all migrations     |
| **Use Case**         | Schema changes        | Maintenance tasks      | Data seeding             |
| **Transaction**      | Configurable          | Configurable           | Configurable             |
| **Tags Support**     | Yes                   | Yes                    | No                       |

## Troubleshooting

### Common Issues

**1. Maintenance migrations not running**
- Check that `IncludeMaintenanceMigrations` is true (default)
- Verify the migration class has the `[Maintenance]` attribute
- Ensure the assembly is being scanned correctly

**2. Order of execution unclear**
- Use logging to trace execution order
- Remember: BeforeAll → (BeforeEach → Migration → AfterEach)* → BeforeProfiles → Profiles → AfterAll

**3. Transaction conflicts**
- Use `TransactionBehavior.None` for operations that can't run in transactions
- Be careful with DDL operations in maintenance migrations

### Debugging Maintenance Migrations

```csharp
[Maintenance(MigrationStage.BeforeEach)]
public class DebuggingMaintenance : Migration
{
    public override void Up()
    {
        // Log execution for debugging
        Execute.Sql($@"
            INSERT INTO DebugLog (Stage, Message, Timestamp)
            VALUES ('BeforeEach', 'Maintenance migration executed', '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}')
        ");
    }

    public override void Down() { }
}
```

Maintenance migrations provide powerful capabilities for database lifecycle management, allowing you to perform essential maintenance tasks at the right stages of your migration process.
