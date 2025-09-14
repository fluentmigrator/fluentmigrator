# Configuration

FluentMigrator provides extensive configuration options to customize the migration runner behavior, database connections, naming conventions, and more. This guide covers all configuration aspects across different runner types.

## Core Configuration Concepts

### Service Registration

FluentMigrator uses .NET dependency injection for configuration:

```csharp
services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        // Runner configuration here
    )
    .AddLogging(lb => lb.AddFluentMigratorConsole());
```

### Runner Configuration Builder

The `ConfigureRunner` method provides a fluent interface for all configuration options:

```csharp
.ConfigureRunner(rb => rb
    .AddSqlServer()                                   // Database provider
    .WithGlobalConnectionString(connectionString)     // Connection string
    .ScanIn(typeof(MyMigration).Assembly).For.All()   // Assembly scanning
    .WithVersionTable(new CustomVersionTable())       // Version table
    .ConfigureGlobalProcessorOptions(opts => { ... }) // Processor options
)
```

## Database Provider Configuration

### Single Provider Setup

```csharp
.ConfigureRunner(rb => rb
    .AddSqlServer()
    .WithGlobalConnectionString("Server=.;Database=MyDb;Integrated Security=true")
    .ScanIn(typeof(MyMigration).Assembly).For.All())
```

### Available Database Providers

| Provider   | Method               | Package Required                |
|------------|----------------------|---------------------------------|
| SQL Server | `.AddSqlServerXXX()` | FluentMigrator.Runner.SqlServer |
| PostgreSQL | `.AddPostgresXXX()`  | FluentMigrator.Runner.Postgres  |
| MySQL      | `.AddMySqlXXX()`     | FluentMigrator.Runner.MySql     |
| SQLite     | `.AddSQLite()`       | FluentMigrator.Runner.SQLite    |
| Oracle     | `.AddOracleXXX()`    | FluentMigrator.Runner.Oracle    |
| Firebird   | `.AddFirebird()`     | FluentMigrator.Runner.Firebird  |
| IBM DB2    | `.AddDb2()`          | FluentMigrator.Runner.Db2       |
| SAP HANA   | `.AddHana()`         | FluentMigrator.Runner.Hana      |
| Snowflake  | `.AddSnowflake()`    | FluentMigrator.Runner.Snowflake |
| Redshift   | `.AddRedshift()`     | FluentMigrator.Runner.Redshift  |

## Assembly Scanning Configuration

### Single Assembly

```csharp
.ScanIn(typeof(MyMigration).Assembly).For.All()
```

### Multiple Assemblies

```csharp
.ScanIn(typeof(Migration1).Assembly, typeof(Migration2).Assembly).For.All()
```

### Specific Migration Types

```csharp
.ScanIn(Assembly.GetExecutingAssembly())
    .For.Migrations()              // Only migrations
    .For.Profiles()                // Only profiles
    .For.MaintenanceMigrations()   // Only maintenance migrations
    .For.All()                     // All types (default)
```

### Assembly Scanning

```csharp
.WithMigrationsIn(Assembly.GetExecutingAssembly())
.WithMigrationsIn("SpecificNamespace")
```

## Global Processor Options

### Basic Processor Configuration

```csharp
.ConfigureGlobalProcessorOptions(opt => {
    opt.Timeout = TimeSpan.FromMinutes(5);
    opt.ProviderSwitches = "ForceQuote=false";
    opt.ConnectionString = connectionString;
})
```

### Available Processor Options

| Option             | Type       | Description                    | Default     |
|--------------------|------------|--------------------------------|-------------|
| `Timeout`          | `TimeSpan` | Command execution timeout      | 30 seconds  |
| `ProviderSwitches` | `string`   | Provider-specific switches     | Empty       |
| `ConnectionString` | `string`   | Database connection string     | From runner |
| `PreviewOnly`      | `bool`     | Generate SQL without execution | `false`     |
| `StripComments`    | `bool`     | Remove comments from SQL       | `false`     |

### Provider Switches

#### SQL Server Switches
```csharp
opt.ProviderSwitches = "ForceQuote=true;TrustServerCertificate=true";
```

#### PostgreSQL Switches
```csharp
opt.ProviderSwitches = "Force Quote=true;CommandTimeout=60";
```

#### MySQL Switches
```csharp
opt.ProviderSwitches = "MySqlVersion=5.7.17";
```

## Version Table Configuration

### Default Version Table

FluentMigrator automatically creates a `VersionInfo` table to track applied migrations.

### Custom Version Table

```csharp
public class CustomVersionTable : DefaultVersionTableMetaData
{
    public override string TableName => "MigrationHistory";
    public override string SchemaName => "dbo";
    public override string ColumnName => "Version";
    public override string DescriptionColumnName => "Description";
    public override string AppliedOnColumnName => "AppliedDate";
}

// Use custom version table
.WithVersionTable(new CustomVersionTable())
```

### Version Table in Specific Schema

```csharp
.WithVersionTable(new DefaultVersionTableMetaData()
{
    SchemaName = "migrations"
})
```

## Naming Conventions and Convention Sets

### Default Naming Conventions

FluentMigrator uses these default naming patterns:

- **Primary Key**: `PK_<TableName>`
- **Foreign Key**: `FK_<TableName>_<ReferencedTable>_<ColumnName>`
- **Index**: `IX_<TableName>_<ColumnName>`
- **Unique Constraint**: `UC_<TableName>_<ColumnName>`
- **Check Constraint**: `CK_<TableName>_<ColumnName>`

### Custom Naming Convention

FluentMigrator supports changing naming conventions for:

 * Columns
 * Constraints
 * Indexes
 * Sequences
 * Schemas
 * Auto Names
 * Root Paths

While FluentMigrator comes with reasonable naming conventions, some organizations may have strict standards. In such cases, overriding FluentMigrator's opinion may be desirable.

#### High-Level Tasks

1. Implement the `IConventionSet` interface (example: see below)
2. Register the `IConventionSet` as singleton: `services.AddSingleton<IConventionSet, YourConventionSet>()`

Example `IConventionSet` implementation :

```csharp
public class YourConventionSet : IConventionSet
{
    public YourConventionSet()
        : this(new DefaultConventionSet())
    {
    }

    public YourConventionSet(IConventionSet innerConventionSet)
    {
        ForeignKeyConventions = new List<IForeignKeyConvention>()
        {
            /* This is where you do your stuff */
            new YourCustomDefaultForeignKeyNameConvention(),
            innerConventionSet.SchemaConvention,
        };

        ColumnsConventions = innerConventionSet.ColumnsConventions;
        ConstraintConventions = innerConventionSet.ConstraintConventions;
        IndexConventions = innerConventionSet.IndexConventions;
        SequenceConventions = innerConventionSet.SequenceConventions;
        AutoNameConventions = innerConventionSet.AutoNameConventions;
        SchemaConvention = innerConventionSet.SchemaConvention;
        RootPathConvention = innerConventionSet.RootPathConvention;
    }

    /// <inheritdoc />
    public IRootPathConvention RootPathConvention { get; }

    /// <inheritdoc />
    public DefaultSchemaConvention SchemaConvention { get; }

    /// <inheritdoc />
    public IList<IColumnsConvention> ColumnsConventions { get; }

    /// <inheritdoc />
    public IList<IConstraintConvention> ConstraintConventions { get; }

    /// <inheritdoc />
    public IList<IForeignKeyConvention> ForeignKeyConventions { get; }

    /// <inheritdoc />
    public IList<IIndexConvention> IndexConventions { get; }

    /// <inheritdoc />
    public IList<ISequenceConvention> SequenceConventions { get; }

    /// <inheritdoc />
    public IList<IAutoNameConvention> AutoNameConventions { get; }
}
```

### Override Default Names

```csharp
// Custom primary key name
Create.Table("Users")
    .WithColumn("Id").AsInt32().PrimaryKey("PK_Users_ID")
    .WithColumn("Name").AsString(100);

// Custom foreign key name
Create.ForeignKey("FK_Orders_Customer")
    .FromTable("Orders").ForeignColumn("CustomerId")
    .ToTable("Customers").PrimaryColumn("Id");
```

## Logging Configuration

### Console Logging

```csharp
.AddLogging(lb => lb.AddFluentMigratorConsole())
```

### File Logging with Serilog

```csharp
// Install: Serilog.Extensions.Logging.File
.AddLogging(lb => lb.AddFile("logs/migrations.log"))
```

### Custom Logging Provider

```csharp
.AddLogging(lb => lb
    .AddConsole()
    .AddDebug()
    .SetMinimumLevel(LogLevel.Information))
```

### Structured Logging

```csharp
// Install: Serilog.Extensions.Logging, Serilog.Sinks.Console
var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/migrations.log")
    .CreateLogger();

.AddLogging(lb => lb.AddSerilog(logger))
```

## Environment-Specific Configuration

### Configuration by Environment

```csharp
public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    var environment = configuration["Environment"];
    var connectionString = configuration.GetConnectionString(environment == "Production" ? "Production" : "Development");

    services.AddFluentMigratorCore()
        .ConfigureRunner(rb => rb
            .AddSqlServer()
            .WithGlobalConnectionString(connectionString)
            .ScanIn(typeof(MyMigration).Assembly).For.All())
        .AddLogging(lb => lb.AddFluentMigratorConsole());
}
```

### appsettings.json Configuration

```json
{
  "ConnectionStrings": {
    "Development": "Server=.;Database=MyApp_Dev;Integrated Security=true",
    "Staging": "Server=staging;Database=MyApp_Staging;User Id=user;Password=pass",
    "Production": "Server=prod;Database=MyApp;User Id=produser;Password=prodpass"
  },
  "FluentMigrator": {
    "Timeout": "00:05:00",
    "ProviderSwitches": "ForceQuote=true"
  }
}
```

```csharp
// Using configuration
var fluentMigratorConfig = configuration.GetSection("FluentMigrator");
var timeout = fluentMigratorConfig.GetValue<TimeSpan>("Timeout");
var providerSwitches = fluentMigratorConfig.GetValue<string>("ProviderSwitches");

.ConfigureGlobalProcessorOptions(opt => {
    opt.Timeout = timeout;
    opt.ProviderSwitches = providerSwitches;
})
```

## Transaction Configuration

### Global Transaction Behavior

```csharp
.ConfigureGlobalProcessorOptions(opt => {
    opt.TransactionBehavior = TransactionBehavior.Default; // Use transactions
    // or
    opt.TransactionBehavior = TransactionBehavior.None;    // No transactions
})
```

### Per-Migration Transaction Control

```csharp
[Migration(1)]
[Maintenance(MigrationStage.BeforeAll, TransactionBehavior.None)]
public class SetupMigration : Migration
{
    public override void Up()
    {
        // This migration runs without a transaction
        Execute.Sql("CREATE DATABASE MyApp");
    }

    public override void Down() { }
}
```

## Preview and Output Configuration

### Preview Mode (SQL Generation Only)

```csharp
.ConfigureGlobalProcessorOptions(opt => {
    opt.PreviewOnly = true;
})
```

### SQL Output to File

For console and dotnet-fm runners:

```bash
# Console runner
Migrate.exe -p sqlserver -c "..." -a "MyApp.dll" --output --outputFileName "migration.sql"

# dotnet-fm runner
dotnet fm migrate -p sqlserver -c "..." -a "MyApp.dll" --output --outputFileName "migration.sql"
```

## Advanced Configuration Scenarios

### Multiple Database Support

```csharp
// Configuration for multi-tenant applications
services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddSqlServer()
        .WithGlobalConnectionString(defaultConnectionString)
        .ScanIn(typeof(SharedMigration).Assembly).For.All())
    .AddScoped<ITenantMigrationRunner, TenantMigrationRunner>();

public class TenantMigrationRunner : ITenantMigrationRunner
{
    public void MigrateTenant(string tenantConnectionString)
    {
        using var scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        // Override connection string for this tenant
        runner.Processor.ConnectionString = tenantConnectionString;
        runner.MigrateUp();
    }
}
```

### Custom Processor Factory

```csharp
public class CustomSqlServerProcessorFactory : SqlServerProcessorFactory
{
    public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
    {
        // Custom processor creation logic
        var processor = base.Create(connectionString, announcer, options);

        // Apply custom configuration
        processor.Options.Timeout = TimeSpan.FromMinutes(30);

        return processor;
    }
}

// Register custom factory
services.Configure<SelectingProcessorAccessorOptions>(opt =>
{
    opt.ProcessorFactories.Clear();
    opt.ProcessorFactories.Add(new CustomSqlServerProcessorFactory());
});
```

### Conditional Configuration

```csharp
public void ConfigureServices(IServiceCollection services, IHostEnvironment env)
{
    var builder = services.AddFluentMigratorCore()
        .ConfigureRunner(rb => rb
            .AddSqlServer()
            .WithGlobalConnectionString(connectionString)
            .ScanIn(typeof(MyMigration).Assembly).For.All());

    if (env.IsDevelopment())
    {
        builder.ConfigureGlobalProcessorOptions(opt => {
            opt.Timeout = TimeSpan.FromMinutes(1); // Shorter timeout in dev
        });
    }
    else
    {
        builder.ConfigureGlobalProcessorOptions(opt => {
            opt.Timeout = TimeSpan.FromMinutes(10); // Longer timeout in production
        });
    }

    builder.AddLogging(lb => env.IsDevelopment()
        ? lb.AddFluentMigratorConsole()
        : lb.AddFile("logs/migrations.log"));
}
```

## Configuration Validation

### Validate Configuration at Startup

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Validate FluentMigrator configuration
    using var scope = app.ApplicationServices.CreateScope();
    var runner = scope.ServiceProvider.GetService<IMigrationRunner>();

    if (runner == null)
    {
        throw new InvalidOperationException("FluentMigrator is not properly configured");
    }

    // Test database connection
    try
    {
        var appliedMigrations = runner.GetExecutedMigrations();
        Console.WriteLine($"Database connection successful. {appliedMigrations.Count()} migrations applied.");
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"Database connection failed: {ex.Message}", ex);
    }
}
```

### Configuration Health Checks

```csharp
// Install: Microsoft.Extensions.Diagnostics.HealthChecks
services.AddHealthChecks()
    .AddCheck<FluentMigratorHealthCheck>("fluentmigrator");

public class FluentMigratorHealthCheck : IHealthCheck
{
    private readonly IMigrationRunner _runner;

    public FluentMigratorHealthCheck(IMigrationRunner runner)
    {
        _runner = runner;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var appliedMigrations = _runner.GetExecutedMigrations();
            return Task.FromResult(HealthCheckResult.Healthy($"{appliedMigrations.Count()} migrations applied"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Migration runner unhealthy", ex));
        }
    }
}
```

## Best Practices

### ✅ Recommended Practices

- **Environment-specific configuration**: Use different settings for dev/staging/production
- **Connection string security**: Store sensitive connection strings securely (Azure Key Vault, etc.)
- **Timeout configuration**: Set appropriate timeouts based on your migration complexity
- **Logging configuration**: Enable appropriate logging for troubleshooting
- **Transaction behavior**: Use transactions unless you have specific requirements
- **Version table customization**: Use custom version tables for schema organization

### ❌ Avoid These Patterns

- **Hardcoded connection strings**: Always use configuration providers
- **Excessive timeout values**: Don't set extremely high timeouts without good reason
- **Ignoring transaction behavior**: Understand when transactions are and aren't used
- **Missing error handling**: Always handle configuration errors gracefully
- **Provider switches without documentation**: Document any custom provider switches

## Troubleshooting Configuration Issues

### Common Configuration Problems

**Issue**: `InvalidOperationException: No database provider configured`
- **Solution**: Ensure you've called the appropriate provider method (`.AddSqlServer()`, etc.)

**Issue**: `ArgumentException: Assembly cannot be found`
- **Solution**: Verify the assembly path and ensure the assembly contains migrations

**Issue**: `SqlException: Login timeout expired`
- **Solution**: Increase connection timeout or check database connectivity

**Issue**: `FluentMigratorException: Version table cannot be created`
- **Solution**: Ensure database user has DDL permissions

### Configuration Debugging

```csharp
.AddLogging(lb => lb
    .AddConsole()
    .AddFilter("FluentMigrator", LogLevel.Debug)) // Enable debug logging
```

## See Also

- [Installation](./installation.md) - Package installation and setup
- [Quick Start](./quick-start.md) - Basic migration runner usage
- [In-Process Runner](../runners/in-process.md) - Detailed in-process runner setup
- [Database Providers](../providers/sql-server.md) - Provider-specific configuration
- [Best Practices](../advanced/best-practices.md) - Migration best practices
