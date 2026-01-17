# Column Visibility

This ADR documents database provider support for column visibility features and proposes an API design to support visibility toggles in FluentMigrator migrations.

## Background

Many database management systems support hiding columns from wildcard operations like `SELECT *`. This is particularly useful when working with data access layers like Entity Framework Core, where certain columns (such as computed columns, audit fields, or internal metadata) should be excluded from default queries unless explicitly requested.

Currently, FluentMigrator does not provide built-in support for column visibility operations. Users must resort to "escape hatches" like `Execute.Sql`, `Execute.EmbeddedScript`, or `Execute.WithConnection` to manually create columns with visibility settings.

## Database Provider Support

The following table summarizes column visibility support across different database providers:

| Database       | Support | Keywords | CREATE TABLE Syntax | ALTER TABLE ADD COLUMN Syntax | ALTER TABLE ALTER COLUMN Syntax | Notes |
| -------------- | ------- | -------- | ------------------- | ----------------------------- | ------------------------------- | ----- |
| SQL Server     | ✓ (2016+) | `HIDDEN` | `CREATE TABLE tablename (columnname datatype HIDDEN)` | `ALTER TABLE tablename ADD columnname datatype HIDDEN` | `ALTER TABLE tablename ALTER COLUMN columnname ADD HIDDEN` | Introduced in SQL Server 2016 for temporal tables. Hidden columns are excluded from `SELECT *` but can be explicitly selected. Cannot be used with primary key columns. |
| Oracle         | ✓ (12c+) | `INVISIBLE`, `VISIBLE` | `CREATE TABLE tablename (columnname datatype INVISIBLE)` | `ALTER TABLE tablename ADD (columnname datatype INVISIBLE)` | `ALTER TABLE tablename MODIFY (columnname INVISIBLE)` or `VISIBLE` | Introduced in Oracle 12c. Invisible columns are not returned by `SELECT *` or `DESCRIBE` unless explicitly requested. Can toggle between `VISIBLE` and `INVISIBLE`. |
| MySQL          | ✓ (8.0.23+) | `INVISIBLE`, `VISIBLE` | `CREATE TABLE tablename (columnname datatype INVISIBLE)` | `ALTER TABLE tablename ADD COLUMN columnname datatype INVISIBLE` | `ALTER TABLE tablename MODIFY COLUMN columnname datatype INVISIBLE` or `VISIBLE` | Introduced in MySQL 8.0.23. Invisible columns are not included in `SELECT *` statements. Can be toggled using `ALTER TABLE ... MODIFY COLUMN`. |
| MariaDB        | ✓ (10.3+) | `INVISIBLE`, `VISIBLE` | `CREATE TABLE tablename (columnname datatype INVISIBLE)` | `ALTER TABLE tablename ADD COLUMN columnname datatype INVISIBLE` | `ALTER TABLE tablename MODIFY COLUMN columnname datatype INVISIBLE` or `VISIBLE` | Introduced in MariaDB 10.3. Similar behavior to MySQL. Invisible columns do not appear in `SELECT *` queries. |
| DB2            | ✓ (v9.7+) | `IMPLICITLY HIDDEN`, `NOT HIDDEN` | `CREATE TABLE tablename (columnname datatype IMPLICITLY HIDDEN)` | `ALTER TABLE tablename ADD COLUMN columnname datatype IMPLICITLY HIDDEN` | `ALTER TABLE tablename ALTER COLUMN columnname SET IMPLICITLY HIDDEN` or `SET NOT HIDDEN` | Supports implicitly hidden columns. Hidden columns are not included in `SELECT *` or `INSERT` without explicit column list. Can be accessed by explicitly specifying the column name. |
| PostgreSQL     | ✗ | N/A | N/A | N/A | N/A | No native support for column visibility. Workarounds include views or table inheritance, but these are not equivalent features. |
| SQLite         | ✗ | N/A | N/A | N/A | N/A | No native support for column visibility. |
| Firebird       | ✗ | N/A | N/A | N/A | N/A | No native support for column visibility. |
| Snowflake      | ✗ | N/A | N/A | N/A | N/A | No native support for column visibility. Uses tagging and masking policies for similar use cases. |
| SAP HANA       | ✗ | N/A | N/A | N/A | N/A | No native support for column visibility. |
| Redshift       | ✗ | N/A | N/A | N/A | N/A | Based on PostgreSQL; no native support for column visibility. |
| Jet (MS Access)| ✗ | N/A | N/A | N/A | N/A | No native support for column visibility. |

### References

1. **SQL Server**: [Temporal Tables Documentation](https://docs.microsoft.com/en-us/sql/relational-databases/tables/temporal-tables)
2. **Oracle**: [Invisible Columns](https://docs.oracle.com/database/121/SQLRF/statements_7002.htm#SQLRF01402)
3. **MySQL**: [Invisible Columns](https://dev.mysql.com/doc/refman/8.0/en/invisible-columns.html)
4. **MariaDB**: [Invisible Columns](https://mariadb.com/kb/en/invisible-columns/)
5. **DB2**: [Hidden Columns](https://www.ibm.com/docs/en/db2/12.1.x?topic=concepts-hidden-columns), [ALTER TABLE Statement](https://www.ibm.com/docs/en/db2/12.1.x?topic=statements-alter-table#sdx-synid_column-options)

## Use Cases

1. **Temporal/Audit Columns**: System-managed columns like `SysStartTime`, `SysEndTime`, `RowVersion`, `CreatedDate`, `ModifiedDate` that should not appear in default result sets.
2. **Computed Columns**: Derived columns that may be expensive to calculate and should only be retrieved when explicitly needed.
3. **Internal Metadata**: Application-specific columns for internal use that should not be exposed to most queries.
4. **Entity Framework Core**: When using EF Core, shadow properties or columns not mapped to entity properties benefit from being hidden.
5. **Migration Compatibility**: When adding columns to existing tables where `SELECT *` queries are prevalent in legacy code.

## Proposed API Design

The API should extend the existing fluent interface for column creation with visibility options. The design follows FluentMigrator's existing patterns for column options.

### Core Interface Extension

Add visibility methods to `IColumnOptionSyntax`:

```csharp
public interface IColumnOptionSyntax<TNext, TNextFk>
{
    // ... existing methods ...

    /// <summary>
    /// Marks the column as hidden/invisible. The column will not appear in SELECT * queries
    /// but can be explicitly selected. Supported by: SQL Server 2016+, Oracle 12c+, MySQL 8.0.23+, MariaDB 10.3+
    /// </summary>
    /// <returns>The next step</returns>
    TNext Hidden();

    /// <summary>
    /// Explicitly marks the column as visible (default behavior).
    /// This is useful when toggling visibility on an existing column.
    /// Supported by: Oracle 12c+, MySQL 8.0.23+, MariaDB 10.3+
    /// </summary>
    /// <returns>The next step</returns>
    TNext Visible();
}
```

### Usage Examples

#### Creating a Hidden Column in a New Table

```csharp
[Migration(20240101120000)]
public class AddUsersTableWithHiddenColumns : Migration
{
    public override void Up()
    {
        Create.Table("Users")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Username").AsString(50).NotNullable()
            .WithColumn("Email").AsString(100).NotNullable()
            .WithColumn("CreatedDate").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime).Hidden()
            .WithColumn("ModifiedDate").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime).Hidden()
            .WithColumn("RowVersion").AsBinary(8).NotNullable().Hidden();
    }

    public override void Down()
    {
        Delete.Table("Users");
    }
}
```

#### Adding a Hidden Column to an Existing Table

```csharp
[Migration(20240101130000)]
public class AddAuditColumnsToProducts : Migration
{
    public override void Up()
    {
        Alter.Table("Products")
            .AddColumn("CreatedBy").AsString(50).NotNullable().WithDefaultValue("system").Hidden()
            .AddColumn("ModifiedBy").AsString(50).NotNullable().WithDefaultValue("system").Hidden();
    }

    public override void Down()
    {
        Delete.Column("CreatedBy").FromTable("Products");
        Delete.Column("ModifiedBy").FromTable("Products");
    }
}
```

#### Toggling Visibility on an Existing Column

```csharp
[Migration(20240101140000)]
public class ToggleColumnVisibility : Migration
{
    public override void Up()
    {
        // Make a previously visible column hidden
        Alter.Column("LegacyField").OnTable("Products")
            .AsString(100).Nullable().Hidden();
    }

    public override void Down()
    {
        // Restore visibility
        Alter.Column("LegacyField").OnTable("Products")
            .AsString(100).Nullable().Visible();
    }
}
```

#### Database-Specific Conditional Visibility

```csharp
[Migration(20240101150000)]
public class AddTemporalColumnsConditionally : Migration
{
    public override void Up()
    {
        // Apply hidden columns only for databases that support it
        if (IfDatabase("SqlServer", "Oracle", "MySql", "MariaDB", "DB2").Create.Column("SysStartTime")
            .OnTable("Orders")
            .AsDateTime2().NotNullable()
            .WithDefault(SystemMethods.CurrentDateTime)
            .Hidden();
        else
        {
            // For databases without support, create visible column
            Create.Column("SysStartTime")
                .OnTable("Orders")
                .AsDateTime().NotNullable()
                .WithDefault(SystemMethods.CurrentDateTime);
        }
    }

    public override void Down()
    {
        Delete.Column("SysStartTime").FromTable("Orders");
    }
}
```

## Implementation Considerations

### 1. ColumnDefinition Model Extension

Add a visibility property to the `ColumnDefinition` model:

```csharp
public class ColumnDefinition
{
    // ... existing properties ...

    /// <summary>
    /// Gets or sets the column visibility. 
    /// Null indicates default behavior (visible).
    /// True indicates the column should be hidden/invisible.
    /// False explicitly indicates the column should be visible.
    /// </summary>
    public virtual bool? IsHidden { get; set; }
}
```

### 2. Generator Implementation

Each database generator would need to implement visibility based on the provider's capabilities:

```csharp
// SqlServer2016Generator.cs
protected override string GetColumnVisibilityClause(ColumnDefinition column)
{
    if (column.IsHidden == true)
    {
        return "HIDDEN";
    }
    return string.Empty;
}

// OracleGenerator.cs (12c+)
protected override string GetColumnVisibilityClause(ColumnDefinition column)
{
    if (column.IsHidden == true)
    {
        return "INVISIBLE";
    }
    else if (column.IsHidden == false)
    {
        return "VISIBLE";
    }
    return string.Empty;
}

// MySqlGenerator.cs (8.0.23+)
protected override string GetColumnVisibilityClause(ColumnDefinition column)
{
    if (column.IsHidden == true)
    {
        return "INVISIBLE";
    }
    else if (column.IsHidden == false)
    {
        return "VISIBLE";
    }
    return string.Empty;
}

// Db2Generator.cs (v9.7+)
protected override string GetColumnVisibilityClause(ColumnDefinition column)
{
    if (column.IsHidden == true)
    {
        return "IMPLICITLY HIDDEN";
    }
    return string.Empty;
}

// PostgresGenerator.cs and others without support
protected override string GetColumnVisibilityClause(ColumnDefinition column)
{
    if (column.IsHidden.HasValue)
    {
        // Log warning or throw exception
        throw new NotSupportedException(
            "Column visibility is not supported by PostgreSQL. " +
            "Consider using database views or other workarounds.");
    }
    return string.Empty;
}
```

### 3. Version-Specific Support

Visibility support should be version-gated for generators:

- SQL Server: Only supported in SQL Server 2016 and later
- Oracle: Only supported in Oracle 12c and later
- MySQL: Only supported in MySQL 8.0.23 and later
- MariaDB: Only supported in MariaDB 10.3 and later
- DB2: Only supported in DB2 v9.7 and later

Generators should either:
1. Throw a descriptive exception when the feature is used with an unsupported version, or
2. Silently ignore the visibility setting (emit a warning log) and create a visible column

### 4. ALTER COLUMN Considerations

When altering columns:
- SQL Server uses `ALTER TABLE ... ALTER COLUMN ... HIDDEN` (SQL Server 2016+)
- Oracle uses `ALTER TABLE ... MODIFY (column_name INVISIBLE)` or `VISIBLE`
- MySQL uses `ALTER TABLE ... MODIFY COLUMN column_name datatype INVISIBLE` or `VISIBLE`
- MariaDB uses `ALTER TABLE ... MODIFY COLUMN column_name datatype INVISIBLE` or `VISIBLE`
- DB2 uses `ALTER TABLE ... ALTER COLUMN column_name SET IMPLICITLY HIDDEN` (v9.7+)

### 5. Constraints and Limitations

- **SQL Server**: `HIDDEN` cannot be applied to primary key columns or columns with unique constraints in some contexts
- **Oracle**: `INVISIBLE` columns are not included in `DESCRIBE` output
- **MySQL/MariaDB**: Must have at least one visible column in a table
- **DB2**: `IMPLICITLY HIDDEN` columns are not returned by `SELECT *` or `INSERT` statements without explicit column list
- All providers: Hidden/invisible columns are still accessible via explicit column selection

### 6. Testing Strategy

Tests should verify:
1. Column creation with `Hidden()` generates correct SQL for supported databases
2. Column creation with `Visible()` generates correct SQL where applicable
3. Unsupported databases throw appropriate exceptions or log warnings
4. `ALTER COLUMN` operations correctly toggle visibility
5. Database-specific `IfDatabase()` conditions work correctly with visibility

## Alternative Approaches Considered

### 1. Database-Specific Extension Methods

Instead of core API support, provide database-specific extension methods:

```csharp
// SQL Server specific
Create.Column("AuditDate").OnTable("Users")
    .AsDateTime().NotNullable()
    .SqlServer().Hidden();

// Oracle specific
Create.Column("AuditDate").OnTable("Users")
    .AsDateTime().NotNullable()
    .Oracle().Invisible();
```

**Pros**: Clear indication of database-specific feature
**Cons**: More verbose, harder to write portable migrations, inconsistent with existing patterns

### 2. Additional Features Dictionary

Use the existing `AdditionalFeatures` dictionary on `ColumnDefinition`:

```csharp
Create.Column("AuditDate").OnTable("Users")
    .AsDateTime().NotNullable()
    .WithAdditionalFeature("Visibility", "Hidden");
```

**Pros**: No API changes needed, fully flexible
**Cons**: Not type-safe, not discoverable, inconsistent with FluentMigrator's fluent style

### 3. No Built-in Support

Continue requiring users to use `Execute.Sql()` for visibility features:

```csharp
Execute.Sql(@"
    ALTER TABLE Users 
    ADD AuditDate DATETIME2 NOT NULL 
    DEFAULT GETDATE() HIDDEN
");
```

**Pros**: No development effort required
**Cons**: Not database-agnostic, defeats FluentMigrator's purpose, harder to maintain

## Recommendation

Implement the proposed core API extension (`Hidden()` and `Visible()` methods) for the following reasons:

1. **Consistency**: Follows existing FluentMigrator patterns for column options
2. **Discoverability**: IDE autocomplete naturally suggests the feature
3. **Type Safety**: Compile-time checking ensures proper usage
4. **Portability**: Works with database-specific `IfDatabase()` constructs for conditional application
5. **Maintainability**: Centralizes visibility logic in generators rather than scattered SQL strings

The implementation should:
- Add `Hidden()` and `Visible()` methods to `IColumnOptionSyntax`
- Add `IsHidden` nullable boolean property to `ColumnDefinition`
- Implement generator support for SQL Server 2016+, Oracle 12c+, MySQL 8.0.23+, MariaDB 10.3+, and DB2 v9.7+
- Throw descriptive exceptions or log warnings for unsupported databases
- Include comprehensive integration tests for supported providers

## Future Considerations

1. **Index Support**: Some databases allow indexes on hidden columns; this should be verified and tested
2. **Foreign Key Support**: Verify whether hidden columns can participate in foreign key relationships
3. **Default Constraints**: Ensure default values work correctly with hidden columns
4. **Computed Columns**: Test interaction between hidden and computed column features
5. **Documentation**: Update user documentation with visibility examples and database support matrix
6. **Breaking Changes**: Consider if this warrants a major version bump (likely not, as it's additive)
