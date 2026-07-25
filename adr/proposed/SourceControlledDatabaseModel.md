# A source-controlled database model — `FluentMigrator.Net.Sdk`

## Context

FluentMigrator describes *change* extremely well and *state* not at all. A
repository of migrations tells you how the database got where it is; nothing in
the build tells you what is actually declared to exist. Everything downstream
that wants declared state — drift detection, migration testing, an MCP server
diffing source control against a live database, an Aspire dashboard listing
what a deployment will touch — has to either reimplement discovery or read the
database it was supposed to be checking.

Meanwhile the layout question keeps being answered privately. ReadyRoll,
RoundhousE, Flyway and SSDT are all *the same idea* — SQL files in a directory
tree, classified by where they sit — with four incompatible conventions and
four bespoke runners. A team adopting FluentMigrator alongside any of them has
no shared vocabulary for "this file is a view, that one is a versioned
migration."

## Decision

Ship an MSBuild project SDK whose build produces, alongside the ordinary class
library, a deterministic, dialect-neutral manifest of the database object
model: `$(MSBuildProjectName).sourcemodel.json`.

```xml
<Project Sdk="FluentMigrator.Net.Sdk">
  <PropertyGroup>
    <DatabaseDialect>sqlserver</DatabaseDialect>
    <DefaultSchema>dbo</DefaultSchema>
  </PropertyGroup>
</Project>
```

The SDK ships on the FluentMigrator release train and is pinned in
`global.json` under `msbuild-sdks`, the way `Microsoft.NET.Sdk` ships with the
.NET SDK it belongs to. That is not just tidiness: this SDK emits an implicit
`FluentMigrator` package reference, so a separately-versioned SDK would give a
project two numbers to reconcile and a matrix to support. One number, and the
reference it emits lines up by construction.

The project is a *real* class library: migrations, maintenance migrations and
seed implementations compile into the assembly through the normal `Compile`
glob; scripts and data embed as resources. The manifest classifies rather than
duplicates them.

A companion SDK, `FluentMigrator.Net.Sdk.Host`, composes module projects into a
deployable host and is covered in
[`FluentMigrator-Host-Design.md`](FluentMigrator-Host-Design.md).

## Three levels, named precisely because they are different things

1. **Provider model** — the contract everything plugs into: pivot items
   carrying classification metadata (`ObjectType`, `NamePolicy`, `Execution`,
   `Stage`, `UniqueIdentity`, …), the `CollectSourceModel` hook, and the open
   manifest vocabulary. In the ADO.NET sense of the term.
2. **Provider** — the content kind: `sql` (default), future `dbt`, `lookml`,
   `poco`. *Not* `filesystem` — that names the transport, and everything in an
   MSBuild project lives on the filesystem; what distinguishes this provider is
   that its sources are SQL text.
3. **Convention** — a layout dialect *within* a provider: how paths and
   filenames map to the object model.

The proof that these axes are orthogonal: ReadyRoll/SCA, RoundhousE, Flyway and
SSDT are all the *same provider* with different conventions.

| Tool | Layout signature | As a convention pack |
|---|---|---|
| **fluentmigrator** (default) | `Migrations/`, `Schema/<Type>/<schema>.<object>.sql`, `Tools/` | built-in |
| **flyway** | flat `sql/`, `V__`/`U__`/`R__`/`B__` prefixes, `callback/` | built-in (`NamePolicy=FlywayPrefixed` parses version + description) |
| **readyroll / sca** | `Deploy-Changes/` numbered + `Programmable-Objects/<Type>/dbo.usp_X.sql` | roadmap — `Programmable-Objects` is `SchemaDotObject` verbatim |
| **roundhouse** | folder-per-pipeline-stage (`alterations/`, `views/`, `permissions/`, `runAfterOtherAnyTimeScripts/`) | roadmap — folder → `Execution`/`Stage` metadata map |
| **ssdt** | schema-first tree `dbo/Tables/Users.sql`, everything state-based | roadmap — a `SchemaFromPath` NamePolicy |

The unifying observation across all five: every artifact classifies on a
normalized **execution facet** — `once` (journaled, ordered: V migrations,
`alterations/`, `Deploy-Changes/`, `Migrations/*.cs`), `onChange` (idempotent,
checksum-driven: `R__`, `Programmable-Objects/`, `views|sprocs|functions/`,
`Schema/*`), or `always` (staged: callbacks, `permissions/`, maintenance).
Convention packs declare it per pivot; the manifest carries it in
`properties.execution`/`properties.stage`; the `checksum` field is what makes
`onChange` mechanically real. Two repositories in different conventions
therefore produce *comparable manifests* — which is what lets downstream
tooling stay convention-agnostic.

## Layout pivots

Analogous in spirit to [.NET artifacts-output pivots](https://learn.microsoft.com/en-us/dotnet/core/sdk/artifacts-output),
but for inputs: **one fixed, deterministic layout** — a single glob per role
carrying its own classification metadata — rather than inference over multiple
possible matches. Extension means declaring more pivots, never teaching the SDK
new heuristics. The full table is in the package README; the shape is:

```
Maintenance/**/*.cs                       maintenance migrations   (compiled)
Migrations/**/*.cs                        migrations               (compiled)
Migrations/Data/**/*.csv                  data loaded by migrations (embedded)
Migrations/Scripts/**/*.sql               scripts run by migrations (embedded,
                                          or copied when ContentType=External)
Schema/{Functions,StoredProcedures,Triggers,Views,Types}/<schema>.<object>.sql
                                          programmable objects     (modeled)
Seed/**/*.{cs,csv}                        seed implementations and data
Tools/CreateDatabase.sql                  creates the database container
Tools/DatabaseSchema.sql                  baseline: transitive closure
```

There is deliberately **no `Schema/Tables` pivot**: tables evolve through
migrations; `Schema/` holds idempotently re-creatable programmable objects.

The v0.1 directory-name inference (alias tables, singularization) was deleted,
not deprecated. Classification is pivot metadata; the only parsing the task does
is the `SchemaDotObject` filename policy.

## Consequences and constraints discovered while implementing

Three MSBuild evaluation-order constraints shape the design, and each one, if
ignored, fails *silently* — which for a document whose value is being committed
and diffed is the one unacceptable failure mode.

**Pivots must be declared before the project body; convention selection cannot
be.** Body-level edits like `<MigrationScript Update="…" ContentType="External" />`
require the item to already exist, so pivots are declared in props. But
`$(SourceModelConvention)` is naturally set *in* that body, after the props
have been evaluated — so it cannot be an import condition without silently
producing an empty model. Resolution: import every built-in pack, stamp each
item with `Convention` metadata, and filter the unselected packs out at build
time. This is how the .NET SDK handles `EnableDefaultCompileItems`, for exactly
the same reason. Custom packs are imported by path from
`$(CustomSourceModelConventionPacks)`, which — being read at import time — must
be set in `Directory.Build.props` or as a global property.

**Properties that derive from other body-settable properties must resolve at
target time.** `$(VersionTableSchema)` falls back to `$(DefaultSchema)`; resolved
in props it captures the empty pre-body value, and the host manifest ends up
with an empty schema. It is resolved in the targets instead.

**A timestamp-based up-to-date check cannot see a deleted input.** Remove a view
from `Schema/` and every surviving input is still older than the manifest, so
the target is skipped and the manifest keeps describing an object that no longer
exists. The manifest target's `Inputs` therefore include a stamp file whose
*content* is the input list, written with `WriteOnlyWhenDifferent`, so the set
changing — additions and removals alike — invalidates the manifest without
giving up incrementality.

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
lowercase hex, `\n` line endings, rewritten only on change. `type` is an open
vocabulary and `properties` an open bag, so future providers need no schema
change. `dependsOn` deliberately does not exist in v1 rather than existing and
lying — populating it is a per-dialect parsing problem (see below).

## Alternatives considered

**A separate `.sqlproj`-style project type.** Rejected: SSDT's model requires
its own project system, its own build, and its own tooling story, and the whole
point here is that a migrations project is already a class library. Layering on
`Microsoft.NET.Sdk` resolves in-box with no `global.json` SDK pinning.

**A compiled MSBuild task assembly.** Rejected for v1 in favour of an inline
task via `RoslynCodeTaskFactory`: no bootstrap problem, and it runs under both
Framework MSBuild (Visual Studio) and the dotnet CLI. The cost is a
conservative dialect — no LINQ, no `System.Text.Json` — to stay inside the
factory's default reference set on both runtimes. It graduates to a compiled
assembly when dependency extraction lands.

**Inferring classification from directory names.** Rejected — this was the v0.1
design. Heuristics are unbounded, undiscoverable and untestable; pivot metadata
is none of those.

## Roadmap

- **New object kinds** (sequences, RLS policies, Snowflake stages/pipes):
  declare a pivot — `<SqlObject Include="Schema/Sequences/**/*.sql"
  ObjectType="sequence" NamePolicy="SchemaDotObject" UniqueIdentity="true" />`
  — no SDK change.
- **dbt** (`FluentMigrator.Net.Sdk.Dbt`): a provider package declaring its own
  pivots (`models/**/*.yml`, `models/**/*.sql` → `type=model|source|seed|test`)
  and a target hooked `BeforeTargets="CollectSourceModel"` mapping them into
  `SourceModelFile` with `Provider=dbt`. The manifest task never changes.
  **LookML** (`…Sdk.LookML`) is the same shape.
- **Dependency extraction**: per-dialect parsing of view/procedure bodies,
  emitting object references, topologically ordered. Requires graduating the
  inline task to a compiled assembly.
- **An `ISchemaProvider` adapter over `.sourcemodel.json`** gives an MCP server
  a declared-state source to diff against live snapshots — drift between
  *source control* and *database*, not just database-to-database.

### The longer arc, in dependency order

1. The source-model and host manifests are a proto-metadata layer — checksummed
   declared state per version.
2. That makes migration testing tractable, because a migration's implicit
   world-state assumption becomes an assertable fixture ("given the manifest at
   version N, applying M yields manifest N+1") instead of a full database.
3. An Atlan-style unified metadata layer is the same idea with an org-wide
   registry as the fixture store, letting tests assert *partial* world-state
   facts.
4. At which point dbt-style declarative models and EasyMigrator-style
   POCO-as-schema are just additional providers — a `Provider=poco` pivot whose
   "source files" are types the Roslyn generator reads, diffed against declared
   state to synthesize migrations.

The open `type` vocabulary, open `properties` bag, and provider seam exist so
none of these require a manifest v2.

## Status

Proposed. Implemented under
[`src/FluentMigrator.Net.Sdk`](../../src/FluentMigrator.Net.Sdk), with samples
under `samples/FluentMigrator.Net.Sdk/` and a build-level test suite at
`test/FluentMigrator.Net.Sdk.SmokeTests/smoke-test.sh`.

Both SDK projects are in `FluentMigrator.sln` and carry no `<Version>` of their
own, so `VersionPrefix` from `Global.props` and CI's `-p:Version=$(semVer)`
govern them exactly as they govern the runtime libraries. This is the
`Microsoft.NET.Sdk` model — an MSBuild SDK ships with the thing it targets —
and it is the reason the SDK can emit an implicit `FluentMigrator` reference at
all without introducing an SDK-version × runtime-version matrix.

Open questions before this moves to `implemented`:

- The `Seed/**/*.cs` pivot classifies seeds today but nothing enforces
  conformance, because `ISeed` does not exist yet. Its design is settled and
  written up below; the implementation is not in this change.
- The manifest's relationship to a committed baseline is settled (see *Lock
  file*) but not implemented.

`AdditionalFiles` metadata was an open question here and is now closed:
moving the `AdditionalFiles` include into `SelectSourceModelConvention` (a
target, so post-body) as part of the convention fix also made body-level
`Update` edits visible to source generators. Verified — an
`<SeedData Update="Seed/seed_customers.csv" ObjectType="reference-data" />` in
the project body lands in the generated editor config as
`build_metadata.AdditionalFiles.ObjectType = reference-data`. That matters
because it means a generator can be *steered per file from the project body*,
which the seed design below depends on.

## `ISeed`

`ISeed` belongs in `FluentMigrator.Abstractions` alongside `IMigration`, so the
contract is available to a source generator and to the runner without dragging
in `FluentMigrator.Runner.Core`.

**The signature cannot take `Migration`.** `Migration` and `MigrationBase` live
in the `FluentMigrator` project, which references `Abstractions`, not the other
way round. `IInsertExpressionRoot` *is* in `Abstractions`
(`Builders/Insert/IInsertExpressionRoot.cs`), and it is the better parameter
anyway: it constrains a seed to data operations, which is what distinguishes a
seed from a migration, and it is trivial to fake in a unit test.

```csharp
namespace FluentMigrator;

public interface ISeed
{
    int Order { get; }
    void AddData(IInsertExpressionRoot insert);
}
```

### Lifecycle: reuse `MigrationStage`, do not invent a scheduler

The established usage is a maintenance migration whose `Up()` resolves
`IEnumerable<ISeed>` and calls each one. dbt generalizes further: seeds are
invocable at an arbitrary point in the lifecycle. FluentMigrator already has the
vocabulary for that in `MigrationStage` (`BeforeAll`, `BeforeEach`, `AfterEach`,
`BeforeProfiles`, `AfterAll`), so the generalization is an attribute, not a
second execution engine:

```csharp
[Seed(MigrationStage.AfterAll, Order = 10)]
public sealed class OrderStatusSeed : ISeed { … }
```

Seeds are then materialized into the *existing* maintenance pipeline rather than
run by anything new. This also gives the pivot metadata somewhere real to land:
the SDK already stamps `Stage=seed` on both seed pivots.

**Ordering has to be a composite key.** `int Order` is only a local sort key.
Under `FluentMigrator.Net.Sdk.Host`, several modules contribute seeds to one
database, and two modules independently using `Order = 1` is otherwise
unresolvable. The effective order is (module index in the target's plan, then
`Order`, then type name) — the module index comes from the host manifest, which
is exactly the composition problem that manifest exists to answer.

**Seeds need an execution facet, and today's pivots omit one.** The seed pivots
declare `Stage` but no `Execution`, so the manifest carries none. That is a gap,
not a neutral default: test-data seeding is `always`, while lookup/reference
tables — the enum case — are `onChange`, re-applied when the checksum moves.
`onChange` for seed data is arguably the single most useful thing the `checksum`
field buys, and it should be the default for `seed-data` with a per-item
override.

### Native AOT

Three distinct reflection hazards, each with a compile-time answer:

**Discovery.** `IEnumerable<ISeed>` from DI requires the concrete types to be
registered. `WithMigrationsIn(assembly)` is `[RequiresUnreferencedCode]`; the
AOT-safe seam added in 9.0.0 is `ITypeSource`/`WithTypes(…)`. Seeds must go
through it. The SDK's generator already knows every seed type at compile time —
the `Seed/**/*.cs` pivot told it — so it can emit the registration:
`.WithGeneratedSeeds()`. No `Assembly.GetTypes()`, which is the same conclusion,
for the same reason, that the Host ADR reaches for command discovery.

**CSV seeds.** dbt-style CSV lookup tables become a generated `[Seed]` class per
file, emitting literal `Insert.IntoTable(…).Row(…)` calls. The CSV is already an
`AdditionalFile` carrying `ObjectType`/`Stage`, so the generator has everything
it needs. Runtime CSV parsing is *not* itself an AOT problem, so this stays a
per-file choice — `SeedLoading=Generated|Runtime` metadata, the same idiom as
`ContentType=Embedded|External` on migration scripts — because a large seed file
is better embedded than turned into a million lines of C#.

**The enum + `[Display]` case.** Reading `[Display(Name = …)]` off enum members
with `GetCustomAttribute` is the genuinely trim-unsafe part: attributes can be
trimmed away even when the enum itself is rooted. The compile-time equivalent is
a generator attribute:

```csharp
[SeedFromEnum(typeof(OrderStatus), Table = "order_statuses")]
public sealed partial class OrderStatusSeed;
```

The generator reads the enum's members and their `[Display]` values from the
*compilation* — where attributes always exist — and emits literal rows. No
runtime reflection, and as a bonus the resulting values are declared state the
manifest can checksum, so a renamed enum member shows up as a reviewable diff
rather than as silent drift.

## Lock file

The manifest plays the `project.deps.json` role: a build output describing what
was resolved. What is missing is the `packages.lock.json` role — a *committed*
baseline that a build can be held to. Both should exist, with distinct names and
distinct lifecycles:

| Artifact | Analogue | Location | Committed |
|---|---|---|---|
| `$(MSBuildProjectName).sourcemodel.json` | `project.deps.json` | `bin/` | no |
| `$(MSBuildProjectName).sourcemodel.lock.json` | `packages.lock.json` | next to the project | yes |

NuGet's knobs map across directly, which is the point — the UX is already
understood:

| NuGet | Here |
|---|---|
| `RestorePackagesWithLockFile` | `GenerateSourceModelLockFile` |
| `RestoreLockedMode` (CI: fail if the lock would change) | `SourceModelLockedMode` |
| `--force-evaluate` | `-t:RewriteSourceModelLock` |

This resolves what was the third open question — "commit the manifest or not?"
— as *both, with different jobs*. It also supplies the missing half of the Host
ADR's *Schema versioning, applied* section, which calls for "compare current
against a stored baseline, fail on incompatible removal" applied to the JSON
manifest as a CI gate: the lock file is that stored baseline. And it is where
`onChange` semantics become reviewable — a changed view checksum appears in a
pull request diff instead of only at deploy time.
