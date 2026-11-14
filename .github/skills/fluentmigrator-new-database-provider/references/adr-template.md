# ADR template for a new FluentMigrator database provider

Copy everything below the line into `adr/proposed/<Db>.md`, replace `<Db>`, and fill
in every table row. Do not delete rows you have not researched — an unfilled row is a
visible TODO; a deleted row is a silent gap that becomes a bug report later.

Legend used throughout the matrices:

- ✅ Supported — the standard `GenericGenerator` output works unchanged, or the exact
  syntax is written in the Notes column.
- ⚠️ Partial — works with a caveat (version-gated, restricted to certain types, needs
  a different clause order, requires multiple statements). Notes must say which.
- ❌ Not supported — the engine cannot do it. The generator will route this through
  `CompatibilityMode.HandleCompatibility(...)`.

The DDL matrix is keyed on `IMigrationGenerator` methods rather than on SQL statements
because that is the unit of work in the generator: one row = one `Generate` override
decision = one unit test.

---

# ADR: <Db> Processor/Generator/TypeMap

## Status
Proposed

## Context

One paragraph: what the engine is (OLTP/OLAP, client-server/embedded, wire-compatible
with what), why users would run FluentMigrator against it, and which existing provider
it most resembles.

### Use Cases
- ...

### .NET Driver
- Package: `<PackageId>` (version tested: `x.y.z`, licence: `<licence>`)
- Assembly / factory type: `<Assembly>`, `<Namespace>.<FactoryType>`
- Factory access: `static Instance` member | public parameterless ctor (one of the two is required for the NativeAOT path in `ReflectionBasedDbFactory.TryCreateFromPreloadedType`; neither → blocker)
- `TestEntry` resolver string: `"<Namespace>.<FactoryType>, <AssemblyName>"` (constant; used in `() => Type.GetType(...)`)
- Targets: netstandard2.0 ✅/❌, net48 ✅/❌, NativeAOT/trim-safe ✅/❌/unknown
- Native binaries required: yes/no; if yes, which package provides them. The runner package never hard-references the driver — the consuming app does — so note here what the consumer must install.

### Connection String Format
- Example(s) with every keyword the processor or tests rely on.

### Schema Model
- Naming hierarchy (catalog → schema → table, or database → table).
- Default schema name and identifier case-folding rules.
- Max identifier length. Whether `CREATE SCHEMA` exists.

### Transactions
- Transactional DDL: yes/no/partial (which statements auto-commit).
- Implication for `GenericProcessorBase.BeginTransaction/Commit/Rollback` (keep defaults or override).

### Statement Batching
- Statement separator; whether the driver accepts multi-statement commands; whether
  a `<Db>BatchParser` (see `SnowflakeBatchParser`) is needed for `Execute.Script`.

## Research Findings

### DDL Support Matrix (`IMigrationGenerator`)

| `IMigrationGenerator` method | SQL operation | Support | Exact syntax / notes | Doc link |
|---|---|---|---|---|
| `Generate(CreateSchemaExpression)` | `CREATE SCHEMA` | | | |
| `Generate(DeleteSchemaExpression)` | `DROP SCHEMA` | | | |
| `Generate(AlterSchemaExpression)` | move table between schemas | | | |
| `Generate(CreateTableExpression)` | `CREATE TABLE` | | inline PK? inline FK? `IF NOT EXISTS`? | |
| `Generate(AlterTableExpression)` | `ALTER TABLE` (description only) | | | |
| `Generate(DeleteTableExpression)` | `DROP TABLE` | | `IF EXISTS`? | |
| `Generate(RenameTableExpression)` | rename table | | `RENAME TABLE a TO b` vs `ALTER TABLE a RENAME TO b` | |
| `Generate(CreateColumnExpression)` | `ALTER TABLE ADD [COLUMN]` | | `COLUMN` keyword required/forbidden? `NOT NULL` without default allowed on non-empty table? | |
| `Generate(AlterColumnExpression)` | change type / nullability / default | | one statement or three? `TYPE` keyword? `USING` needed? | |
| `Generate(DeleteColumnExpression)` | `ALTER TABLE DROP COLUMN` | | multiple columns in one statement? | |
| `Generate(RenameColumnExpression)` | rename column | | | |
| `Generate(CreateIndexExpression)` | `CREATE [UNIQUE] INDEX` | | clustered? `INCLUDE`? filtered (`WHERE`)? nulls-distinct? | |
| `Generate(DeleteIndexExpression)` | `DROP INDEX` | | needs `ON table`? schema-qualified index name? | |
| `Generate(CreateForeignKeyExpression)` | `ADD CONSTRAINT ... FOREIGN KEY` | | which `ON DELETE/UPDATE` rules exist (CASCADE/SET NULL/SET DEFAULT/NO ACTION/RESTRICT)? enforced or informational? | |
| `Generate(DeleteForeignKeyExpression)` | `DROP CONSTRAINT` / `DROP FOREIGN KEY` | | | |
| `Generate(CreateConstraintExpression)` — PRIMARY KEY | `ADD CONSTRAINT ... PRIMARY KEY` | | allowed after table creation? | |
| `Generate(CreateConstraintExpression)` — UNIQUE | `ADD CONSTRAINT ... UNIQUE` | | | |
| `Generate(DeleteConstraintExpression)` | `DROP CONSTRAINT` | | by name? PK needs `DROP PRIMARY KEY`? | |
| `Generate(AlterDefaultConstraintExpression)` | set column default | | `ALTER COLUMN SET DEFAULT` vs named default constraint | |
| `Generate(DeleteDefaultConstraintExpression)` | drop column default | | | |
| `Generate(CreateSequenceExpression)` | `CREATE SEQUENCE` | | `INCREMENT BY`/`MINVALUE`/`MAXVALUE`/`START WITH`/`CACHE`/`CYCLE` — which exist? | |
| `Generate(DeleteSequenceExpression)` | `DROP SEQUENCE` | | | |
| `Generate(InsertDataExpression)` | `INSERT` | | multi-row `VALUES`? identity insert toggle? | |
| `Generate(UpdateDataExpression)` | `UPDATE` | | `UPDATE ... WHERE 1=1` for all rows? | |
| `Generate(DeleteDataExpression)` | `DELETE` | | `IS NULL` handling in `WHERE`? | |

### Column feature matrix (`ColumnBase` clauses)

| Feature | Support | Syntax / notes |
|---|---|---|
| Identity / auto-increment | | `GENERATED ... AS IDENTITY`, `AUTO_INCREMENT`, `SERIAL`, sequence-backed? |
| Identity with seed/increment | | |
| Computed / generated columns (`.WithColumnExpression`) | | stored vs virtual |
| Column description / comment (`.WithColumnDescription`) | | inline `COMMENT` vs `COMMENT ON COLUMN` |
| Table description | | |
| Collation | | |
| Clause order (type, nullability, default, identity, PK) | | any deviation from `ColumnBase.ClauseOrder` |

### Type Mapping (`DbType` → native type)

| `DbType` | Native type | Sized variant | Max size / precision | Notes |
|---|---|---|---|---|
| `AnsiStringFixedLength` | | | | |
| `AnsiString` | | | | |
| `Binary` | | | | |
| `Boolean` | | | | |
| `Byte` | | | | |
| `Currency` | | | | |
| `Date` | | | | |
| `DateTime` | | | | |
| `DateTime2` | | | | |
| `DateTimeOffset` | | | | |
| `Decimal` | | | | |
| `Double` | | | | |
| `Guid` | | | | |
| `Int16` | | | | |
| `Int32` | | | | |
| `Int64` | | | | |
| `Object` | | | | |
| `SByte` | | | | |
| `Single` | | | | |
| `String` | | | | |
| `StringFixedLength` | | | | |
| `Time` | | | | |
| `UInt16` | | | | |
| `UInt32` | | | | |
| `UInt64` | | | | |
| `VarNumeric` | | | | |
| `Xml` | | | | |

### Literal / quoting rules (`<Db>Quoter`)

| Item | Value |
|---|---|
| Identifier quote open/close | |
| Escape of quote inside identifier | |
| String literal escape | |
| Boolean literal | |
| Date / DateTime / DateTimeOffset literal format | |
| Guid literal | |
| Binary literal | |
| `SystemMethods.CurrentDateTime` | |
| `SystemMethods.CurrentUTCDateTime` | |
| `SystemMethods.NewGuid` / `NewSequentialId` | |
| `SystemMethods.CurrentUser` | |

### Introspection Queries (`<Db>Processor`)

Give the exact SQL, parameterised with `{0}`, `{1}`, ... as the processor will call
`string.Format`. Note case-sensitivity (`ILIKE`, `LOWER(...)`, `UPPER(...)`).

| Method | Query |
|---|---|
| `SchemaExists` | |
| `TableExists` | |
| `ColumnExists` | |
| `ConstraintExists` | |
| `IndexExists` | |
| `SequenceExists` | |
| `DefaultValueExists` | |
| `ReadTableData` | `SELECT * FROM {quoted table}` |

## Architecture Decision

### 1. Project Structure
Tree of `src/FluentMigrator.Runner.<Db>/` (and `src/FluentMigrator.Extensions.<Db>/`
if fluent-API extensions are needed for engine-specific column/index options).
State the `ProcessorIdConstants.<Db>` / `GeneratorIdConstants.<Db>` values and aliases.

### 2. NuGet Dependencies
`FluentMigrator.Runner.Core` (+ `FluentMigrator.Extensions.<Db>` if created). The driver is
loaded via `TestEntry` with a constant `Type.GetType` lambda, not referenced — required for
NativeAOT (`TrimAot.props`). State the driver's own trim/AOT status and any consumer-side
`TrimmerRootAssembly` / `DynamicDependency` needs.

### 3. `<Db>DbFactory`
The `TestEntry` list.

### 4. `<Db>TypeMap`
Anything beyond "register the table above".

### 5. `<Db>Quoter`
Overrides list.

### 6. `<Db>Column`
Overrides list with reasons (clause order, identity, nullable, default, PK inline vs separate).

### 7. `<Db>Generator`
| Method / property | Action (override / inherit / HandleCompatibility) | Reason |
|---|---|---|

### 8. `<Db>Processor`
Base class, transaction behaviour, any `Process(string)` special-casing (e.g. batch
splitting, statement terminator stripping).

### 9. `<Db>Options` (if any)
Option keys, defaults, how they reach the quoter/generator.

### 10. `Add<Db>()` registration
Service lifetimes (scoped processor/generator/quoter; `ITypeMap` scoped or singleton).

### 11. Unsupported Operations Summary
| FluentMigrator operation | Reason | LOOSE behaviour | STRICT behaviour |
|---|---|---|---|

## Integration Tests

### Container strategy
- Official `Testcontainers.<Db>` module: yes/no. If no: image `<registry/image:tag>`,
  exposed port, env vars, wait strategy, licence-acceptance flag, supported
  architectures, startup time.
- Or: embedded engine, follows the SQLite pattern, no container.

### Test infrastructure changes
`appsettings.json` block, `IntegrationTestOptions` property, `IntegrationTestsSetup`
entry, `ProcessorTestCaseSource` entry, CI env vars, test helpers.

### Engine-specific testing considerations
Isolation model, required pre-created schema/user, slow startup, case-folding traps.

## References
- Official DDL docs, type docs, `information_schema` docs, driver repo, Docker image page.

## Conclusion
Two or three sentences: is this a good fit, which existing provider is the template,
and what the notable gaps are.
