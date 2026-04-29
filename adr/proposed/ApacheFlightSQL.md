# ADR: Map Out Possible Support for Apache Flight SQL Protocol

## Status
Proposed

## Context

[Apache Arrow Flight SQL](https://arrow.apache.org/docs/format/FlightSql.html) is a client-server protocol built on top of Apache Arrow Flight (a high-throughput gRPC-based data transport) that standardises how SQL databases expose query and metadata operations over Arrow record-batch streams. Rather than reinventing a wire format, Flight SQL wraps SQL statements in well-known Protobuf message types and carries result sets as zero-copy Arrow IPC streams.

### Protocol overview

| Flight SQL RPC | Protobuf command | Typical use |
|---|---|---|
| `GetFlightInfo` + `DoGet` | `CommandStatementQuery` | Execute a `SELECT` and stream results |
| `DoPut` | `CommandStatementUpdate` | Execute DDL / DML and return affected-row counts |
| `GetFlightInfo` + `DoGet` | `CommandGetTables` / `CommandGetColumns` | Introspect schema metadata |
| `DoPut` | `CommandPreparedStatementUpdate` | Execute a parameterised DML statement |
| `DoAction(CreatePreparedStatement)` | — | Prepare a statement |

DDL statements (e.g. `CREATE TABLE`) are always issued as plain SQL strings wrapped in a `CommandStatementUpdate` message delivered over `DoPut`. The Flight SQL specification defines *how* to transport the statement; the SQL dialect is fully determined by the underlying engine.

### Why this is relevant to FluentMigrator

All FluentMigrator `IMigrationGenerator` implementations produce SQL strings. A Flight SQL–based processor could route those SQL strings through `CommandStatementUpdate → DoPut` rather than a traditional ADO.NET `DbCommand.ExecuteNonQuery()`. This creates a single, protocol-level adapter that could in principle serve multiple Flight SQL–compatible engines. The key unknowns are:

1. Whether each engine's SQL dialect is close enough to a generic generator (e.g. `GenericGenerator`) to avoid per-engine overrides.
2. Whether a viable .NET ADO.NET or ADBC bridge exists to avoid writing raw gRPC client code.
3. Which DDL operations each engine supports natively.

### Databases in scope

The databases evaluated below were chosen because they explicitly advertise Flight SQL support and are likely migration targets:

| Database | Category | Flight SQL status |
|---|---|---|
| **Apache Doris** | OLAP / MPP | GA (v2.1+) |
| **StarRocks** | OLAP / MPP | GA (v3.1+) |
| **Dremio** | Data lakehouse / query engine | GA (Dremio v22+) |
| **Spice.AI OSS (Spice OSS)** | Data acceleration / federation | GA – Flight SQL is the primary interface |
| **InfluxDB v3 (Edge / Cloud Serverless)** | Time-series | GA |
| **Apache Datafusion (standalone server)** | Embeddable query engine | GA via `datafusion-flight-sql-server` |
| **DuckDB (server mode)** | Embedded OLAP | Community (`duckdb-flight-sql-server`) – see separate ADR |

---

## Research Findings

### `IMigrationGenerator` operation support matrix

The table below maps each `IMigrationGenerator` method to its DDL support status across the Flight SQL–compatible databases evaluated. Symbols:

- ✅ Supported – standard SQL syntax works as-is
- ⚠️ Partial – supported with caveats (see notes column)
- ❌ Not Supported – the database does not implement the operation

> **Note:** `INSERT` / `DELETE` / `UPDATE` (`InsertDataExpression`, `DeleteDataExpression`, `UpdateDataExpression`) are standard ANSI DML and are omitted from the per-database breakdown for brevity; all evaluated engines support them via `CommandStatementUpdate`.

#### Apache Doris

[Official DDL reference](https://doris.apache.org/docs/sql-manual/sql-statements/Data-Definition-Statements/)

Doris uses a **MySQL-compatible** SQL dialect. Schema objects map to MySQL `DATABASE` rather than ANSI `SCHEMA`.

| `IMigrationGenerator` method | SQL operation | Support | Notes |
|---|---|---|---|
| `Generate(CreateSchemaExpression)` | `CREATE DATABASE` | ✅ | Doris uses `DATABASE` not `SCHEMA`; generator must emit `CREATE DATABASE` |
| `Generate(DeleteSchemaExpression)` | `DROP DATABASE` | ✅ | Same dialect note |
| `Generate(CreateTableExpression)` | `CREATE TABLE` | ✅ | Must specify a distribution key (`DISTRIBUTED BY HASH(col) BUCKETS n`); a sensible default (first PK column, 10 buckets) is needed |
| `Generate(AlterTableExpression)` | `ALTER TABLE … RENAME` | ✅ | Supported |
| `Generate(AlterColumnExpression)` | `ALTER TABLE … MODIFY COLUMN` | ⚠️ | Type changes are restricted; Doris uses a column-store format and may silently widen types; schema-change jobs are asynchronous |
| `Generate(CreateColumnExpression)` | `ALTER TABLE … ADD COLUMN` | ✅ | Asynchronous schema change; processor must poll or use `WAIT` variants |
| `Generate(DeleteColumnExpression)` | `ALTER TABLE … DROP COLUMN` | ✅ | Asynchronous schema change |
| `Generate(DeleteTableExpression)` | `DROP TABLE` | ✅ | `IF EXISTS` supported |
| `Generate(RenameTableExpression)` | `ALTER TABLE … RENAME` | ✅ | |
| `Generate(RenameColumnExpression)` | `ALTER TABLE … RENAME COLUMN` | ✅ | v2.0+ |
| `Generate(CreateIndexExpression)` | `CREATE INDEX` | ⚠️ | Only inverted and bitmap indexes; B-tree / hash indexes are not supported; clustered indexes do not exist |
| `Generate(DeleteIndexExpression)` | `DROP INDEX` | ✅ | |
| `Generate(CreateForeignKeyExpression)` | `ALTER TABLE … ADD CONSTRAINT FOREIGN KEY` | ❌ | Doris does not enforce foreign-key constraints |
| `Generate(DeleteForeignKeyExpression)` | `ALTER TABLE … DROP FOREIGN KEY` | ❌ | Not enforced or stored |
| `Generate(AlterDefaultConstraintExpression)` | `ALTER TABLE … MODIFY COLUMN … DEFAULT` | ⚠️ | Must be expressed as part of a `MODIFY COLUMN` statement; no standalone `SET DEFAULT` clause |
| `Generate(DeleteDefaultConstraintExpression)` | `ALTER TABLE … MODIFY COLUMN … DEFAULT NULL` | ⚠️ | Same as above – default is removed by setting `DEFAULT NULL` on a `MODIFY COLUMN` |
| `Generate(CreateConstraintExpression)` | `ALTER TABLE … ADD CONSTRAINT` | ⚠️ | `PRIMARY KEY` and `UNIQUE` are stored as metadata only (not enforced); `CHECK` constraints are not supported |
| `Generate(DeleteConstraintExpression)` | `ALTER TABLE … DROP KEY` / `DROP CONSTRAINT` | ⚠️ | MySQL-style `DROP KEY` syntax; enforcement model differs from ANSI |
| `Generate(AlterSchemaExpression)` | Move table across databases | ❌ | Not supported; tables cannot be moved between Doris databases |
| `Generate(CreateSequenceExpression)` | `CREATE SEQUENCE` | ❌ | Doris does not support sequences; auto-increment columns exist but cannot be managed as standalone objects |
| `Generate(DeleteSequenceExpression)` | `DROP SEQUENCE` | ❌ | Not supported |

---

#### StarRocks

[Official DDL reference](https://docs.starrocks.io/docs/sql-reference/sql-statements/table_bucket_part_index/CREATE_TABLE/)

StarRocks originated as a fork of Apache Doris and shares the same MySQL-compatible DDL dialect. The main differences are that StarRocks has richer index types and a more mature `ALTER TABLE` implementation.

| `IMigrationGenerator` method | SQL operation | Support | Notes |
|---|---|---|---|
| `Generate(CreateSchemaExpression)` | `CREATE DATABASE` | ✅ | MySQL dialect; `DATABASE` instead of `SCHEMA` |
| `Generate(DeleteSchemaExpression)` | `DROP DATABASE` | ✅ | |
| `Generate(CreateTableExpression)` | `CREATE TABLE` | ✅ | Requires distribution clause (`DISTRIBUTED BY HASH(col)`); engine type (`OLAP`, `HIVE`, etc.) may need to be specified |
| `Generate(AlterTableExpression)` | `ALTER TABLE … RENAME` | ✅ | |
| `Generate(AlterColumnExpression)` | `ALTER TABLE … MODIFY COLUMN` | ⚠️ | Some type conversions require `SCHEMA CHANGE` jobs; these are asynchronous in the storage-layer tables |
| `Generate(CreateColumnExpression)` | `ALTER TABLE … ADD COLUMN` | ✅ | |
| `Generate(DeleteColumnExpression)` | `ALTER TABLE … DROP COLUMN` | ✅ | |
| `Generate(DeleteTableExpression)` | `DROP TABLE` | ✅ | |
| `Generate(RenameTableExpression)` | `ALTER TABLE … RENAME` | ✅ | |
| `Generate(RenameColumnExpression)` | `ALTER TABLE … RENAME COLUMN` | ✅ | v2.5+ |
| `Generate(CreateIndexExpression)` | `CREATE INDEX` | ⚠️ | Bitmap, inverted, and `NGRAMBF` indexes supported; traditional B-tree indexes are not |
| `Generate(DeleteIndexExpression)` | `DROP INDEX` | ✅ | |
| `Generate(CreateForeignKeyExpression)` | `ALTER TABLE … ADD CONSTRAINT FOREIGN KEY` | ❌ | Metadata-only; not enforced |
| `Generate(DeleteForeignKeyExpression)` | `ALTER TABLE … DROP FOREIGN KEY` | ❌ | Not enforced |
| `Generate(AlterDefaultConstraintExpression)` | `ALTER TABLE … MODIFY COLUMN` | ⚠️ | Expressed via `MODIFY COLUMN` |
| `Generate(DeleteDefaultConstraintExpression)` | `ALTER TABLE … MODIFY COLUMN … DEFAULT NULL` | ⚠️ | |
| `Generate(CreateConstraintExpression)` | `ALTER TABLE … ADD CONSTRAINT` | ⚠️ | `PRIMARY KEY` / `UNIQUE` stored as metadata; `CHECK` not supported |
| `Generate(DeleteConstraintExpression)` | `ALTER TABLE … DROP …` | ⚠️ | MySQL-style syntax |
| `Generate(AlterSchemaExpression)` | Move table across databases | ❌ | Not supported |
| `Generate(CreateSequenceExpression)` | `CREATE SEQUENCE` | ❌ | Not supported |
| `Generate(DeleteSequenceExpression)` | `DROP SEQUENCE` | ❌ | Not supported |

---

#### Dremio

[Official DDL reference](https://docs.dremio.com/software/sql-reference/sql-commands/ddl/)

Dremio is a data lakehouse query engine. Its "tables" are backed by Apache Iceberg. The SQL dialect is ANSI-like but is oriented toward data lake objects (spaces, sources, folders) rather than classic relational schemas.

| `IMigrationGenerator` method | SQL operation | Support | Notes |
|---|---|---|---|
| `Generate(CreateSchemaExpression)` | `CREATE FOLDER` / `CREATE SCHEMA` | ⚠️ | Dremio organises objects in spaces and folders rather than ANSI schemas; `CREATE SCHEMA` is unsupported – use `CREATE FOLDER` or REST API; requires mapping |
| `Generate(DeleteSchemaExpression)` | `DROP FOLDER` | ⚠️ | Same mapping issue |
| `Generate(CreateTableExpression)` | `CREATE TABLE … AS SELECT` / `CREATE TABLE` | ⚠️ | Dremio creates Iceberg tables; empty `CREATE TABLE` (without `AS SELECT`) is supported in v22+; column-level constraints (PK, FK, NOT NULL) are stored as metadata only |
| `Generate(AlterTableExpression)` | `ALTER TABLE` | ⚠️ | Supports `RENAME`, `ADD COLUMNS`, `DROP COLUMN`, `CHANGE COLUMN`; no blanket support for all ANSI `ALTER TABLE` forms |
| `Generate(AlterColumnExpression)` | `ALTER TABLE … CHANGE COLUMN` | ⚠️ | Type promotions only (widening); narrowing is not supported by Iceberg |
| `Generate(CreateColumnExpression)` | `ALTER TABLE … ADD COLUMNS` | ✅ | |
| `Generate(DeleteColumnExpression)` | `ALTER TABLE … DROP COLUMN` | ✅ | |
| `Generate(DeleteTableExpression)` | `DROP TABLE` | ✅ | |
| `Generate(RenameTableExpression)` | `ALTER TABLE … RENAME TO` | ✅ | |
| `Generate(RenameColumnExpression)` | `ALTER TABLE … RENAME COLUMN` | ✅ | Iceberg schema evolution |
| `Generate(CreateIndexExpression)` | N/A | ❌ | Dremio does not support user-managed indexes; query acceleration uses reflections |
| `Generate(DeleteIndexExpression)` | N/A | ❌ | Not supported |
| `Generate(CreateForeignKeyExpression)` | Metadata constraint | ⚠️ | `ALTER TABLE … ADD PRIMARY KEY / FOREIGN KEY` accepted as Iceberg metadata; not enforced |
| `Generate(DeleteForeignKeyExpression)` | `ALTER TABLE … DROP CONSTRAINT` | ⚠️ | Metadata only |
| `Generate(AlterDefaultConstraintExpression)` | N/A | ❌ | Dremio / Iceberg does not support column defaults in DDL |
| `Generate(DeleteDefaultConstraintExpression)` | N/A | ❌ | Not supported |
| `Generate(CreateConstraintExpression)` | Metadata constraint | ⚠️ | Stored as Iceberg metadata; not enforced |
| `Generate(DeleteConstraintExpression)` | `ALTER TABLE … DROP CONSTRAINT` | ⚠️ | Metadata only |
| `Generate(AlterSchemaExpression)` | Move table across folders | ❌ | Not supported via SQL |
| `Generate(CreateSequenceExpression)` | `CREATE SEQUENCE` | ❌ | Not supported |
| `Generate(DeleteSequenceExpression)` | `DROP SEQUENCE` | ❌ | Not supported |

---

#### Spice.AI OSS

[Official documentation](https://docs.spiceai.org)

Spice OSS is a portable data acceleration and federation runtime built on Apache Arrow DataFusion. Flight SQL is its primary query interface. Spice is fundamentally a read-acceleration layer; schema modification is performed by configuring *spicepods* (YAML manifests), not by issuing DDL.

| `IMigrationGenerator` method | SQL operation | Support | Notes |
|---|---|---|---|
| `Generate(CreateSchemaExpression)` | N/A | ❌ | Schemas are defined in `spicepod.yaml`; no DDL interface |
| `Generate(DeleteSchemaExpression)` | N/A | ❌ | Same |
| `Generate(CreateTableExpression)` | N/A | ❌ | Tables ("datasets") are declared in YAML manifests; `CREATE TABLE` is not supported |
| `Generate(AlterTableExpression)` | N/A | ❌ | Not supported via SQL |
| `Generate(AlterColumnExpression)` | N/A | ❌ | Not supported via SQL |
| `Generate(CreateColumnExpression)` | N/A | ❌ | Not supported via SQL |
| `Generate(DeleteColumnExpression)` | N/A | ❌ | Not supported via SQL |
| `Generate(DeleteTableExpression)` | N/A | ❌ | Not supported via SQL |
| `Generate(RenameTableExpression)` | N/A | ❌ | Not supported via SQL |
| `Generate(RenameColumnExpression)` | N/A | ❌ | Not supported via SQL |
| `Generate(CreateIndexExpression)` | N/A | ❌ | Not supported |
| `Generate(DeleteIndexExpression)` | N/A | ❌ | Not supported |
| `Generate(CreateForeignKeyExpression)` | N/A | ❌ | Not supported |
| `Generate(DeleteForeignKeyExpression)` | N/A | ❌ | Not supported |
| `Generate(AlterDefaultConstraintExpression)` | N/A | ❌ | Not supported |
| `Generate(DeleteDefaultConstraintExpression)` | N/A | ❌ | Not supported |
| `Generate(CreateConstraintExpression)` | N/A | ❌ | Not supported |
| `Generate(DeleteConstraintExpression)` | N/A | ❌ | Not supported |
| `Generate(AlterSchemaExpression)` | N/A | ❌ | Not supported |
| `Generate(CreateSequenceExpression)` | N/A | ❌ | Not supported |
| `Generate(DeleteSequenceExpression)` | N/A | ❌ | Not supported |

**Verdict:** Spice OSS is unsuitable as a FluentMigrator target. It exposes Flight SQL for *querying* only; all schema management is configuration-driven.

---

#### InfluxDB v3 (Edge / Cloud Serverless)

[Official Flight SQL documentation](https://docs.influxdata.com/influxdb/cloud-serverless/reference/client-libraries/flight/)

InfluxDB v3 is a time-series database that uses Apache Arrow DataFusion and exposes Flight SQL as its primary query interface. The SQL dialect is ANSI-compatible for queries; DDL is minimal because the schema is mostly inferred from writes (schema-on-write).

| `IMigrationGenerator` method | SQL operation | Support | Notes |
|---|---|---|---|
| `Generate(CreateSchemaExpression)` | N/A | ❌ | Buckets / databases are managed via the InfluxDB HTTP API or CLI, not SQL |
| `Generate(DeleteSchemaExpression)` | N/A | ❌ | Same |
| `Generate(CreateTableExpression)` | N/A | ❌ | Measurements (tables) are created automatically on first write; no `CREATE TABLE` DDL |
| `Generate(AlterTableExpression)` | N/A | ❌ | Not supported |
| `Generate(AlterColumnExpression)` | N/A | ❌ | Schema is inferred; column types cannot be changed after initial write |
| `Generate(CreateColumnExpression)` | N/A | ❌ | Columns are added automatically on write |
| `Generate(DeleteColumnExpression)` | N/A | ❌ | Column deletion requires data compaction tools |
| `Generate(DeleteTableExpression)` | `DELETE FROM <measurement>` | ⚠️ | Data can be deleted but the measurement schema persists until all data is gone |
| `Generate(RenameTableExpression)` | N/A | ❌ | Not supported |
| `Generate(RenameColumnExpression)` | N/A | ❌ | Not supported |
| `Generate(CreateIndexExpression)` | N/A | ❌ | InfluxDB manages its own columnar indexing |
| `Generate(DeleteIndexExpression)` | N/A | ❌ | Not supported |
| `Generate(CreateForeignKeyExpression)` | N/A | ❌ | Not supported |
| `Generate(DeleteForeignKeyExpression)` | N/A | ❌ | Not supported |
| `Generate(AlterDefaultConstraintExpression)` | N/A | ❌ | Not supported |
| `Generate(DeleteDefaultConstraintExpression)` | N/A | ❌ | Not supported |
| `Generate(CreateConstraintExpression)` | N/A | ❌ | Not supported |
| `Generate(DeleteConstraintExpression)` | N/A | ❌ | Not supported |
| `Generate(AlterSchemaExpression)` | N/A | ❌ | Not supported |
| `Generate(CreateSequenceExpression)` | N/A | ❌ | Not supported |
| `Generate(DeleteSequenceExpression)` | N/A | ❌ | Not supported |

**Verdict:** InfluxDB v3 uses Flight SQL purely as a query transport; its schema-on-write model means FluentMigrator-style DDL migrations are not applicable.

---

#### Apache DataFusion (standalone Flight SQL server)

[DataFusion Flight SQL server example](https://github.com/apache/arrow-datafusion/tree/main/datafusion-examples/examples/flight)

DataFusion is an embeddable query engine that can be wrapped in a Flight SQL server. In this configuration, DDL support depends entirely on the host application. The DataFusion engine itself supports a subset of ANSI DDL.

| `IMigrationGenerator` method | SQL operation | Support | Notes |
|---|---|---|---|
| `Generate(CreateSchemaExpression)` | `CREATE SCHEMA` | ✅ | Supported in DataFusion's SQL parser |
| `Generate(DeleteSchemaExpression)` | `DROP SCHEMA` | ✅ | |
| `Generate(CreateTableExpression)` | `CREATE TABLE` / `CREATE EXTERNAL TABLE` | ⚠️ | In-memory or external (Parquet, CSV); `CREATE TABLE` creates an in-memory table unless the host maps it to a persistent store |
| `Generate(AlterTableExpression)` | Limited | ⚠️ | `ALTER TABLE … RENAME` supported; full `ALTER TABLE` coverage is partial |
| `Generate(AlterColumnExpression)` | Not natively supported | ❌ | DataFusion does not currently implement `ALTER COLUMN` |
| `Generate(CreateColumnExpression)` | `ALTER TABLE … ADD COLUMN` | ⚠️ | Planned / partial support; depends on DataFusion version |
| `Generate(DeleteColumnExpression)` | N/A | ❌ | Not supported |
| `Generate(DeleteTableExpression)` | `DROP TABLE` | ✅ | |
| `Generate(RenameTableExpression)` | `ALTER TABLE … RENAME TO` | ✅ | |
| `Generate(RenameColumnExpression)` | N/A | ❌ | Not supported |
| `Generate(CreateIndexExpression)` | N/A | ❌ | DataFusion manages its own physical planning; user-managed indexes are not exposed |
| `Generate(DeleteIndexExpression)` | N/A | ❌ | Not supported |
| `Generate(CreateForeignKeyExpression)` | N/A | ❌ | Not supported |
| `Generate(DeleteForeignKeyExpression)` | N/A | ❌ | Not supported |
| `Generate(AlterDefaultConstraintExpression)` | N/A | ❌ | Not supported |
| `Generate(DeleteDefaultConstraintExpression)` | N/A | ❌ | Not supported |
| `Generate(CreateConstraintExpression)` | N/A | ❌ | Constraints are not enforced or stored |
| `Generate(DeleteConstraintExpression)` | N/A | ❌ | Not supported |
| `Generate(AlterSchemaExpression)` | N/A | ❌ | Not supported |
| `Generate(CreateSequenceExpression)` | N/A | ❌ | Not supported |
| `Generate(DeleteSequenceExpression)` | N/A | ❌ | Not supported |

---

### Cross-database summary

The following table provides a high-level view of DDL coverage across all evaluated engines. ✅ = broadly supported, ⚠️ = partial, ❌ = not supported.

| `IMigrationGenerator` method | Doris | StarRocks | Dremio | Spice.AI | InfluxDB v3 | DataFusion |
|---|:---:|:---:|:---:|:---:|:---:|:---:|
| `CreateSchema` | ✅ | ✅ | ⚠️ | ❌ | ❌ | ✅ |
| `DeleteSchema` | ✅ | ✅ | ⚠️ | ❌ | ❌ | ✅ |
| `CreateTable` | ✅ | ✅ | ⚠️ | ❌ | ❌ | ⚠️ |
| `AlterTable` | ✅ | ✅ | ⚠️ | ❌ | ❌ | ⚠️ |
| `AlterColumn` | ⚠️ | ⚠️ | ⚠️ | ❌ | ❌ | ❌ |
| `CreateColumn` | ✅ | ✅ | ✅ | ❌ | ❌ | ⚠️ |
| `DeleteColumn` | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| `DeleteTable` | ✅ | ✅ | ✅ | ❌ | ⚠️ | ✅ |
| `RenameTable` | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ |
| `RenameColumn` | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| `CreateIndex` | ⚠️ | ⚠️ | ❌ | ❌ | ❌ | ❌ |
| `DeleteIndex` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `CreateForeignKey` | ❌ | ❌ | ⚠️ | ❌ | ❌ | ❌ |
| `DeleteForeignKey` | ❌ | ❌ | ⚠️ | ❌ | ❌ | ❌ |
| `AlterDefaultConstraint` | ⚠️ | ⚠️ | ❌ | ❌ | ❌ | ❌ |
| `DeleteDefaultConstraint` | ⚠️ | ⚠️ | ❌ | ❌ | ❌ | ❌ |
| `CreateConstraint` | ⚠️ | ⚠️ | ⚠️ | ❌ | ❌ | ❌ |
| `DeleteConstraint` | ⚠️ | ⚠️ | ⚠️ | ❌ | ❌ | ❌ |
| `AlterSchema` | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| `CreateSequence` | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| `DeleteSequence` | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |

### .NET driver landscape

A critical prerequisite for any FluentMigrator processor is an ADO.NET–compatible driver (`DbProviderFactory` / `DbConnection`). The current options for Flight SQL in .NET are:

| Approach | Package / project | ADO.NET compatible | Maturity |
|---|---|:---:|---|
| Apache Arrow ADBC .NET | [`Apache.Arrow.Adbc`](https://github.com/apache/arrow-adbc) (incubating) | ⚠️ | `AdbcConnection` wraps an ADO.NET–like API but is not a `DbConnection` subclass; shim required |
| Raw gRPC Flight client | `Apache.Arrow.Flight` + hand-written gRPC | ❌ | Low-level; no ADO.NET surface |
| Database-specific ODBC driver | Doris/StarRocks MySQL ODBC, Dremio ODBC | ✅ | Varies; bypasses Flight SQL entirely |
| Database-specific JDBC bridge | `JDBC Bridge for ADO.NET` or `IKVM` | ⚠️ | Complex; not production-grade in .NET |
| Dremio `.NET` client | None first-party | ❌ | Community-only |

The `Apache.Arrow.Adbc` package is the most promising path. Version `0.14+` exposes `AdbcConnection`, `AdbcCommand`, and `AdbcDataReader` which are structurally equivalent to their `System.Data` counterparts. A thin `FlightSqlDbConnection : DbConnection` wrapper could delegate to `AdbcConnection`.

---

## Architecture Decision

### Recommendation

Based on the research above, the findings are:

1. **Apache Doris and StarRocks** are the strongest candidates for a dedicated FluentMigrator runner. Both have broad DDL coverage, an established MySQL-compatible SQL dialect (allowing reuse of `MySqlGenerator` as a base), and production-quality Flight SQL endpoints. The primary challenges are the requirement for a distribution clause in `CREATE TABLE` and the asynchronous nature of some schema-change operations.

2. **Dremio** has partial DDL support oriented toward Iceberg table evolution. It is a viable candidate for a limited runner that supports `CREATE TABLE`, `ALTER TABLE` column operations, and renames – but not indexes, sequences, or schema-level moves.

3. **Spice.AI OSS** and **InfluxDB v3** are **not suitable** as FluentMigrator targets because they do not support SQL DDL at all; their Flight SQL endpoints are query-only.

4. **Apache DataFusion** server mode could support a minimal runner for use in test/development scenarios but its DDL surface is too narrow for production migrations.

5. A **generic Flight SQL processor** is not feasible as a single artefact because Flight SQL defines only the transport, not the SQL dialect. Each engine would still need its own `IMigrationGenerator` implementation.

### Proposed implementation strategy

If one or more of the above databases are to be implemented, the recommended approach is:

#### Per-engine runners (preferred)

Following the same pattern as `FluentMigrator.Runner.MySql`, `FluentMigrator.Runner.Postgres`, etc.:

```
src/FluentMigrator.Runner.Doris/      (Apache Doris)
src/FluentMigrator.Runner.StarRocks/  (StarRocks)
src/FluentMigrator.Runner.Dremio/     (Dremio)
```

Each project contains:
- A `*TypeMap` extending `TypeMapBase`
- A `*Quoter` extending `GenericQuoter`
- A `*Generator` extending `GenericGenerator` (or `MySqlGenerator` for Doris/StarRocks)
- A `*Processor` extending `GenericProcessorBase`
- A `*RunnerBuilderExtensions` registration helper

#### Shared Flight SQL transport layer (optional)

A shared project `FluentMigrator.Runner.FlightSql` could house:
- A `FlightSqlDbFactory` wrapping `Apache.Arrow.Adbc.AdbcConnection`
- A `FlightSqlDbCommand` that routes DDL strings via `CommandStatementUpdate → DoPut`
- An `AdbcConnectionAdapter : DbConnection` bridge (pending upstreaming into `Apache.Arrow.Adbc`)

This layer would be an *optional* transport substitute; the per-engine generator and type-map would remain engine-specific.

#### Distribution clause for Doris / StarRocks

Both engines require a distribution strategy in `CREATE TABLE`. Since `CreateTableExpression` has no distribution-strategy concept, the runner should apply a convention-based default (e.g. distribute by primary key, 10 buckets) and expose it as a configurable option on `DorisRunnerBuilderExtensions` / `StarRocksRunnerBuilderExtensions`.

#### Asynchronous schema changes

Doris and StarRocks execute some `ALTER TABLE` operations as asynchronous background jobs. The processor should poll `SHOW ALTER TABLE COLUMN WHERE TableName = '…' ORDER BY JobId DESC LIMIT 1` and wait for the job to reach `FINISHED` or `CANCELLED` state before returning. A configurable timeout (default: 60 s) should be exposed via processor options.

### Unsupported operations (all engines)

The following FluentMigrator operations cannot be mapped to any of the evaluated Flight SQL engines and should produce a `DatabaseOperationNotSupportedException` in `STRICT` compatibility mode (or a no-op in `LOOSE` mode):

| Operation | Reason |
|---|---|
| `AlterSchema` (move table across schemas) | No Flight SQL engine evaluated supports cross-schema table moves via SQL |
| `CreateSequence` / `DeleteSequence` | None of the evaluated engines implement SQL sequences |
| `CreateIndex` with `Clustered()` | OLAP engines use their own physical organisation strategies; clustered index semantics do not apply |
| `CreateForeignKey` | Enforced foreign keys are not supported by any of the OLAP-oriented engines evaluated |

---

## References

- [Apache Arrow Flight SQL specification](https://arrow.apache.org/docs/format/FlightSql.html)
- [Apache Arrow ADBC .NET](https://github.com/apache/arrow-adbc/tree/main/csharp)
- [Apache Arrow Flight .NET client](https://github.com/apache/arrow/tree/main/csharp/src/Apache.Arrow.Flight)
- [Apache Doris DDL reference](https://doris.apache.org/docs/sql-manual/sql-statements/Data-Definition-Statements/)
- [Apache Doris Flight SQL support](https://doris.apache.org/docs/lakehouse/arrow-flight-sql/)
- [StarRocks DDL reference](https://docs.starrocks.io/docs/sql-reference/sql-statements/table_bucket_part_index/CREATE_TABLE/)
- [StarRocks Arrow Flight SQL](https://docs.starrocks.io/docs/integrations/other-integrations/Arrow_Flight_SQL/)
- [Dremio DDL reference](https://docs.dremio.com/software/sql-reference/sql-commands/ddl/)
- [Dremio Flight SQL documentation](https://docs.dremio.com/software/client-applications/arrow-flight/)
- [Spice.AI OSS documentation](https://docs.spiceai.org)
- [Spice.AI Flight SQL interface](https://docs.spiceai.org/clients/arrow-flight-sql)
- [InfluxDB v3 Flight SQL](https://docs.influxdata.com/influxdb/cloud-serverless/reference/client-libraries/flight/)
- [Apache DataFusion Flight SQL example](https://github.com/apache/arrow-datafusion/tree/main/datafusion-examples/examples/flight)
- [Apache Arrow ADBC `AdbcConnection` API](https://arrow.apache.org/adbc/current/driver/adbc.html)
