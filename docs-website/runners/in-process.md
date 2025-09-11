# In-Process Migration Runner

The in-process runner is the **recommended approach** for most applications. It provides the best integration with your application lifecycle, dependency injection, and error handling.

## Quick Start

### Basic Setup

```csharp
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    static void Main(string[] args)
    {
        using var serviceProvider = CreateServices();
        using var scope = serviceProvider.CreateScope();

        // Run migrations
        UpdateDatabase(scope.ServiceProvider);
    }

    private static ServiceProvider CreateServices()
    {
        return new ServiceCollection()
            // Add common FluentMigrator services
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                // Add database support (choose your provider)
                .AddSqlServer()
                // Set the connection string
                .WithGlobalConnectionString("Server=.;Database=MyDb;Integrated Security=true")
                // Define the assembly containing the migrations
                .ScanIn(typeof(AddLogTable).Assembly).For.All())
            // Enable logging to console
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            // Build the service provider
            .BuildServiceProvider(false);
    }

    private static void UpdateDatabase(IServiceProvider serviceProvider)
    {
        // Instantiate the runner
        var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

        // Execute the migrations
        runner.MigrateUp();
    }
}
```

## Configuration

The in-process runner offers extensive configuration options for database providers, assembly scanning, processor options, and more.

For comprehensive configuration details, see the [Configuration Guide](/intro/configuration.md).

### Basic Provider Setup
```csharp
.ConfigureRunner(rb => rb
    .AddSqlServer()                                    // Database provider
    .WithGlobalConnectionString(connectionString)      // Connection string
    .ScanIn(typeof(MyMigration).Assembly).For.All())  // Assembly scanning
```

### Common Configuration Patterns
```csharp
.ConfigureRunner(rb => rb
    .AddSqlServer()
    .WithGlobalConnectionString(connectionString)
    .ScanIn(typeof(Migration1).Assembly, typeof(Migration2).Assembly) // Multiple assemblies
    .For.All())
```

## Application Integration

### ASP.NET Core Startup

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add FluentMigrator
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSqlServer()
                .WithGlobalConnectionString(Configuration.GetConnectionString("DefaultConnection"))
                .ScanIn(typeof(AddUserTable).Assembly).For.All())
            .AddLogging(lb => lb.AddFluentMigratorConsole());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Run migrations on startup
        MigrateDatabase(app);
    }

    private void MigrateDatabase(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }
}
```

### Console Application with Hosting
```csharp
using Microsoft.Extensions.Hosting;

class Program
{
    static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        // Run migrations
        using var scope = host.Services.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();

        await host.RunAsync();
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddFluentMigratorCore()
                    .ConfigureRunner(rb => rb
                        .AddSqlServer()
                        .WithGlobalConnectionString(context.Configuration.GetConnectionString("DefaultConnection"))
                        .ScanIn(typeof(MyMigration).Assembly).For.All());
            });
}
```

## Migration Operations

### Migrate to Latest Version
```csharp
runner.MigrateUp();
```

### Migrate to Specific Version
```csharp
runner.MigrateUp(20200801120000); // Migrate to version 20200801120000
```

### Rollback Migrations
```csharp
runner.MigrateDown(20200801120000); // Rollback to version 20200801120000
runner.Rollback(3); // Rollback last 3 migrations
```

### List Applied Migrations
```csharp
var appliedMigrations = runner.GetExecutedMigrations();
foreach (var migration in appliedMigrations)
{
    Console.WriteLine($"Applied: {migration}");
}
```

## Error Handling

### Basic Error Handling
```csharp
try
{
    runner.MigrateUp();
    Console.WriteLine("Migrations completed successfully");
}
catch (FluentMigratorException ex)
{
    Console.WriteLine($"Migration failed: {ex.Message}");
    // Log details for debugging
    Console.WriteLine($"Details: {ex.InnerException?.Message}");
}
```

### Advanced Error Handling
```csharp
try
{
    runner.MigrateUp();
    Console.WriteLine("Migrations completed successfully");
}
catch (FluentMigratorException ex)
{
    Console.WriteLine($"Migration failed: {ex.Message}");
    // Log details for debugging
    Console.WriteLine($"Details: {ex.InnerException?.Message}");
}
```

For advanced processor configuration options, see the [Configuration Guide](/intro/configuration.md#global-processor-options).

## Logging

The in-process runner supports various logging configurations. See the [Configuration Guide](/intro/configuration.md#logging-configuration) for comprehensive logging setup options.

### Basic Console Logging
```csharp
.AddLogging(lb => lb.AddFluentMigratorConsole())
```

## Best Practices

### ✅ Do
- Use dependency injection for better testability
- Configure proper logging for production deployments
- Handle migration errors gracefully
- Use transactions appropriately
- Test migrations in a staging environment first

### ❌ Don't
- Run migrations without proper error handling
- Use in-process runner in high-concurrency scenarios without locks
- Ignore migration failures
- Run untested migrations in production

## Troubleshooting

### Common Issues

**Issue**: `No migrations found`
- **Solution**: Ensure migrations are public, inherit from `Migration`, and have `[Migration]` attribute

**Issue**: `Assembly not found`
- **Solution**: Verify the assembly containing migrations is referenced correctly

**Issue**: `Database connection failed`
- **Solution**: Check connection string and database server availability

**Issue**: `Version table not found`
- **Solution**: Ensure database user has permission to create tables

See the [FAQ](/intro/faq.md) for more troubleshooting help.
