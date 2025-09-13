# Auto-Reversing Migrations

FluentMigrator supports automatic reversal of many migration operations through its auto-reversing capability. When a migration implements only the `Up()` method, FluentMigrator can often automatically generate the appropriate `Down()` operations to reverse the changes.

## How Auto-Reversing Works

FluentMigrator tracks the operations performed in your `Up()` method and can reverse many of them automatically when rolling back migrations. This feature reduces boilerplate code and ensures consistency between forward and backward migrations.

### Supported Auto-Reversing Operations

The following operations support automatic reversal:

#### Table Operations
```csharp
public class CreateUserTable : AutoReversingMigration
{
    public override void Up()
    {
        // ✅ Auto-reversible: CREATE TABLE → DROP TABLE
        Create.Table("Users")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Username").AsString(50).NotNullable()
            .WithColumn("Email").AsString(255).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable();
    }

    // Down() method not needed - FluentMigrator will generate:
    // Delete.Table("Users");
}
```

#### Column Operations
```csharp
public class AddUserColumns : AutoReversingMigration
{
    public override void Up()
    {
        // ✅ Auto-reversible: ADD COLUMN → DROP COLUMN
        Alter.Table("Users")
            .AddColumn("FirstName").AsString(100).Nullable()
            .AddColumn("LastName").AsString(100).Nullable()
            .AddColumn("PhoneNumber").AsString(20).Nullable();
    }

    // Automatically generates:
    // Alter.Table("Users").DropColumn("PhoneNumber");
    // Alter.Table("Users").DropColumn("LastName");
    // Alter.Table("Users").DropColumn("FirstName");
}
```

#### Index Operations
```csharp
public class AddUserIndexes : AutoReversingMigration
{
    public override void Up()
    {
        // ✅ Auto-reversible: CREATE INDEX → DROP INDEX
        Create.Index("IX_Users_Username").OnTable("Users")
            .OnColumn("Username").Ascending()
            .WithOptions().Unique();

        Create.Index("IX_Users_Email").OnTable("Users")
            .OnColumn("Email").Ascending();
    }

    // Automatically generates:
    // Delete.Index("IX_Users_Email").OnTable("Users");
    // Delete.Index("IX_Users_Username").OnTable("Users");
}
```

#### Foreign Key Constraints
```csharp
public class AddOrderForeignKeys : AutoReversingMigration
{
    public override void Up()
    {
        // ✅ Auto-reversible: ADD CONSTRAINT → DROP CONSTRAINT
        Create.ForeignKey("FK_Orders_Users")
            .FromTable("Orders").ForeignColumn("UserId")
            .ToTable("Users").PrimaryColumn("Id")
            .OnDelete(Rule.Cascade);
    }

    // Automatically generates:
    // Delete.ForeignKey("FK_Orders_Users").OnTable("Orders");
}
```

#### Schema Operations
```csharp
public class CreateReportingSchema : AutoReversingMigration
{
    public override void Up()
    {
        // ✅ Auto-reversible: CREATE SCHEMA → DROP SCHEMA
        Create.Schema("Reporting");

        Create.Table("Reports")
            .InSchema("Reporting")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey()
            .WithColumn("Name").AsString(200).NotNullable();
    }

    // Automatically generates:
    // Delete.Table("Reports").InSchema("Reporting");
    // Delete.Schema("Reporting");
}
```

### Complete Auto-Reversible Example

```csharp
[Migration(20241201120000)]
public class CreateOrdersSystem : AutoReversingMigration
{
    public override void Up()
    {
        // Create tables
        Create.Table("Categories")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Description").AsString(500).Nullable();

        Create.Table("Products")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(200).NotNullable()
            .WithColumn("CategoryId").AsInt32().NotNullable()
            .WithColumn("Price").AsDecimal(10, 2).NotNullable()
            .WithColumn("SKU").AsString(50).NotNullable();

        Create.Table("Orders")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("UserId").AsInt32().NotNullable()
            .WithColumn("OrderDate").AsDateTime().NotNullable()
            .WithColumn("TotalAmount").AsDecimal(12, 2).NotNullable();

        // Create indexes
        Create.Index("IX_Products_CategoryId").OnTable("Products")
            .OnColumn("CategoryId").Ascending();

        Create.Index("IX_Products_SKU").OnTable("Products")
            .OnColumn("SKU").Ascending()
            .WithOptions().Unique();

        Create.Index("IX_Orders_UserId").OnTable("Orders")
            .OnColumn("UserId").Ascending();

        // Create foreign keys
        Create.ForeignKey("FK_Products_Categories")
            .FromTable("Products").ForeignColumn("CategoryId")
            .ToTable("Categories").PrimaryColumn("Id");

        Create.ForeignKey("FK_Orders_Users")
            .FromTable("Orders").ForeignColumn("UserId")
            .ToTable("Users").PrimaryColumn("Id");
    }

    // No Down() method needed!
    // FluentMigrator will automatically generate the reverse operations
}
```

## Operations That Don't Auto-Reverse

Some operations cannot be automatically reversed because they might result in data loss or require custom logic:

### Data Operations
```csharp
public class SeedInitialData : Migration
{
    public override void Up()
    {
        // ❌ Not auto-reversible: Data operations
        Insert.IntoTable("Categories").Row(new { Name = "Electronics" });
        Insert.IntoTable("Categories").Row(new { Name = "Books" });

        Update.Table("Users").Set(new { IsActive = true }).AllRows();

        Delete.FromTable("TempData").AllRows();
    }

    // Must provide explicit Down() method
    public override void Down()
    {
        Delete.FromTable("Categories").Row(new { Name = "Electronics" });
        Delete.FromTable("Categories").Row(new { Name = "Books" });

        Update.Table("Users").Set(new { IsActive = false }).AllRows();

        // Cannot restore deleted TempData
    }
}
```

### Execute.Sql Operations
```csharp
public class CustomSqlOperations : Migration
{
    public override void Up()
    {
        // ❌ Not auto-reversible: Custom SQL
        Execute.Sql("CREATE VIEW UserOrderSummary AS SELECT...");
        Execute.Sql("UPDATE Users SET Status = 'Active' WHERE Status IS NULL");
        Execute.Sql("CREATE TRIGGER tr_Users_Audit...");
    }

    // Must provide explicit Down() method
    public override void Down()
    {
        Execute.Sql("DROP TRIGGER tr_Users_Audit");
        Execute.Sql("UPDATE Users SET Status = NULL WHERE Status = 'Active'");
        Execute.Sql("DROP VIEW UserOrderSummary");
    }
}
```

### Column Modifications
```csharp
public class ModifyUserColumns : Migration
{
    public override void Up()
    {
        // ❌ Not auto-reversible: Column modifications might cause data loss
        Alter.Column("Username").OnTable("Users").AsString(100).NotNullable(); // Was 50
        Alter.Column("Email").OnTable("Users").AsString(320).NotNullable();    // Was 255
    }

    // Must provide explicit Down() method
    public override void Down()
    {
        Alter.Column("Email").OnTable("Users").AsString(255).NotNullable();
        Alter.Column("Username").OnTable("Users").AsString(50).NotNullable();
    }
}
```

## Mixed Auto-Reversible and Custom Down

You can combine auto-reversible operations with custom Down() logic:

```csharp
public class MixedOperations : AutoReversingMigration
{
    public override void Up()
    {
        // Auto-reversible operations
        Create.Table("UserPreferences")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("UserId").AsInt32().NotNullable()
            .WithColumn("PreferenceName").AsString(50).NotNullable()
            .WithColumn("PreferenceValue").AsString(500).Nullable();

        Create.Index("IX_UserPreferences_UserId").OnTable("UserPreferences")
            .OnColumn("UserId").Ascending();

        // Non-auto-reversible operations
        Insert.IntoTable("UserPreferences").Row(new
        {
            UserId = 1,
            PreferenceName = "Theme",
            PreferenceValue = "Dark"
        });

        Execute.Sql("UPDATE Users SET LastLoginAt = GETDATE() WHERE LastLoginAt IS NULL");
    }

    public override void Down()
    {
        // Handle non-auto-reversible operations
        Execute.Sql("UPDATE Users SET LastLoginAt = NULL WHERE LastLoginAt IS NOT NULL");
        Delete.FromTable("UserPreferences").AllRows();

        // Auto-reversible operations (table, index) will be handled automatically
        // You don't need to explicitly delete them in Down()
    }
}
```

## Auto-Reversal Order

FluentMigrator reverses operations in the opposite order they were created, respecting dependency relationships:

```csharp
public override void Up()
{
    // Step 1: Create table
    Create.Table("Orders");

    // Step 2: Create index
    Create.Index("IX_Orders_Date").OnTable("Orders");

    // Step 3: Create foreign key
    Create.ForeignKey("FK_Orders_Users").FromTable("Orders");
}

// Auto-generated Down() equivalent:
public override void Down()
{
    // Step 3 reversed first: Drop foreign key
    Delete.ForeignKey("FK_Orders_Users").OnTable("Orders");

    // Step 2 reversed: Drop index
    Delete.Index("IX_Orders_Date").OnTable("Orders");

    // Step 1 reversed last: Drop table
    Delete.Table("Orders");
}
```

## Disabling Auto-Reversal

If you want to provide a custom `Down()` method for a migration that could be auto-reversed, simply implement the `Down()` method. FluentMigrator will use your custom implementation instead of generating automatic reversal:

```csharp
public class CustomReversalLogic : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("AuditLog")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Action").AsString(100).NotNullable()
            .WithColumn("Timestamp").AsDateTime().NotNullable();
    }

    public override void Down()
    {
        // Custom logic: Archive audit data before dropping table
        Execute.Sql(@"
            INSERT INTO ArchivedAuditLog (Action, Timestamp, ArchivedAt)
            SELECT Action, Timestamp, GETDATE()
            FROM AuditLog");

        Delete.Table("AuditLog");
    }
}
```

## Best Practices

### ✅ Recommended Practices

1. **Use auto-reversal for simple structural changes** (tables, columns, indexes, constraints)
2. **Test rollback operations** in development environments
3. **Provide explicit Down() methods for data operations** and complex logic
4. **Document any data loss implications** in migration comments
5. **Use auto-reversal to reduce code duplication** and maintain consistency

### ❌ Avoid

1. **Don't rely on auto-reversal for data migrations** - always provide explicit Down() methods
2. **Don't assume auto-reversal works for custom SQL** - it only applies to fluent API operations
3. **Don't forget to test rollback scenarios** even with auto-reversible migrations
4. **Don't mix auto-reversal with partial custom Down() methods** without understanding the interaction

## Complex Scenarios

### Conditional Auto-Reversal
```csharp
public class ConditionalOperations : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("PlatformSpecificTable")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity();

        // Database-specific operations - both auto-reversible
        IfDatabase(ProcessorIdConstants.SqlServer)
            .Create.Index("IX_PlatformSpecific").OnTable("PlatformSpecificTable")
                .OnColumn("Id").Ascending()
                .WithOptions().NonClustered();

        IfDatabase(ProcessorIdConstants.Postgres)
            .Create.Index("IX_PlatformSpecific").OnTable("PlatformSpecificTable")
                .OnColumn("Id").Ascending();
    }

    // Auto-reversal will handle database-specific operations correctly
}
```

### Schema Evolution with Auto-Reversal
```csharp
public class SchemaEvolution : AutoReversingMigration
{
    public override void Up()
    {
        // Phase 1: Create new schema structure (auto-reversible)
        Create.Schema("V2");

        Create.Table("NewUsers")
            .InSchema("V2")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("FullName").AsString(200).NotNullable()
            .WithColumn("EmailAddress").AsString(320).NotNullable();

        // Phase 2: Data migration (requires custom Down())
        Execute.Sql(@"
            INSERT INTO V2.NewUsers (FullName, EmailAddress)
            SELECT FirstName + ' ' + LastName, Email
            FROM Users");
    }

    public override void Down()
    {
        // Handle data migration reversal
        Delete.FromTable("NewUsers").InSchema("V2").AllRows();

        // Structure changes (table, schema) will be auto-reversed
    }
}
```

## Error Handling in Auto-Reversible Migrations

FluentMigrator's auto-reversal respects transaction boundaries. If a rollback fails, the entire migration transaction is rolled back:

```csharp
public class SafeAutoReversibleMigration : AutoReversingMigration
{
    public override void Up()
    {
        try
        {
            // Auto-reversible operations within transaction
            Create.Table("TemporaryProcessing")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Data").AsString(1000).NotNullable();

            Create.Index("IX_Temp_Data").OnTable("TemporaryProcessing")
                .OnColumn("Data").Ascending();

            // If this fails, auto-reversible operations are rolled back too
            Execute.Sql("INSERT INTO TemporaryProcessing (Data) VALUES ('Test')");
        }
        catch (Exception)
        {
            // FluentMigrator will handle rollback of auto-reversible operations
            throw;
        }
    }

    // Even with errors, auto-reversal ensures clean rollback
}
```

Auto-reversing migrations in FluentMigrator provide a powerful way to reduce boilerplate code while maintaining clean rollback capabilities. Use them for structural changes while always implementing custom Down() methods for data operations and complex business logic.
