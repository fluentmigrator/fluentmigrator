# Indexes

Indexes are crucial for database performance optimization. FluentMigrator provides comprehensive support for creating, modifying, and managing database indexes across different database providers.

## Basic Index Operations

### Creating Simple Indexes

```csharp
public class BasicIndexes : Migration
{
    public override void Up()
    {
        Create.Table("Users")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("FirstName").AsString(50).NotNullable()
            .WithColumn("LastName").AsString(50).NotNullable()
            .WithColumn("Email").AsString(255).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable();

        // Single column index
        Create.Index("IX_Users_LastName")
            .OnTable("Users")
            .OnColumn("LastName");

        // Multi-column index
        Create.Index("IX_Users_FirstName_LastName")
            .OnTable("Users")
            .OnColumn("FirstName").Ascending()
            .OnColumn("LastName");

        // Index with sort order
        Create.Index("IX_Users_CreatedAt_Desc")
            .OnTable("Users")
            .OnColumn("CreatedAt").Descending();
    }

    public override void Down()
    {
        Delete.Index("IX_Users_LastName").OnTable("Users");
        Delete.Index("IX_Users_FirstName_LastName").OnTable("Users");
        Delete.Index("IX_Users_CreatedAt_Desc").OnTable("Users");
        Delete.Table("Users");
    }
}
```

### Unique Indexes

```csharp
public class UniqueIndexes : Migration
{
    public override void Up()
    {
        Create.Table("Products")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("SKU").AsString(50).NotNullable()
            .WithColumn("CompanyId").AsInt32().NotNullable()
            .WithColumn("InternalCode").AsString(20).NotNullable();

        // Simple unique index
        Create.Index("UQ_Products_SKU")
            .OnTable("Products")
            .OnColumn("SKU")
            .Unique();

        // Composite unique index
        Create.Index("UQ_Products_CompanyId_InternalCode")
            .OnTable("Products")
            .OnColumn("CompanyId").Ascending()
            .OnColumn("InternalCode")
            .Unique();
    }

    public override void Down()
    {
        Delete.Index("UQ_Products_SKU").OnTable("Products");
        Delete.Index("UQ_Products_CompanyId_InternalCode").OnTable("Products");
        Delete.Table("Products");
    }
}
```

### NULL handling in unique indexes

Database engines disagree on whether two NULLs conflict in a unique index. SQL Server and Db2 treat NULLs as equal, so a unique index allows at most one row with NULL in the indexed columns. PostgreSQL, SQLite, MySQL and Firebird treat NULLs as distinct, so any number of such rows is allowed. A migration that relies on the engine default therefore means different things on different engines.

To state the intent in a provider-agnostic way, call `NullsDistinct()` or `NullsNotDistinct()` right after `Unique()`. The names follow the SQL:2023 `NULLS [NOT] DISTINCT` clause. Each provider maps the intent to the native syntax that expresses it, or reports that it can't.

```csharp
Create.Index("UX_Prices_Product_Tier")
    .OnTable("Prices")
    .OnColumn("ProductId").Ascending()
    .OnColumn("TierPriceId").Unique().NullsNotDistinct();
```

| Method | Meaning |
|---|---|
| `NullsDistinct()` | Index values must be unique. Rows with NULL in an indexed column never conflict, so any number of them may exist. |
| `NullsNotDistinct()` | Index values must be unique. NULLs compare equal, so at most one row may hold a given combination that includes NULL. |

The setting applies to the whole index, even though you call it on a column. Calling `NullsDistinct()` on one column and `NullsNotDistinct()` on another column of the same index throws an `InvalidOperationException`.

#### Semantics per provider

The table describes the SQL that each generator emits for the two options.

| Provider | Native behavior | `NullsDistinct()` | `NullsNotDistinct()` |
|---|---|---|---|
| SQL Server 2008 and later | Not distinct | Filtered index: `WHERE [c1] IS NOT NULL AND [c2] IS NOT NULL` | No change (native) |
| SQL Server 2005 | Not distinct | Compatibility mode | No change (native) |
| SQL Server 2000 | Not distinct | Compatibility mode | Compatibility mode |
| PostgreSQL 15 and later | Distinct | `NULLS DISTINCT` | `NULLS NOT DISTINCT` |
| PostgreSQL 14 and earlier | Distinct | No change (native) | Compatibility mode |
| SQLite | Distinct | No change (native) | Compatibility mode |
| MySQL 4, 5 and 8 | Distinct | No change (native) | Compatibility mode |
| Firebird | Distinct | No change (native) | Compatibility mode |
| Db2 and Db2 for i | Not distinct | Compatibility mode | No change (native) |
| Oracle and Oracle 12c | Mixed (see [provider notes](#provider-notes)) | Compatibility mode | Compatibility mode |
| SAP HANA | Not declared | Compatibility mode | Compatibility mode |
| Redshift and Snowflake | Indexes not supported | Compatibility mode (the whole index) | Compatibility mode (the whole index) |

In the table:

- **No change (native)** means the engine already behaves as requested, so the generator emits the same `CREATE UNIQUE INDEX` statement as `Unique()`.
- **Compatibility mode** means the generator can't express the request. The generator's compatibility mode decides what happens:
  - `CompatibilityMode.STRICT` throws a `DatabaseOperationNotSupportedException`.
  - `CompatibilityMode.LOOSE`, the default, ignores the request and emits a plain unique index with the engine's native behavior.

To make unsupported requests fail instead of silently falling back to the engine default, set the compatibility mode to `STRICT`:

```csharp
services.Configure<GeneratorOptions>(opt => opt.CompatibilityMode = CompatibilityMode.STRICT);
```

#### Provider notes

**SQL Server.** SQL Server has no clause that makes NULLs distinct in a unique index. On SQL Server 2008 and later, `NullsDistinct()` emits a filtered index that excludes rows where any indexed column is NULL. If you also call `Filter()`, the generator combines your filter and the generated predicate with `AND`. Filtered indexes don't exist in SQL Server 2005 and earlier.

**PostgreSQL.** PostgreSQL 15 added the `NULLS [NOT] DISTINCT` clause, which both options emit. Earlier versions have no equivalent of `NULLS NOT DISTINCT`, and a partial index can't express it.

**Db2.** A Db2 unique index treats NULLs as equal, so `NullsNotDistinct()` needs no extra syntax. The Db2 generator doesn't emit any NULL-related index clause, so `NullsDistinct()` goes through compatibility mode. Db2 for i uses the same generator and behaves the same way. Db2 for Linux, UNIX and Windows has `EXCLUDE NULL KEYS`, but that clause only skips keys in which every column is NULL. A composite key with only some NULL columns still compares equal, so the clause doesn't provide distinct semantics for multi-column indexes.

**Oracle.** Oracle doesn't index a key in which every column is NULL, so any number of such rows may exist. A composite key with only some NULL columns is indexed, and those NULLs compare equal. Neither option describes this behavior for every index, so both go through compatibility mode.

**Snowflake.** Snowflake doesn't support indexes, so the whole index goes through compatibility mode. If you need the rows that a filtered unique index would cover for reads, create a materialized view in an `IfDatabase` block:

```csharp
IfDatabase(ProcessorIdConstants.Snowflake).Execute.Sql(@"
    CREATE OR REPLACE MATERIALIZED VIEW mv_active_records AS
    SELECT unique_column
    FROM my_table
    WHERE is_active = true;");
```

A materialized view covers read scenarios only. It doesn't enforce uniqueness when rows are inserted or updated, so it doesn't replace a filtered unique index for read-write scenarios. Materialized views require Snowflake Enterprise Edition or higher.

#### Provider-specific options

The `NullsDistinct()` and `NullsNotDistinct()` extensions after `Unique()`, and the `UniqueNullsDistinct()` and `UniqueNullsNotDistinct()` index options, in `FluentMigrator.SqlServer` and `FluentMigrator.Postgres` are obsolete. The per-column overloads after `Ascending()` or `Descending()` aren't affected. When you set both a provider-specific option and one of the provider-agnostic options on the same index, the provider-specific option takes precedence.

::: warning
The legacy `NullsNotDistinct()` emits a `WHERE column IS NOT NULL` filter on SQL Server and on PostgreSQL 14 and earlier. That filter makes NULLs distinct, which is the opposite of the provider-agnostic `NullsNotDistinct()`. To keep that behavior, replace the legacy call with the provider-agnostic `NullsDistinct()`.
:::

### Filtered Indexes

```csharp
public class FilteredIndexes : Migration
{
    public override void Up()
    {
        Create.Table("Orders")
            .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
            .WithColumn("CustomerId").AsInt32().NotNullable()
            .WithColumn("Status").AsString(20).NotNullable()
            .WithColumn("OrderDate").AsDateTime().NotNullable()
            .WithColumn("CompletedDate").AsDateTime().Nullable();

        // Filtered index for SQL Server, or PostgreSQL
        IfDatabase(ProcessorIdConstants.SqlServer, ProcessorIdConstants.Postgres).Delegate(() =>
        {
            Create.Index("IX_Orders_CustomerId_Active")
                .OnTable("Orders")
                .OnColumn("CustomerId").Ascending()
                .WithOptions()
                .Filter("Status = 'Active'");

            // Filtered index excluding NULL values
            Create.Index("IX_Orders_CompletedDate")
                .OnTable("Orders")
                .OnColumn("CompletedDate").Ascending()
                .WithOptions()
                .Filter("CompletedDate IS NOT NULL");
        });
    }

    public override void Down()
    {
        IfDatabase(ProcessorIdConstants.SqlServer, ProcessorIdConstants.Postgres).Delegate(() =>
        {
            Delete.Index("IX_Orders_CustomerId_Active").OnTable("Orders");
            Delete.Index("IX_Orders_CompletedDate").OnTable("Orders");
        });
        Delete.Table("Orders");
    }
}
```

## Database-Specific Index Features

Different database providers offer specialized indexing capabilities and advanced options:

### SQL Server Specific
- **Clustered vs Non-Clustered**: Advanced index clustering options - [SQL Server Provider](../providers/sql-server.md#clustered-and-non-clustered-indexes)
- **Covering Indexes**: Include columns for covering index optimization - [SQL Server Provider](../providers/sql-server.md#included-columns-covering-indexes)
- **Filtered Indexes**: WHERE clause conditions for selective indexing - [SQL Server Provider](../providers/sql-server.md#filtered-indexes)

### PostgreSQL Specific
- **Algorithm-Specific Indexes**: B-tree, Hash, GIN, GiST algorithms - [PostgreSQL Provider](../providers/postgresql.md#algorithm-specific-indexes)

## See Also
- [Execute SQL](../operations/execute-sql.md) - When you need to create indexes with provider-specific SQL
- [Columns](./columns.md) - Column indexing strategies and patterns
- [Constraints](./constraints.md) - Index-backed constraint types
