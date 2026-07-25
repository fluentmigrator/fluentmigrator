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

```xml
<Project Sdk="FluentMigrator.Net.Sdk.Host/0.1.0">
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
| `TreatHostWarningsAsErrors` | `false` | Promote FMSDK2xx warnings |
| `GenerateMigrationHost` | `false` | Reserved: `OutputType=Exe` entry-point generation over `FluentMigrator.Runner.Host` |

Diagnostics: `FMSDK201` declared order contradicts the reference graph (warn),
`FMSDK202` multiple roots (error), `FMSDK203` root not first (error),
`FMSDK204` unknown module in target (error), `FMSDK205` no root (warn),
`FMSDK206` custom manifest file missing (error), `FMSDK207` unknown host
context (error), `FMSDK208` no runner registered for a target's dialect (warn).
