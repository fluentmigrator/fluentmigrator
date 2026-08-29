---
name: fluentmigrator-new-database-provider
description: Step-by-step workflow for adding support for a new database engine/driver to FluentMigrator (https://github.com/fluentmigrator/fluentmigrator) — a new Processor, Generator, Quoter, TypeMap, Column, DbFactory, DI registration, unit tests, TestContainers-backed integration tests, and the ADR that must be written first. Use this whenever someone wants FluentMigrator to "support", "add a provider/processor/generator for", or "port migrations to" any database (DuckDB, ClickHouse, CockroachDB, YugabyteDB, TiDB, SingleStore, Databricks, BigQuery, Trino, Informix, Sybase, Ingres, H2, Exasol, etc.), asks how FluentMigrator's Runner.* projects are structured, asks how to add a TestContainers container to the FluentMigrator test suite, or asks how to write a FluentMigrator ADR for a new dialect. Also use it when reviewing a PR that adds a new FluentMigrator provider.
---

# Adding a new database provider to FluentMigrator

FluentMigrator turns fluent C# expressions (`Create.Table(...)`, `Alter.Column(...)`) into
SQL via an `IMigrationGenerator`, and executes/introspects via an `IMigrationProcessor`.
Every supported engine lives in its own `src/FluentMigrator.Runner.<Db>/` project and its
own `test/FluentMigrator.Tests/{Unit/Generators,Integration/Processors}/<Db>/` folders.

The single most common failure when adding a provider is writing generator code first
and discovering halfway through that the engine cannot do `ALTER COLUMN ... TYPE`,
has no schemas, or cannot add a primary key after `CREATE TABLE`. Those discoveries then
get hard-coded ad hoc. So the workflow here is deliberately **research → ADR → tests →
code**, not the other way round. Do not skip the ADR phase.

Reference files (read when you reach that phase):

- `references/adr-template.md` — the ADR skeleton with the full `IMigrationGenerator`
  support matrix, `DbType` map table, and introspection-query table to fill in.
- `references/touchpoints.md` — every file in the repo that must be touched for a
  provider to be fully wired (DI, CLI, console, MSBuild, analyzers, docs, CI).
- `references/code-skeletons.md` — annotated C# skeletons for each class and test.
- `references/testcontainers.md` — sourcing Docker images, official `Testcontainers.*`
  modules vs. `ContainerBuilder`, wait strategies, licences/arch caveats, CI wiring.

## Phase 0 — Orient in the repo (10 minutes, always)

Before writing anything, look at the two closest existing providers and copy their shape.
Pick by **dialect family**, not by popularity:

| New engine looks like | Copy from |
|---|---|
| PostgreSQL wire/dialect (CockroachDB, YugabyteDB, Redshift-ish, Materialize) | `FluentMigrator.Runner.Postgres` / `FluentMigrator.Runner.Redshift` |
| MySQL dialect (MariaDB, TiDB, SingleStore, Doris, StarRocks, Vitess) | `FluentMigrator.Runner.MySql` |
| Embedded / in-process, file-based (DuckDB, H2-like, LiteDB-style) | `FluentMigrator.Runner.SQLite` |
| Cloud warehouse, weak DDL, optional quoting (Databricks, BigQuery, ClickHouse) | `FluentMigrator.Runner.Snowflake` / `Redshift` |
| Enterprise RDBMS with versioned processors (Informix, Sybase ASE, Ingres) | `FluentMigrator.Runner.Db2` / `Oracle` / `SqlServer` |

`Redshift` is the smallest complete provider (nine files) and the best template for a
first pass. `Snowflake` shows options (`SnowflakeOptions`), a batch parser, and quoting
toggles. Read at least:

```
src/FluentMigrator.Runner.Redshift/**                   (whole project — it's small)
src/FluentMigrator.Runner.Core/Generators/Generic/GenericGenerator.cs
src/FluentMigrator.Runner.Core/Generators/Base/ColumnBase.cs
src/FluentMigrator.Runner.Core/Generators/Base/TypeMapBase.cs
src/FluentMigrator.Runner.Core/Processors/GenericProcessorBase.cs
src/FluentMigrator/ProcessorId.cs  and  src/FluentMigrator/GeneratorIdConstants.cs
adr/proposed/DuckDB.md                                   (a complete new-provider ADR)
test/FluentMigrator.Tests/Containers/ContainerBase.cs
test/FluentMigrator.Tests/Integration/IntegrationTestsSetup.cs
test/FluentMigrator.Tests/appsettings.json
```

Also check `adr/proposed/` for an existing ADR about the target engine (e.g.
`DuckDB.md`, `ApacheFlightSQL.md` covers Doris/StarRocks/Dremio/InfluxDB). If one
exists, update it rather than writing a second one.

## Phase 1 — Research the engine (before any code)

Answer every question below from the engine's **official documentation** (link the
pages; the ADR needs a References section). Do not rely on memory — dialect details
change between versions and most wrong generator code comes from a half-remembered
syntax.

1. **Driver.** Which ADO.NET `DbProviderFactory` exists? Package name, assembly name,
   fully-qualified factory type, and whether it has a `.Instance` static or needs
   reflection. Does it support .NET Framework 4.8 (the runner projects multi-target
   `net48;netstandard2.0`) and NativeAOT/trimming (`TrimAot.props`)? Is the licence
   compatible with Apache-2.0 redistribution as a transitive dependency?
2. **Naming hierarchy.** database/catalog → schema → table? Two-level only? Does
   `CREATE SCHEMA` exist, or is "schema" really "database" (MySQL family)? What is the
   default schema name (`public`, `dbo`, `main`, `PUBLIC`)? Is it case-folding upper,
   lower, or preserved? Max identifier length?
3. **Quoting.** Identifier quote characters (`"`, `` ` ``, `[]`), how a quote is escaped
   inside an identifier, string-literal escaping, boolean/date/guid literal syntax,
   `CURRENT_TIMESTAMP` / UTC-now / new-GUID function names.
4. **DDL support matrix.** Go through *every* `IMigrationGenerator.Generate(...)`
   overload (there are 24 — the template lists them) and mark ✅ / ⚠️ / ❌ with the
   exact syntax. Pay special attention to the operations that commonly differ:
   `ALTER COLUMN` (type change, nullability, default — three separate statements on
   many engines), `RENAME TABLE` vs `ALTER TABLE ... RENAME TO`, adding a primary key
   after creation, `DROP CONSTRAINT` by name, `IF [NOT] EXISTS` variants, sequences,
   identity/auto-increment syntax, computed columns, column descriptions/comments,
   clustered/covering/filtered indexes, foreign key `ON DELETE/UPDATE` rules
   (SET DEFAULT is often missing), transactional DDL (does a failed migration roll back
   DDL?), and statement batching/separators.
5. **Type map.** `DbType` → native type for all 27 `DbType` values, including sized
   variants and their max sizes (`VARCHAR(n)` cap, `DECIMAL` max precision, binary cap).
6. **Introspection.** SQL for schema/table/column/constraint/index/sequence/default
   exists. Prefer `information_schema`; fall back to engine catalog tables.
7. **Container.** Is there an official Docker image? An official `Testcontainers.<Db>`
   NuGet module? Licence acceptance flag? Only amd64? See `references/testcontainers.md`.

## Phase 2 — Write the ADR first

Create `adr/proposed/<Db>.md` from `references/adr-template.md`. The ADR is the design
contract: the support matrix in it dictates which `Generate` methods get overridden,
which return `CompatibilityMode.HandleCompatibility("...")`, and which unit tests assert
a throw in `STRICT` mode. Reviewers (and future maintainers) read the ADR to understand
*why* the generator does something odd; without it, every dialect quirk looks like a bug.

The ADR must contain, in this order: Status, Context (use cases, driver, connection
string format, schema model), Research Findings (DDL matrix, type map, introspection
queries), Architecture Decision (project structure, NuGet deps, one subsection per class
listing the overrides and the *reason* for each), Unsupported Operations Summary,
Integration Tests (container or embedded strategy, test infrastructure changes),
References, Conclusion. `adr/proposed/DuckDB.md` is the worked example; match its
level of detail.

Commit the ADR as its own commit (or open a draft PR with only the ADR) so the design
can be discussed before implementation. Update the ADR whenever implementation reveals
the research was wrong — the matrix in the merged ADR must match the shipped code.

## Phase 3 — Scaffold the project

Follow `references/touchpoints.md` for the exhaustive list. The core files:

```
src/FluentMigrator.Runner.<Db>/
├── FluentMigrator.Runner.<Db>.csproj          copy Redshift's; RootNamespace=FluentMigrator.Runner
├── <Db>RunnerBuilderExtensions.cs             Add<Db>() DI registration
├── Generators/<Db>/
│   ├── I<Db>TypeMap.cs  : ITypeMap
│   ├── <Db>TypeMap.cs   : TypeMapBase
│   ├── <Db>Quoter.cs    : GenericQuoter
│   ├── <Db>Column.cs    : ColumnBase<I<Db>TypeMap>
│   ├── <Db>DescriptionGenerator.cs : GenericDescriptionGenerator (or reuse EmptyDescriptionGenerator)
│   └── <Db>Generator.cs : GenericGenerator
└── Processors/<Db>/
    ├── <Db>DbFactory.cs : ReflectionBasedDbFactory
    ├── <Db>Options.cs   (only if the provider needs options, e.g. quoting toggle)
    └── <Db>Processor.cs : GenericProcessorBase
```

Then add `ProcessorIdConstants.<Db>` and `GeneratorIdConstants.<Db>` (use
`nameof(<Db>)`; add aliases like `"IBM DB2"` only when users would type them), add the
project to `FluentMigrator.sln`, and register `.Add<Db>()` in the four aggregator call
sites (`FluentMigrator.Runner`, `DotNet.Cli`, `Console`, `MSBuild`) plus the test
harness call sites. Package versions go in `Directory.Packages.props` (central package
management — no `Version=` on `PackageReference`).

**Driver loading is not a per-provider choice — NativeAOT decides it.** Every runner
project imports `TrimAot.props` (`IsAotCompatible`, `IsTrimmable`,
`VerifyReferenceAotCompatibility` on net8.0), and `ReflectionBasedDbFactory` refuses to
probe by name when `AotSupport.IsDynamicCodeSupported` is false. The only path that
survives AOT is a `TestEntry` whose third argument is a constant-string
`() => Type.GetType("<Namespace>.<FactoryType>, <AssemblyName>")` — the trimmer can
statically resolve that, and `TryCreateFromPreloadedType` runs before the dynamic-code
gate. So:

- Every `TestEntry` **must** carry that lambda. A two-argument `TestEntry` works on JIT
  and silently returns "no factory found" under AOT.
- The runner package does **not** hard-reference the driver; the consuming application
  references it, which is what makes `Type.GetType` resolve. This is the SQLite/Postgres/
  Snowflake pattern and the reason the runner packages stay driver-version-agnostic.
- If the driver itself is not trim/AOT-safe, say so in the ADR, add the provider to the
  AOT smoke tests anyway to prove the *runner* is fine, and document the limitation on
  the provider docs page. Do not disable `TrimAot.props` for the project.
- A driver whose factory has neither a static `Instance` member nor a public
  parameterless constructor cannot be created by `TryCreateFromPreloadedType`; that is
  a blocker to raise in the ADR, not something to work around with reflection.

## Phase 4 — Write the unit tests before the generator

Unit tests for generators are pure string assertions and need no database, so write
them first from the ADR's matrix and let them fail. Create
`test/FluentMigrator.Tests/Unit/Generators/<Db>/` with one class per base test class:

| Test class | Extends | Abstract tests to implement |
|---|---|---|
| `<Db>ColumnTests` | `BaseColumnTests` | 22 |
| `<Db>ConstraintsTests` | `BaseConstraintsTests` | 37 |
| `<Db>DataTests` | `BaseDataTests` | 16 |
| `<Db>IndexTests` | `BaseIndexTests` | 10 |
| `<Db>SchemaTests` | `BaseSchemaTests` | 3 |
| `<Db>SequenceTests` | `BaseSequenceTests` | 4 |
| `<Db>TableTests` | `BaseTableTests` | 25 |
| `<Db>DescriptionGeneratorTests` | `BaseDescriptionGeneratorTests` | 8 (only if descriptions are supported) |
| `<Db>GeneratorTests` | — | provider-specific: options, quoting toggles, `GeneratorId` |

Every abstract test must be overridden — the base classes are the contract. For an
operation the ADR marks ❌, the override asserts the `CompatibilityMode` behaviour:
`STRICT` → `Assert.Throws<DatabaseOperationNotSupportedException>`, `LOOSE` →
`result.ShouldBeEmpty()`. Use `GeneratorTestHelper.Get*Expression()` to build
expressions and `Shouldly`'s `ShouldBe` on the exact SQL string, statement terminator
included. Expected strings come straight from the ADR matrix; if you find yourself
inventing SQL while writing a test, go back and fix the ADR.

Also add the new types to the three cross-provider unit tests that enumerate every
provider: `Unit/Runners/DatabaseIdentifierTests.cs` (`[TestCase]` mapping
processor/generator types to their id constants), `Unit/Processors/ProcessorConstructorSelectionTests.cs`,
and `Unit/Runners/DatabaseIdConstantsSelectionTests.cs`.

## Phase 5 — Implement Generator → Quoter → TypeMap → Column

Implement in the order the unit tests fail. Guidance per class (skeletons in
`references/code-skeletons.md`):

- **TypeMap**: `SetupTypeMaps()` calls `SetTypeMap(DbType, template)` and
  `SetTypeMap(DbType, template, maxSize)`; use `$size` / `$precision` placeholders.
  Register the unsized default first, then sized variants in ascending size order.
- **Quoter**: override `OpenQuote`/`CloseQuote`/escape strings only if not `"`;
  override `FormatSystemMethods`, `FormatDateTime`, `FormatBool`, `FormatGuid`,
  `QuoteSchemaName` (return empty when schemas are unsupported) as the ADR dictates.
- **Column**: override `FormatIdentity` (required), and `FormatNullable`,
  `FormatDefaultValue`, `FormatPrimaryKey`, `FormatCollation`, `ClauseOrder` when the
  engine orders clauses differently. `ShouldPrimaryKeysBeAddedSeparately` controls
  inline vs. `ALTER TABLE ADD CONSTRAINT`.
- **Generator**: prefer overriding the format-string properties (`RenameTable`,
  `AlterColumn`, `AddColumn`, `DropIndex`, ...) over overriding `Generate(...)` methods;
  override `Generate` only when the statement shape differs, and call
  `CompatibilityMode.HandleCompatibility(message)` for ❌ rows. Set `GeneratorId` and
  `GeneratorIdAliases`. Multi-statement results (e.g. column + description) end each
  statement with `AppendSqlStatementEndToken`.

Do not invent "generic" fallbacks for unsupported features. An engine that cannot
change a column type should say so via `CompatibilityMode`, not emit a
drop-and-recreate the user did not ask for (unless the ADR explicitly decides that,
as the SQLite provider does for table rebuilds).

## Phase 6 — Processor + DbFactory + integration tests

The processor is mostly introspection SQL plus `Read`/`Exists`/`Execute` boilerplate
copied verbatim from Redshift/Postgres. Implement `DatabaseType`,
`DatabaseTypeAliases`, both constructors (the `IMigrationConnectionFactory` one marked
`[ActivatorUtilitiesConstructor]`; the `IConnectionStringAccessor` one `[Obsolete]`),
and the seven `*Exists` methods using the ADR's queries with `FormatToSafeName` /
`FormatToSafeSchemaName` for escaping. Override transaction methods only when the
engine lacks transactional DDL.

Integration tests live in `test/FluentMigrator.Tests/Integration/Processors/<Db>/` and
mirror the unit layout against `Integration/Processors/Base*Tests.cs` (table, column,
constraint, index, schema, sequence, schema-extensions, plus `<Db>ProcessorTests` and
optionally `<Db>MigrationRunnerTests`). Each class:

- carries `[Category("Integration")] [Category("<Db>")]`;
- calls `IntegrationTestOptions.<Db>.IgnoreIfNotEnabled()` in setup so the suite passes
  on machines without the engine;
- builds its own `ServiceProvider` via `ServiceCollectionExtensions.CreateServices()
  .ConfigureRunner(r => r.Add<Db>())` with a `PassThroughConnectionStringReader`;
- uses a `Helpers/<Db>TestTable.cs` (and `<Db>TestSequence.cs`) `IDisposable` helper
  that creates a uniquely-named table in setup and drops it on dispose.

Wire the options: add a `<Db>` block to `appsettings.json` with `IsEnabled: false`
(and `ContainerEnabled: false` for server engines), add
`public static DatabaseServerOptions <Db> => GetOptions(ProcessorIdConstants.<Db>);` to
`IntegrationTestOptions.cs`, add a `ProcessorTestCase<<Db>Processor>` entry to
`Integration/TestCases/ProcessorTestCaseSource.cs`, and add `.Add<Db>()` to
`IntegrationTestBase.ExecuteWithProcessor`.

For server engines, add `test/FluentMigrator.Tests/Containers/<Db>Container.cs`
extending `ContainerBase` (three overrides: `ServerOptions`, `Port`, `Build()`), add it to
the `Task.WhenAll` list in `IntegrationTestsSetup.cs`, and enable it in
`.github/workflows/pull-requests.yaml` via the `FM_TestConnectionStrings__<Db>__*`
environment variables on the Linux matrix leg. Everything about choosing and configuring
the image is in `references/testcontainers.md` — read it before writing `Build()`.
Embedded engines skip the container entirely and follow the SQLite pattern.

Run locally with the container enabled before pushing:

```
FM_TestConnectionStrings__<Db>__IsEnabled=true \
FM_TestConnectionStrings__<Db>__ContainerEnabled=true \
dotnet test test/FluentMigrator.Tests --framework net8.0 --filter "Category=<Db>"
```

## Phase 7 — Finish the PR

- Update the ADR to match reality and, once merged, the maintainers move it to
  `adr/implemented/`; leave it in `adr/proposed/` in the PR.
- Add the provider to `docs-website/providers/` (own page for a major engine, or a
  section in `others.md`), including connection-string format, `Add<Db>()` usage, the
  unsupported-operations table copied from the ADR, and any `<Db>Options`.
- Add a `### New` entry to `CHANGELOG.md` under the unreleased heading, and your name
  under `### Contributors`.
- Add `<Db>` to `SqlDialect` in `src/FluentMigrator.Analyzers/Analysis/SqlDialect.cs`
  (and `SqlDialectResolver` / `SqlLiteralSyntax`) if the engine's literal syntax
  matters to the Roslyn analyzers.
- Confirm `dotnet pack` package validation passes — the `net48` target and public API
  baseline are checked on the Windows CI leg.
- PR description: link the ADR, list the ❌ operations, state which CI legs run the
  container, and say how you verified against a real instance.

## Review checklist (use when reviewing someone else's provider PR)

- ADR exists, was written before code, and its matrix matches the overrides.
- Every abstract base test is overridden; ❌ operations test both compatibility modes.
- No `Generate` override silently returns `string.Empty` without going through
  `CompatibilityMode.HandleCompatibility`.
- `ProcessorIdConstants`/`GeneratorIdConstants` added and covered by
  `DatabaseIdentifierTests`.
- All aggregator `Add*()` call sites updated (runner, CLI, console, MSBuild, tests).
- Container uses `.WithReuse(true)`, a real wait strategy, and pinned image tag;
  `IsEnabled` defaults to `false` in `appsettings.json`.
- Every `TestEntry` has the constant-string `Type.GetType` lambda (AOT path); no hard
  driver `PackageReference` in the runner project.
- Docs page, CHANGELOG, and `Directory.Packages.props` updated.
