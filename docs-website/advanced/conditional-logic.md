# Conditional Logic in Migrations

FluentMigrator provides powerful conditional logic capabilities that allow you to create adaptive migrations that behave differently based on database providers, environments, existing schema state, or custom conditions. This guide covers all aspects of conditional migration logic.

## Database Provider Conditionals

Provider conditionals can be achieved using the `IfDatabase` method, which allows you to execute specific migration steps based on the database type.

It can take a single provider name or an array of provider names, or a custom predicate function:

```csharp
// Will also match "SqlServer2019", "SqlServer2022", etc.
IfDatabase(ProcessorIdConstants.SqlServer).Execute.Sql("...");

// Matches multiple providers
IfDatabase(new[] { ProcessorIdConstants.SqlServer, ProcessorIdConstants.Postgres }).Execute.Sql("...");

// Custom predicate function
IfDatabase(db =>
    db.StartsWith(ProcessorIdConstants.SqlServer, StringComparison.InvariantCultureIgnoreCase) ||
    db.StartsWith(ProcessorIdConstants.Postgres, StringComparison.InvariantCultureIgnoreCase)
).Execute.Sql("...");
```

### Provider Detection

```csharp
[Migration(202401151000)]
public class DatabaseProviderConditionals : Migration
{
    public override void Up()
    {
        Create.Table("CrossPlatformTable")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable();

        // SQL Server specific features
        IfDatabase(ProcessorIdConstants.SqlServer).Execute.Sql(@"
            CREATE INDEX IX_CrossPlatformTable_Name_Active
            ON CrossPlatformTable (Name)
            WHERE Name IS NOT NULL");

        // PostgreSQL specific features
        IfDatabase(ProcessorIdConstants.Postgres).Execute.Sql(@"
                    CREATE INDEX IX_CrossPlatformTable_JsonData
                    ON CrossPlatformTable USING GIN (JsonData)");

        // MySQL specific features
        IfDatabase(ProcessorIdConstants.MySql).Execute.Sql("ALTER TABLE CrossPlatformTable ENGINE=InnoDB");

        // SQLite specific features
        IfDatabase(ProcessorIdConstants.SQLite).Execute.Sql("CREATE INDEX IX_CrossPlatformTable_Name ON CrossPlatformTable (Name)");

        // Oracle specific features
        IfDatabase(ProcessorIdConstants.Oracle).Execute.Sql("CREATE INDEX IX_CrossPlatformTable_Name ON CrossPlatformTable (UPPER(Name))");
    }

    public override void Down()
    {
        Delete.Table("CrossPlatformTable");
    }
}
```

## Schema State Conditionals

### Conditional Based on Existing Schema

```csharp
[Migration(202401151400)]
public class SchemaStateConditionals : Migration
{
    public override void Up()
    {
        // Check if tables exist before creating or altering them
        if (!Schema.Table("Users").Exists())
        {
            CreateUsersTable();
        }
        else
        {
            ModifyExistingUsersTable();
        }

        // Conditional column addition based on existing columns
        if (!Schema.Table("Users").Column("Email").Exists())
        {
            Alter.Table("Users")
                .AddColumn("Email").AsString(255).Nullable();
        }

        // Conditional index creation
        if (!Schema.Table("Users").Index("IX_Users_Email").Exists())
        {
            Create.Index("IX_Users_Email")
                .OnTable("Users")
                .OnColumn("Email");
        }

        // Conditional constraint creation
        if (!Schema.Table("Users").Constraint("CK_Users_Email_Format").Exists())
        {
            Execute.Sql(@"
                ALTER TABLE Users
                ADD CONSTRAINT CK_Users_Email_Format
                CHECK (Email IS NULL OR Email LIKE '%@%')");
        }

        // Check for foreign key relationships
        if (Schema.Table("Orders").Exists() &&
            !Schema.Table("Orders").Constraint("FK_Orders_Users").Exists())
        {
            Create.ForeignKey("FK_Orders_Users")
                .FromTable("Orders").ForeignColumn("UserId")
                .ToTable("Users").PrimaryColumn("Id");
        }
    }

    private void CreateUsersTable()
    {
        Console.WriteLine("Creating Users table...");

        Create.Table("Users")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Username").AsString(50).NotNullable()
            .WithColumn("Email").AsString(255).Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime);
    }

    private void ModifyExistingUsersTable()
    {
        Console.WriteLine("Modifying existing Users table...");

        // Only add columns that don't exist
        if (!Schema.Table("Users").Column("LastLoginAt").Exists())
        {
            Alter.Table("Users")
                .AddColumn("LastLoginAt").AsDateTime().Nullable();
        }

        if (!Schema.Table("Users").Column("IsActive").Exists())
        {
            Alter.Table("Users")
                .AddColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true);
        }
    }

    public override void Down()
    {
        // Conditional rollback - only drop if we created it
        if (Schema.Table("Users").Exists())
        {
            // Remove constraints we may have added
            Execute.Sql("ALTER TABLE Users DROP CONSTRAINT IF EXISTS CK_Users_Email_Format");

            // Remove indexes we may have added
            if (Schema.Table("Users").Index("IX_Users_Email").Exists())
            {
                Delete.Index("IX_Users_Email").OnTable("Users");
            }

            // Remove columns we may have added
            if (Schema.Table("Users").Column("LastLoginAt").Exists())
            {
                Delete.Column("LastLoginAt").FromTable("Users");
            }

            if (Schema.Table("Users").Column("IsActive").Exists())
            {
                Delete.Column("IsActive").FromTable("Users");
            }
        }
    }
}
```

## Custom Conditional Logic

### Feature Flag-Based Conditionals

```csharp
[Migration(202401151600)]
public class FeatureFlagConditionals : Migration
{
    public override void Up()
    {
        var featureFlags = GetFeatureFlags();

        Create.Table("FeatureBasedTable")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable();

        // Apply features based on flags
        if (featureFlags.Contains("ENHANCED_SEARCH"))
        {
            EnableEnhancedSearch();
        }

        if (featureFlags.Contains("ADVANCED_ANALYTICS"))
        {
            EnableAdvancedAnalytics();
        }
    }

    private HashSet<string> GetFeatureFlags()
    {
        var flagsString = Environment.GetEnvironmentVariable("FEATURE_FLAGS") ?? "";
        return flagsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim().ToUpper())
            .ToHashSet();
    }

    private void EnableEnhancedSearch()
    {
        Console.WriteLine("Enabling enhanced search features...");

        Alter.Table("FeatureBasedTable")
            .AddColumn("SearchKeywords").AsString(1000).Nullable()
            .AddColumn("SearchRanking").AsInt32().NotNullable().WithDefaultValue(0);

        // Create full-text search support
        IfDatabase(ProcessorIdConstants.SqlServer).Execute.Sql("CREATE FULLTEXT CATALOG SearchCatalog AS DEFAULT");
        IfDatabase(ProcessorIdConstants.Postgres).Execute.Sql("ALTER TABLE FeatureBasedTable ADD COLUMN SearchVector tsvector");
    }

    private void EnableAdvancedAnalytics()
    {
        Console.WriteLine("Enabling advanced analytics features...");

        // Create analytics tables
        Create.Table("AnalyticsEvents")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("EntityId").AsInt32().NotNullable()
            .WithColumn("EventType").AsString(50).NotNullable()
            .WithColumn("EventData").AsString(2000).Nullable()
            .WithColumn("UserId").AsInt32().Nullable()
            .WithColumn("SessionId").AsString(100).Nullable()
            .WithColumn("Timestamp").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime);

        Create.Table("AnalyticsSummary")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("EntityId").AsInt32().NotNullable()
            .WithColumn("SummaryDate").AsDate().NotNullable()
            .WithColumn("ViewCount").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("InteractionCount").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("UniqueUsers").AsInt32().NotNullable().WithDefaultValue(0);

        // Add analytics columns to main table
        Alter.Table("FeatureBasedTable")
            .AddColumn("AnalyticsEnabled").AsBoolean().NotNullable().WithDefaultValue(true)
            .AddColumn("LastAnalyticsUpdate").AsDateTime().Nullable();
    }

    public override void Down()
    {
        // Clean up feature-specific tables and columns
        Execute.Sql("DROP TABLE IF EXISTS SecurityEvents");
        Execute.Sql("DROP TABLE IF EXISTS AnalyticsSummary");
        Execute.Sql("DROP TABLE IF EXISTS AnalyticsEvents");

        IfDatabase(ProcessorIdConstants.SqlServer).Execute.Sql("DROP FULLTEXT INDEX ON FeatureBasedTable");

        Delete.Table("FeatureBasedTable");
    }
}
```

## Best Practices for Conditional Logic

### ✅ Do:
- Use database provider conditionals for database-specific optimizations
- Validate schema state before making assumptions
- Document all conditional logic clearly
- Test all conditional paths thoroughly

### ❌ Don't:
- Rely on conditionals for core business logic that should be in application code
- Create overly complex nested conditions that are hard to maintain
- Skip error handling in conditional branches
- Make assumptions about schema state without validation
- Use conditionals to work around poor design decisions

Conditional logic in migrations enables powerful, adaptive database evolution while maintaining compatibility across different environments and scenarios.
