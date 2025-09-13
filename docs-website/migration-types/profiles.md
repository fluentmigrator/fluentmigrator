# Profiles

Profiles allow you to selectively apply migrations based on runtime conditions or deployment environments. They are useful for environment-specific data seeding, testing scenarios, or feature toggles.

::: info Evolution
While Profiles are not deprecated, **Maintenance Migrations** are now the preferred approach for most use cases. Maintenance Migrations offer more flexibility and can run at different stages of the migration process.

However, Profiles remain useful for environment-specific data seeding and conditional migration scenarios.
:::

## What are Profiles?

Profiles are special migrations that:
- Run **only when explicitly specified** via command-line parameter or configuration
- Are executed **every time** they're specified (not version-tracked like regular migrations)
- Are perfect for **data seeding** and **environment-specific setup**

## Basic Profile Example

```csharp
[Profile("Development")]
public class CreateDevSeedData : Migration
{
    public override void Up()
    {
        Insert.IntoTable("Users").Row(new
        {
            Id = 1,
            Username = "devuser",
            Email = "dev@example.com",
            DisplayName = "Development User",
            CreatedAt = DateTime.Now
        });

        Insert.IntoTable("Users").Row(new
        {
            Id = 2,
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User",
            CreatedAt = DateTime.Now
        });
    }

    public override void Down()
    {
        // Profiles typically don't implement Down()
        // as they're meant for data seeding
    }
}
```

## Multiple Environment Profiles

### Development Profile
```csharp
[Profile("Development")]
public class DevelopmentData : Migration
{
    public override void Up()
    {
        // Development-specific test data
        Insert.IntoTable("Categories").Row(new
        {
            Name = "Development Category",
            IsActive = true
        });

        Insert.IntoTable("Products").Row(new
        {
            Name = "Test Product",
            CategoryId = 1,
            Price = 9.99m,
            InStock = true
        });
    }

    public override void Down() { }
}
```

### Testing Profile
```csharp
[Profile("Testing")]
public class TestingData : Migration
{
    public override void Up()
    {
        // Testing environment data
        Insert.IntoTable("Users").Row(new
        {
            Username = "testrunner",
            Email = "test@company.com",
            Role = "Admin"
        });

        // Create test scenarios
        for (int i = 1; i <= 10; i++)
        {
            Insert.IntoTable("Orders").Row(new
            {
                OrderNumber = $"TEST-{i:000}",
                UserId = 1,
                Total = i * 10.0m,
                Status = "Completed"
            });
        }
    }

    public override void Down() { }
}
```

### Production Profile
```csharp
[Profile("Production")]
public class ProductionConfiguration : Migration
{
    public override void Up()
    {
        // Production-only configuration
        Insert.IntoTable("Settings").Row(new
        {
            Key = "MaintenanceMode",
            Value = "false"
        });

        Insert.IntoTable("Settings").Row(new
        {
            Key = "MaxConcurrentUsers",
            Value = "1000"
        });

        // Create admin user (only in production setup)
        Insert.IntoTable("Users").Row(new
        {
            Username = "admin",
            Email = "admin@company.com",
            Role = "SuperAdmin",
            IsActive = true
        });
    }

    public override void Down() { }
}
```

## Running Profiles

### In-Process Runner
```csharp
using var serviceProvider = new ServiceCollection()
    .AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddSqlServer()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(MyMigration).Assembly).For.All())
    .BuildServiceProvider(false);

using var scope = serviceProvider.CreateScope();
var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

// Run normal migrations first
runner.MigrateUp();

// Then run profile
runner.MigrateUp("Development");
```

### Console Runner
```bash
# Run migrations with Development profile
Migrate.exe -p sqlserver -c "Server=.;Database=MyDb;Integrated Security=true" -a "MyApp.dll" --profile Development
```

### dotnet-fm CLI
```bash
# Run migrations with Testing profile
dotnet fm migrate -p sqlserver -c "Server=.;Database=MyDb;Integrated Security=true" -a "MyApp.dll" --profile Testing
```

## Advanced Profile Usage

### Multiple Profiles
```csharp
[Profile("Development"), Profile("Testing")]
public class SharedTestData : Migration
{
    public override void Up()
    {
        // This runs in both Development AND Testing environments
        Insert.IntoTable("CommonSettings").Row(new
        {
            Key = "DebugMode",
            Value = "true"
        });
    }

    public override void Down() { }
}
```

### Conditional Profile Logic
```csharp
[Profile("Development")]
public class ConditionalDevData : Migration
{
    public override void Up()
    {
        // Only seed data if tables are empty
        if (!Schema.Table("Users").Exists() ||
            Execute.Sql("SELECT COUNT(*) FROM Users").Returns.Single<int>() == 0)
        {
            Insert.IntoTable("Users").Row(new
            {
                Username = "devuser",
                Email = "dev@example.com"
            });
        }
    }

    public override void Down() { }
}
```

### Profile with Database-Specific Logic
```csharp
[Profile("Development")]
public class DatabaseSpecificDevData : Migration
{
    public override void Up()
    {
        IfDatabase(ProcessorIdConstants.SqlServer)
            .Insert.IntoTable("TestData").Row(new { Data = "SQL Server specific data" });

        IfDatabase("Sqlite")
            .Insert.IntoTable("TestData").Row(new { Data = "SQLite specific data" });

        IfDatabase(ProcessorIdConstants.Postgres)
            .Insert.IntoTable("TestData").Row(new { Data = "PostgreSQL specific data" });
    }

    public override void Down() { }
}
```

## Profile vs Maintenance Migrations

### When to Use Profiles
- ✅ Environment-specific data seeding
- ✅ One-time setup data per environment
- ✅ Feature toggles based on environment
- ✅ Test data generation

### When to Use Maintenance Migrations Instead
- ✅ Data cleanup operations
- ✅ Multi-stage operations (BeforeAll, AfterEach, etc.)
- ✅ Complex conditional logic with tags
- ✅ Operations that need to run at specific migration stages

### Comparison Example

**Profile Approach:**
```csharp
[Profile("Development")]
public class DevSeedData : Migration
{
    public override void Up()
    {
        Insert.IntoTable("Users").Row(new { Username = "dev" });
    }
    public override void Down() { }
}
```

**Maintenance Migration Approach:**
```csharp
[Maintenance(MigrationStage.AfterAll, TransactionBehavior.Default)]
[Tags("Development")]
public class DevSeedDataMaintenance : Migration
{
    public override void Up()
    {
        Insert.IntoTable("Users").Row(new { Username = "dev" });
    }
    public override void Down() { }
}
```

## Best Practices

### ✅ Do
- Use Profiles primarily for data seeding
- Keep Profile migrations simple and idempotent
- Use descriptive Profile names that match your environments
- Document which Profiles should be run in which environments
- Consider using Maintenance Migrations for complex scenarios

### ❌ Don't
- Use Profiles for schema changes (use regular migrations)
- Make Profiles dependent on specific migration versions
- Implement complex business logic in Profile migrations
- Use Profiles for operations that should be version-tracked

## Environment-Specific Deployment

### Development Environment
```bash
# Development deployment script
dotnet fm migrate -p sqlserver -c "$DEV_CONNECTION" -a "MyApp.dll"
dotnet fm migrate -p sqlserver -c "$DEV_CONNECTION" -a "MyApp.dll" --profile Development
```

### Testing Environment
```bash
# Testing deployment script
dotnet fm migrate -p sqlserver -c "$TEST_CONNECTION" -a "MyApp.dll"
dotnet fm migrate -p sqlserver -c "$TEST_CONNECTION" -a "MyApp.dll" --profile Testing
```

### Production Environment
```bash
# Production deployment script
dotnet fm migrate -p sqlserver -c "$PROD_CONNECTION" -a "MyApp.dll"
dotnet fm migrate -p sqlserver -c "$PROD_CONNECTION" -a "MyApp.dll" --profile Production
```

## Integration with Configuration

### ASP.NET Core Integration
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    using var scope = app.ApplicationServices.CreateScope();
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

    // Always run migrations
    runner.MigrateUp();

    // Run environment-specific profile
    if (env.IsDevelopment())
    {
        runner.MigrateUp("Development");
    }
    else if (env.IsStaging())
    {
        runner.MigrateUp("Testing");
    }
    else if (env.IsProduction())
    {
        runner.MigrateUp("Production");
    }
}
```

### Configuration-Driven Profiles
```csharp
// appsettings.json
{
  "Migration": {
    "Profile": "Development",
    "SeedData": true
  }
}

// Application code
var profile = Configuration["Migration:Profile"];
var shouldSeedData = Configuration.GetValue<bool>("Migration:SeedData");

if (shouldSeedData && !string.IsNullOrEmpty(profile))
{
    runner.MigrateUp(profile);
}
```

## Troubleshooting

### Profile Not Running
- ✅ Check that the Profile name matches exactly (case-sensitive)
- ✅ Verify the migration class is public and inherits from `Migration`
- ✅ Ensure the `[Profile]` attribute is applied correctly
- ✅ Confirm the assembly containing the profile is being scanned

### Profile Running Multiple Times
This is expected behavior. Profiles run every time they're specified, unlike regular migrations which are version-tracked.

### Data Conflicts
```csharp
[Profile("Development")]
public class SafeDevData : Migration
{
    public override void Up()
    {
        // Use INSERT IGNORE or similar to avoid conflicts
        Execute.Sql(@"
            IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'devuser')
            BEGIN
                INSERT INTO Users (Username, Email)
                VALUES ('devuser', 'dev@example.com')
            END
        ");
    }

    public override void Down() { }
}
```

See the [FAQ](/intro/faq.md) for more troubleshooting guidance.
