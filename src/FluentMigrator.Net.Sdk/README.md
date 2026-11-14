# FluentMigrator.Net.Sdk

An MSBuild project SDK for **source-controlled database models**. The project
is a real class library — migrations compile into the assembly, migration
scripts and data embed as resources — and every build additionally produces a
deterministic, dialect-neutral manifest (`$(MSBuildProjectName).sourcemodel.json`)
describing the full database object model.

```xml
<Project Sdk="FluentMigrator.Net.Sdk">
  <PropertyGroup>
    <DatabaseDialect>sqlserver</DatabaseDialect>
    <DefaultSchema>dbo</DefaultSchema>
  </PropertyGroup>
</Project>
```

## Versioning

This SDK ships on the same release train as the FluentMigrator runtime
libraries, the way `Microsoft.NET.Sdk` ships with the .NET SDK it belongs to.
Pin it once in `global.json` and every project gets it without repeating a
version:

```json
{
  "msbuild-sdks": {
    "FluentMigrator.Net.Sdk": "9.0.0",
    "FluentMigrator.Net.Sdk.Host": "9.0.0"
  }
}
```

Use the same version as the `FluentMigrator` packages you reference. Pinning
inline instead — `<Project Sdk="FluentMigrator.Net.Sdk/9.0.0">` — works too and
is the right choice for a single project. Because the version matches the
runtime libraries, the implicit `FluentMigrator` reference this SDK emits (see
Central Package Management below) lines up with the SDK by construction.

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
lowercase hex, `\n` line endings, rewritten only on change. Clean diffs *are*
the source control model; the copy you commit and diff is the lock file
(below), not this build output. `type` is an open vocabulary and `properties`
an open bag, so future providers need no schema change.

It also has to hold across machines, so two things never depend on the
checkout:

- **Checksums are over normalized text.** `checksum` is the SHA-256 of the
  file after dropping a leading UTF-8 BOM and replacing every CRLF with LF.
  A Windows checkout with `core.autocrlf=true` and a Linux CI run produce the
  same value, and so agree about `onChange`. To reproduce one, apply the same
  two steps before hashing.
- **Paths are relative.** `path` is relative to `SourceModelRoot` (the project
  directory by default). A file outside it is recorded by its `Link`
  metadata, or else relative to the root with `../` and an `FMSDK103` warning;
  an absolute path never reaches the manifest.

The manifest is rebuilt whenever anything it's made from changes: the set of
files, a file's content, an item's classification metadata (even when an
imported `.props` or `.targets` file sets it), or one of the properties in the
table below (even from `-p:` or `Directory.Build.props`). The check compares
content hashes, not timestamps, so a checkout, a `git stash pop` or a restored
CI cache can't make it skip a changed file. Otherwise the build skips it.
`dotnet clean` removes the manifest, and so does a build with
`GenerateSourceModelManifest=false`, so an old one never lingers in `bin/`.

## Committing the source model: the lock file

The manifest is a build output in `obj/` and `bin/`, like `project.deps.json`.
Don't commit it, and don't point `SourceModelManifestFileName` into the source
tree: it is a file name only, and a path in it is error `FMSDK003`. In the
source tree, git checkouts, merges, hand edits and several configurations or
target frameworks writing one file would all defeat the incremental build.

To commit the source model, so pull requests show which objects changed, turn
on the lock file, the way `packages.lock.json` commits a restore:

```xml
<PropertyGroup>
  <GenerateSourceModelLockFile>true</GenerateSourceModelLockFile>
  <!-- In CI: fail instead of updating the lock. -->
  <SourceModelLockedMode Condition="'$(CI)' == 'true'">true</SourceModelLockedMode>
</PropertyGroup>
```

`$(MSBuildProjectName).sourcemodel.lock.json` then sits next to the project
(`SourceModelLockFilePath` moves it). It has the manifest's content, and it
works differently from the manifest because it lives in the source tree:

- **It is compared on every build, by content.** There is no up-to-date check
  to fool: a hand-edited or mis-merged lock is corrected by the next build, or
  reported in locked mode.
- **Line endings don't matter.** The comparison ignores CRLF and a BOM, so a
  checkout with `core.autocrlf` isn't a change, and the build doesn't rewrite
  the file. It writes LF.
- **There is one writer per project.** With several target frameworks, the
  outer build writes it, and only if every framework produced the same source
  model (`FMSDK005` otherwise).
- **Locked mode never writes.** A missing or different lock is `FMSDK004`,
  which names the objects that were added, removed or changed. Without locked
  mode, CI would quietly regenerate a stale lock and pass.
- **`dotnet build -t:RewriteSourceModelLock`** is the explicit writer, even in
  locked mode. Use it after resolving a merge conflict in the lock rather than
  editing the file by hand.
- **Clean never deletes it.** It isn't a build output.

Recommended in `.gitattributes`, so the committed file is LF everywhere:

```
*.sourcemodel.lock.json text eol=lf
```

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
| `SourceModelManifestFileName` | `$(MSBuildProjectName).sourcemodel.json` | Output file name, without a path (`FMSDK003`) |
| `GenerateSourceModelLockFile` | `false` | Compare the lock file every build; create or update it |
| `SourceModelLockedMode` | `false` | Fail (`FMSDK004`) instead of writing the lock; for CI |
| `SourceModelLockFilePath` | `$(MSBuildProjectDirectory)/$(MSBuildProjectName).sourcemodel.lock.json` | Where the lock file goes |

Diagnostics: `FMSDK001` duplicate object identity — case-insensitive, only for
pivots declaring `UniqueIdentity` (error); `FMSDK002` missing file (error);
`FMSDK003` path in `SourceModelManifestFileName` (error); `FMSDK004` locked
mode: lock file missing or out of date (error); `FMSDK005` target frameworks
produced different source models, so there is no single lock (error);
`FMSDK100` file with no `ObjectType` (warning); `FMSDK101` empty file (warning);
`FMSDK102` `Tools/` artifact in a non-root module (warning); `FMSDK103` file
outside `SourceModelRoot` without `Link` metadata (warning); `FMSDK104`
`Execution` other than `once`, `onChange` or `always` (warning; a known value in
another case is written in its canonical spelling instead).

## Building and testing

```bash
./test/FluentMigrator.Net.Sdk.SmokeTests/smoke-test.sh
```

builds the samples under `samples/FluentMigrator.Net.Sdk/` against the in-repo
SDK payload and asserts the manifest contents, embed-vs-copy mapping,
diagnostics, incrementality, the flyway convention pack, and packing. The
pull request workflow runs it on Ubuntu.
