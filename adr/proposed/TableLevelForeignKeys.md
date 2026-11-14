# ADR: Table-Level (Composite) Foreign Keys in Create.Table

## Status
Proposed

Written as the intermediate design step requested in [#2090](https://github.com/fluentmigrator/fluentmigrator/issues/2090). No implementation is proposed here; the intent is to settle the syntax first.

## Context

FluentMigrator can express a foreign key in two ways, and neither can express a
**composite foreign key declared as part of `CREATE TABLE`**.

The request in #2090 is the SQL this cannot produce:

```sql
CREATE TABLE Area(
  ArticleId TEXT NOT NULL,
  AreaGroupIndex INTEGER NOT NULL,
  "Index" INTEGER NOT NULL,
  PRIMARY KEY(ArticleId, AreaGroupIndex, "Index"),
  FOREIGN KEY(ArticleId, AreaGroupIndex) REFERENCES AreaGroup(ArticleId, "Index") ON DELETE CASCADE
)
```

Two foreign columns referencing two primary columns, inside the table definition.

### Current state

**Column-level.** `IColumnOptionSyntax<TNext, TNextFk>` exposes:

```csharp
TNextFk ForeignKey(string primaryTableName, string primaryColumnName);
```

One foreign column, one primary column. This is generated **inline** by providers that
need it — `SQLiteColumn.Generate` collects every column with `IsForeignKey`, emits a
`FOREIGN KEY (...) REFERENCES ...` clause per column, and renames the resulting
`CreateForeignKeyExpression` to `$$IGNORE$$_...` so the follow-up statement is skipped
rather than rejected.

**Table-level, after the fact.** `Create.ForeignKey()...ForeignColumns(a, b).ToTable(t).PrimaryColumns(c, d)`
does support composites — but it emits `ALTER TABLE ... ADD CONSTRAINT`, which is a
separate statement.

### The gap, precisely

| Want | Column-level `ForeignKey(...)` | `Create.ForeignKey(...)` |
|---|---|---|
| Single-column FK, inline in `CREATE TABLE` | yes | no — emits `ALTER TABLE` |
| Composite FK, inline in `CREATE TABLE` | **no** | no |
| Composite FK, as `ALTER TABLE` | no | yes |

The empty cell is the feature. It is **not SQLite-specific** — no provider can express an
inline composite foreign key today. SQLite merely makes it *load-bearing*, because it has
no `ALTER TABLE ... ADD CONSTRAINT` at all, so the `Create.ForeignKey` route is closed and
the inline route is the only one that exists. That is why #2090, [#1784](https://github.com/fluentmigrator/fluentmigrator/issues/1784)
and [#2117](https://github.com/fluentmigrator/fluentmigrator/issues/2117) all surface as SQLite reports.

## Provider Support

Table-level `FOREIGN KEY (cols) REFERENCES t (cols)` inside `CREATE TABLE` is SQL-92 and
universally accepted. The differences are in enforcement and in whether the `ALTER TABLE`
alternative exists.

| Provider | Inline table-level FK in `CREATE TABLE` | `ALTER TABLE ADD CONSTRAINT` | Notes |
|---|---|---|---|
| **SQL Server** | Supported | Supported | Both routes available |
| **PostgreSQL** | Supported | Supported | Both routes available |
| **MySQL / MariaDB** | Supported | Supported | Enforced on InnoDB; parsed and ignored on MyISAM |
| **Oracle** | Supported | Supported | Both routes available |
| **SQLite** | Supported | **Not supported** | Inline is the only route. `PRAGMA foreign_keys` governs enforcement, not syntax |
| **Firebird** | Supported | Supported | Both routes available |
| **DB2** | Supported | Supported | Both routes available |
| **SAP HANA** | Supported | Supported | Both routes available |
| **Snowflake** | Supported (syntax) | Supported (syntax) | Constraints recorded but **not enforced** |
| **Redshift** | Supported (syntax) | Supported (syntax) | Constraints informational, **not enforced**; used by the query planner |

No provider requires a fallback. The generator work is uniform, which is what makes this a
tractable cross-provider feature rather than a per-provider special case.

## Options Considered

### Option A — extend the column-level syntax (proposed in #2090)

```csharp
TNextFk ForeignKey(string primaryTableName, string[] primaryColumnNames);
TNextFk ForeignKey(string foreignKeyName, string primaryTableName, string[] primaryColumnNames);
TNextFk ForeignKey(string foreignKeyName, string primaryTableSchema, string primaryTableName, string[] primaryColumnNames);
// plus ReferencedBy(...) mirrors
```

**This does not close the gap.** The overloads widen the *primary* side to many columns
while the *foreign* side stays fixed at the one column the call hangs off. The motivating
example needs two foreign columns — `FOREIGN KEY(ArticleId, AreaGroupIndex)` — and there is
no column to hang that from.

Worse, `1 foreign column → N primary columns` is not a valid foreign key in any provider.
Every accepted form has equal degree on both sides. So the only shapes these overloads add
beyond today are shapes the database will reject.

The column-count concern was raised on the issue and answered with "let people write it,
run it, and if it fails, fix it, possibly with an Analyzer". That is a reasonable stance for
a *mismatch a user could plausibly intend to write correctly*. It is a weaker one for an API
where **every** new overload is degenerate by construction.

The `ReferencedBy(...)` half is a genuinely separate and useful idea — declaring the
inverse — but it is orthogonal to composite support and should be decided on its own merits.

### Option B — a table-level clause (proposed by @tom42 on #2090)

```csharp
Create.Table("Area")
  .WithColumn("ArticleId").AsString().NotNullable()
  .WithColumn("AreaGroupIndex").AsInt32().NotNullable()
  .WithColumn("Index").AsInt32().NotNullable()
  .WithForeignKey()
    .ForeignColumns("ArticleId", "AreaGroupIndex")
    .ToTable("AreaGroup").PrimaryColumns("ArticleId", "Index")
    .OnDelete(Rule.Cascade);
```

This mirrors SQL itself: a table-level constraint is a peer of the column list, not a
property of one column. It also reuses the vocabulary of `Create.ForeignKey` —
`ForeignColumns` / `ToTable` / `PrimaryColumns` / `OnDelete` — so there is one set of names
for one concept, and the existing `ForeignKeyDefinition` model carries it unchanged.

Costs: a new branch in the `Create.Table` fluent interface chain, and `WithForeignKey()`
must return a syntax that can still chain back to `.WithColumn(...)` so constraints and
columns can be interleaved.

### Option C — Option B, plus a symmetric column-level convenience

Add Option B, and separately widen the column-level overload **symmetrically** for the case
where one column references a composite target — which is still invalid SQL — or leave the
column-level API alone.

There is no valid symmetric widening of a one-column call site, so in practice Option C
collapses into Option B.

## Decision

**Adopt Option B.** It is the only option that produces the SQL in #2090, it matches how SQL
models the construct, and it reuses the existing definition model and vocabulary.

Specifically:

1. Add `WithForeignKey()` to the `Create.Table` syntax chain, returning a builder with
   `ForeignColumns(params string[])`, `ToTable(string)` / `ToTable(string schema, string table)`,
   `PrimaryColumns(params string[])`, `OnDelete(Rule)`, `OnUpdate(Rule)`, `OnDeleteOrUpdate(Rule)`,
   and a `Named(string)` for an explicit constraint name.
2. Populate the existing `ForeignKeyDefinition` and attach it to `CreateTableExpression`
   as a new `ForeignKeys` collection, rather than inventing a parallel model.
3. Generate the clauses in `ColumnBase.Generate(IEnumerable<ColumnDefinition> columns, string tableName)` — the same method
   `SQLiteColumn` overrides to append its FK clauses — so every provider inherits the
   behaviour and SQLite's existing path becomes a special case of the general one.
4. Keep `$$IGNORE$$_` semantics for the column-level route so existing migrations are
   untouched.

**Do not adopt the `string[] primaryColumnNames` overloads** in their proposed form. Revisit
`ReferencedBy(...)` separately.

## Consequences

**Positive.** #2090 becomes expressible. SQLite gains composite foreign keys for the first
time, since inline is its only route. The `SQLiteColumn` FK-collection logic generalises
rather than staying a provider quirk. Snowflake and Redshift accept the syntax, so no
provider needs an exclusion.

**Negative.** The `Create.Table` interface chain grows a branch, which is the part of the
fluent API most sensitive to change — every `ICreateTableColumnOption*` interface that can
be followed by `WithForeignKey()` needs the new method, and those interfaces are public.

**Neutral.** Column-count mismatch between `ForeignColumns` and `PrimaryColumns` becomes
checkable, because both are known at the same call site. Whether to validate eagerly, defer
to the database, or add an analyzer is deliberately left open — with Option B the check is
at least *possible*, which it is not for Option A.

## Out of Scope

- **Adding a foreign key to an existing table on SQLite.** That needs the 12-step table
  rebuild (create replacement, copy, drop, rename). It is a distinct feature with real data
  risk and deserves its own ADR.
- **`ReferencedBy(...)`**, the inverse declaration.
- **`AlterTableExpression` support**, which has the same shape but different
  provider constraints.
- **Enforcement differences** on Snowflake and Redshift. FluentMigrator emits DDL; whether
  the engine enforces it is the engine's business.

## Validation Plan

If accepted, the implementation should carry:

- Generator unit tests per provider asserting the emitted `CREATE TABLE`, including the
  #2090 example verbatim.
- A SQLite test proving a composite FK now round-trips, since SQLite is where the gap bites.
- A test that column-level `ForeignKey(...)` output is byte-for-byte unchanged, so existing
  migrations are demonstrably unaffected.
- Integration coverage on at least one enforcing provider (SQL Server or PostgreSQL) that a
  violating insert is actually rejected — syntax acceptance alone does not prove the
  constraint landed.

## References

- [#2090](https://github.com/fluentmigrator/fluentmigrator/issues/2090) — the request, and both proposed syntaxes
- [#1747](https://github.com/fluentmigrator/fluentmigrator/issues/1747) — related SQLite primary key report
- [#1784](https://github.com/fluentmigrator/fluentmigrator/issues/1784), [#2117](https://github.com/fluentmigrator/fluentmigrator/issues/2117) — the SQLite `Create.ForeignKey` reports this feature would give a real answer to
- `src/FluentMigrator.Runner.SQLite/Generators/SQLite/SQLiteColumn.cs` — the existing inline FK generation to generalise
- `src/FluentMigrator.Abstractions/Builders/IColumnOptionSyntax.cs` — the current column-level surface
