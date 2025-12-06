# Migration Versioning

Effective migration versioning is crucial for maintaining database schema consistency across environments and team members. This guide covers versioning strategies, numbering schemes, and best practices for managing migration versions in FluentMigrator.

## Version Numbering Strategies

### 1. Timestamp-Based Versioning (Recommended)

The most common and recommended approach uses timestamps in the format `YYYYMMDDHHNN`:

```csharp
[Migration(202401151430)] // January 15, 2024, 14:30
public class AddUserEmailColumn : Migration
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

[Migration(202401151435)] // January 15, 2024, 14:35
public class AddUserEmailIndex : Migration
{
    public override void Up()
    {
        Create.Index("IX_Users_Email")
            .OnTable("Users")
            .OnColumn("Email")
            .Unique();
    }

    public override void Down()
    {
        Delete.Index("IX_Users_Email").OnTable("Users");
    }
}
```

#### Advantages:
- Natural chronological ordering
- Easy to understand when migration was created
- Minimal conflicts in team environments
- Self-documenting timeline

#### Best Practices for Timestamps:
```csharp
// Use UTC time to avoid timezone confusion
[Migration(202401151430)] // Use UTC timestamp

// Leave gaps for emergency fixes
[Migration(202401151400)] // Main feature
[Migration(202401151405)] // Related changes
// Gap available for hotfix if needed: 202401151410-202401151459

// Group related migrations by time proximity
[Migration(202401151500)] // Start of new feature
[Migration(202401151505)] // Continue feature
[Migration(202401151510)] // Complete feature
```

### 2. Sequential Numbering

Simple incremental numbering approach:

```csharp
[Migration(1)]
public class CreateUserTable : Migration { /* ... */ }

[Migration(2)]
public class AddUserEmailColumn : Migration { /* ... */ }

[Migration(3)]
public class CreateProductTable : Migration { /* ... */ }
```

#### When to Use Sequential:
- Small teams with tight coordination
- Single developer projects
- When you need simple, predictable ordering

#### Challenges:
- High conflict potential in team environments
- No temporal information
- Difficult to insert emergency fixes

### Semantic Versioning Integration

Combine semantic version with sequential numbers:

```csharp
[Migration(10001)] // Version 1.0.0, Migration 1
public class CreateInitialSchema : Migration { /* ... */ }

[Migration(10002)] // Version 1.0.0, Migration 2
public class AddBasicIndexes : Migration { /* ... */ }

[Migration(11001)] // Version 1.1.0, Migration 1
public class AddUserProfiles : Migration { /* ... */ }

[Migration(20001)] // Version 2.0.0, Migration 1
public class MajorSchemaRefactor : Migration { /* ... */ }
```

### Feature-Based Versioning

Group migrations by feature with sub-versions:

```csharp
// User Management Feature (100xxx)
[Migration(100001)]
public class CreateUserTable : Migration { /* ... */ }

[Migration(100002)]
public class AddUserRoles : Migration { /* ... */ }

[Migration(100003)]
public class AddUserPreferences : Migration { /* ... */ }

// Product Catalog Feature (200xxx)
[Migration(200001)]
public class CreateProductTables : Migration { /* ... */ }

[Migration(200002)]
public class AddProductCategories : Migration { /* ... */ }

// Order Management Feature (300xxx)
[Migration(300001)]
public class CreateOrderTables : Migration { /* ... */ }
```

### Hotfix and Emergency Migrations

```csharp
// Regular migration
[Migration(202401151500)]
public class AddUserPreferences : Migration { /* ... */ }

// Emergency hotfix after the above migration
[Migration(20240115150001)] // Added seconds for emergency fix
public class FixUserPreferencesDefault : Migration
{
    public override void Up()
    {
        // Emergency fix for production issue
        Execute.Sql("UPDATE Users SET Preferences = '{}' WHERE Preferences IS NULL");
    }

    public override void Down()
    {
        Execute.Sql("UPDATE Users SET Preferences = NULL WHERE Preferences = '{}'");
    }
}
```

## Best Practices Summary

### ✅ Do:
- Use timestamp-based versioning (YYYYMMDDHHNN) for team environments
- Leave gaps in numbering for emergency fixes
- Document migration purpose and changes
- Validate version conflicts before deployment
- Test rollback scenarios

### ❌ Don't:
- Use sequential numbering in team environments without coordination
- Skip version validation in CI/CD pipelines
- Apply migrations without understanding dependencies
- Ignore out-of-order migration warnings
- Deploy migrations without rollback testing
- Use random or arbitrary version numbers

Effective migration versioning ensures smooth database evolution and team collaboration while maintaining system reliability and traceability.
