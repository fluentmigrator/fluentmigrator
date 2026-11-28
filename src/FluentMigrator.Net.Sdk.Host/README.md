# FluentMigrator.Net.Sdk.Host

An MSBuild project SDK for FluentMigrator **host projects** — the composition
root for one or more [`FluentMigrator.Net.Sdk`](https://www.nuget.org/packages/FluentMigrator.Net.Sdk)
module projects.

A module project must not know connection strings, targets, or order; that is
the host project's job. A host project additionally declares *how* the
resulting commands are reachable — a CLI, an MSBuild task, an Aspire dashboard
button, an MCP tool. Its build output is a single manifest,
`$(MSBuildProjectName).host.json`, that a runner consumes instead of each front
door reinventing composition.

Both SDKs ship on the same release train as the FluentMigrator runtime
libraries and must be pinned to the same version as each other — they share a
manifest contract (`GetDeploymentModuleInfo`) with no other version
negotiation. While they are previews, both add the same prerelease label to the
runtime libraries' version (9.0.0-alpha.1 with FluentMigrator 9.0.0). Pin them
together in `global.json`; see the
[`FluentMigrator.Net.Sdk`](https://www.nuget.org/packages/FluentMigrator.Net.Sdk)
readme.

```xml
<Project Sdk="FluentMigrator.Net.Sdk.Host">
  <PropertyGroup>
    <HostContexts>dotnet-cli;aspire</HostContexts>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="../Core/Core.csproj" />
    <ProjectReference Include="../Sales/Sales.csproj" />
  </ItemGroup>

  <ItemGroup>
    <DeploymentTarget Include="primary">
      <ConnectionStringName>Primary</ConnectionStringName>
      <Dialect>sqlserver</Dialect>
      <!-- omit Modules for discovery: all referenced modules, root first,
           then ProjectReference declaration order -->
    </DeploymentTarget>
    <DeploymentTarget Include="tenant-ma">
      <ConnectionStringName>Tenant_MA</ConnectionStringName>
      <Modules>Core;Sales</Modules>   <!-- explicit full order, still linted -->
    </DeploymentTarget>
  </ItemGroup>
</Project>
```

Design rationale is recorded in
[`adr/proposed/FluentMigrator-Host-Design.md`](https://github.com/fluentmigrator/fluentmigrator/blob/main/adr/proposed/FluentMigrator-Host-Design.md)
and [`adr/proposed/SourceControlledDatabaseModel.md`](https://github.com/fluentmigrator/fluentmigrator/blob/main/adr/proposed/SourceControlledDatabaseModel.md).

## Two independent axes

The manifest records **contexts × targets**, and they are orthogonal:

- A **hosting context** is *how* a command is invoked. The Host ADR enumerates
  eight front doors that all resolve to the same `IMigrationCommand` set
  through `MigrationHost` — argv, MSBuild task properties, an Aspire command
  payload, an MCP tool call, a browser JS-interop bridge.
- A **runner** is *what* executes the resulting SQL, resolved per deployment
  target from that target's dialect. One host project can drive a `sqlserver`
  target and a `postgres` target through the same `aspire` context.

### Hosting contexts

| Context | Package | Reference kind | Availability |
|---|---|---|---|
| `dotnet-cli` | `FluentMigrator.DotNet.Cli` | tool | shipping |
| `console` | `FluentMigrator.Console` | tool | shipping |
| `msbuild` | `FluentMigrator.MSBuild` | package | shipping |
| `tui` | `FluentMigrator.TUI` | package | planned |
| `aspire` | `FluentMigrator.Aspire` | package | planned |
| `mcp` | `FluentMigrator.Mcp` | package | planned |
| `copilot-canvas` | `FluentMigrator.CopilotCanvas` | package | planned |
| `repl` | `FluentMigrator.Repl` | package | planned |

`$(HostContexts)` defaults to `dotnet-cli`. Selecting a **planned** context is
legal — it is recorded in the manifest so tooling can be built against it — and
adds no package reference until that pack's version property names one.
Selecting an unknown context is `FMSDK207`, with the known set in the message.

`ReferenceKind=tool` contexts are installed with `dotnet tool install` and
invoked out of process, so they are never referenced by the host project.
For `ReferenceKind=package` shipping contexts the mode matrix is identical to
`FluentMigrator.Net.Sdk`'s implicit `FluentMigrator` reference, so one rule
covers both SDKs: version property + CPM → `VersionOverride`; version property,
no CPM → classic reference; unset + CPM → versionless; unset, no CPM → no
reference. Version properties are named per pack, e.g.
`$(MsBuildHostContextVersion)`, `$(AspireHostContextVersion)`.

Contribute your own front door with a props file listed in
`$(CustomHostContextPacks)` declaring a `HostContextDefinition` item.

### Runners

Dialect → runner package is a registry (`MigrationRunnerPackage` items) covering
every in-box FluentMigrator processor. A target whose dialect is not registered
still builds; the manifest simply carries no runner for it (`FMSDK208`).

A target without its own `Dialect` inherits the first one its modules declare,
which is the root's, since the root deploys first. The manifest records the
target's effective `dialect` either way, so a consumer can see which dialect
chose the runner without repeating that rule. If the modules of such a target
declare different dialects, `FMSDK210` says so; set `Dialect` on the target.

## Ordering contract

**Declared order is the contract; the module reference graph is the linter.**
`DeploymentTarget` document order wins unconditionally — MSBuild reads top-down,
and evaluation order *is* the declaration. MSBuild *build* order stays
graph-driven; *deploy* order stays yours. When they disagree, `FMSDK201` tells
you rather than silently reordering (`AcknowledgeModuleOrder=true` per target
for the legitimate cases, e.g. B referenced by A but independently deployable
first).

**Root module:** self-declared in the module project, never inferred:

```xml
<IsDatabaseRootModule>true</IsDatabaseRootModule>
```

The root is the one module per database that owns `Tools/` (CreateDatabase,
baseline) and database-scoped `Maintenance/` migrations. Composition validates
exactly-one-root-first per target.

**Escape hatch**, Rails `database.yml` style:

```xml
<PropertyGroup>
  <HostManifestFile>host.custom.json</HostManifestFile>
</PropertyGroup>
```

Your hand-authored manifest ships verbatim; generation and linting are skipped.
Your file, your order, your rules.

## Cross-database ordering

Prior art surveyed: SSDT validates references but refuses to orchestrate
multi-dacpac order; Rails walks `database.yml` declaration order serially;
Django has a DAG within a database and declaration order across; dbt's `ref()`
DAG works because models are declarative and idempotent, which imperative
migrations are not. v1 follows the field — declaration order at the database
level, graph as validation. Interleaved cross-database migration dependencies
are explicitly out of scope for v1; the manifest's shape leaves room for a
future wave strategy merging timestamps across targets, which separate
VersionInfo tables make mechanically possible.

## Properties and diagnostics

| Property | Default | Purpose |
|---|---|---|
| `HostContexts` | `dotnet-cli` | `;`-separated hosting contexts to expose |
| `CustomHostContextPacks` | *(empty)* | `;`-separated paths to third-party context packs |
| `GenerateHostManifest` | `true` | Turn manifest generation off |
| `HostManifestFile` | *(empty)* | Ship a hand-authored manifest verbatim |
| `HostManifestFileName` | `$(MSBuildProjectName).host.json` | Output name |
| `TreatHostWarningsAsErrors` | `$(TreatWarningsAsErrors)`, else `false` | Promote FMSDK2xx warnings, except codes in `NoWarn` or `WarningsNotAsErrors` |
| `RequireSourceModelProvenance` | `false` | Make `FMSDK211` and `FMSDK212` errors |
| `SourceModelContractProperties` | the modules' contract properties | Properties that warn (`FMSDK213`) when they arrive as global properties; the same list as FluentMigrator.Net.Sdk's |
| `GenerateMigrationHost` | `false` | Reserved: `OutputType=Exe` entry-point generation over `FluentMigrator.Runner.Host` |

Diagnostics (the warnings become errors with `TreatHostWarningsAsErrors=true`,
and `FMSDK211` and `FMSDK212` also with `RequireSourceModelProvenance=true`).
`TreatHostWarningsAsErrors` follows `TreatWarningsAsErrors` unless you set it,
and a code in `NoWarn` or `WarningsNotAsErrors` is never promoted, as with C#
warnings; see FluentMigrator.Net.Sdk's README for why.

| Code | Severity | Meaning |
|---|---|---|
| `FMSDK201` | warning | The declared order contradicts the reference graph |
| `FMSDK202` | error | Multiple roots |
| `FMSDK203` | error | The root is not first |
| `FMSDK204` | error | An unknown module in a target |
| `FMSDK205` | warning | No root |
| `FMSDK206` | error | The custom manifest file is missing |
| `FMSDK207` | error | An unknown host context |
| `FMSDK208` | warning | No runner is registered for a target's dialect |
| `FMSDK209` | error | A module is listed twice in a target's `Modules` |
| `FMSDK210` | warning | The modules of a target without a `Dialect` declare different dialects |
| `FMSDK211` | warning | A module's source model manifest has no provenance beside it |
| `FMSDK212` | warning | A module's manifest doesn't have the SHA-256 its provenance records |
| `FMSDK213` | warning | A module contract property such as `DatabaseDialect` is a global property of the host, so MSBuild passes it on to every module; set a target's dialect with its `Dialect` metadata instead |

## Module provenance

Each module's source model manifest comes with a provenance file (see
FluentMigrator.Net.Sdk's README, "Provenance") that records how it was produced
and its SHA-256. The host accepts a module manifest only with that provenance
beside it and with the hash it records; otherwise it warns, or fails with
`RequireSourceModelProvenance=true`. Each module entry in the host manifest
records `sourceModelSha256`, so the chain of hashes runs from the host manifest
to each module manifest, and from there to its provenance.
