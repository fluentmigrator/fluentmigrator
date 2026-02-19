# Tags

Tags provide a powerful filtering mechanism for migrations, allowing you to selectively run migrations based on labels. Unlike Profiles, which are special migrations that run every time they're specified, Tags are applied to regular migrations to control when they execute.

::: info Tags vs Profiles
**Tags** filter which migrations run based on labels, while **Profiles** are special migrations that execute every time they're specified. Tags are ideal for feature-based or environment-based migration filtering.
:::

## What are Tags?

Tags allow you to:
- **Filter migrations** by applying labels like "Production", "Development", "Feature1", etc.
- **Organize migrations** by functional area, environment, or feature
- **Control deployment** by running only tagged subsets of migrations
- **Support multi-tenant** scenarios with tenant-specific migrations

## Basic Tag Usage

### Simple Tag
```csharp
[Migration(202401151200)]
[Tags("Production")]
public class ProductionOnlyMigration : Migration
{
    public override void Up()
    {
        Create.Table("ProductionSpecificTable")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("ProductionData").AsString(255).NotNullable();
    }

    public override void Down()
    {
        Delete.Table("ProductionSpecificTable");
    }
}
```

### Multiple Tags
```csharp
[Migration(202401151300)]
[Tags("Development", "Testing")]
public class DevAndTestMigration : Migration
{
    public override void Up()
    {
        Create.Table("TestDataTable")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("TestData").AsString(500).Nullable();

        // Add test data for non-production environments
        Insert.IntoTable("TestDataTable").Row(new
        {
            TestData = "Sample test data for development and testing"
        });
    }

    public override void Down()
    {
        Delete.Table("TestDataTable");
    }
}
```

## Tag Behavior

### RequireAll (Default)
By default, ALL tags must match for the migration to run:

```csharp
[Migration(202401151400)]
[Tags("Production", "Database")]
public class ProductionDatabaseMigration : Migration
{
    public override void Up()
    {
        // This migration only runs when BOTH "Production" AND "Database" tags are specified
        Create.Index("IX_Production_Optimized")
            .OnTable("Users")
            .OnColumn("Email");
    }

    public override void Down()
    {
        Delete.Index("IX_Production_Optimized");
    }
}
```

### RequireAny
Use `TagBehavior.RequireAny` when you want the migration to run if ANY of the tags match:

```csharp
[Migration(202401151500)]
[Tags(TagBehavior.RequireAny, "Development", "Testing", "Staging")]
public class NonProductionMigration : Migration
{
    public override void Up()
    {
        // This migration runs if ANY of Development, Testing, OR Staging tags are specified
        Create.Table("DebugLog")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("LogLevel").AsString(20).NotNullable()
            .WithColumn("Message").AsString(1000).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime);
    }

    public override void Down()
    {
        Delete.Table("DebugLog");
    }
}
```

## Running Tagged Migrations

### In-Process Runner
```csharp
using var serviceProvider = new ServiceCollection()
    .AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddSqlServer()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(MyMigration).Assembly).For.All())
    .Configure<RunnerOptions>(opt =>
    {
        opt.Tags = new[] { "Production" }; // Only run Production tagged migrations
    })
    .BuildServiceProvider(false);

using var scope = serviceProvider.CreateScope();
var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
runner.MigrateUp();
```

### Console Tool (Migrate.exe)
```bash
# Run only Production tagged migrations
Migrate.exe -p sqlserver -c "Server=.;Database=MyDb;Integrated Security=true" -a "MyApp.dll" --tag Production

# Run multiple tags (requires ALL tags to match by default)
Migrate.exe -p sqlserver -c "Server=.;Database=MyDb;Integrated Security=true" -a "MyApp.dll" --tag Production --tag Database

# Run migrations without any tags (untagged migrations)
Migrate.exe -p sqlserver -c "Server=.;Database=MyDb;Integrated Security=true" -a "MyApp.dll"
```

### dotnet-fm CLI
```bash
# Run Development tagged migrations only
dotnet fm migrate -p sqlserver -c "Server=.;Database=MyDb;Integrated Security=true" -a "MyApp.dll" --tag Development

# Run Feature1 tagged migrations
dotnet fm migrate -p sqlserver -c "Server=.;Database=MyDb;Integrated Security=true" -a "MyApp.dll" --tag Feature1
```

## Common Tag Patterns

### Environment-Based Tags
```csharp
[Migration(202401151600)]
[Tags("Production")]
public class ProductionOptimizations : Migration
{
    public override void Up()
    {
        Create.Index("IX_Users_Email_Optimized")
            .OnTable("Users")
            .OnColumn("Email")
            .WithOptions().Clustered();
    }

    public override void Down()
    {
        Delete.Index("IX_Users_Email_Optimized");
    }
}

[Migration(202401151700)]
[Tags(TagBehavior.RequireAny, "Development", "Testing")]
public class NonProductionFeatures : Migration
{
    public override void Up()
    {
        Create.Table("FeatureFlags")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("FeatureName").AsString(100).NotNullable()
            .WithColumn("IsEnabled").AsBoolean().NotNullable().WithDefaultValue(false);
    }

    public override void Down()
    {
        Delete.Table("FeatureFlags");
    }
}
```

### Feature-Based Tags
```csharp
[Migration(202401151800)]
[Tags("UserManagement")]
public class UserManagementMigration : Migration
{
    public override void Up()
    {
        Create.Table("UserProfiles")
            .WithColumn("UserId").AsInt32().NotNullable()
            .WithColumn("ProfileData").AsString(2000).Nullable()
            .WithColumn("LastUpdated").AsDateTime().NotNullable();

        Create.ForeignKey("FK_UserProfiles_Users")
            .FromTable("UserProfiles").ForeignColumn("UserId")
            .ToTable("Users").PrimaryColumn("Id");
    }

    public override void Down()
    {
        Delete.ForeignKey("FK_UserProfiles_Users").OnTable("UserProfiles");
        Delete.Table("UserProfiles");
    }
}

[Migration(202401151900)]
[Tags("Reporting")]
public class ReportingMigration : Migration
{
    public override void Up()
    {
        Create.Table("Reports")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("ReportName").AsString(255).NotNullable()
            .WithColumn("ReportData").AsString().NotNullable()
            .WithColumn("GeneratedAt").AsDateTime().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Reports");
    }
}
```

### Multi-Tenant Tags
```csharp
// Base class for tenant-specific migrations
[Tags("TenantA")]
public abstract class TenantABaseMigration : Migration
{
}

[Migration(202401152000)]
public class TenantASpecificTable : TenantABaseMigration
{
    public override void Up()
    {
        Create.Table("TenantAData")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("TenantSpecificField").AsString(255).NotNullable();
    }

    public override void Down()
    {
        Delete.Table("TenantAData");
    }
}

[Migration(202401152100)]
[Tags("TenantB", "TenantA")]
public class SharedTenantMigration : Migration
{
    public override void Up()
    {
        // This migration runs for both TenantA and TenantB
        Create.Table("SharedTenantTable")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("SharedData").AsString(255).NotNullable();
    }

    public override void Down()
    {
        Delete.Table("SharedTenantTable");
    }
}
```

### Database-Specific Tags
```csharp
[Migration(202401152200)]
[Tags("SqlServer")]
public class SqlServerSpecificMigration : Migration
{
    public override void Up()
    {
        // SQL Server specific optimizations
        Execute.Sql(@"
            CREATE NONCLUSTERED INDEX IX_Users_Email_Includes
            ON Users (Email)
            INCLUDE (FirstName, LastName, CreatedAt)
        ");
    }

    public override void Down()
    {
        Execute.Sql("DROP INDEX IX_Users_Email_Includes ON Users");
    }
}

[Migration(202401152300)]
[Tags("PostgreSQL")]
public class PostgreSQLSpecificMigration : Migration
{
    public override void Up()
    {
        // PostgreSQL specific features
        Execute.Sql(@"
            CREATE INDEX CONCURRENTLY IX_Users_Email_GIN
            ON Users USING GIN (to_tsvector('english', Email))
        ");
    }

    public override void Down()
    {
        Execute.Sql("DROP INDEX IX_Users_Email_GIN");
    }
}
```

## Advanced Tag Scenarios

### Tag Inheritance
```csharp
// Base migration class with common tags
[Tags("Core")]
public abstract class CoreMigration : Migration
{
    // Common functionality for core migrations
    protected void LogMigrationStart(string migrationName)
    {
        Execute.Sql($"INSERT INTO MigrationLog (MigrationName, StartedAt) VALUES ('{migrationName}', GETDATE())");
    }
}

[Migration(202401152500)]
[Tags("UserManagement")] // Inherits "Core" tag from base class
public class UserTableMigration : CoreMigration
{
    public override void Up()
    {
        LogMigrationStart("UserTableMigration");

        Create.Table("Users")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Username").AsString(50).NotNullable()
            .WithColumn("Email").AsString(255).NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Users");
    }
}
```

## Tag Configuration and Deployment

### Environment-Specific Deployment
```bash
# Development environment
dotnet fm migrate -p sqlserver -c "$DEV_CONNECTION" -a "MyApp.dll" --tag Development

# Testing environment
dotnet fm migrate -p sqlserver -c "$TEST_CONNECTION" -a "MyApp.dll" --tag Testing

# Production environment - run core migrations only
dotnet fm migrate -p sqlserver -c "$PROD_CONNECTION" -a "MyApp.dll" --tag Production
```

### Feature Flag Deployment
```bash
# Deploy base application
dotnet fm migrate -p sqlserver -c "$CONNECTION" -a "MyApp.dll" --tag Core

# Deploy specific features as they're enabled
dotnet fm migrate -p sqlserver -c "$CONNECTION" -a "MyApp.dll" --tag UserManagement
dotnet fm migrate -p sqlserver -c "$CONNECTION" -a "MyApp.dll" --tag Reporting

# Deploy experimental features to staging only
dotnet fm migrate -p sqlserver -c "$STAGING_CONNECTION" -a "MyApp.dll" --tag Experimental
```

### CI/CD Pipeline Integration
```yaml
# Azure DevOps Pipeline example
stages:
- stage: Deploy_Core
  jobs:
  - job: MigrateCore
    steps:
    - task: DotNetCoreCLI@2
      inputs:
        command: 'custom'
        custom: 'fm'
        arguments: 'migrate -p sqlserver -c "$(ConnectionString)" -a "$(Build.ArtifactStagingDirectory)/MyApp.dll" --tag Core'

- stage: Deploy_Features
  condition: and(succeeded(), eq(variables['DeployFeatures'], 'true'))
  jobs:
  - job: MigrateFeatures
    steps:
    - task: DotNetCoreCLI@2
      inputs:
        command: 'custom'
        custom: 'fm'
        arguments: 'migrate -p sqlserver -c "$(ConnectionString)" -a "$(Build.ArtifactStagingDirectory)/MyApp.dll" --tag UserManagement --tag Reporting'
```

## Best Practices

### ✅ Do
- Use descriptive tag names that clearly indicate purpose
- Document tag strategy in your project README
- Use environment-specific tags for deployment control
- Combine tags logically with TagBehavior settings
- Test tagged migration scenarios in staging environments

### ❌ Don't
- Use too many tags on a single migration (keep it focused)
- Change tag names after migrations are deployed to production
- Use tags as a substitute for proper migration versioning
- Create overly complex tag hierarchies that are hard to understand

## Tags vs Other Migration Features

### Tags vs Profiles
- **Tags**: Filter regular migrations by labels - run once per version
- **Profiles**: Special migrations that run every time specified - for data seeding

### Tags vs Maintenance Migrations
- **Tags**: Filter any migration type by labels
- **Maintenance Migrations**: Run at specific stages (BeforeAll, AfterEach, etc.)

### Tags vs Conditional Logic
- **Tags**: Filter at deployment time - decided before running
- **Conditional Logic**: Filter at runtime - decided during migration execution

### Example Comparison
```csharp
// Tagged migration - filtered at deployment time
[Migration(202401152600)]
[Tags("Production")]
public class TaggedMigration : Migration
{
    public override void Up()
    {
        Create.Index("IX_Production_Only").OnTable("Users").OnColumn("Email");
    }
    public override void Down()
    {
        Delete.Index("IX_Production_Only");
    }
}

// Conditional migration - filtered at runtime
[Migration(202401152700)]
public class ConditionalMigration : Migration
{
    public override void Up()
    {
        IfDatabase(ProcessorIdConstants.SqlServer)
            .Create.Index("IX_SqlServer_Only").OnTable("Users").OnColumn("Email");
    }
    public override void Down()
    {
        IfDatabase(ProcessorIdConstants.SqlServer)
            .Delete.Index("IX_SqlServer_Only");
    }
}
```

## Troubleshooting

### Tags Not Working
- ✅ Verify tag names match exactly (case-sensitive)
- ✅ Check that tagged migrations are in scanned assemblies
- ✅ Ensure tag parameters are passed correctly to runner
- ✅ Confirm TagBehavior setting matches your intent

### No Migrations Run
- ✅ Check if all migrations have tags when using tag filtering
- ✅ Verify tag combination logic (RequireAll vs RequireAny)
- ✅ Ensure at least one migration matches the tag criteria

### Unexpected Migrations Run
- ✅ Review migrations for multiple or inherited tags
- ✅ Check for untagged migrations running when not intended
- ✅ Verify tag behavior settings

See the [FAQ](/intro/faq.md) for additional troubleshooting guidance.
