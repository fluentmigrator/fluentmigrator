# FluentMigrator.Net.Sdk

An MSBuild project SDK for **source-controlled database models**. The project
is a real class library — migrations compile into the assembly, migration
scripts and data embed as resources — and every build additionally produces a
deterministic, dialect-neutral manifest (`$(MSBuildProjectName).sourcemodel.json`)
describing the full database object model.

```xml
<Project Sdk="FluentMigrator.Net.Sdk/0.4.0">
  <PropertyGroup>
    <DatabaseDialect>sqlserver</DatabaseDialect>
    <DefaultSchema>dbo</DefaultSchema>
    <FluentMigratorVersion>7.1.0</FluentMigratorVersion>
  </PropertyGroup>
</Project>
```

Design rationale is recorded in
[`adr/proposed/SourceControlledDatabaseModel.md`](https://github.com/fluentmigrator/fluentmigrator/blob/main/adr/proposed/SourceControlledDatabaseModel.md).
The companion [`FluentMigrator.Net.Sdk.Host`](https://www.nuget.org/packages/FluentMigrator.Net.Sdk.Host)
package composes several of these module projects into a deployable host.

## Layout pivots

One fixed, deterministic layout — a single glob per role carrying its own
classification metadata — rather than inference over multiple possible
matches. Extension means declaring more pivots, never teaching the SDK new
heuristics.

| Pivot | Role | Build action |
|---|---|---|
| `Maintenance/**/*.cs` | Maintenance migrations | Compile |
| `Migrations/**/*.cs` | Migrations | Compile |
| `Migrations/Data/**/*.csv` | Data files loaded by migrations | EmbeddedResource |
| `Migrations/Scripts/**/*.sql` | Scripts run by migrations | EmbeddedResource; `ContentType=External` → Content, copied to output |
| `Schema/Functions/<schema>.<object>.sql` | Functions (scalar-, table-valued, aggregate) | modeled |
| `Schema/StoredProcedures/<schema>.<object>.sql` | Stored procedures | modeled |
| `Schema/Triggers/<schema>.<object>.sql` | Triggers | modeled |
| `Schema/Views/<schema>.<object>.sql` | Views | modeled |
| `Schema/Types/<schema>.<object>.sql` | User-defined types | modeled |
| `Seed/**/*.csv` | Seed data | EmbeddedResource |
| `Seed/**/*.cs` | `ISeed` implementations | Compile |
| `Tools/CreateDatabase.sql` | Creates the database container | modeled (`role: create-database`) |
| `Tools/DatabaseSchema.sql` | Baseline: full transitive closure of all objects | modeled (`role: baseline`) |

There is deliberately **no `Schema/Tables` pivot**: tables evolve through
migrations; `Schema/` holds idempotently re-creatable programmable objects.

**Filename policy** for `Schema/*` is `SchemaDotObject`: text before the first
`.` is the schema, the rest is the object name (`sales.open_orders.sql` →
`sales`.`open_orders`). A file with no schema segment
(`Schema/Types/ZipCode.sql`) falls back to `$(DefaultSchema)`. Per-item
overrides: `ObjectType`, `Schema`, `ObjectName` metadata.

Flipping a script from embedded to external:

```xml
<ItemGroup>
  <MigrationScript Update="Migrations/Scripts/V002_rebuild_stats.sql"
                   ContentType="External" />
</ItemGroup>
```

The mapping to `EmbeddedResource`/`Content` runs at target time
(`BeforeTargets="AssignTargetPaths"`), so body-level `Update` edits are
respected.

## Conventions

A *convention* is a layout dialect within a provider — how paths and filenames
map to the object model. `$(SourceModelConvention)` selects one:

```xml
<SourceModelConvention>flyway</SourceModelConvention>
```

| Convention | Layout signature |
|---|---|
| `fluentmigrator` (default) | `Migrations/`, `Schema/<Type>/<schema>.<object>.sql`, `Tools/` |
| `flyway` | flat `sql/`, `V__`/`U__`/`R__`/`B__` prefixes, `callback/` |

Every convention normalizes onto the same **execution facet** vocabulary —
`once` (journaled, ordered), `onChange` (idempotent, checksum-driven), or
`always` (staged) — so two repositories in different layouts still produce
comparable manifests.

All built-in convention packs are imported before the project body (that is
what makes body-level `Update` edits possible), and the packs the project did
not select are filtered out at build time. Custom packs are imported by path
from `$(CustomSourceModelConventionPacks)`; because that list is read at import
time it must be set in `Directory.Build.props` or as a global property, while
`$(SourceModelConvention)` may be set anywhere. A custom pack must stamp
`Convention` metadata matching the value the project selects.

## Central Package Management

Under CPM (`ManagePackageVersionsCentrally=true`), no version property is
needed — the SDK emits a **versionless** implicit
`<PackageReference Include="FluentMigrator" />` and the version comes from your
`Directory.Packages.props` via the normal CPM join at restore. Mode matrix: CPM
alone → versionless reference; CPM + `FluentMigratorVersion` → `VersionOverride`
(a plain `Version` under CPM is NU1008); no CPM → classic versioned reference,
opt-in via `FluentMigratorVersion`. A missing `PackageVersion` entry under CPM
fails restore with NU1010, whose message names the fix. Suppress entirely with
`IncludeFluentMigratorReference=false`.

## The Roslyn generator contract

`DefaultSchema` and `DatabaseDialect` (plus `IsDatabaseRootModule`,
`VersionTableName`, `VersionTableSchema`) are forwarded to the compiler as
`build_property.*` analyzer config, readable from
`AnalyzerConfigOptionsProvider.GlobalOptions`. All modeled files additionally
flow to the compiler as `AdditionalFiles` with `ObjectType`, `NamePolicy` and
`Role` exposed via `CompilerVisibleItemMetadata`, so a generator can see the
full object model without reparsing the project file.

## Manifest format (v1)

```json
{
  "manifestVersion": 1,
  "project": "AdventureLite",
  "dialect": "sqlserver",
  "convention": "fluentmigrator",
  "defaultSchema": "dbo",
  "role": "root",
  "providers": ["sql"],
  "objects": [
    {
      "provider": "sql",
      "type": "view",
      "schema": "sales",
      "name": "open_orders",
      "path": "Schema/Views/sales.open_orders.sql",
      "checksum": "sha256:…",
      "properties": {"execution": "onChange"}
    }
  ]
}
```

Determinism is a design requirement: objects sorted (type, schema, name, path),
lowercase hex, `\n` line endings, rewritten only on change. The manifest is
meant to be committed and diffed — clean diffs *are* the source control model.
`type` is an open vocabulary and `properties` an open bag, so future providers
need no schema change.

## Properties and diagnostics

| Property | Default | Purpose |
|---|---|---|
| `DatabaseDialect` | `generic` | Stamped into the manifest; compiler-visible |
| `DefaultSchema` | *(empty)* | Fallback schema for schema-less filenames; compiler-visible. Empty by default — any concrete value is a dialect opinion |
| `SourceModelConvention` | `fluentmigrator` | Layout convention pack to select |
| `FluentMigratorVersion` | *(unset)* | Classic mode: adds the implicit `FluentMigrator` reference with this version. Under CPM, becomes a `VersionOverride` |
| `IncludeFluentMigratorReference` | auto | Defaults to `true` under CPM or when `FluentMigratorVersion` is set; `false` suppresses it |
| `IsDatabaseRootModule` | `false` | The one module per database that owns `Tools/` and database-scoped `Maintenance/` |
| `VersionTableName` / `VersionTableSchema` | `VersionInfo` / `$(DefaultSchema)` | Per-module VersionInfo table; compiler-visible |
| `EnableDefaultSourceModelItems` | `true` | Disable built-in pivots for full manual control |
| `GenerateSourceModelManifest` | `true` | Turn the manifest step off |
| `TreatSourceModelWarningsAsErrors` | `false` | Promote FMSDK1xx warnings |
| `SourceModelManifestFileName` | `$(MSBuildProjectName).sourcemodel.json` | Output name |

Diagnostics: `FMSDK001` duplicate object identity — case-insensitive, only for
pivots declaring `UniqueIdentity` (error); `FMSDK002` missing file (error);
`FMSDK100` file with no `ObjectType` (warning); `FMSDK101` empty file (warning);
`FMSDK102` `Tools/` artifact in a non-root module (warning).

## Building and testing

```bash
./test/FluentMigrator.Net.Sdk.SmokeTests/smoke-test.sh
```

builds the samples under `samples/FluentMigrator.Net.Sdk/` against the in-repo
SDK payload and asserts the manifest contents, embed-vs-copy mapping,
diagnostics, incrementality, the flyway convention pack, and packing.
