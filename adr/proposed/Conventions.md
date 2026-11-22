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
with `_`, as the current conventions do. A pattern is changed with `Update`,
in `Directory.Build.props` for a whole repository or in the project file for
one module:

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

**Why the defaults are imported first, before `Microsoft.NET.Sdk`.** `Update`
only changes an item declared earlier in evaluation, and does nothing,
silently, otherwise. `Directory.Build.props` is imported from inside
`Microsoft.NET.Sdk`'s props, which `Sdk.props` imports at line 25, and the
SDK's convention props come later, at line 67. Declared there, the defaults
wouldn't exist yet when a repository-wide `Update` in `Directory.Build.props`
runs. So the `SqlObjectNaming` and `SqlIdentifierLimit` defaults go in their
own props file, imported at the top of `Sdk.props`. That's possible because
they're literals that depend on no property. Evaluation order then matches
precedence: the SDK's default, then the repository's `Update`, then the
module's own `Update`, each overriding the one before.

`Directory.Build.targets` would also see the defaults, but it's imported after
the project body, so a repository-wide `Update` there would override a module's
own `Update`. That's backwards: the more specific setting has to win.

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
registry lists both as names for the same dialect. Whichever one won, the
choice would be arbitrary.

So the SDK normalizes each entry's dialect before comparing:

1. It splits the dialect into the longest registry name it starts with and a
   version suffix, which may be empty: `sqlserver2016` is `sqlserver` and
   `2016`.
2. It replaces the registry name with the first registry entry for the same
   runner package, and keeps the suffix: `postgresql` becomes `postgres`.

Two entries for the same kind with the same normalized dialect are a build
error that asks the user to keep one. `postgres` and `postgresql` normalize
to the same dialect and are rejected. `sqlserver` and `sqlserver2016` don't,
so the override in the previous example stays legal. Length then only decides
between a general entry and a more specific one.

**Why not compare runner packages.** An earlier draft rejected two entries
whose dialects map to the same runner package. `sqlserver` and `sqlserver2016`
both map to `FluentMigrator.Runner.SqlServer`, so that rule rejected exactly
the case longest-match exists for.

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

### 7. Names that are too long: an opt-in fallback pattern with a hash

A pattern can produce a name longer than the database allows. A foreign key
between two long table names with several columns passes SQL Server's 128
characters easily. A kind can declare a fallback:

```xml
<SqlObjectNaming Update="ForeignKey"
                 Pattern="FK_{ForeignTable}_{ForeignColumns}_{PrimaryTable}_{PrimaryColumns}"
                 OverflowPattern="FK_{ForeignTable}_{Hash}" />
```

The rule has three steps:

1. If the name from `Pattern` fits the dialect's limit, it's used unchanged.
2. Otherwise the name comes from `OverflowPattern`. `{Hash}` is the first 8
   hexadecimal digits of the SHA-256 hash of the UTF-8 bytes of the full name
   from step 1.
3. If that's still too long, for example because the table name alone is
   long, the text before `{Hash}` is cut from the right until it fits. `{Hash}`
   itself is never cut.

Names given with `WithName(...)` are never rewritten. The user chose them, and
a convention only fills in what was left empty.

**Why a hash, and not only cutting.** Cutting alone makes different names
equal. Two foreign keys from the same long table whose names differ only after
the limit become the same name, and the second `CREATE` fails. FluentMigrator
already has this bug: `FirebirdTruncator.Truncate` is
`name.Substring(0, 31)`.

**Why a hash of the full name, and not a counter.** EF Core keeps shortened
names unique by numbering them, because it has the whole model and can see
every other name. FluentMigrator runs one migration at a time and has no model
of the database. A hash of the full name is unique without knowing the other
names, and it's deterministic: `Delete.ForeignKey().FromTable(...)` computes
the same name its `Create` did. Rails 7.1 uses the same approach for generated
index names that are too long, falling back to a short `idx_on_..._<hash>`
form.

**Why a fallback pattern, and not quantifiers on tokens.** One proposal marked
the tokens that may shrink, as in `{Columns*}` or `{Id+}`. Rejected for three
reasons:

- Readers know `+`, `*` and `?` from regular expressions as counts.
  `{Columns+}` reads as "one or more columns", not "may be shortened".
- Quantifiers say which parts may shrink, but not what keeps the result
  unique, so they produce the collisions above.
- With several shrinkable tokens, the order they shrink in has to be specified,
  and every detail of it becomes part of every generated name.

A fallback pattern shows the short form as the user will see it in the
database, and the only cutting rule is step 3.

**Why the algorithm is fixed in this ADR.** The hash is part of the name, and
`Delete` recomputes the name. Changing the algorithm, the digest length or the
input encoding later would make every `Delete` that relies on the convention
miss an object created under the old rule. 9.0 took that kind of break once
already (decision 2). SHA-256 and not MD5, because MD5 is unavailable on
FIPS-restricted systems. Eight hex digits, because the shortest limit FluentMigrator
supports is Oracle's 30 bytes, and `FK_` plus a 9-character `_<hash>` still
leaves 18 bytes for the readable part.

**Why fallback is opt-in.** PostgreSQL silently cuts long names to 63 bytes,
in `CREATE` and in `DROP` alike, so long convention names work there today by
accident. If the fallback were on by default, a `Delete` for an object created
before the upgrade would compute the hashed name, while the database holds
PostgreSQL's cut one. Projects without `OverflowPattern` keep today's
behavior, which is to send the full name and let the database decide.

**What `FirebirdTruncator.Pack` teaches.** FluentMigrator already shortens with
a hash for Firebird (`PackKeyNames`). Each of its problems is a rule here:

- It hashes `Encoding.ASCII` bytes, which turn every non-ASCII character into
  `?`, so different names can hash alike. This ADR hashes UTF-8.
- It starts every packed name with `fk_`, including indexes and primary keys.
  Here the prefix is part of each kind's pattern.
- Its limit is a fixed 31 (`FirebirdOptions.MaxNameLength`), although
  Firebird 4.0 allows 63 characters. Here the limit is a setting (below), and
  the Firebird generator has to read the same setting before the SDK's
  Firebird default can rise above 31.
- It applies to `Delete` expressions as well as `Create`. That's right, and
  this design keeps it: the name is computed the same way for both.

#### The limit is pinned in the project, not read from the server

The SDK declares one limit per dialect, with its unit:

```xml
<ItemGroup>
  <SqlIdentifierLimit Include="sqlserver" MaxLength="128" Unit="Characters" />
  <SqlIdentifierLimit Include="postgres"  MaxLength="63"  Unit="Bytes" />
  <SqlIdentifierLimit Include="oracle"    MaxLength="128" Unit="Bytes" />
  <SqlIdentifierLimit Include="mysql"     MaxLength="64"  Unit="Characters" />
  <SqlIdentifierLimit Include="db2"       MaxLength="128" Unit="Bytes" />
  <SqlIdentifierLimit Include="firebird"  MaxLength="31"  Unit="Bytes" />
</ItemGroup>
```

A project on an older Oracle server lowers its entry:
`<SqlIdentifierLimit Update="oracle" MaxLength="30" />`. Entries match the
running processor the same way naming entries do (decision 3). SQLite has no
limit and no entry.

**Why Firebird's default is 31 bytes, not the server's 63 characters.** The
default has to be a limit every part of FluentMigrator accepts, not only the
server. The Firebird generator still checks names against
`FirebirdOptions.MaxNameLength`, a fixed 31 characters. With
`TruncateLongNames` off, the default, it throws "Name too long"; with it on,
it cuts the name at 31 characters, which removes the hash. With a default of
63, a 40-character fallback name would pass the SDK's limit and then fail, or
lose its hash, in the generator. 31 bytes is never more than 31 characters,
so names within it always pass the generator's check.

| Dialect | Limit | When it differs | Source |
|---|---|---|---|
| SQL Server | 128 characters | | [Database identifiers](https://learn.microsoft.com/en-us/sql/relational-databases/databases/database-identifiers#:~:text=Both%20regular%20and%20delimited%20identifiers%20must%20contain%20from%201%20through%20128%20characters) |
| PostgreSQL | 63 bytes, longer names silently cut | `NAMEDATALEN` is set when PostgreSQL is compiled | [Lexical structure](https://www.postgresql.org/docs/current/sql-syntax-lexical.html#:~:text=longer%20names%20can%20be%20written%20in%20commands%2C%20but%20they%20will%20be%20truncated) |
| Oracle | 128 bytes | 30 bytes when `COMPATIBLE` is lower than 12.2 | [Database object names and qualifiers](https://docs.oracle.com/en/database/oracle/oracle-database/19/sqlrf/Database-Object-Names-and-Qualifiers.html#:~:text=If%20COMPATIBLE%20is%20set%20to%20a%20value%20of%2012.2%20or%20higher%2C%20then%20names%20must%20be%20from%201%20to%20128%20bytes%20long) |
| MySQL | 64 characters | | [Identifier length limits](https://dev.mysql.com/doc/refman/8.0/en/identifier-length.html) |
| Db2 LUW | 128 bytes; longer is an error | | [SQL and XML limits](https://www.ibm.com/docs/en/db2/11.5.x?topic=sql-xml-limits#:~:text=If%20an%20attempt%20is%20made%20to%20create%20an%20identifier%20whose%20name%20is%20longer%20than%20the%20limit) |
| Firebird | 63 characters | 31 bytes before 4.0 or on databases older than ODS 13; server settings can lower it | [Firebird 4.0 release notes](https://firebirdsql.org/file/documentation/release_notes/html/en/4_0/rlsnotes40.html#:~:text=Object%20identifiers%20in%20an%20ODS%2013%20database%20can%20be%20up%20to%2063%20characters%20in%20length) |

**Why pinned and not detected.** Every "when it differs" entry in that table can
change under an existing database. An Oracle database raises `COMPATIBLE`, or
a Firebird database moves to ODS 13. If the limit were read from the server,
a foreign key created at 30 bytes would get the hashed name, and its `Delete`,
run years later at 128 bytes, would compute the full name and miss it. A
limit in the project only changes when someone edits the project.

**Why the unit matters.** At 63 bytes, a PostgreSQL name in Japanese fits about
21 characters. Counting characters there would produce names longer than 63
bytes, which PostgreSQL then cuts by itself, from the end, where the hash is.
Step 3 counts in the dialect's unit, measuring bytes as UTF-8, and only cuts
between characters.

`FirebirdTruncator` still runs later, inside the Firebird generator. With the
default Firebird limit of 31 bytes, convention names already fit, and it
leaves them alone.

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
  - two entries for one kind with the same normalized dialect, such as
    `postgres` and `postgresql` (error, decision 3);
  - a dialect that isn't in the `MigrationRunnerPackage` registry, allowing a
    version suffix, so `sqlserver2016` passes. A warning, not an error,
    because third-party processors are allowed;
  - a module with a fixed `DatabaseDialect` and an `@dialect` entry that can
    never match it (warning);
  - a `ConventionSetType` that doesn't exist, doesn't implement
    `IConventionSet`, or has no `(IConventionSet)` constructor (error);
  - an `OverflowPattern` without `{Hash}` (error, because cutting without a
    hash makes different names equal; decision 7);
  - a `SqlIdentifierLimit` with a `Unit` other than `Characters` or `Bytes`,
    or a `MaxLength` too small to hold the kind's prefix and `_<hash>`
    (error).
- **Generator input, limits.** `SqlIdentifierLimit` items reach the generator
  the same way as `SqlObjectNaming`, and the generated factory passes them to
  `PatternConventionSet`. No processor or generator gains a "maximum
  identifier length" API; the project is the only source of the limit.

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

**Quantifiers on tokens (`{Columns*}`, `{Id+}`) to shorten long names.**
Rejected; see decision 7.

**Cutting long names without a hash.** Rejected because different names become
equal; see decision 7.

**Reading the identifier limit from the server.** Rejected because the limit
can change under an existing database, and with it every computed name; see
decision 7.

**Hashing long names by default.** Rejected because it would rename what
PostgreSQL currently cuts silently, and break `Delete` for those objects; see
decision 7.

## Open questions

- **Raising Firebird's default to 63 characters.** Firebird 4.0 allows it on
  ODS 13 databases, but the Firebird generator first has to take its limit
  from the project's `SqlIdentifierLimit` instead of the fixed
  `FirebirdOptions.MaxNameLength`. Until then, a project can raise the limit
  only if it doesn't use `OverflowPattern` or the generator's truncation.

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
