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

## Security Labels

::: warning Minimum Version
Security Labels support is available starting from **FluentMigrator 8.0**.
:::

PostgreSQL Security Labels allow you to apply access control labels to database objects. This is commonly used with security label providers like SELinux (sepgsql) or [PostgreSQL Anonymizer](https://postgresql-anonymizer.readthedocs.io/) (anon).

### Raw Security Labels

#### Creating Security Labels
```csharp
using FluentMigrator.Postgres;

// Create a security label on a table with a provider
Create.SecurityLabel("sepgsql")
    .OnTable("users")
    .InSchema("public")
    .WithLabel("system_u:object_r:sepgsql_table_t:s0");

// Create a security label on a column
Create.SecurityLabel("anon")
    .OnColumn("email")
    .OnTable("users")
    .InSchema("public")
    .WithLabel("MASKED WITH FUNCTION anon.fake_email()");

// Create a security label on a schema
Create.SecurityLabel("sepgsql")
    .OnSchema("public")
    .WithLabel("system_u:object_r:sepgsql_schema_t:s0");

// Create a security label on a role
Create.SecurityLabel("anon")
    .OnRole("masked_user")
    .WithLabel("MASKED");

// Create a security label on a view
Create.SecurityLabel("anon")
    .OnView("user_view")
    .InSchema("public")
    .WithLabel("TABLESAMPLE BERNOULLI(10)");
```

#### Deleting Security Labels
```csharp
// Delete a security label from a table
Delete.SecurityLabel()
    .For("anon")
    .FromTable("users")
    .InSchema("public");

// Delete a security label from a column
Delete.SecurityLabel()
    .For("anon")
    .FromColumn("email")
    .OnTable("users")
    .InSchema("public");

// Delete a security label from a role
Delete.SecurityLabel()
    .For("anon")
    .FromRole("masked_user");
```

### PostgreSQL Anonymizer Integration

FluentMigrator provides strongly-typed support for [PostgreSQL Anonymizer](https://postgresql-anonymizer.readthedocs.io/) masking rules using a generic builder type. When using the typed builder, the provider is automatically set to "anon".

#### Using the Generic Builder

Use `Create.SecurityLabel<AnonSecurityLabelBuilder>()` to access the strongly-typed API:

```csharp
using FluentMigrator.Builder.SecurityLabel.Anon;

// Mask with fake email
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("email")
    .OnTable("users")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithFakeEmail());
```

#### Fake Data Masking
```csharp
// Mask with fake first name
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("first_name")
    .OnTable("users")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithFakeFirstName());

// Mask with fake last name
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("last_name")
    .OnTable("users")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithFakeLastName());

// Mask with dummy last name
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("last_name")
    .OnTable("users")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithDummyLastName());

// Mask with fake company
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("company")
    .OnTable("users")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithFakeCompany());

// Mask with fake city, country, address, phone
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("city")
    .OnTable("users")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithFakeCity());

Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("phone")
    .OnTable("users")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithFakePhone());
```

#### Static Value Masking
```csharp
// Mask with a static value
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("ssn")
    .OnTable("users")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithValue("CONFIDENTIAL"));
```

#### Pseudo-Anonymization
```csharp
// Mask email using another column as username
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("email")
    .OnTable("users")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithPseudoEmail("username"));
```

#### Random Data Masking
```csharp
// Mask with random string (default 12 characters)
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("token")
    .OnTable("users")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithRandomString());

// Mask with random string of specific length
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("code")
    .OnTable("users")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithRandomString(8));

// Mask with random integer
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("age")
    .OnTable("users")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithRandomInt());

// Mask with random integer in range
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("age")
    .OnTable("users")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithRandomInt(18, 65));

// Mask with random date
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("birth_date")
    .OnTable("users")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithRandomDate());

// Mask with random date in range
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("birth_date")
    .OnTable("users")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithRandomDateBetween("1970-01-01", "2000-12-31"));
```

#### Partial Scrambling
```csharp
// Partial scrambling: keep first 2 and last 2 characters, replace middle with '*'
// "John Doe" becomes "Jo***oe"
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("name")
    .OnTable("users")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithPartialScrambling(2, '*', 2));
```

#### Custom Function Masking
```csharp
// Mask with a custom PostgreSQL Anonymizer function
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("data")
    .OnTable("users")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithFunction("anon.lorem_ipsum(2)"));
```

#### Role-Based Masking
```csharp
// Mark a role as masked (for role-based dynamic masking)
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnRole("analyst")
    .WithLabel(label => label.Masked());
```

### Financial Data Masking (French formats)
```csharp
// Mask with fake IBAN
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("iban")
    .OnTable("bank_accounts")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithFakeIban());

// Mask with fake SIRET (French business registration)
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("siret")
    .OnTable("companies")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithFakeSiret());

// Mask with fake SIREN (French company registration)
Create.SecurityLabel<AnonSecurityLabelBuilder>()
    .OnColumn("siren")
    .OnTable("companies")
    .InSchema("public")
    .WithLabel(label => label.MaskedWithFakeSiren());
```

### Creating Custom Security Label Providers

You can create your own security label builder by implementing `ISecurityLabelSyntaxBuilder`:

```csharp
using FluentMigrator.Builder.SecurityLabel;

public class MySepgsqlLabelBuilder : SecurityLabelSyntaxBuilderBase
{
    public override string ProviderName => "sepgsql";

    public MySepgsqlLabelBuilder SystemObject()
    {
        RawLabel("system_u:object_r:sepgsql_table_t:s0");
        return this;
    }

    public MySepgsqlLabelBuilder UserObject(string user, string role)
    {
        RawLabel($"{user}:object_r:{role}:s0");
        return this;
    }
}

// Usage
Create.SecurityLabel<MySepgsqlLabelBuilder>()
    .OnTable("users")
    .InSchema("public")
    .WithLabel(label => label.SystemObject());
```

### Complete Example Migration
```csharp
using FluentMigrator;
using FluentMigrator.Postgres;
using FluentMigrator.Builder.SecurityLabel.Anon;

[Migration(20250101120000)]
public class ApplyDataMasking : Migration
{
    public override void Up()
    {
        // Create masking rules for sensitive columns
        Create.SecurityLabel<AnonSecurityLabelBuilder>()
            .OnColumn("email")
            .OnTable("users")
            .InSchema("public")
            .WithLabel(label => label.MaskedWithFakeEmail());

        Create.SecurityLabel<AnonSecurityLabelBuilder>()
            .OnColumn("first_name")
            .OnTable("users")
            .InSchema("public")
            .WithLabel(label => label.MaskedWithFakeFirstName());

        Create.SecurityLabel<AnonSecurityLabelBuilder>()
            .OnColumn("last_name")
            .OnTable("users")
            .InSchema("public")
            .WithLabel(label => label.MaskedWithFakeLastName());

        Create.SecurityLabel<AnonSecurityLabelBuilder>()
            .OnColumn("phone")
            .OnTable("users")
            .InSchema("public")
            .WithLabel(label => label.MaskedWithFakePhone());

        Create.SecurityLabel<AnonSecurityLabelBuilder>()
            .OnColumn("ssn")
            .OnTable("users")
            .InSchema("public")
            .WithLabel(label => label.MaskedWithValue("XXX-XX-XXXX"));

        // Mark a role as masked for dynamic masking
        Create.SecurityLabel<AnonSecurityLabelBuilder>()
            .OnRole("analyst")
            .WithLabel(label => label.Masked());
    }

    public override void Down()
    {
        // Remove masking rules
        Delete.SecurityLabel()
            .For("anon")
            .FromColumn("email")
            .OnTable("users")
            .InSchema("public");

        Delete.SecurityLabel()
            .For("anon")
            .FromColumn("first_name")
            .OnTable("users")
            .InSchema("public");

        Delete.SecurityLabel()
            .For("anon")
            .FromColumn("last_name")
            .OnTable("users")
            .InSchema("public");

        Delete.SecurityLabel()
            .For("anon")
            .FromColumn("phone")
            .OnTable("users")
            .InSchema("public");

        Delete.SecurityLabel()
            .For("anon")
            .FromColumn("ssn")
            .OnTable("users")
            .InSchema("public");

        Delete.SecurityLabel()
            .For("anon")
            .FromRole("analyst");
    }
}
