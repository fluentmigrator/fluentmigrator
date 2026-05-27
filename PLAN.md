# Plan: Move FluentMigrator CLI tooling to System.CommandLine

## Background

Two external runner tools currently use two different option-parsing libraries:

| Tool | Binary | Parser | NuGet target |
|---|---|---|---|
| `FluentMigrator.DotNet.Cli` | `dotnet-fm` | `McMaster.Extensions.CommandLineUtils` 4.1.1 (attribute model) | .NET 8/9/10 |
| `FluentMigrator.Console` | `Migrate.exe` | bundled `Mono.Options` / Jonathan Pryor's `NDesk.Options` (in `Options.cs`) | .NET 4.8 + .NET 8 |

The goal is to replace both with `System.CommandLine` 2.0.x (now stable), centralise shared
option definitions in a new **`FluentMigrator.Hosting.Commands`** library, and lay the
groundwork for future Aspire AppHost integration.

---

## Part 1 – Option / Argument Cross-walk

### 1.1  `FluentMigrator.DotNet.Cli` command tree (McMaster)

The tool uses a subcommand tree.  Options live on base-class types and are
inherited via a `Parent` property on child commands.

```
dotnet-fm                          (Root)
├── migrate      [ConnectionCommand options]
│   ├── up       [--target <version>]
│   └── down     --target <version>  (required)
├── rollback     [ConnectionCommand options]
│   ├── by       <steps>  (positional argument, required)
│   ├── to       <version> (positional argument, required)
│   └── all
├── validate
│   └── versions [ConnectionCommand options]
└── list
    ├── migrations [ConnectionCommand options]
    └── processors
```

#### MigrationCommand options (inherited by all leaf commands except `list processors`)

| McMaster option string | Short flag(s) | Type | Default | Maps to `RunnerOptions` / `MigratorOptions` |
|---|---|---|---|---|
| `--assembly <ASSEMBLY_NAME>` | `-a` | `IEnumerable<string>` | **required** | `AssemblyNames` |
| `--namespace <NAMESPACE>` | `-n` | `string` | `null` | `Namespace` |
| `--nested` | | `bool` | `false` | `NestedNamespaces` |
| `--start-version` | | `long?` | `null` | `StartVersion` |
| `--working-directory <DIR>` | | `string` | `null` | `WorkingDirectory` |
| `--tag` | `-t` | `IEnumerable<string>` (multi-value) | `[]` | `Tags` |
| `--allow-breaking-changes` | `-b` | `bool` | `false` | `AllowBreakingChange` |
| `--default-schema-name` | | `string` | `null` | `DefaultSchemaName` |
| `--strip` | | `(bool hasValue, bool? value)` SingleOrNoValue | `true` | `StripComments` |
| `--include-untagged-migrations` | | `(bool hasValue, bool? value)` SingleOrNoValue | `true` | `IncludeUntaggedMigrations` |
| `--include-untagged-maintenances` | | `bool` | `false` | `IncludeUntaggedMaintenances` |
| `--allowDirtyAssemblies` | | `bool` | `false` | (special – handled in `Program.cs`) |

#### ConnectionCommand options (additionally inherited)

| McMaster option string | Short flag(s) | Type | Default | Maps to |
|---|---|---|---|---|
| `--connection <CONNECTION_STRING_OR_NAME>` | `-c` | `string` | `null` | `ProcessorOptions.ConnectionString` |
| `--transaction-mode <MODE>` | `-m` | `TransactionMode` enum | `Migration` | `RunnerOptions.TransactionPerSession` |
| `--no-connection` | | `bool` | `false` | `RunnerOptions.NoConnection` |
| `--processor <PROCESSOR_NAME>` | `-p` | `string` | **required** | `SelectingProcessorAccessorOptions.ProcessorId` |
| `--processor-switches <SWITCHES>` | `-s` | `string` | `null` | `ProcessorOptions.ProviderSwitches` |
| `--preview` | | `bool` | `false` | `ProcessorOptions.PreviewOnly` |
| `--verbose` | `-V` | `bool` | `false` | `FluentMigratorLoggerOptions.ShowSql/.ShowElapsedTime` |
| `--profile <PROFILE>` | | `string` | `null` | `RunnerOptions.Profile` |
| `--timeout <TIMEOUT_SEC>` | | `int` (0 = not set) | `0` | `ProcessorOptions.Timeout` |
| `--output[=<FILENAME>]` | `-o` | `(bool enabled, string filename)` SingleOrNoValue | disabled | `LogFileFluentMigratorLoggerOptions` |

#### Per-leaf-command options

| Command | Extra option | Short | Type | Default | Maps to |
|---|---|---|---|---|---|
| `migrate up` | `--target <VERSION>` | `-t` | `long?` | `null` | `RunnerOptions.Version` |
| `migrate down` | `--target <VERSION>` | `-t` | `long` | **required** | `RunnerOptions.Version` |
| `rollback by` | `<steps>` (positional) | | `int` | **required** | `RunnerOptions.Steps` |
| `rollback to` | `<version>` (positional) | | `long` | **required** | `RunnerOptions.Version` |

> **Conflict note:** `-t` is used for `--tag` on `MigrationCommand` level **and** for
> `--target` on `migrate up` / `migrate down`.  In McMaster this works because child
> commands define their own option set independently of the parent's options; they
> access the parent's options via the auto-populated `Parent` property.  In
> System.CommandLine, each command owns its own options, so this conflict does not arise.

#### Task strings injected into `RunnerOptions.Task`

| CLI invocation | `Task` value |
|---|---|
| `migrate` (default) | `"migrate:up"` |
| `migrate up` | `"migrate:up"` |
| `migrate down --target X` | `"migrate:down"` |
| `rollback` (default) | `"rollback"` |
| `rollback by N` | `"rollback"` |
| `rollback to V` | `"rollback:toversion"` |
| `rollback all` | `"rollback:all"` |
| `validate versions` | `"validateversionorder"` |
| `list migrations` | `"listmigrations"` |
| `list processors` | (no task – enumerates processors only) |

---

### 1.2  `FluentMigrator.Console` option structure (Mono.Options / OptionSet)

The console runner is **flat** – there are no subcommands.  The operation is
selected by a `--task` flag.  The parser supports GNU long options (`--foo`),
short single-char options (`-f`), key=value (`--foo=value`), and optional-value
(`--foo:`).

| Mono.Options prototype | Short aliases | Type | Default | Maps to |
|---|---|---|---|---|
| `assembly=\|a=\|target=` | | `string` | **required** | `AssemblySourceOptions.AssemblyNames[0]` |
| `provider=\|dbType=\|db=` | | `string` | **required** | `SelectingProcessorAccessorOptions.ProcessorId` |
| `connectionString=\|connection=\|conn=\|c=` | | `string` | `null` | `ProcessorOptions.ConnectionString` |
| `connectionStringConfigPath=\|configPath=` | | `string` | `null` | .NET Framework `machine.config` lookup (net48 only) |
| `namespace=\|ns=` | | `string` | `null` | `TypeFilterOptions.Namespace` |
| `nested` | | `bool` | `false` | `TypeFilterOptions.NestedNamespaces` |
| `output\|out\|o` | | `bool flag` | `false` | enables SQL file output |
| `outputSemicolonDelimiter\|outsemdel\|osd` | | `bool flag` | `false` | `LogFileFluentMigratorLoggerOptions.OutputSemicolonDelimiter` |
| `outputFilename=\|outfile=\|of=` | | `string` | `null` | `LogFileFluentMigratorLoggerOptions.OutputFileName` |
| `preview\|p` | | `bool flag` | `false` | `ProcessorOptions.PreviewOnly` |
| `steps=` | | `int` | `0` | `RunnerOptions.Steps` |
| `task=\|t=` | | `string` | `"migrate"` | `RunnerOptions.Task` |
| `version=` | | `long` | `0` | `RunnerOptions.Version` |
| `startVersion=` | | `long` | `0` | `RunnerOptions.StartVersion` |
| `noConnection` | | `bool flag` | `false` | `RunnerOptions.NoConnection` |
| `verbose=` | | `bool` (any non-null value) | `false` | `FluentMigratorLoggerOptions` |
| `stopOnError=` | | `bool` | `false` | `StopOnErrorLoggerProvider` injection |
| `workingdirectory=\|wd=` | | `string` | `null` | `DefaultConventionSet` |
| `profile=` | | `string` | `null` | `RunnerOptions.Profile` |
| `timeout=` | | `int` (seconds) | `null` | `ProcessorOptions.Timeout` |
| `tag=` | | `string` (multi-value, repeated) | `[]` | `RunnerOptions.Tags` |
| `include-untagged:` | | optional-value | `true` (migrations) | `RunnerOptions.IncludeUntaggedMigrations/Maintenances` |
| `providerswitches=` | | `string` | `null` | `ProcessorOptions.ProviderSwitches` |
| `strip\|strip-comments` | | `bool flag` | `true` | `ProcessorOptions.StripComments` |
| `help\|h\|?` | | `bool flag` | `false` | shows usage |
| `transaction-per-session\|tps` | | `bool flag` | `false` | `RunnerOptions.TransactionPerSession` |
| `allow-breaking-changes\|abc` | | `bool flag` | `false` | `RunnerOptions.AllowBreakingChange` |
| `default-schema-name=` | | `string` | `null` | `DefaultConventionSet` |

#### Console task strings

| `--task` value | Operation |
|---|---|
| `migrate` / `migrate:up` | Apply all pending migrations up |
| `migrate:down` | Migrate down to version specified by `--version` |
| `rollback` | Roll back N steps (default 1, override with `--steps`) |
| `rollback:toversion` | Roll back to a specific version |
| `rollback:all` | Roll back all applied migrations |
| `validateversionorder` | Validate migration version ordering |
| `listmigrations` | List applied migrations |

#### Exit codes

| Code | Meaning |
|---|---|
| 0 | Success |
| 1 | Validation error (missing required option) |
| 2 | Option parse error (`OptionException`) |
| 3 | Unknown exception |
| 4 | `FluentMigratorException` |
| 5 | `RunnerException` |
| 6 | `MissingMigrationsException` |

---

### 1.3  Side-by-side option comparison and System.CommandLine mapping

| Concept | DotNet.Cli (McMaster) | Console (Mono.Options) | System.CommandLine target name | Notes |
|---|---|---|---|---|
| Assembly path(s) | `-a` / `--assembly` (multi) | `--assembly` / `-a` / `--target` (single) | `--assembly` / `-a` | DotNet.Cli supports multiple assemblies; Console supports only one |
| DB provider | `-p` / `--processor` | `--provider` / `--dbType` / `--db` | `--processor` / `-p` (keep; also alias `--provider`) | Different primary names |
| Connection string | `-c` / `--connection` | `--connectionString` / `--connection` / `--conn` / `-c` | `--connection` / `-c` | |
| Config path for conn | *(not present)* | `--connectionStringConfigPath` / `--configPath` | `--connection-string-config-path` (Console only) | .NET Framework machine.config feature |
| Namespace filter | `-n` / `--namespace` | `--namespace` / `--ns` | `--namespace` / `-n` | |
| Include nested NS | `--nested` | `--nested` | `--nested` | |
| Target version | via `up --target` / positional in `to` | `--version` | `--target` / `--version` (alias) | |
| Start version | `--start-version` | `--startVersion` | `--start-version` | |
| Steps (rollback) | positional arg in `rollback by` | `--steps` | `--steps` (option on rollback) | |
| Task selection | subcommand | `--task` / `--t` | subcommands (retain `--task` alias in Console) | |
| No-connection mode | `--no-connection` | `--noConnection` | `--no-connection` | |
| Verbose | `-V` / `--verbose` | `--verbose` | `--verbose` / `-V` | |
| Stop on error | *(not present)* | `--stopOnError` | `--stop-on-error` (Console only) | |
| Preview/dry-run | `--preview` | `--preview` / `--p` | `--preview` | |
| Output SQL to file | `-o` / `--output[=<FILENAME>]` (optional value) | `--output` (flag) + `--outputFilename` | `--output` (bool flag) + `--output-filename` | McMaster combined; Console split |
| Output semicolon | *(not present)* | `--outputSemicolonDelimiter` | `--output-semicolon-delimiter` (Console only) | |
| Working directory | `--working-directory` | `--workingdirectory` / `--wd` | `--working-directory` / `--wd` | |
| Profile | `--profile` | `--profile` | `--profile` | |
| Timeout (seconds) | `--timeout` | `--timeout` | `--timeout` | |
| Tag filter (multi) | `--tag` / `-t` | `--tag` (repeat for multiple) | `--tag` / `-t` | |
| Include untagged migrations | `--include-untagged-migrations` (opt-value) | `--include-untagged:migrations` (opt-value) | `--include-untagged-migrations` | |
| Include untagged maintenances | `--include-untagged-maintenances` | `--include-untagged:maintenance` | `--include-untagged-maintenances` | |
| Provider switches | `-s` / `--processor-switches` | `--providerswitches` | `--processor-switches` / `-s` (also `--provider-switches`) | |
| Transaction mode | `-m` / `--transaction-mode Migration\|Session` | `--transaction-per-session` / `--tps` (bool) | `--transaction-mode` (enum) + `--transaction-per-session` (alias) | |
| Allow breaking changes | `-b` / `--allow-breaking-changes` | `--allow-breaking-changes` / `--abc` | `--allow-breaking-changes` / `-b` | |
| Default schema | `--default-schema-name` | `--default-schema-name` | `--default-schema-name` | |
| Strip comments | `--strip` (opt-value) | `--strip` / `--strip-comments` | `--strip` | |
| Allow dirty assemblies | `--allowDirtyAssemblies` | *(not present)* | `--allow-dirty-assemblies` | DotNet.Cli only |
| Help | `[HelpOption]` (auto) | `--help` / `--h` / `--?` | built-in `-h` / `--help` (System.CommandLine built-in) | |

---

## Part 2 – Backward Compatibility Constraints

### 2.1 Breaking changes to avoid

The following must continue to work unchanged after the migration:

1. **DotNet.Cli short flags**: `-a`, `-c`, `-p`, `-n`, `-t` (tag), `-V`, `-b`, `-m`, `-s`, `-o`
2. **Console option names with all documented aliases**: e.g. `--assembly`, `-a`, `--target`;
   `--provider`, `--dbType`, `--db`; `--connectionString`, `--connection`, `--conn`, `-c`
3. **Console `--task` flag semantics** and all task name strings
4. **`--allowDirtyAssemblies`** camelCase form (DotNet.Cli)
5. **Single-char short options prefixed with `-`** (not `--`)
6. **`--no-connection`** / `--noConnection` (both forms, tool-specific)
7. **Exit code contract** for Console (0 = success; 1 = validation; 2 = parse error;
   3/4/5/6 = runtime exceptions)

### 2.2 Syntax differences to watch

| McMaster / Mono | System.CommandLine |
|---|---|
| `--option value` (space) | ✅ supported |
| `--option=value` (equals) | ✅ supported |
| `-o value` (short, space) | ✅ supported |
| `-ovalue` (short, no space) | ❌ **NOT supported** – document breaking change |
| `SingleOrNoValue` (`--strip`, `--output`, `--include-untagged-migrations`) | Replaced with an `Option<bool?>` or split into flag + value option |
| Optional-value `--include-untagged:migrations` syntax (colon key) | Requires special `--include-untagged-migrations` / `--include-untagged-maintenances` split |
| Mono.Options `--flag-` (minus suffix to disable a flag) | Not supported by System.CommandLine – document breaking change |
| Multiple values via repeated flag: `--tag foo --tag bar` | ✅ supported via `Option<string[]>` |
| Positional arguments (e.g. `rollback by 3`) | ✅ supported via `Argument<T>` |

---

## Part 3 – New Project: `FluentMigrator.Hosting.Commands`

### 3.1 Purpose

A shared, testable library that owns:
- The complete System.CommandLine command tree (for the dotnet-fm style interface)
- `MigratorOptions` (the internal data model, moved from `FluentMigrator.DotNet.Cli`)
- `TransactionMode` enum (moved from `FluentMigrator.DotNet.Cli`)
- A `FluentMigratorCommandBuilder` class exposing a `BuildRootCommand()` factory
- The `Setup` helper class (service-provider configuration, moved from `FluentMigrator.DotNet.Cli`)

### 3.2 Project file

```xml
<!-- src/FluentMigrator.Hosting.Commands/FluentMigrator.Hosting.Commands.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>netstandard2.0;net8.0;net9.0;net10.0</TargetFrameworks>
    <Description>Centralised System.CommandLine command definitions for FluentMigrator</Description>
    <IsPackable>true</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="System.CommandLine" Version="2.0.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\FluentMigrator.Runner\FluentMigrator.Runner.csproj" />
  </ItemGroup>
</Project>
```

> `System.CommandLine` 2.0.0 targets `netstandard2.0`, so it is compatible with both
> the .NET 4.8 and modern .NET targets of `FluentMigrator.Console`.

### 3.3 File layout

```
src/FluentMigrator.Hosting.Commands/
  FluentMigrator.Hosting.Commands.csproj
  MigratorOptions.cs              ← moved from FluentMigrator.DotNet.Cli
  TransactionMode.cs              ← moved from FluentMigrator.DotNet.Cli
  Setup.cs                        ← moved from FluentMigrator.DotNet.Cli
                                    (IConsole ref removed; uses System.Console / IConsole from S.CL)
  CommandBuilders/
    FluentMigratorCommandBuilder.cs   ← builds the full System.CommandLine RootCommand
    MigrationOptions.cs               ← shared Option<T> instances (assembly, processor, etc.)
    ConnectionOptions.cs              ← connection-level Option<T> instances
```

### 3.4 System.CommandLine tree skeleton

```csharp
// FluentMigratorCommandBuilder.BuildRootCommand()
var root = new RootCommand("The external FluentMigrator runner (dotnet-fm)");

// -- shared option builders (return Option<T> to be added to multiple commands)
var assemblyOpt   = MigrationOptions.Assembly();       // Option<string[]>
var processorOpt  = ConnectionOptions.Processor();     // Option<string>  required
var connectionOpt = ConnectionOptions.Connection();    // Option<string?>
// … (all other shared options)

// -- migrate
var migrate = new Command("migrate", "Apply migrations");
migrate.AddAlias("m");
// add all ConnectionCommand options to `migrate`
migrate.SetHandler(ctx => ExecuteMigrateUp(ctx, /* option refs */));

// -- migrate up
var migrateUp = new Command("up", "Apply pending migrations up");
migrateUp.AddOption(targetVersionOpt);          // Note: uses --target, NOT -t
migrateUp.SetHandler(ctx => ExecuteMigrateUp(ctx, /* option refs */));
migrate.AddCommand(migrateUp);

// -- migrate down
var migrateDown = new Command("down", "Migrate down to a target version");
var targetRequired = new Option<long>("--target", "Target version") { IsRequired = true };
migrateDown.AddOption(targetRequired);
migrateDown.SetHandler(ctx => ExecuteMigrateDown(ctx, /* option refs */));
migrate.AddCommand(migrateDown);

root.AddCommand(migrate);
// … rollback, validate, list …

return root;
```

### 3.5 Handling `SingleOrNoValue` options

McMaster's `SingleOrNoValue` semantics (option present with no value → default-on; option
present with value → use value; option absent → default) must be replicated for:

- `--output[=<FILENAME>]`  →  split into `--output` (bool flag) and `--output-filename`
- `--strip[=true|false]`  →  `Option<bool?>` defaulting to `null` (treated as `true` when
  absent but `--strip` flag is given, false when `--strip-` or `--strip false` is given)
- `--include-untagged-migrations[=true|false]`  →  same pattern as `--strip`

The `MigratorOptions` mapping logic is updated accordingly.

---

## Part 4 – Migration Plan for `FluentMigrator.DotNet.Cli`

### Phase 1 – Create `FluentMigrator.Hosting.Commands`

- [ ] Create project file `src/FluentMigrator.Hosting.Commands/FluentMigrator.Hosting.Commands.csproj`
- [ ] Move `MigratorOptions.cs` (de-coupled from command classes; factory methods replaced by
      direct property setters or a builder)
- [ ] Move `TransactionMode.cs`
- [ ] Move `Setup.cs` (replace `McMaster.Extensions.CommandLineUtils.IConsole` with
      `System.CommandLine.IConsole` or `System.IO.TextWriter`)
- [ ] Create `CommandBuilders/ConnectionOptions.cs` – shared `Option<T>` factory for
      ConnectionCommand-level options
- [ ] Create `CommandBuilders/MigrationOptions.cs` – shared `Option<T>` factory for
      MigrationCommand-level options
- [ ] Create `CommandBuilders/FluentMigratorCommandBuilder.cs` – assembles the `RootCommand`
      and wires all `SetHandler` calls
- [ ] Add `FluentMigrator.Hosting.Commands` to `FluentMigrator.sln`

### Phase 2 – Migrate `FluentMigrator.DotNet.Cli`

- [ ] Remove `McMaster.Extensions.CommandLineUtils` `PackageReference`
- [ ] Add `ProjectReference` to `FluentMigrator.Hosting.Commands`
- [ ] Simplify `Program.cs` to `FluentMigratorCommandBuilder.BuildRootCommand().InvokeAsync(args)`
- [ ] Delete all files under `Commands/` (replaced by `FluentMigrator.Hosting.Commands`)
- [ ] Delete `MigratorOptions.cs` and `TransactionMode.cs` (moved)
- [ ] Delete `Setup.cs` (moved)
- [ ] Keep `DirtyAssemblyResolveHelper` compile-include reference
- [ ] Update `FluentMigrator.DotNet.Cli.csproj` `<Import>` and package refs

### Phase 3 – Migrate `FluentMigrator.Console`

- [ ] Add `PackageReference` for `System.CommandLine` to `FluentMigrator.Console.csproj`
- [ ] Rewrite `MigratorConsole.Run()` to build a System.CommandLine `RootCommand` with flat
      option structure (no subcommands), keeping the `--task` flag for backward compat
- [ ] Preserve all documented option aliases (see §1.2 above)
- [ ] Remove `Options.cs` (the bundled Mono.Options source file, ~700 lines)
- [ ] Keep `StopOnErrorLoggerProvider` and `--stop-on-error` option
- [ ] Keep `--connection-string-config-path` / `--configPath` on net48 target
  - Use `#if NETFRAMEWORK` guards for the machine.config lookup code
- [ ] Update exit-code contract (wrap System.CommandLine invocation in try/catch to preserve
      codes 3–6; return System.CommandLine's native error codes for parse errors)
- [ ] Update `Program.cs` accordingly

---

## Part 5 – Future Work (out of scope for this PR but tracked here)

### 5.1 Aspire Integration — `Aspire.Hosting.SystemCommandLine`

The issue envisions a thin bridge library that:
1. Walks the System.CommandLine `RootCommand` tree produced by
   `FluentMigratorCommandBuilder.BuildRootCommand()`
2. Registers each leaf subcommand as an Aspire resource command (dashboard button + CLI verb)
3. Exposes `SystemCommandLineResourceBuilderExtensions.AddSystemCommandLineCommands(…)`

This depends on `FluentMigrator.Hosting.Commands` being a clean, public API — which is why
we're creating it as a separate NuGet package.

### 5.2 MSBuild Task Integration

The issue also mentions a future pattern where System.CommandLine option types could be
shared with an MSBuild task adapter, allowing the same migration options to be driven from
`dotnet build` targets (Cake, FAKE, etc.).

---

## Part 6 – Test Plan

### 6.1 Unit tests for `FluentMigrator.Hosting.Commands`

Add tests in `test/FluentMigrator.Tests/` (or a new
`test/FluentMigrator.DotNet.Cli.Tests/` project):

- Verify `BuildRootCommand()` produces a properly structured tree (correct names, aliases,
  required/optional flags)
- Verify that each handler correctly populates `MigratorOptions` fields
- Verify backward-compat aliases resolve to the same `MigratorOptions` as the primary name
- Regression tests for `SingleOrNoValue` replacement logic (`--strip`, `--output`,
  `--include-untagged-migrations`)
- Snapshot tests (using Verify or plain `Assert.Equal`) comparing `MigratorOptions` produced
  by representative command lines before and after migration

### 6.2 Regression table (before/after invocation equivalence)

| Old invocation (McMaster / Mono.Options) | Equivalent System.CommandLine invocation |
|---|---|
| `dotnet fm migrate -p sqlserver -c "cs" -a app.dll` | unchanged |
| `dotnet fm migrate up -p sqlserver -c "cs" -a app.dll --target 20230101` | unchanged |
| `dotnet fm migrate down -p sqlserver -c "cs" -a app.dll -t 20220101` | `--target 20220101` (no `-t` short alias) |
| `dotnet fm rollback -p sqlserver -c "cs" -a app.dll` | unchanged |
| `dotnet fm rollback by 3 -p sqlserver -c "cs" -a app.dll` | unchanged |
| `dotnet fm rollback to 20220101 -p sqlserver -c "cs" -a app.dll` | unchanged |
| `dotnet fm rollback all -p sqlserver -c "cs" -a app.dll` | unchanged |
| `dotnet fm list processors` | unchanged |
| `dotnet fm list migrations -p sqlserver -c "cs" -a app.dll` | unchanged |
| `dotnet fm validate versions -p sqlserver -c "cs" -a app.dll` | unchanged |
| `dotnet fm migrate -p sqlserver -c "cs" -a app.dll --strip` | unchanged |
| `dotnet fm migrate -p sqlserver -c "cs" -a app.dll -o` | unchanged (output flag only) |
| `dotnet fm migrate -p sqlserver -c "cs" -a app.dll -o=out.sql` | unchanged |
| `Migrate.exe --assembly app.dll --provider sqlserver -c "cs"` | unchanged |
| `Migrate.exe -a app.dll -db sqlserver -c "cs" --task rollback --steps 3` | unchanged |
| `Migrate.exe --strip-` (**flag negation**) | ⚠️ **breaking** – use `--strip false` |
| `Migrate.exe --noConnection` | `--no-connection` (new name, old alias kept) |
