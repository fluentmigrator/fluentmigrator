# ADR: DuckDB Processor/Generator/TypeMap

## Status
Proposed

## Context
DuckDB is an in-process OLAP (Online Analytical Processing) database designed for high-performance analytical queries. Unlike traditional client-server databases, DuckDB runs embedded within the application process, similar to SQLite, but optimized for analytical workloads rather than transactional ones. It supports a large subset of standard SQL, including DDL operations, making it a viable candidate for FluentMigrator support.

### Use Cases
- Local analytics and data engineering pipelines that need schema evolution
- Testing environments where a lightweight embedded database is preferred over PostgreSQL or SQL Server
- Reproducible data processing workflows where schema changes need to be versioned

### .NET Driver
DuckDB provides a first-party ADO.NET driver via the [DuckDB.NET](https://github.com/Giorgi/DuckDB.NET) NuGet package family:
- **`DuckDB.NET.Data`** – The ADO.NET provider (`DuckDBClientFactory`, `DuckDBConnection`, etc.)
- **`DuckDB.NET.Binaries`** – Bundled native DuckDB binaries (for convenience; not required if native libraries are deployed separately)

The `DuckDBClientFactory` is the `DbProviderFactory` used by `ReflectionBasedDbFactory`.

### Connection String Format
DuckDB connection strings use a file path as the data source:
- File-based: `Data Source=analytics.db`
- In-memory: `Data Source=:memory:`

### Schema Model
DuckDB uses a three-level naming hierarchy: **catalog → schema → table**. Each DuckDB file is a catalog, the default schema within a catalog is `main`. Additional schemas can be created with `CREATE SCHEMA`. This maps well to FluentMigrator's schema concept. The default catalog is always named `memory` for in-memory databases and the file name (without extension) for file-based databases.

## Research Findings

### DDL Support Summary

| Operation | Support | Notes |
|-----------|---------|-------|
| `CREATE TABLE` | ✅ Supported | Full support including constraints, defaults, primary keys, and foreign keys |
| `DROP TABLE` | ✅ Supported | `IF EXISTS` is supported |
| `ALTER TABLE ... ADD COLUMN` | ✅ Supported | Including `NOT NULL` with a default value |
| `ALTER TABLE ... DROP COLUMN` | ✅ Supported | |
| `ALTER TABLE ... RENAME COLUMN` | ✅ Supported | |
| `ALTER TABLE ... RENAME TO` | ✅ Supported | |
| `ALTER TABLE ... ALTER COLUMN ... TYPE` | ✅ Supported (v0.10.0+) | Requires the new type to be compatible with the old type or an explicit `USING` expression |
| `ALTER TABLE ... ALTER COLUMN ... SET DEFAULT` | ✅ Supported | |
| `ALTER TABLE ... ALTER COLUMN ... DROP DEFAULT` | ✅ Supported | |
| `ALTER TABLE ... ALTER COLUMN ... SET NOT NULL` | ✅ Supported | |
| `ALTER TABLE ... ALTER COLUMN ... DROP NOT NULL` | ✅ Supported | |
| `CREATE INDEX` | ✅ Supported | DuckDB uses ART (Adaptive Radix Tree) indexes |
| `DROP INDEX` | ✅ Supported | |
| `CREATE SEQUENCE` | ✅ Supported | |
| `DROP SEQUENCE` | ✅ Supported | |
| `CREATE SCHEMA` | ✅ Supported | |
| `DROP SCHEMA` | ✅ Supported | |
| `CREATE VIEW` | ✅ Supported | |
| `DROP VIEW` | ✅ Supported | |
| Foreign key constraints | ✅ Supported | Enforced at DML time; `PRAGMA foreign_keys` not required |
| Unique constraints | ✅ Supported | Enforced; implemented as unique indexes |
| Check constraints | ✅ Supported | |
| Triggers | ❌ Not Supported | DuckDB does not support SQL triggers |
| Stored procedures | ❌ Not Supported | DuckDB supports macros (`CREATE MACRO`) and user-defined functions (UDFs) but not traditional stored procedures |
| Clustered indexes | ❌ Not Supported | DuckDB does not support clustered indexes; all ART indexes are non-clustered |
| `INCLUDE` columns on indexes | ❌ Not Supported | DuckDB ART indexes do not have covering/include column syntax |
| `ALTER TABLE ... ADD CONSTRAINT PRIMARY KEY` | ❌ Not Supported (post-creation) | Primary keys must be defined at `CREATE TABLE` time; they cannot be added later via `ALTER TABLE` |
| `ALTER TABLE ... DROP CONSTRAINT` | ✅ Supported | Can drop named constraints |

### Type Mapping

The following table describes the proposed `DbType` → DuckDB SQL type mapping for `DuckDBTypeMap`:

| .NET `DbType` | DuckDB SQL Type | Notes |
|---------------|-----------------|-------|
| `DbType.Boolean` | `BOOLEAN` | |
| `DbType.Byte` | `UTINYINT` | Unsigned 8-bit integer (0–255) |
| `DbType.SByte` | `TINYINT` | Signed 8-bit integer (−128–127) |
| `DbType.Int16` | `SMALLINT` | |
| `DbType.UInt16` | `USMALLINT` | |
| `DbType.Int32` | `INTEGER` | |
| `DbType.UInt32` | `UINTEGER` | |
| `DbType.Int64` | `BIGINT` | |
| `DbType.UInt64` | `UBIGINT` | |
| `DbType.Single` | `FLOAT` | 4-byte IEEE 754 |
| `DbType.Double` | `DOUBLE` | 8-byte IEEE 754 |
| `DbType.Decimal` | `DECIMAL(18,4)` | Default precision; sized variant uses `DECIMAL(p,s)` |
| `DbType.Currency` | `DECIMAL(18,4)` | Matches SQL Server `money` semantics |
| `DbType.VarNumeric` | `DOUBLE` | Variable-precision floating point |
| `DbType.AnsiString` | `VARCHAR` | Sized variant uses `VARCHAR(n)` |
| `DbType.String` | `VARCHAR` | DuckDB `VARCHAR` is always UTF-8 |
| `DbType.AnsiStringFixedLength` | `CHAR` | Fixed-length character type |
| `DbType.StringFixedLength` | `CHAR` | Fixed-length character type |
| `DbType.Binary` | `BLOB` | Variable-length binary |
| `DbType.Date` | `DATE` | |
| `DbType.Time` | `TIME` | |
| `DbType.DateTime` | `TIMESTAMP` | Microsecond precision; no time zone |
| `DbType.DateTime2` | `TIMESTAMP` | Same as `TIMESTAMP` in DuckDB |
| `DbType.DateTimeOffset` | `TIMESTAMPTZ` | Timestamp with time zone |
| `DbType.Guid` | `UUID` | 128-bit UUID; native DuckDB type |
| `DbType.Xml` | `VARCHAR` | DuckDB has no native XML type |
| `DbType.Object` | `VARCHAR` | Fallback; stored as text |

### Introspection Queries
DuckDB exposes catalog metadata via the `information_schema` views and `duckdb_*` table functions. The proposed introspection queries for `DuckDBProcessor` are:

**Schema exists:**
```sql
SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name = 'schemaName'
```

**Table exists:**
```sql
SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'schemaName' AND table_name = 'tableName'
```

**Column exists:**
```sql
SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = 'schemaName' AND table_name = 'tableName' AND column_name = 'columnName'
```

**Constraint exists:**
```sql
SELECT COUNT(*) FROM information_schema.table_constraints WHERE constraint_schema = 'schemaName' AND table_name = 'tableName' AND constraint_name = 'constraintName'
```

**Index exists:**
```sql
SELECT COUNT(*) FROM duckdb_indexes() WHERE schema_name = 'schemaName' AND table_name = 'tableName' AND index_name = 'indexName'
```

**Sequence exists:**
```sql
SELECT COUNT(*) FROM duckdb_sequences() WHERE schema_name = 'schemaName' AND sequence_name = 'sequenceName'
```

**Default value exists:**
```sql
SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = 'schemaName' AND table_name = 'tableName' AND column_name = 'columnName' AND column_default IS NOT NULL
```

## Architecture Decision

### 1. Project Structure

A new project `src/FluentMigrator.Runner.DuckDB/` should be created, mirroring the structure of other runner projects:

```
src/FluentMigrator.Runner.DuckDB/
├── FluentMigrator.Runner.DuckDB.csproj
├── DuckDBRunnerBuilderExtensions.cs
├── Generators/
│   └── DuckDB/
│       ├── DuckDBColumn.cs
│       ├── DuckDBGenerator.cs
│       ├── DuckDBQuoter.cs
│       ├── DuckDBTypeMap.cs
│       └── IDuckDBTypeMap.cs
└── Processors/
    └── DuckDB/
        ├── DuckDBDbFactory.cs
        └── DuckDBProcessor.cs
```

A `ProcessorIdConstants.DuckDB` constant (`"DuckDB"`) should be added to `src/FluentMigrator/ProcessorId.cs`, as well as a corresponding `GeneratorIdConstants.DuckDB` constant.

### 2. NuGet Dependencies

The `FluentMigrator.Runner.DuckDB.csproj` should reference:
- `DuckDB.NET.Data` (the ADO.NET provider; consumers must also bring `DuckDB.NET.Binaries` or provide the native library themselves)
- `FluentMigrator.Runner.Core`

`DuckDB.NET.Binaries` should **not** be a hard dependency of the runner package so that consumers can choose between platform-specific binaries or a cross-platform bundle. This is the same pattern used by SQLite runner packages that rely on consumers providing `Microsoft.Data.Sqlite`.

### 3. DuckDBDbFactory

`DuckDBDbFactory` should extend `ReflectionBasedDbFactory` and load the `DuckDBClientFactory` from the `DuckDB.NET.Data` assembly:

```csharp
private static readonly TestEntry[] _testEntries =
{
    new TestEntry("DuckDB.NET.Data", "DuckDB.NET.Data.DuckDBClientFactory"),
};
```

### 4. DuckDBTypeMap

`DuckDBTypeMap` should implement `IDuckDBTypeMap : ITypeMap` and extend `TypeMapBase`. It should register types as described in the type mapping table above. Where a sized variant exists (e.g., `VARCHAR(n)`, `CHAR(n)`, `DECIMAL(p,s)`), the sized overload of `SetTypeMap` should be used to register both the default (unsized) and the sized forms.

Unlike SQLite, DuckDB enforces actual type constraints, so each `DbType` should map to the most semantically appropriate DuckDB native type rather than a generic fallback.

### 5. DuckDBQuoter

`DuckDBQuoter` should extend `GenericQuoter`. DuckDB uses standard double-quote (`"`) identifier quoting and single-quote (`'`) string literal quoting. No special overrides are expected beyond the standard generic quoter behaviour, though `FormatSystemMethods` should be overridden to map:
- `SystemMethods.CurrentDateTime` → `CURRENT_TIMESTAMP`
- `SystemMethods.CurrentUTCDateTime` → `CURRENT_TIMESTAMP`
- `SystemMethods.NewGuid` → `gen_random_uuid()`

### 6. DuckDBGenerator

`DuckDBGenerator` should extend `GenericGenerator`. Most DDL operations are supported natively; the following overrides are needed:

| Method | Action | Reason |
|--------|--------|--------|
| `Generate(CreateIndexExpression)` | Override | DuckDB does not support clustered indexes, `INCLUDE` columns, or `FILLFACTOR`; unsupported options should be silently ignored or raise a `CompatibilityMode` error |
| `Generate(CreateConstraintExpression)` | Override for `CHECK` and `UNIQUE` only | Primary key constraints cannot be added post-creation via `ALTER TABLE`; throw a `DatabaseOperationNotSupportedException` for `ADD PRIMARY KEY` |
| `Generate(DeleteConstraintExpression)` | Supported | Standard `ALTER TABLE ... DROP CONSTRAINT` syntax works |
| `Generate(CreateSequenceExpression)` | Supported | Standard `CREATE SEQUENCE` syntax is used |
| `Generate(DeleteSequenceExpression)` | Supported | Standard `DROP SEQUENCE` syntax is used |
| `Generate(AlterColumnExpression)` | Supported | DuckDB supports `ALTER TABLE ... ALTER COLUMN ... TYPE` (v0.10.0+); however, not all type conversions are implicit—if the database raises an error, the user must either provide an explicit `USING` expression via `Execute.Sql()` or temporarily set `CompatibilityMode` to `LOOSE` to skip the statement |
| `Generate(AlterDefaultConstraintExpression)` | Supported | DuckDB supports `ALTER TABLE ... ALTER COLUMN ... SET DEFAULT` |
| `Generate(DeleteDefaultConstraintExpression)` | Supported | DuckDB supports `ALTER TABLE ... ALTER COLUMN ... DROP DEFAULT` |

### 7. DuckDBProcessor

`DuckDBProcessor` should extend `GenericProcessorBase`. It should implement all schema/table/column introspection methods using the queries listed in the Research Findings section above. The `SchemaExists` method should check `information_schema.schemata`.

DuckDB is an in-process database: only one write connection is allowed at a time. FluentMigrator's `GenericProcessorBase` uses a single lazy connection, which is compatible with this constraint.

Transaction support is fully available in DuckDB. `BeginTransaction`, `CommitTransaction`, and `RollbackTransaction` can use the default `GenericProcessorBase` implementations.

### 8. DuckDBRunnerBuilderExtensions

The `AddDuckDB(this IMigrationRunnerBuilder builder)` extension method should register:
- `DuckDBDbFactory` as scoped
- `DuckDBTypeMap` as the `IDuckDBTypeMap` singleton
- `DuckDBGenerator` as scoped, also registered as `IMigrationGenerator`
- `DuckDBProcessor` as scoped, also registered as `IMigrationProcessor`

### 9. Unsupported Operations Summary

The following FluentMigrator operations cannot be supported and should be handled via `CompatibilityMode`: silently produce no-op SQL (empty string) in `LOOSE` mode, and throw a `DatabaseOperationNotSupportedException` in `STRICT` mode:

| FluentMigrator Operation | Reason |
|--------------------------|--------|
| `Create.PrimaryKey()` post table creation | DuckDB does not support `ALTER TABLE ... ADD PRIMARY KEY` after table creation; primary keys must be defined in `CREATE TABLE` |
| `Create.Index().Clustered()` | DuckDB has no concept of clustered indexes |
| `Create.Index().Include(column)` | DuckDB ART indexes do not support covering index `INCLUDE` columns |

## Integration Tests

### Why DuckDB Does Not Need a TestContainers Container

Unlike the other database providers supported by FluentMigrator (PostgreSQL, SQL Server, MySQL, Oracle, Firebird, DB2), **DuckDB is an in-process embedded database** — it runs entirely inside the calling process and does not expose a network port. There is no DuckDB server daemon and no official `Testcontainers.DuckDB` NuGet module.

This means the DuckDB integration tests should follow the same pattern as **SQLite** rather than the container-based pattern used for server databases.

### Proposed Changes to the Test Infrastructure

#### 1. `appsettings.json`

Add a `DuckDB` entry with an in-memory connection string, enabled by default (matching the SQLite entry):

```json
"DuckDB": {
    "ConnectionString": "Data Source=:memory:",
    "IsEnabled": true
}
```

No `ContainerEnabled` field is needed because DuckDB does not use a container.

#### 2. `IntegrationTestOptions.cs`

Add a static property for DuckDB alongside the other database options (note: `ProcessorIdConstants.DuckDB` must first be added to `src/FluentMigrator/ProcessorId.cs` as described in the [Project Structure](#1-project-structure) section above):

```csharp
// ReSharper disable once InconsistentNaming
public static DatabaseServerOptions DuckDB => GetOptions(ProcessorIdConstants.DuckDB);
```

#### 3. `IntegrationTestsSetup.cs`

No changes are needed. Because DuckDB is in-process, there is no container to start. `Task.WhenAll([...])` in `IntegrationTestsSetup.OneTimeSetUp` should not include a `DuckDBContainer` entry.

#### 4. Integration Test Structure

Integration tests should be placed at:

```
test/FluentMigrator.Tests/Integration/Processors/DuckDB/
├── DuckDBProcessorTests.cs
├── DuckDBTableTests.cs
├── DuckDBColumnTests.cs
├── DuckDBConstraintTests.cs
├── DuckDBIndexTests.cs
├── DuckDBSchemaTests.cs
└── DuckDBSequenceTests.cs
```

Each test class follows the SQLite pattern: a `CreateProcessorServices` helper configures the runner with `AddDuckDB()` and a `PassThroughConnectionStringReader` pointing to `"Data Source=:memory:"`, and `[OneTimeSetUp]` / `[SetUp]` / `[TearDown]` methods manage the service scope lifecycle. The test class attributes should be:

```csharp
[TestFixture]
[Category("Integration")]
[Category("DuckDB")]
// ReSharper disable once InconsistentNaming
public class DuckDBProcessorTests
{
    private ServiceProvider ServiceProvider { get; set; }
    private IServiceScope ServiceScope { get; set; }
    private DuckDBProcessor Processor { get; set; }

    private ServiceProvider CreateProcessorServices(Action<IServiceCollection> initAction = null)
    {
        IntegrationTestOptions.DuckDB.IgnoreIfNotEnabled();

        var services = ServiceCollectionExtensions.CreateServices()
            .ConfigureRunner(r => r.AddDuckDB())
            .AddScoped<IConnectionStringReader>(
                _ => new PassThroughConnectionStringReader("Data Source=:memory:"));

        initAction?.Invoke(services);
        return services.BuildServiceProvider();
    }
    // ...
}
```

### DuckDB-Specific Testing Considerations

Because DuckDB is in-process, the following testing considerations apply:

1. **In-memory isolation**: Each test class creates its own `ServiceProvider` (and thus its own in-memory database instance). Each `[SetUp]` creates a new service scope, so tables created by one test are visible to subsequent tests within the same `ServiceProvider` lifetime but not across providers. This is the same isolation model as SQLite integration tests.

2. **File-based testing**: If file-based persistence needs to be tested, a cross-platform temporary path such as `Path.Combine(Path.GetTempPath(), "fluentmigrator_duckdb_test.db")` can be substituted for the in-memory connection string. The test `[TearDown]` or `[OneTimeTearDown]` should delete the file after each test. This is analogous to the `ReplaceConnectionStringDataDirectory()` approach used by SQLite.

3. **No schema setup required**: Since DuckDB creates an in-memory database automatically on first connection, there is no equivalent to the Oracle `TestSchema` user or the SQL Server database name that must be pre-created.

4. **Concurrent write restriction**: DuckDB in-memory mode only allows a single writer at a time. Because `GenericProcessorBase` uses a single connection per processor instance and integration tests run on a single service scope at a time, this constraint is naturally satisfied without any special handling in tests.

## References

- [DuckDB SQL Introduction](https://duckdb.org/docs/stable/sql/introduction)
- [DuckDB Data Types](https://duckdb.org/docs/stable/sql/data_types/overview)
- [DuckDB ALTER TABLE Documentation](https://duckdb.org/docs/stable/sql/statements/alter_table)
- [DuckDB CREATE TABLE Documentation](https://duckdb.org/docs/stable/sql/statements/create_table)
- [DuckDB CREATE INDEX Documentation](https://duckdb.org/docs/stable/sql/indexes)
- [DuckDB CREATE SEQUENCE Documentation](https://duckdb.org/docs/stable/sql/statements/create_sequence)
- [DuckDB Catalog and Schema](https://duckdb.org/docs/stable/sql/catalog)
- [DuckDB.NET GitHub Repository](https://github.com/Giorgi/DuckDB.NET)
- [DuckDB.NET.Data NuGet Package](https://www.nuget.org/packages/DuckDB.NET.Data)

## Conclusion

DuckDB is well-suited for FluentMigrator support. Its near-complete standard SQL DDL support, native .NET ADO.NET driver, and embedded architecture map cleanly onto the existing FluentMigrator runner model. The main unsupported operations (post-creation primary key addition, clustered indexes, and covering index `INCLUDE` columns) are niche features that are already absent or limited in other supported databases such as SQLite. The implementation should follow the same pattern as `FluentMigrator.Runner.SQLite`, using `GenericProcessorBase` and `GenericGenerator` as base classes.
