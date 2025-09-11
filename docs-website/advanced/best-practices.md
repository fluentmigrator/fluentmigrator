# Best Practices

This comprehensive guide covers best practices for using FluentMigrator effectively, including database design, migration patterns, team collaboration, testing strategies, and production deployment considerations.

## Migration Design Principles

### 1. Keep Migrations Small and Focused

```csharp
// ✅ Good: Single focused migration
[Migration(202401011200, "Add email column to Users table")]
public class AddEmailToUsers : Migration
{
    public override void Up()
    {
        Alter.Table("Users")
            .AddColumn("Email").AsString(255).Nullable();
    }

    public override void Down()
    {
        Delete.Column("Email").FromTable("Users");
    }
}

// ❌ Bad: Multiple unrelated changes in one migration
[Migration(202401011300, "Add email column and create Products table")]
public class MassiveChanges : Migration
{
    public override void Up()
    {
        // Adding email column
        Alter.Table("Users").AddColumn("Email").AsString(255).Nullable();

        // Creating completely unrelated table
        Create.Table("Products")...

        // Modifying another unrelated table
        Alter.Table("Orders")...
    }
    // This makes it hard to rollback specific changes
}
```

### 2. Always Provide Rollback Logic

```csharp
[Migration(202401011400, "Add status column to Users table with default value")]
public class AddUserStatusColumn : Migration
{
    public override void Up()
    {
        Alter.Table("Users")
            .AddColumn("Status").AsString(20).NotNullable().WithDefaultValue("Active");

        // Update existing users to have a status
        Execute.Sql("UPDATE Users SET Status = 'Active' WHERE Status IS NULL");
    }

    public override void Down()
    {
        // Always provide meaningful rollback
        Delete.Column("Status").FromTable("Users");
    }
}
```

### 3. Use Descriptive Migration Names, Description and Version Numbers

```csharp
// ✅ Good: Descriptive names with meaningful timestamps
[Migration(202401151430, "Add user email verification columns")] // YYYYMMDDHHNN format
public class AddUserEmailVerificationColumns : Migration { }

[Migration(202401151435, "Create product catalog tables")]
public class CreateProductCatalogTables : Migration { }

[Migration(202401151440, "Add indexes to User table for performance")]
public class AddIndexesToUserTable : Migration { }

// ❌ Bad: Generic names and random numbers without description
[Migration(1)]
public class Migration1 : Migration { }

[Migration(12345)]
public class UpdateStuff : Migration { }
```

## Team Collaboration Best Practices

### 1. Migration Naming and Organization

```csharp
// Use consistent timestamp format: YYYYMMDDHHNN
// Group related migrations by feature/story

// Feature: User Profile Enhancement
[Migration(202401201400)]
public class AddUserProfileFields : Migration { }

[Migration(202401201405)]
public class AddUserPreferencesTable : Migration { }

[Migration(202401201410)]
public class AddUserProfileIndexes : Migration { }

// Feature: Order Management System
[Migration(202401201500)]
public class CreateOrderTables : Migration { }

[Migration(202401201505)]
public class AddOrderStatusTracking : Migration { }

[Migration(202401201510)]
public class AddOrderIndexesAndConstraints : Migration { }
```

### 2. Documentation in Migrations

```csharp
[Migration(202401201600)]
public class AddUserTierSystemForBillingRestructure : Migration
{
    /// <summary>
    /// Story: US-1234 - Implement user tier system for new billing structure
    ///
    /// This migration adds support for user tiers (Basic, Premium, Enterprise)
    /// to support the new billing system launching in Q2.
    ///
    /// Breaking Changes: None - all existing users will default to 'Basic' tier
    /// Data Migration: All existing users will be set to 'Basic' tier
    /// </summary>
    public override void Up()
    {
        // Add user tier support
        Alter.Table("Users")
            .AddColumn("UserTier").AsString(20).NotNullable().WithDefaultValue("Basic");

        // Add constraint to ensure valid tier values
        Execute.Sql(@"
            ALTER TABLE Users
            ADD CONSTRAINT CK_Users_UserTier
            CHECK (UserTier IN ('Basic', 'Premium', 'Enterprise'))");

        // Create billing-related table for tier pricing
        Create.Table("UserTierPricing")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("TierName").AsString(20).NotNullable()
            .WithColumn("MonthlyPrice").AsDecimal(10, 2).NotNullable()
            .WithColumn("AnnualPrice").AsDecimal(10, 2).NotNullable()
            .WithColumn("MaxUsers").AsInt32().Nullable() // NULL = unlimited
            .WithColumn("EffectiveDate").AsDateTime().NotNullable()
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true);

        // Insert initial pricing data
        Insert.IntoTable("UserTierPricing")
            .Row(new { TierName = "Basic", MonthlyPrice = 0.00m, AnnualPrice = 0.00m, MaxUsers = 5, EffectiveDate = DateTime.Now, IsActive = true })
            .Row(new { TierName = "Premium", MonthlyPrice = 29.99m, AnnualPrice = 299.99m, MaxUsers = 50, EffectiveDate = DateTime.Now, IsActive = true })
            .Row(new { TierName = "Enterprise", MonthlyPrice = 99.99m, AnnualPrice = 999.99m, MaxUsers = (int?)null, EffectiveDate = DateTime.Now, IsActive = true });
    }

    public override void Down()
    {
        Delete.Table("UserTierPricing");

        Execute.Sql("ALTER TABLE Users DROP CONSTRAINT CK_Users_UserTier");
        Delete.Column("UserTier").FromTable("Users");
    }
}
```

## Testing Strategies

### 1. Migration Testing Patterns

```csharp
// Create a base test class for migration testing
public abstract class MigrationTestBase
{
    protected IServiceProvider ServiceProvider { get; private set; }
    protected IMigrationRunner Runner { get; private set; }

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString("Data Source=:memory:")
                .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        ServiceProvider = services.BuildServiceProvider(false);
        Runner = ServiceProvider.GetRequiredService<IMigrationRunner>();
    }

    [TearDown]
    public void TearDown()
    {
        ServiceProvider?.Dispose();
    }
}

// Example migration test
[TestFixture]
public class AddUserEmailMigrationTests : MigrationTestBase
{
    [Test]
    public void Migration_ShouldAddEmailColumn_WhenApplied()
    {
        // Arrange
        Runner.MigrateUp(202401201000); // Run migrations up to before our target migration

        // Act
        Runner.MigrateUp(202401201100); // Run our specific migration

        // Assert
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var columnExists = connection.ExecuteScalar<int>(@"
            SELECT COUNT(*)
            FROM pragma_table_info('Users')
            WHERE name = 'Email'") > 0;

        Assert.IsTrue(columnExists, "Email column should exist after migration");
    }

    [Test]
    public void Migration_ShouldRemoveEmailColumn_WhenRolledBack()
    {
        // Arrange
        Runner.MigrateUp(202401201100);

        // Act
        Runner.MigrateDown(202401201000);

        // Assert
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var columnExists = connection.ExecuteScalar<int>(@"
            SELECT COUNT(*)
            FROM pragma_table_info('Users')
            WHERE name = 'Email'") > 0;

        Assert.IsFalse(columnExists, "Email column should not exist after rollback");
    }

    [Test]
    public void Migration_ShouldPreserveExistingData_WhenApplied()
    {
        // Arrange
        Runner.MigrateUp(202401201000);

        // Insert test data
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        connection.Execute("INSERT INTO Users (Username, Status, CreatedAt) VALUES ('testuser', 'Active', datetime('now'))");

        var originalCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Users");

        // Act
        Runner.MigrateUp(202401201100);

        // Assert
        var newCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Users");
        Assert.AreEqual(originalCount, newCount, "Migration should preserve existing data");

        // Verify new column is present and nullable
        var emailValue = connection.ExecuteScalar("SELECT Email FROM Users WHERE Username = 'testuser'");
        Assert.IsNull(emailValue, "New Email column should be nullable for existing records");
    }
}
```

### 2. Integration Testing

```csharp
[TestFixture]
public class DatabaseMigrationIntegrationTests
{
    private string _connectionString;
    private IServiceProvider _serviceProvider;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Use a test database for integration tests
        _connectionString = "Data Source=test_migration_db.sqlite";

        _serviceProvider = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(_connectionString)
                .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .BuildServiceProvider(false);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _serviceProvider?.Dispose();
        if (File.Exists("test_migration_db.sqlite"))
            File.Delete("test_migration_db.sqlite");
    }

    [Test]
    public void FullMigrationCycle_ShouldCompleteSuccessfully()
    {
        // Arrange
        var runner = _serviceProvider.GetRequiredService<IMigrationRunner>();

        // Act & Assert - Run all migrations up
        Assert.DoesNotThrow(() => runner.MigrateUp());

        // Verify final database state
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var tables = connection.Query<string>(@"
            SELECT name FROM sqlite_master
            WHERE type='table' AND name NOT LIKE 'sqlite_%'
            ORDER BY name");

        var expectedTables = new[] { "Users", "Companies", "Orders", "VersionInfo" };
        CollectionAssert.IsSubsetOf(expectedTables, tables.ToList());

        // Test rollback of last few migrations
        Assert.DoesNotThrow(() => runner.Rollback(3));

        // Test re-applying migrations
        Assert.DoesNotThrow(() => runner.MigrateUp());
    }

    [Test]
    public void Migration_ShouldHandleConcurrentExecution()
    {
        // Test that migration runner handles concurrent execution properly
        var runner1 = _serviceProvider.GetRequiredService<IMigrationRunner>();
        var runner2 = _serviceProvider.GetRequiredService<IMigrationRunner>();

        var task1 = Task.Run(() => runner1.MigrateUp());
        var task2 = Task.Run(() => runner2.MigrateUp());

        Assert.DoesNotThrow(() => Task.WaitAll(task1, task2));
    }
}
```

This comprehensive best practices guide provides the foundation for successful database migrations with FluentMigrator. Following these patterns will help ensure your migrations are reliable, maintainable, and safe for production deployment.
