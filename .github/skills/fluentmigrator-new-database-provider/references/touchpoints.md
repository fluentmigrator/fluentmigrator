# Every file a new provider touches

Use this as the PR checklist. Grep for an existing provider name (`Snowflake` or
`Redshift`) across the repo to confirm nothing has moved:

```bash
grep -rln "Snowflake" --include=*.cs --include=*.csproj --include=*.props \
  --include=*.json --include=*.yml --include=*.yaml --include=*.md --include=*.sln . \
  | grep -v "Runner.Snowflake\|Extensions.Snowflake\|/Snowflake/"
```

## Source

| File | Change |
|---|---|
| `src/FluentMigrator.Runner.<Db>/FluentMigrator.Runner.<Db>.csproj` | New. Copy Redshift's. `TargetFrameworks=net48;netstandard2.0`, `RootNamespace=FluentMigrator.Runner`, sign with `FluentMigrator.snk`, import `TrimAot.props` and `PackageLibrary.props`, reference `FluentMigrator.Runner.Core` (and `FluentMigrator.Extensions.<Db>` if created). |
| `src/FluentMigrator.Runner.<Db>/<Db>RunnerBuilderExtensions.cs` | New. `public static IMigrationRunnerBuilder Add<Db>(this IMigrationRunnerBuilder builder)`. |
| `src/FluentMigrator.Runner.<Db>/Generators/<Db>/*.cs` | New: `I<Db>TypeMap`, `<Db>TypeMap`, `<Db>Quoter`, `<Db>Column`, `<Db>Generator`, optionally `<Db>DescriptionGenerator`. |
| `src/FluentMigrator.Runner.<Db>/Processors/<Db>/*.cs` | New: `<Db>DbFactory`, `<Db>Processor`, optionally `<Db>Options`. |
| `src/FluentMigrator.Runner.<Db>/BatchParser/<Db>BatchParser.cs` | Only if the engine needs custom `GO`/`;`-style script splitting (see Snowflake, SqlServer). |
| `src/FluentMigrator.Extensions.<Db>/` | Only if engine-specific fluent options are needed (`.WithColumnFoo()`); mirrors `Extensions.Snowflake`. Uses `IsAdditionalFeatureSupported` / `AdditionalFeatures` on expressions. |
| `src/FluentMigrator/ProcessorId.cs` | Add `public const string <Db> = nameof(<Db>);` to `ProcessorIdConstants` (alphabetical). Add alias constants only for names users actually type. |
| `src/FluentMigrator/GeneratorIdConstants.cs` | Add `public const string <Db> = nameof(<Db>);`. |
| `src/FluentMigrator.Runner/FluentMigrator.Runner.csproj` | Add `ProjectReference` to the new runner project. |
| `src/FluentMigrator.Runner/FluentMigrationRunnerBuilderExtensions.cs` | Add `.Add<Db>()` to `AddAllDatabases()` (or equivalent aggregator). |
| `src/FluentMigrator.DotNet.Cli/Setup.cs` + `.csproj` | Add `.Add<Db>()` and project reference so `dotnet-fm` can target it. |
| `src/FluentMigrator.Console/MigratorConsole.cs` + `.csproj` | Same for the classic `Migrate.exe` console (net48 — check the driver supports it; Hana was removed from here for that reason). |
| `src/FluentMigrator.MSBuild/Migrate.cs` | Same for the MSBuild task, subject to the same net48 caveat. |
| `src/FluentMigrator.Analyzers/Analysis/SqlDialect.cs`, `SqlDialectResolver.cs`, `SqlLiteralSyntax.cs` | Add the dialect if the Roslyn analyzers should understand its literal/quoting rules. |
| `FluentMigrator.sln` | Add the new project(s) under the `src` solution folder. |
| `Directory.Packages.props` | `PackageVersion` entries for the driver (test project only — the runner never references it) and `Testcontainers.<Db>`. |
| `TrimAot.props` | Never edit for a provider. If the driver is not trim-safe, document it in the ADR and on the docs page; the runner project still builds AOT-compatible because it does not reference the driver. |

## Tests

| File | Change |
|---|---|
| `test/FluentMigrator.Tests/FluentMigrator.Tests.csproj` | `ProjectReference` to the runner project; `PackageReference` to the driver (tests must actually load it — see `SnowflakeTableTests.EnsureReference()`) and to `Testcontainers.<Db>`. |
| `test/FluentMigrator.Tests/Unit/Generators/<Db>/*.cs` | New: one class per `Base*Tests`, plus `<Db>GeneratorTests`. |
| `test/FluentMigrator.Tests/Unit/Runners/DatabaseIdentifierTests.cs` | Add `[TestCase(typeof(<Db>Processor), typeof(<Db>Generator), ProcessorIdConstants.<Db>, GeneratorIdConstants.<Db>)]` and `.Add<Db>()` in the service setup. |
| `test/FluentMigrator.Tests/Unit/Runners/DatabaseIdConstantsSelectionTests.cs` | Add `.Add<Db>()`. |
| `test/FluentMigrator.Tests/Unit/Processors/ProcessorConstructorSelectionTests.cs` | `yield return typeof(<Db>Processor);` — verifies the `[ActivatorUtilitiesConstructor]` overload is the one DI picks. |
| `test/FluentMigrator.Tests/Unit/TokenSubstitution/TokenSubstitutionMatrixTests.cs` | Add the generator to the matrix if it participates in token substitution. |
| `test/FluentMigrator.Tests/Integration/Processors/<Db>/*.cs` | New: `<Db>ProcessorTests`, `<Db>TableTests`, `<Db>ColumnTests`, `<Db>ConstraintTests`, `<Db>IndexTests`, `<Db>SchemaTests`, `<Db>SchemaExtensionsTests`, `<Db>SequenceTests`, optionally `<Db>MigrationRunnerTests`. |
| `test/FluentMigrator.Tests/Helpers/<Db>TestTable.cs`, `<Db>TestSequence.cs` | New `IDisposable` fixtures that create/drop a uniquely named table/sequence. |
| `test/FluentMigrator.Tests/Integration/IntegrationTestBase.cs` | Add `.Add<Db>()` in `ExecuteWithProcessor`. |
| `test/FluentMigrator.Tests/Integration/TestCases/ProcessorTestCaseSource.cs` | Add `new ProcessorTestCase<<Db>Processor>(() => IntegrationTestOptions.<Db>, "<Db>")`. |
| `test/FluentMigrator.Tests/Integration/MigrationRunnerTests.cs` | Check the `[TestCaseSource]`-driven tests; some have per-provider exclusions (e.g. `[IgnoreProcessor]`-style attributes) that may need a new entry for ❌ operations. |
| `test/FluentMigrator.Tests/IntegrationTestOptions.cs` | `public static DatabaseServerOptions <Db> => GetOptions(ProcessorIdConstants.<Db>);` (with `.GetOptionsForPlatform()` / `Is64BitProcess` guards as needed). |
| `test/FluentMigrator.Tests/appsettings.json` | New `<Db>` block, `IsEnabled: false`. |
| `test/FluentMigrator.Tests/Containers/<Db>Container.cs` | New (server engines only). |
| `test/FluentMigrator.Tests/Integration/IntegrationTestsSetup.cs` | Add `new <Db>Container().Start()` to `Task.WhenAll`. |
| `test/FluentMigrator.Tests.Aot/` | Add an `AddFluentMigratorSlim().ConfigureRunner(rb => rb.Add<Db>())` registration to `DependencyInjectionTests` (proves the runner resolves under AOT). Add a `MigrationRunnerTests` case too if the driver is AOT-safe and embedded (SQLite is the precedent). |

## CI and docs

| File | Change |
|---|---|
| `.github/workflows/pull-requests.yaml` | `FM_TestConnectionStrings__<Db>__IsEnabled` / `__ContainerEnabled` env vars on the Linux leg. |
| `azure-pipelines*.yml` | Same, if those pipelines still run integration tests. |
| `adr/proposed/<Db>.md` | The ADR (written first). |
| `docs-website/providers/<db>.md` or `others.md` | Provider docs: install, `Add<Db>()`, connection string, options, unsupported-operations table, container notes for contributors. |
| `docs-website/intro/installation.md`, `configuration.md` | Add the package/`Add<Db>()` to the lists that enumerate providers. |
| `CHANGELOG.md` | `### New` bullet + `### Contributors`. |
| `README.md` | Supported-databases list, if one is maintained there. |
| `docs/README-nuget.md` | Supported-databases list in the NuGet readme. |
