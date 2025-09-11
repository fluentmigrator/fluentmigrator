# PostgreSQL Provider

FluentMigrator provides comprehensive support for PostgreSQL with extensions for PostgreSQL-specific features.

## Supported Versions

FluentMigrator supports:
- **PostgreSQL 16** ✅ (Latest)
- **PostgreSQL 15** ✅
- **PostgreSQL 14** ✅
- **PostgreSQL 13** ✅
- **PostgreSQL 12** ✅
- **PostgreSQL 11** ✅ (Minimum supported)

## Installation

Install the PostgreSQL provider package:

```bash
# For .NET CLI
dotnet add package FluentMigrator.Runner.Postgres

# For Package Manager Console
Install-Package FluentMigrator.Runner.Postgres
```

## Configuration

### Basic Configuration
```csharp
services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString("Host=localhost;Database=myapp;Username=myuser;Password=mypass")
        .ScanIn(typeof(MyMigration).Assembly).For.Migrations());
```

## Column types

## PostgreSQL Extensions Package

For advanced PostgreSQL features, install the extensions package:

```bash
# For .NET CLI
dotnet add package FluentMigrator.Extensions.Postgres

# For Package Manager Console
Install-Package FluentMigrator.Extensions.Postgres
```

```xml
<PackageReference Include="FluentMigrator.Extensions.Postgres" Version="7.2.0" />
```

## PostgreSQL Specific Features

### Data Types

Column types are specified in the [PostgresTypeMap](https://github.com/fluentmigrator/fluentmigrator/blob/main/src/FluentMigrator.Runner.Postgres/Generators/Postgres/PostgresTypeMap.cs).

### Sequences

#### Creating Sequences
```csharp
Create.Sequence("user_id_seq")
    .StartWith(1000)
    .IncrementBy(1)
    .MinValue(1000)
    .MaxValue(999999999)
    .Cache(50);
```

#### Using Sequences
```csharp
Create.Table("Users")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey()
        .WithDefaultValue(RawSql.Insert("nextval('user_id_seq')"))
    .WithColumn("Username").AsString(50).NotNullable();
```

### Indexes

### Indexes and Advanced Index Types

#### Basic PostgreSQL Index Types
```csharp
using FluentMigrator.Postgres;

Create.Table("Users")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey()
    .WithColumn("Email").AsString(255).NotNullable()
    .WithColumn("JsonData").AsCustom("JSONB").Nullable()
    .WithColumn("SearchVector").AsCustom("TSVECTOR").Nullable();

// B-tree index (default)
Create.Index("IX_Users_Email").OnTable("Users")
    .OnColumn("Email").Ascending();
```

#### Algorithm-Specific Indexes
```csharp
// B-tree index (default, most common)
Create.Index("IX_Users_Email_Btree").OnTable("Users")
    .OnColumn("Email")
    .UsingIndexAlgorithm(PostgresIndexAlgorithm.BTree);

// Hash index for equality comparisons only
Create.Index("IX_Users_Status_Hash").OnTable("Users")
    .OnColumn("Status")
    .UsingIndexAlgorithm(PostgresIndexAlgorithm.Hash);

// GIN index for full-text search and JSON/array operations
Create.Index("IX_Articles_Content_Gin").OnTable("Articles")
    .OnColumn("ContentVector")
    .UsingIndexAlgorithm(PostgresIndexAlgorithm.Gin);

Create.Index("IX_Users_JsonData").OnTable("Users")
    .OnColumn("JsonData")
    .UsingIndexAlgorithm(PostgresIndexAlgorithm.Gin);

// GiST index for geometric data and full-text search
Create.Index("IX_Locations_Point_Gist").OnTable("Locations")
    .OnColumn("Coordinates")
    .UsingIndexAlgorithm(PostgresIndexAlgorithm.Gist);

Create.Index("IX_Users_SearchVector").OnTable("Users")
    .OnColumn("SearchVector")
    .UsingIndexAlgorithm(PostgresIndexAlgorithm.Gist);
```

#### Partial Indexes
```csharp
// Index only active users
Create.Index("IX_Users_Active").OnTable("Users")
    .OnColumn("Email")
    .Where("is_active = true");

// Complex partial index
Create.Index("IX_Users_ActiveEmail").OnTable("Users")
    .OnColumn("Email")
    .Where("is_active = true");

Create.Index("IX_Orders_RecentPending").OnTable("Orders")
    .OnColumn("CreatedAt").Descending()
    .Where("status = 'pending' AND created_at > NOW() - INTERVAL '30 days'");
```

#### Expression Indexes
```csharp
// Index on expression for case-insensitive searches
Create.Index("IX_Users_LowerEmail").OnTable("Users")
    .OnColumn(RawSql.Insert("lower(email)"));

// Multi-column expression index
Create.Index("IX_Users_FullName").OnTable("Users")
    .OnColumn(RawSql.Insert("lower(first_name || ' ' || last_name)"));

// Functional index for JSON operations
Create.Index("IX_UserPreferences_Theme").OnTable("UserPreferences")
    .OnColumn(RawSql.Insert("(settings->>'theme')"));
```

### Extensions

#### Enable Extensions
```csharp
[Migration(1)]
public class EnableExtensions : Migration
{
    public override void Up()
    {
        Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\"");
        Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"pg_trgm\"");
        Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"btree_gin\"");
    }

    public override void Down()
    {
        Execute.Sql("DROP EXTENSION IF EXISTS \"btree_gin\"");
        Execute.Sql("DROP EXTENSION IF EXISTS \"pg_trgm\"");
        Execute.Sql("DROP EXTENSION IF EXISTS \"uuid-ossp\"");
    }
}
```

#### Using UUID Extension
```csharp
Create.Table("Documents")
    .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
        .WithDefaultValue(RawSql.Insert("uuid_generate_v4()"))
    .WithColumn("Title").AsString(255).NotNullable();
```

### PostgreSQL Extensions Usage

#### Overriding Identity Values Extensions
For PostgreSQL identity columns, use `WithOverridingSystemValue()` and `WithOverridingUserValue()` extensions:

```csharp
using FluentMigrator.Postgres;

[Migration(1)]
public class PostgresIdentityInserts : Migration
{
    public override void Up()
    {
        // Table with GENERATED ALWAYS identity column
        Create.Table("Users")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey()
                .Identity(PostgresGenerationType.Always, 1, 1)
            .WithColumn("Name").AsString(100).NotNullable();

        // Table with GENERATED BY DEFAULT identity column
        Create.Table("Products")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey()
                .Identity(PostgresGenerationType.ByDefault, 100, 10)
            .WithColumn("Name").AsString(100).NotNullable();

        // Override system values for GENERATED ALWAYS columns
        Insert.IntoTable("Users")
            .WithOverridingSystemValue()
            .Row(new { Id = 100, Name = "System Admin" })
            .Row(new { Id = 200, Name = "Database Admin" });

        // Force system generation for GENERATED BY DEFAULT columns
        Insert.IntoTable("Products")
            .WithOverridingUserValue()
            .Row(new { Id = 500, Name = "Product A" }); // Id will be system-generated, ignoring 500

        // Normal insert for GENERATED BY DEFAULT (allows explicit values)
        Insert.IntoTable("Products")
            .Row(new { Id = 1000, Name = "Product B" }) // Uses explicit Id = 1000
            .Row(new { Name = "Product C" }); // Uses system-generated Id
    }

    public override void Down()
    {
        Delete.Table("Products");
        Delete.Table("Users");
    }
}
```

#### Identity Column Types Comparison
```csharp
// GENERATED ALWAYS AS IDENTITY - System always generates values
Create.Table("AlwaysIdentity")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey()
        .Identity(PostgresGenerationType.Always, 1, 1);

// GENERATED BY DEFAULT AS IDENTITY - Allows explicit values
Create.Table("DefaultIdentity")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey()
        .Identity(PostgresGenerationType.ByDefault, 1, 1);

// Insert scenarios:
// For GENERATED ALWAYS - need WithOverridingSystemValue() to insert explicit values
Insert.IntoTable("AlwaysIdentity")
    .WithOverridingSystemValue()
    .Row(new { Id = 100 });

// For GENERATED BY DEFAULT - explicit values work without extension
Insert.IntoTable("DefaultIdentity")
    .Row(new { Id = 200 }); // Works directly

// Force system generation for BY DEFAULT columns
Insert.IntoTable("DefaultIdentity")
    .WithOverridingUserValue()
    .Row(new { Id = 999 }); // Id ignored, system value used
```

#### Best Practices
- Use `PostgresGenerationType.ByDefault` for flexibility with occasional explicit values
- Use `PostgresGenerationType.Always` when you never want explicit identity values
- `WithOverridingSystemValue()` is only needed for `GENERATED ALWAYS` columns
- `WithOverridingUserValue()` forces system generation even for `GENERATED BY DEFAULT` columns

#### Advanced PostgreSQL Data Types
```csharp
Create.Table("AdvancedTypes")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("JsonData").AsCustom("JSONB").NotNullable()
    .WithColumn("UuidValue").AsGuid().NotNullable()
    .WithColumn("IpAddress").AsCustom("INET").Nullable()
    .WithColumn("MacAddress").AsCustom("MACADDR").Nullable()
    .WithColumn("IntegerRange").AsCustom("INT4RANGE").Nullable()
    .WithColumn("TimestampRange").AsCustom("TSRANGE").Nullable()
    .WithColumn("GeometricPoint").AsCustom("POINT").Nullable()
    .WithColumn("GeometricBox").AsCustom("BOX").Nullable()
    .WithColumn("NetworkCidr").AsCustom("CIDR").Nullable()
    .WithColumn("BitString").AsCustom("BIT(8)").Nullable()
    .WithColumn("VarBitString").AsCustom("VARBIT").Nullable();
```

### JSON/JSONB Operations

#### Working with JSONB
```csharp
// Table with JSONB column
Create.Table("UserPreferences")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("UserId").AsInt32().NotNullable()
    .WithColumn("Settings").AsCustom("JSONB").NotNullable();

// GIN index for JSONB operations
Create.Index("IX_UserPreferences_Settings_Gin").OnTable("UserPreferences")
    .OnColumn("Settings")
    .UsingIndexAlgorithm(PostgresIndexAlgorithm.Gin);

// Index on specific JSON key
Create.Index("IX_UserPreferences_Theme").OnTable("UserPreferences")
    .OnColumn(RawSql.Insert("(settings->>'theme')"));
```

#### Complex JSONB Operations
```csharp
// Insert JSONB data
Insert.IntoTable("UserPreferences")
    .Row(new {
        UserId = 1,
        Settings = RawSql.Insert("'{\"theme\": \"dark\", \"notifications\": true, \"language\": \"en\"}'")
    });

// Query specific JSON keys
Execute.Sql(@"
SELECT * FROM UserPreferences
WHERE settings->>'theme' = 'dark'
  AND (settings->>'notifications')::boolean = true;
");
```

### Full-Text Search

#### Text Search Configuration
```csharp
Create.Table("Articles")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("Title").AsString(255).NotNullable()
    .WithColumn("Content").AsString().NotNullable()
    .WithColumn("SearchVector").AsCustom("TSVECTOR").Nullable();

// Create a trigger to automatically update search vector
Execute.Sql(@"
CREATE OR REPLACE FUNCTION update_search_vector()
RETURNS TRIGGER AS $$
BEGIN
    NEW.search_vector := to_tsvector('english', coalesce(NEW.title,'') || ' ' || coalesce(NEW.content,''));
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER update_articles_search_vector
    BEFORE INSERT OR UPDATE ON Articles
    FOR EACH ROW EXECUTE FUNCTION update_search_vector();
");

// GiST index for text search
Create.Index("IX_Articles_SearchVector").OnTable("Articles")
    .OnColumn("SearchVector")
    .UsingIndexAlgorithm(PostgresIndexAlgorithm.Gist);
```

### Arrays

#### Array Columns
```csharp
Create.Table("UserPermissions")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("UserId").AsInt32().NotNullable()
    .WithColumn("Permissions").AsCustom("TEXT[]").NotNullable()
    .WithColumn("Tags").AsCustom("INTEGER[]").Nullable();

// GIN index for array operations
Create.Index("IX_UserPermissions_Permissions").OnTable("UserPermissions")
    .OnColumn("Permissions")
    .UsingIndexAlgorithm(PostgresIndexAlgorithm.Gin);
```

### Enums

#### PostgreSQL Enums
```csharp
[Migration(1)]
public class CreateEnumTypes : Migration
{
    public override void Up()
    {
        Execute.Sql("CREATE TYPE user_status AS ENUM ('active', 'inactive', 'pending', 'banned')");
        Execute.Sql("CREATE TYPE priority_level AS ENUM ('low', 'medium', 'high', 'urgent')");

        Create.Table("Users")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Username").AsString(50).NotNullable()
            .WithColumn("Status").AsCustom("user_status").NotNullable().WithDefaultValue(RawSql.Insert("'pending'"));

        Create.Table("Tasks")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Title").AsString(255).NotNullable()
            .WithColumn("Priority").AsCustom("priority_level").NotNullable().WithDefaultValue(RawSql.Insert("'medium'"));
    }

    public override void Down()
    {
        Delete.Table("Tasks");
        Delete.Table("Users");
        Execute.Sql("DROP TYPE IF EXISTS priority_level");
        Execute.Sql("DROP TYPE IF EXISTS user_status");
    }
}
```

### Table Inheritance

#### PostgreSQL Table Inheritance
```csharp
Create.Table("Users")
    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
    .WithColumn("Name").AsString(100).NotNullable()
    .WithColumn("Email").AsString(255).NotNullable();

// Child table inheriting from Users
Execute.Sql(@"
CREATE TABLE Employees (
    employee_id SERIAL,
    department_id INTEGER NOT NULL
) INHERITS (Users)");
```

## Performance Optimization

### VACUUM and ANALYZE
```csharp
[Maintenance(MigrationStage.AfterAll)]
public class MaintenanceMigration : Migration
{
    public override void Up()
    {
        // Perform maintenance after large data operations
        Execute.Sql("VACUUM ANALYZE Users");
        Execute.Sql("REINDEX INDEX IX_Users_Email");
    }

    public override void Down()
    {
        // Nothing to do
    }
}
```
## Common Issues and Solutions

### Issue: Case Sensitivity
PostgreSQL is case-sensitive for identifiers. Use consistent naming.

```csharp
// Good - consistent lowercase
Create.Table("users")
    .WithColumn("user_id").AsInt32().NotNullable().PrimaryKey()
    .WithColumn("user_name").AsString(100).NotNullable();

// Or use quoted identifiers for mixed case
Create.Table("Users")
    .WithColumn("UserId").AsInt32().NotNullable().PrimaryKey()
    .WithColumn("UserName").AsString(100).NotNullable();
```

### Issue: UUID Generation
Enable the uuid-ossp extension before using UUID functions:

```csharp
Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\"");

Create.Table("Documents")
    .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
        .WithDefaultValue(RawSql.Insert("uuid_generate_v4()"));
```

### Issue: JSON Operations
Use JSONB for better performance with JSON operations:

```csharp
// Better performance for queries
.WithColumn("Data").AsCustom("JSONB").NotNullable()

// Create GIN index for JSON operations
Create.Index("IX_Data_Gin").OnTable("MyTable")
    .OnColumn("Data")
    .UsingIndexAlgorithm(PostgresIndexAlgorithm.Gin);
```
