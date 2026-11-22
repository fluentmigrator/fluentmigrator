# Conventions: three systems, one word, and SQL object conventions in the project file

## Context

FluentMigrator uses the word *convention* for three unrelated systems:

| System | Where it lives | What it decides |
|---|---|---|
| `IConventionSet` | `FluentMigrator.Runner.Core/Conventions` | Fills in what a migration left empty: object names (`PK_`, `UC_`, `FK_`, `IX_`), the default schema, the root path of `Execute.Script`, the embedded resource names of `AutoScriptMigration` |
| `IMigrationRunnerConventions` | `FluentMigrator.Runner.Core` | Which types are migrations, profiles, maintenance migrations, version tables; how tags match (see [`RunnerConventions.md`](RunnerConventions.md)) |
| `$(SourceModelConvention)` | `FluentMigrator.Net.Sdk` | Which file layout maps paths to objects: `fluentmigrator` or `flyway` (see [`SourceControlledDatabaseModel.md`](SourceControlledDatabaseModel.md)) |

So a question like "can I configure conventions from the project file?" has two
correct answers today: yes (the layout) and no (everything else).

`IConventionSet` itself mixes two kinds of convention. Its naming and schema
members describe the SQL objects a migration creates. `RootPathConvention` and
`AutoNameConventions` describe where a migration's *inputs* come from: files
and embedded resources. Those overlap with the SDK's layout packs, which also
locate SQL scripts.

The SDK and the runner each have a default schema, and nothing connects them:

- `$(DefaultSchema)` places schema-less `Schema/*.sql` files in the manifest.
  `FluentMigrator.Net.Sdk.Conventions.props` makes it compiler-visible, and its
  comment says a FluentMigrator source generator *can* use it to emit a
  matching convention set. No generator does.
- The runner takes its default schema from the host: `--default-schema-name`
  in `dotnet-fm` (`Setup.cs`), `DefaultSchemaName` on the MSBuild task
  (`Migrate.cs`), or a `DefaultConventionSet` registered in code. The Host
  SDK's `msbuild` context adds the package reference and passes nothing.

The failure is silent. Take the composite sample's `Sales` module, with
`<DefaultSchema>sales</DefaultSchema>`. The manifest places
`Schema/Views/open_orders.sql` in `sales`. A host that doesn't pass a schema
runs `Create.Table("Orders")` in the login's default schema, `dbo` on SQL
Server. The view and the table it reads end up in different schemas, and no
build step notices.

Everything in `IConventionSet` beyond the schema and the root path can only
be configured in C#.

## Decision

### 1. Name three kinds of convention

| Kind | Members today | Question it answers |
|---|---|---|
| **SQL object conventions** | the naming members of `IConventionSet` (constraint, foreign key, index, the primary key through the columns list) and `SchemaConvention` | What the objects a migration creates are called, and which schema they're in |
| **Source conventions** | `RootPathConvention`, `AutoNameConventions`, the SDK's `$(SourceModelConvention)` packs | Where a migration's inputs (scripts, data, resources) are found |
| **Discovery conventions** | `IMigrationRunnerConventions` | Which compiled types the runner treats as migrations, and which of them run |

Documentation and ADRs use these names, and say "SQL object convention" rather
than "convention" whenever the context allows both readings.

**Why names and not new interfaces.** Splitting `IConventionSet` along these
lines would break every custom convention set, the kind of break users have
to fix by hand. 9.0 is already adding one member to `IConventionSet`, for
check-constraint naming
([PR #2459](https://github.com/fluentmigrator/fluentmigrator/pull/2459)).
Adding one property breaks custom sets at a single, mechanical line. A split
would break them everywhere they're used. The confusion is about words, so the
fix is words; the SDK ADR does the same for "provider" and "convention" in
*Three levels, named precisely because they are different things*.

### 2. Configure SQL object naming with `SqlObjectNaming` items

The SDK declares one item per kind of named object. Its pattern reproduces
exactly what `DefaultConventionSet` generates today:

```xml
<ItemGroup>
  <SqlObjectNaming Include="PrimaryKey"       Pattern="PK_{Table}" />
  <SqlObjectNaming Include="UniqueConstraint" Pattern="UC_{Table}_{Columns}" />
  <SqlObjectNaming Include="ForeignKey"       Pattern="FK_{ForeignTable}_{ForeignColumns}_{PrimaryTable}_{PrimaryColumns}" />
  <SqlObjectNaming Include="Index"            Pattern="IX_{Table}_{Columns}" />
  <SqlObjectNaming Include="CheckConstraint"  Pattern="CK_{Table}_{Columns}" />
</ItemGroup>
```

`{Columns}`, `{ForeignColumns}` and `{PrimaryColumns}` join the column names
with `_`, as the current conventions do. A project changes a pattern with
`Update`, in the project file or in `Directory.Build.props`:

```xml
<ItemGroup>
  <SqlObjectNaming Update="Index" Pattern="IDX_{Table}_{Columns}" />
</ItemGroup>
```

**Why the defaults must match `DefaultConventionSet` exactly.** A name produced
by a convention is part of the database, and later migrations depend on it.
`Delete.Index().OnTable("Orders").OnColumn("Status")` doesn't store a name; the
index convention computes one, and the delete only works if it matches the
name the index was created with. 9.0 already took this break once, on purpose:
`Create.PrimaryKey()` changed from `PK_Table_Id` to `PK_Table` (Issue #1800),
and the CHANGELOG tells affected users to name the key explicitly or replace
the convention. Moving a project to the SDK must not take that break a second
time without anyone choosing it.

For the same reason, the documentation for `SqlObjectNaming` must say that
changing a pattern is not retroactive. Objects created under the old pattern
keep their names, and a later `Delete` that relies on the convention looks
for the new name.

**Why items and not properties.** Properties such as `<IndexNamePattern>` read
well until a pattern has to differ by dialect (decision 3). Then each dialect
needs its own invented property name, and the SDK has to know every name in
advance. Items already have the operations needed: `Update` changes one entry,
`Include` adds one, and the pivots in `SourceControlledDatabaseModel.md` use
the same shape, so there's one rule to learn.

**Why the SDK declares the defaults in its props.** `Update` only changes an
item that already exists. This is the constraint the SDK ADR records for
pivots: declared before the project body, or body-level edits silently do
nothing.

**Why patterns and not code.** Most teams that change naming change a prefix or
drop a column list. A pattern covers that without a C# file. Logic beyond a
pattern belongs in C#, through decision 5.

### 3. Dialect-specific patterns are resolved at run time

A pattern for one dialect is an added item whose name carries the dialect:

```xml
<ItemGroup>
  <SqlObjectNaming Include="ForeignKey@sqlserver" Pattern="FK_{ForeignTable}_{PrimaryTable}" />
</ItemGroup>
```

**Why not `Condition="'$(DatabaseDialect)' == 'sqlserver'"`.** MSBuild evaluates
every property before any item, so that condition does see the final dialect,
even from `Directory.Build.props`. It works, and for a module with a fixed
dialect it gives the same result. But it fixes the answer into the assembly at
build time, and the dialect isn't always known then. The SDK's default dialect
is `generic`, and the Host can deploy one module to a `sqlserver` target and a
`postgres` target from the same build. A `generic` module built with the
condition would get the default pattern on SQL Server too, without any
warning. FluentMigrator already chooses per dialect at run time elsewhere:
`IfDatabase(...)`, and `AutoScriptMigration`, which passes the running
processor's names to `DefaultAutoNameConvention` as `DatabaseNames`.

**Why the dialect is part of the item's name and not metadata.** `Update`
matches items by name. With `Dialect="sqlserver"` as metadata, a
repository-wide `<SqlObjectNaming Update="ForeignKey" ... />` would also
overwrite every dialect-specific foreign-key pattern. With the dialect in the
name, `Update="ForeignKey"` changes only the default, and
`Update="ForeignKey@sqlserver"` changes only the SQL Server entry.

**How an entry matches.** An entry's dialect matches the running processor when
its `DatabaseType` or one of its `DatabaseTypeAliases` starts with the dialect,
ignoring case. That's the rule `MigrationRunner` already uses to recognize a
processor, and it's why `sqlserver` matches `SqlServer2016`. The runner then
picks a pattern in this order:

1. the longest matching `Kind@dialect` entry;
2. `Kind`;
3. the built-in default.

**Why the longest match wins.** A longer dialect is more specific:

```xml
<SqlObjectNaming Include="ForeignKey@sqlserver"     Pattern="FK_{ForeignTable}_{PrimaryTable}" />
<SqlObjectNaming Include="ForeignKey@sqlserver2016" Pattern="FK_{ForeignTable}_{ForeignColumns}" />
```

On a `SqlServer2016` processor both match. If the shortest won, the 2016 entry
could never apply, and nobody writes a more specific entry expecting it to be
ignored. This is longest-prefix matching, as in network routing.

**Why aliases are an error and not a tie.** `postgres` and `postgresql` both
match `PostgreSQL15_0`, but neither is more specific. The SDK's runner
registry maps both to `FluentMigrator.Runner.Postgres`. Whichever one won,
the choice would be arbitrary. Two entries for the same kind whose dialects
map to the same runner package are a build error that asks the user to keep
one. Length then only decides between a general entry and a specific one.

### 4. One default schema, two consumers

The generated convention set takes its default schema from `$(DefaultSchema)`.
The value that places `Schema/*.sql` files in the manifest also qualifies
schema-less expressions, so the `Sales` example in the context section puts
the view and the table in `sales`.

9.0 already routes the runner's schema through one service,
`IDefaultSchemaNameResolver`, so that schema checks and the `$(DefaultSchema)`
SQL token agree. The generated set feeds that service; nothing reads the
schema a second way.

### 5. C# beyond patterns: `ConventionSetType`

```xml
<PropertyGroup>
  <ConventionSetType>Sales.Migrations.SalesConventionSet</ConventionSetType>
</PropertyGroup>
```

The type implements `IConventionSet` and has a constructor that takes an
`IConventionSet`. It receives the set built from the project file as that
inner set, the way the documentation's `YourConventionSet(IConventionSet
innerConventionSet)` example wraps `DefaultConventionSet`. Custom code can
change one member and defer the rest to the XML.

**Why the generator binds the type and the runner doesn't load it by name.**
Loading a type named in configuration needs reflection at run time, which the
Host ADR keeps off every path that can be compiled ahead of time. Its one
exception is a sidecar assembly the user names on the command line, which is
JIT-only by design. Here the type is in the module's own compilation, so the
source generator can emit `new Sales.Migrations.SalesConventionSet(inner)`.
A missing type or a wrong constructor is then a build error, not a failed
deployment.

### 6. One convention set per module

Each module project produces its own convention set, and the Host runs each
module with its own.

**Why.** The composite sample's modules have different default schemas, `core`
and `sales`. A runner registers one `IConventionSet`, so one set can't serve
both. The Host manifest already gives each module its own version table on
each target, which one runner can't do either, so the Host's runtime has to
build a runner per module anyway. The convention set follows the version
table.

### What stays in code, and why

- **`RootPathConvention`.** The SDK already decides whether a script is
  embedded or copied next to the assembly, so the SDK should fill in the root
  path itself. A path the user sets at build time points at the build machine
  and is wrong once the module is deployed.
- **`AutoNameConventions`.** The SDK's `MigrationScript` folders replace
  `AutoScriptMigration`'s `Scripts.Up.{Version}_{Type}_{Database}.sql` scheme
  for new projects. Existing users keep the C# convention unchanged.
- **Discovery conventions.** The generator finds `[Migration]` types at compile
  time, and [`RunnerConventions.md`](RunnerConventions.md) is the place to
  rethink how migrations are filtered.

## Implementation

- **Generator input.** Source generators see build properties and the metadata
  of `AdditionalFiles`, nothing else. A target joins the `SqlObjectNaming`
  items into one compiler-visible property before the editorconfig is
  generated, the same timing `SelectSourceModelConvention` uses for
  `AdditionalFiles`.
- **Generator output.** A module-level factory:

  ```csharp
  // <auto-generated/>
  internal static class ModuleConventions
  {
      public static IConventionSet Create(string workingDirectory, IQuerySchema processor) =>
          new Sales.Migrations.SalesConventionSet(           // only with ConventionSetType
              new PatternConventionSet(
                  defaultSchema: "sales",
                  workingDirectory,
                  processor,
                  naming: new Dictionary<string, string>
                  {
                      ["PrimaryKey"]           = "PK_{Table}",
                      ["ForeignKey"]           = "FK_{ForeignTable}_{ForeignColumns}_{PrimaryTable}_{PrimaryColumns}",
                      ["ForeignKey@sqlserver"] = "FK_{ForeignTable}_{PrimaryTable}",
                      ["Index"]                = "IDX_{Table}_{Columns}",
                      // ...
                  }));
  }
  ```

- **Runtime.** `PatternConventionSet` is new, in `FluentMigrator.Runner.Core`.
  It resolves each kind's pattern once, because a runner's processor doesn't
  change. It's registered as a factory from the selected processor; whether
  that creates a dependency cycle between the convention set and the processor
  has to be checked when it's built.
- **Diagnostics,** in a new `FMSDK3xx` range:
  - a pattern with an unknown `{Token}`, or a token the kind doesn't have
    (`{ForeignTable}` on an index);
  - an unknown kind (`Include="Indx"`);
  - two entries for one kind whose dialects are aliases of the same runner
    (error, decision 3);
  - a dialect that isn't in the `MigrationRunnerPackage` registry, allowing a
    version suffix, so `sqlserver2016` passes. A warning, not an error,
    because third-party processors are allowed;
  - a module with a fixed `DatabaseDialect` and an `@dialect` entry that can
    never match it (warning);
  - a `ConventionSetType` that doesn't exist, doesn't implement
    `IConventionSet`, or has no `(IConventionSet)` constructor (error).

## Alternatives considered

**A `Condition` on `$(DatabaseDialect)`.** Works, but fixes the dialect at build
time; see decision 3. It stays legal, and the documentation recommends
`@dialect`.

**Dialect as item metadata.** Rejected because `Update` matches by name; see
decision 3.

**Shortest match wins.** Rejected because a more specific entry could never
apply; see decision 3.

**A JSON or YAML conventions file.** Rejected. Teams already share build
settings through `Directory.Build.props`, and a separate file would need its
own discovery, its own override rules and its own way to reach the generator,
all of which MSBuild items already have.

**Loading `ConventionSetType` by reflection.** Rejected; see decision 5.

**Splitting `IConventionSet` into the three kinds.** Rejected; see decision 1.

## Open questions

- **Which default schema wins when both are given.** A project sets
  `$(DefaultSchema)` and the user also passes `--default-schema-name`. The
  command line could win, as it usually does over the project, or the two
  could be required to agree. The first allows a deliberate override; the
  second prevents the silent mismatch this ADR starts from.
- **`CheckConstraint`** depends on check constraints landing
  ([PR #2457](https://github.com/fluentmigrator/fluentmigrator/pull/2457))
  and on the naming convention in their ADR
  ([PR #2459](https://github.com/fluentmigrator/fluentmigrator/pull/2459)).

## Related

- [`DefaultSchema.md`](DefaultSchema.md) names, as a goal, letting users
  "force a consistent default schema across all generators, via overriding the
  Convention Set interface". Decision 4 does that from the project file.
- [`RunnerConventions.md`](RunnerConventions.md) covers discovery conventions.
- [`SourceControlledDatabaseModel.md`](SourceControlledDatabaseModel.md)
  covers source conventions (layout packs) and the SDK.
- [`FluentMigrator-Host-Design.md`](FluentMigrator-Host-Design.md) covers the
  Host, its manifest and its rules on reflection.

## Status

Proposed. Nothing in this ADR is implemented.
