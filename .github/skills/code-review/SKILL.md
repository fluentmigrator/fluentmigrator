---
name: code-review
description: Pull request code review guidance for FluentMigrator (https://github.com/fluentmigrator/fluentmigrator), the .NET database migration framework. Use this whenever reviewing a pull request, diff, or patch in this repository — changes to expressions, generators, quoters, type maps, processors, runner/DI registration, the dotnet-fm/console/MSBuild runners, Roslyn analyzers, tests, docs, ADRs, or build/package configuration. It lists the architectural invariants, recurring bug classes, and compatibility rules (public API package validation, net48/netstandard2.0, NativeAOT/trimming, central package management) that a generic reviewer would not know, so review comments are specific to how FluentMigrator works.
---

# Reviewing a FluentMigrator pull request

FluentMigrator turns fluent C# migrations (`Create.Table(...)`, `Alter.Column(...)`) into
`IMigrationExpression` objects. A per-dialect **generator** turns each expression into
SQL, and a per-driver **processor** executes that SQL and introspects the database. The
project ships as many NuGet packages, and the applications that use them may still run
on .NET Framework 4.8. Most of the risk in a PR is therefore in **SQL correctness across
dialects** and **compatibility for those consuming applications**, not in the C# itself.

Report only issues that matter: wrong SQL, behaviour changes, breaking changes, missing
coverage for the changed behaviour, and violations of the invariants below. Do not
comment on formatting that `.editorconfig` already governs, and do not ask for changes
outside the PR's scope.

## Step 1 — Gather context before commenting

1. Read the PR description and the linked issues. Use the GitHub MCP server to fetch
   them. Most PRs here are titled `Fix #NNNN: ...`, and the issue usually contains the
   failing migration and the expected SQL. Check that the fix covers *that* scenario.
2. If the PR touches a design that has an ADR, read it in `adr/proposed/*.md` or
   `adr/implemented/*.md` (for example `ConnectionManagement.md`,
   `RequireQuoterForTokenSubstitution.md`, `TableLevelForeignKeys.md`, `DuckDB.md`).
   The ADR is the design contract. Flag code that contradicts it, and flag an ADR that
   was not updated when the implementation went a different way.
3. Read the unreleased section at the top of `CHANGELOG.md` to see what the next release
   already promises, especially under `### Breaking`.
4. If the PR adds a new database provider, also apply the review checklist at the end of
   `.github/skills/fluentmigrator-new-database-provider/SKILL.md`.

## Step 2 — Check the architectural invariants

### Generators are dialects; processors are drivers

- A **generator** (`src/FluentMigrator.Runner.<Db>/Generators/**`) represents a database
  *engine version within a dialect*, such as `Oracle12c`, `PostgreSQL15_0` or
  `SqlServer2016`. It must not know which ADO.NET driver is in use.
- A **processor** (`src/FluentMigrator.Runner.<Db>/Processors/**`) wires in the *driver*
  (such as `OracleManaged` or `Oracle12cManaged`) and does the
  introspection.
- Flag any change that leaks driver knowledge into a generator. Examples are new generator
  types or `GeneratorId`s that exist only to match a driver-specific processor id, and
  driver checks inside SQL generation. Pairing a generator with a processor belongs in
  selection (`SelectingGeneratorAccessor`, `SelectingProcessorAccessor`) or in the runner
  front-ends such as `src/FluentMigrator.DotNet.Cli/Setup.cs`, not in the generator.
- New ids go in `ProcessorIdConstants` (`src/FluentMigrator/ProcessorId.cs`) and
  `GeneratorIdConstants` (`src/FluentMigrator/GeneratorIdConstants.cs`). They must be
  covered by `test/FluentMigrator.Tests/Unit/Runners/DatabaseIdentifierTests.cs`.

### Expressions and definitions are provider-neutral

- `src/FluentMigrator.Abstractions` (expressions and `Model/*Definition`) must stay
  provider-neutral. Provider-specific options travel through the `AdditionalFeatures`
  dictionary, using keys defined in `src/FluentMigrator.Extensions.<Db>` (for example
  `SqlServerExtensions.OnlineIndex`). Flag provider names or provider-specific logic
  added to Abstractions.
- **`Clone()` must copy every mutable member.** Definitions implement `ICloneable`. A
  clone that shares a mutable collection (`Columns`, `AdditionalFeatures`, ...) with the
  original lets one expression change another (see `ConstraintDefinition.Clone`,
  PR #2375). Check that every new mutable member is copied in `Clone()` and is covered by
  a test in `Unit/Definitions/`.
- **`Reverse()` must produce a valid inverse.** `AutoReversingMigration` builds `Down()`
  from `IMigrationExpression.Reverse()`. A reversed `Delete*` expression should carry the
  object's identity only, not the options it was created with. Until #1982 (PR #2404),
  `CreateIndexExpression.Reverse` copied `ONLINE` and `DATA_COMPRESSION` into
  `DROP INDEX`. When a PR adds a member or feature to a `Create*` or `Rename*`
  expression, ask what its `Reverse()` does with it, and expect a test in
  `Unit/Expressions/` or `Unit/AutoReversingMigrationTests.cs`.
- The schema must be kept wherever a table name is emitted. `DROP TABLE IF EXISTS`
  dropped the schema on Postgres and SQLite until PR #2387. Check that new or changed SQL
  passes `SchemaName` through the quoter (`Quoter.QuoteTableName(tableName, schemaName)`).

### Unsupported operations go through `CompatibilityMode`

- A generator that cannot express an operation must return
  `CompatibilityMode.HandleCompatibility("<reason>")`
  (`src/FluentMigrator.Runner.Core/Generators/CompatibilityModeExtension.cs`). In `STRICT`
  mode this throws `DatabaseOperationNotSupportedException`, and in `LOOSE` mode it returns
  an empty string. Flag a `Generate(...)` override that silently returns `string.Empty`,
  emits a comment instead of SQL, or adds a drop-and-recreate fallback the user did not
  ask for.
- Tests for an unsupported operation should cover both modes: `Assert.Throws` for
  `STRICT` and `ShouldBeEmpty()` for `LOOSE`.

### Quoting, literals and token substitution

- Identifiers and values must go through the dialect's quoter (`GenericQuoter` and the
  `<Db>Quoter` overrides). Flag user-supplied names or values concatenated into SQL.
  `RawSql` values must be emitted unchanged; the MySQL quoter once corrupted them by
  escaping their backslashes (PR #2344).
- `$(name)` and `$[name]` token substitution lives in
  `src/FluentMigrator.Abstractions/SqlScriptTokenReplacer.cs`. As
  `adr/proposed/RequireQuoterForTokenSubstitution.md` records, `$[name]` requires an
  `IQuoter` and must not get a second, private literal-quoting implementation. Changes
  here usually move the token substitution matrix snapshots
  (`test/FluentMigrator.Tests/Unit/TokenSubstitution/*.verified.md`). Review a snapshot
  diff as a behaviour change, line by line.
- If the change affects how a dialect writes literals, check whether the Roslyn
  analyzers' dialect model (`src/FluentMigrator.Analyzers/Analysis/SqlDialect*.cs` and
  `SqlLiteralSyntax.cs`) needs the same change.

### Culture invariance

- Generated SQL must not depend on the current culture. Numbers, dates and decimals that
  end up in SQL, including through `ToString()` or `string.Format`, must be formatted with
  `CultureInfo.InvariantCulture`.
- `test/FluentMigrator.Tests/GlobalizationGuard.cs` fails the test run under
  globalization-invariant mode, because the culture tests would otherwise pass without
  testing anything. Flag PRs that disable it or set `InvariantGlobalization`.

### Processors, connections and DI

- Processors take an `IMigrationConnectionFactory` in the constructor marked
  `[ActivatorUtilitiesConstructor]`. The constructors taking `IConnectionStringAccessor`
  are `[Obsolete]` and must not gain new callers.
- A processor must not close or dispose a connection whose factory reports
  `OwnsConnection == false` (see `adr/proposed/ConnectionManagement.md`).
- Commands, readers and connections must be disposed. Introspection SQL must escape names
  with `FormatToSafeName` / `FormatToSafeSchemaName` or use parameters; it must not
  interpolate them.
- A new `Add<Db>()` registration or option must be wired into every front-end that
  aggregates providers: `FluentMigrator.Runner`, `FluentMigrator.DotNet.Cli`,
  `FluentMigrator.Console`, `FluentMigrator.MSBuild`, and the test harness.
- CLI changes (`dotnet-fm` and the console runner) must keep returning non-zero exit
  codes on failure (PR #2373), and must reject invalid option values with a clear message
  instead of crashing (PR #2376, PR #2380).

## Step 3 — Check compatibility for consumers

- **Public API.** `PackageLibrary.props` turns on package validation against the last
  release (`PackageValidationBaselineVersion`). Removing or changing a public or
  protected member, a parameter default, or an interface member is a breaking change. An
  intentional break needs a `CompatibilitySuppressions.xml` entry in the affected project
  **and** an entry under `### Breaking` in `CHANGELOG.md` that explains what users must
  change. Prefer adding an overload and marking the old member
  `[Obsolete("Use ... instead.")]`.
- **Target frameworks.** Library projects target `net48` and `netstandard2.0`, and
  `TrimAot.props` adds `net8.0`. Flag APIs that are missing on `netstandard2.0` or `net48`
  unless they are guarded by `#if NET` or `#if NETFRAMEWORK`. Also flag language features
  that need runtime support those targets lack.
- **NativeAOT and trimming.** Runner and library projects import `TrimAot.props`
  (`IsAotCompatible`, `IsTrimmable`). Flag new reflection over type names that are not
  constant strings, `Activator.CreateInstance(Type)` on unknown types, and use of
  `Assembly.Location` without a fallback. A suppression (`UnconditionalSuppressMessage`
  or `ILLink.Descriptors.xml`) needs a justification. Driver factories must be loadable
  through the constant-string `Type.GetType(...)` lambda path in
  `ReflectionBasedDbFactory`. The AOT smoke tests are in `test/FluentMigrator.Tests.Aot`.
- **NuGet.** The repository uses Central Package Management. Versions live only in
  `Directory.Packages.props`: no `Version=` on `<PackageReference>` and no MSBuild
  properties holding versions. Raising the `[8.0.0,)` floors of `Microsoft.Extensions.*`
  or `Microsoft.Data.Sqlite` is a breaking change. Runner packages must not hard-reference
  ADO.NET drivers. Check new dependencies for an Apache-2.0-compatible licence and for
  known advisories.
- **Warnings.** The build is kept free of warnings (PR #2331). Public members need XML
  doc comments, because CS1591 is reported in Release builds. New analyzer rules must be
  listed in `src/FluentMigrator.Analyzers/AnalyzerReleases.Unshipped.md`.

## Step 4 — Check the tests

- Every behaviour change needs a test that fails without the change. Generator changes
  need exact-SQL unit tests in `test/FluentMigrator.Tests/Unit/Generators/<Db>/` for
  **every** dialect version affected. A change in `GenericGenerator` affects every provider
  that does not override the method, so look for dialects the PR missed.
- Tests use NUnit and Shouldly (`result.ShouldBe("...")`), build expressions with
  `GeneratorTestHelper`, and contain no `// Arrange`, `// Act` or `// Assert` comments.
- Integration tests belong in `test/FluentMigrator.Tests/Integration/Processors/<Db>/`.
  They carry `[Category("Integration")]` and a provider category, and call
  `IntegrationTestOptions.<Db>.IgnoreIfNotEnabled()` so the suite passes on machines
  without that database. Regression tests for one issue may live in
  `test/FluentMigrator.Tests/IssueTests/GH<number>/`.
- Flag edits to or deletions of unrelated tests, `[Ignore]` added to make CI pass, and
  snapshot (`*.verified.*`) updates that the PR does not explain.

## Step 5 — Check the documentation

- `CHANGELOG.md`: breaking changes, new features and changed behaviour need an entry under
  the unreleased heading (`### Breaking`, `### New`, `### Changed`, `### Documentation`)
  with the issue or PR number. New external contributors go under `### Contributors`.
  Do not ask for an entry for a plain bug fix; the GitHub release notes cover those.
- `docs-website/`: new or changed user-facing behaviour, options, CLI flags or provider
  capabilities must be documented, for example in `docs-website/runners/dotnet-fm.md` or
  `docs-website/providers/*.md`.
- New source files carry the Apache 2.0 licence header attributed to
  "Fluent Migrator Project" that is used throughout `src/`.
- A design change with real alternatives (a new abstraction, new selection rules,
  connection handling) should add or update an ADR in `adr/proposed/`.

## Writing the review

- Order findings by impact: wrong or dialect-invalid SQL, a broken `Reverse()`, breaking
  API changes and missing `CompatibilityMode` handling first, then missing tests, then
  documentation.
- When you flag generated SQL, give a concrete migration, the exact statement the code
  would emit for it, and, where you can, the statement it should emit.
- Name the dialects and versions affected. "This also changes the output of
  `SqlServer2000Generator` and `FirebirdGenerator`" is more useful than "this may affect
  other providers".
- Suggest the smallest change that fixes the problem, in the style of the surrounding
  code.
