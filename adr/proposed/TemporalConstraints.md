# ADR: Temporal Primary Keys, Unique Constraints and Foreign Keys (PostgreSQL 18+)

## Status
Proposed

Written for [#2453](https://github.com/fluentmigrator/fluentmigrator/issues/2453). No implementation is proposed here; the intent is to settle the syntax first.

## Context

PostgreSQL 18 added temporal constraints: keys and foreign keys over a range of time
rather than a single value. A temporal key says "no two rows for the same `Id` may have
overlapping validity periods"; a temporal foreign key says "for its whole validity period,
this row's parent must exist".

### The SQL

```sql
CREATE EXTENSION IF NOT EXISTS btree_gist;

CREATE TABLE department (
  id       integer   NOT NULL,
  valid_at daterange NOT NULL,
  name     text      NOT NULL,
  CONSTRAINT department_pk PRIMARY KEY (id, valid_at WITHOUT OVERLAPS)
);

CREATE TABLE employee (
  id            integer   NOT NULL,
  valid_at      daterange NOT NULL,
  department_id integer   NOT NULL,
  CONSTRAINT employee_pk PRIMARY KEY (id, valid_at WITHOUT OVERLAPS),
  CONSTRAINT employee_department_fk
    FOREIGN KEY (department_id, PERIOD valid_at)
    REFERENCES department (id, PERIOD valid_at)
);
```

There are two new clauses:

- **`WITHOUT OVERLAPS`** on the last column of a `PRIMARY KEY` or `UNIQUE` constraint.
  The other columns are compared for equality, and the last one for overlap. The docs
  describe `UNIQUE (id, valid_at WITHOUT OVERLAPS)` as behaving like
  `EXCLUDE USING GIST (id WITH =, valid_at WITH &&)`.
- **`PERIOD`** on the last column of both sides of a `FOREIGN KEY`. The non-`PERIOD`
  columns are compared for equality. The `PERIOD` column passes when the matching
  referenced rows' combined periods "completely cover the referencing record's".

### Rules the server enforces

From the [CREATE TABLE reference](https://www.postgresql.org/docs/18/sql-createtable.html#SQL-CREATETABLE-PARMS-UNIQUE),
checked against the `REL_18_STABLE` source and its `without_overlaps` regression test:

| Rule | Server error when broken |
|---|---|
| Table-constraint form only. The column-constraint grammar has no `WITHOUT OVERLAPS` and no `PERIOD` | syntax error |
| `WITHOUT OVERLAPS` and `PERIOD` go on the **last** column | syntax error |
| A temporal key needs at least two columns | `constraint using WITHOUT OVERLAPS needs at least two columns` |
| The `WITHOUT OVERLAPS` / `PERIOD` column must be a range or multirange type | `column "valid_at" in WITHOUT OVERLAPS is not a range or multirange type` |
| Empty ranges are rejected | `empty WITHOUT OVERLAPS value found in column ...` |
| The key is backed by a GiST index, so scalar columns such as `integer` need the `btree_gist` extension, which the docs call "the expected way to use this feature" | `data type integer has no default operator class for access method "gist"` |
| A `PERIOD` foreign key must reference a key declared `WITHOUT OVERLAPS`, and must use `PERIOD` on both sides | `foreign key must use PERIOD when referencing a primary key using WITHOUT OVERLAPS` and related messages |
| A `PERIOD` foreign key supports only `NO ACTION`: `RESTRICT`, `CASCADE`, `SET NULL` and `SET DEFAULT` are rejected for both `ON DELETE` and `ON UPDATE` | `unsupported ON DELETE action for foreign key constraint using PERIOD` |
| `ADD CONSTRAINT ... USING INDEX` cannot create a temporal key | `"..." is not a unique index` |

`pg_get_constraintdef` prints the constraints back as `PRIMARY KEY (id, valid_at WITHOUT OVERLAPS)`
and `FOREIGN KEY (parent_id, PERIOD valid_at) REFERENCES temporal_rng(id, PERIOD valid_at)`.
`pg_constraint.conperiod` is `true` for both.

History: the feature was committed during the PostgreSQL 17 cycle (`46a0cd4cef`, January 2024),
reverted before 17 shipped (`8aee330af5`, May 2024), and recommitted for 18 (`fc0438b4e8`,
September 2024). PostgreSQL 18 is the first release with it.

### What FluentMigrator can express today

None of it. Every route ends at a clause FluentMigrator has no way to emit:

| Want | Route today | Gap |
|---|---|---|
| Temporal PK in `Create.Table` | `.PrimaryKey()` on each column, rendered as a table-level `PRIMARY KEY (...)` by `PostgresColumn.AddPrimaryKeyConstraint` | No way to mark a column `WITHOUT OVERLAPS`, and no guarantee it is last |
| Temporal PK or UNIQUE after the fact | `Create.PrimaryKey(...)` / `Create.UniqueConstraint(...)` | `ConstraintDefinition.Columns` is a `HashSet<string>`, so column order is not part of the contract, and there is no `WITHOUT OVERLAPS` |
| Temporal FK | `Create.ForeignKey(...)` | No `PERIOD`. `ForeignKeyDefinition` does not implement `ISupportAdditionalFeatures`, so even an extension method has nowhere to put it |
| Range column | `.AsCustom("daterange")` | Works; no typed method, but none is needed for this feature |

The workaround is `Execute.Sql`, which leaves the constraints invisible to anything that
reads the expression model, and makes `AutoReversingMigration` unable to reverse them.

### Other databases

MariaDB (10.5+) and Db2 also have `WITHOUT OVERLAPS` keys, but over a declared
application-time `PERIOD`, which is a pair of start and end columns:
`UNIQUE (id, p WITHOUT OVERLAPS)` in MariaDB, and `PRIMARY KEY (id, BUSINESS_TIME WITHOUT OVERLAPS)` in Db2.
FluentMigrator cannot declare a `PERIOD` today. SQL Server's temporal tables are
system-versioned (`SYSTEM_TIME`), which is a different feature. The SQL:2011 `PERIOD FOR`
declaration that MariaDB and Db2 build on is not in PostgreSQL 18.

## Options Considered

### Option A — `Execute.Sql` only (status quo, documented)

Document the SQL and stop there.

Cheap, but it leaves the constraints out of the expression model. `Create.ForeignKey` would
have the same gap the day someone wants a temporal one in a tool that reads migrations, such
as the [source-controlled database model](SourceControlledDatabaseModel.md).

### Option B — a provider-agnostic API in FluentMigrator.Abstractions

Put `WithoutOverlaps(...)` and `Period(...)` on the core syntax interfaces, as was done for
`NullsNotDistinct()` in `CreateIndexNullSemanticsExtensions`.

The difference from `NullsNotDistinct()` is that the argument does not mean the same thing
across databases. In PostgreSQL it is one range column; in MariaDB and Db2 it is the name of
a `PERIOD` over two columns, which FluentMigrator would first need a way to declare. A
portable signature chosen now would be designed against one implementation, and the other two
would have to fit it later.

### Option C — PostgreSQL extension methods, on the existing builders

Add extension methods to `FluentMigrator.Extensions.Postgres`, as the package already does for
`Include(...)`, `UsingGist()`, `AsConcurrently()` and the identity options. Store the choice
in `AdditionalFeatures` under a `PostgresExtensions` key, and render it in a new
`Postgres18_0Generator`.

Costs: `ForeignKeyDefinition` and `CreateForeignKeyExpressionBuilder` have to support
additional features, which is a small change to the core model (see Decision, point 4). The
extension methods also need a way to add a column to a constraint from
`FluentMigrator.Extensions.Postgres`, which references only `FluentMigrator.Abstractions` and so
cannot see the builder class.

### Option D — a dedicated `Create.TemporalPrimaryKey(...)` / `Create.TemporalForeignKey(...)`

New entry points with their own syntax chains.

This duplicates the existing chains (`OnTable`, `WithSchema`, `Columns`, `FromTable`,
`ToTable`, `PrimaryColumns`, `OnDelete`) for what is one extra clause per constraint, and
it can't cover the `Create.Table` route.

## Decision

**Adopt Option C.** Temporal constraints are a PostgreSQL feature with PostgreSQL semantics,
and the Postgres extension package is where FluentMigrator puts those. It reuses every
existing chain, adds one method per constraint kind, and keeps the core API free of a
portable signature that MariaDB and Db2 could not yet use. If FluentMigrator later learns to
declare a `PERIOD`, a portable API can map onto the same feature keys, in the same way
`NullsNotDistinct()` moved from the Postgres package to Abstractions.

### Proposed syntax

```csharp
Execute.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist");

// Temporal primary key inside CREATE TABLE
Create.Table("department")
    .WithColumn("id").AsInt32().NotNullable().PrimaryKey()
    .WithColumn("valid_at").AsCustom("daterange").NotNullable().PrimaryKey().WithoutOverlaps()
    .WithColumn("name").AsString().NotNullable();

Create.Table("employee")
    .WithColumn("id").AsInt32().NotNullable()
    .WithColumn("valid_at").AsCustom("daterange").NotNullable()
    .WithColumn("department_id").AsInt32().NotNullable();

// Temporal primary key after the fact (Create.UniqueConstraint works the same way)
Create.PrimaryKey("employee_pk").OnTable("employee")
    .Columns("id").WithoutOverlaps("valid_at");

// Temporal foreign key
Create.ForeignKey("employee_department_fk")
    .FromTable("employee").ForeignColumn("department_id")
    .ToTable("department").PrimaryColumn("id")
    .Period("valid_at");                    // same column name on both sides
//  .Period("valid_at", "dept_valid_at");   // foreign side, then primary side
```

`Period(...)` returns `ICreateForeignKeyCascadeSyntax`, like `OnDelete(...)` and `OnUpdate(...)`,
so it can come before or after them. `WithoutOverlaps(...)` takes the column name, rather than
marking the last of `Columns(...)`, because `ConstraintDefinition.Columns` does not guarantee
order.

### Specifically

1. **Column.** `WithoutOverlaps()` on `IColumnOptionSyntax<TNext, TNextFk>` sets a
   `PostgresExtensions.WithoutOverlaps` feature on the `ColumnDefinition`, found through
   `IColumnExpressionBuilder` as the identity extensions already do. It throws
   `InvalidOperationException` if the column is not marked `PrimaryKey()`. Column-level
   `.Unique()` creates an index, not a constraint, so it is not covered: temporal `UNIQUE`
   goes through `Create.UniqueConstraint`.
2. **Constraint.** `WithoutOverlaps(string columnName)` on `ICreateConstraintOptionsSyntax`
   adds the column to `ConstraintDefinition.Columns`, so anything reading the model sees the
   whole key, and records its name under `PostgresExtensions.WithoutOverlaps` in the
   constraint's `AdditionalFeatures`. To add the column, the builder implements a small new
   Abstractions interface that exposes its `ConstraintDefinition`, mirroring
   `IColumnExpressionBuilder`.
3. **Foreign key.** `Period(string column)` and `Period(string foreignColumn, string primaryColumn)`
   on `ICreateForeignKeyCascadeSyntax` record both names under
   `PostgresExtensions.ForeignKeyPeriod`. They are **not** added to
   `ForeignColumns`/`PrimaryColumns`, because the generator has to place them after the
   equality columns with a keyword, and the existing `PrimaryColumns.Count == ForeignColumns.Count`
   check stays true either way.
4. **Model.** `ForeignKeyDefinition` implements `ISupportAdditionalFeatures`, and `Clone()`
   copies the features, as `ConstraintDefinition` already does.
   `CreateForeignKeyExpressionBuilder` implements `ISupportAdditionalFeatures` by forwarding to
   it. Nothing else reads the dictionary, so existing foreign keys are unchanged.
5. **Generator.** Add `Postgres18_0Generator`, `Postgres18_0Processor`, `AddPostgres18_0()`
   and the `PostgreSQL18_0` ids, following `Postgres15_0`:
   - its column class overrides `AddPrimaryKeyConstraint`, to emit the flagged column last
     with `WITHOUT OVERLAPS` in `CREATE TABLE`;
   - `Generate(CreateConstraintExpression)` emits the equality columns in their existing
     order, then the `WITHOUT OVERLAPS` column;
   - `Generate(CreateForeignKeyExpression)` appends `, PERIOD x` to both column lists.
6. **Older generators reject it, in every compatibility mode.** On `PostgresGenerator`
   through `Postgres15_0Generator`, any of the three features throws
   `DatabaseOperationNotSupportedException` naming PostgreSQL 18. The loose compatibility mode
   is not used, because dropping the clause would not degrade gracefully. It would create a
   B-tree key that allows overlapping periods, or a foreign key that matches ranges exactly,
   and both would be accepted silently.
7. **Validation the generator can do without a database.** `Postgres18_0Generator` throws
   when a temporal key has fewer than two columns, and when a `Period(...)` foreign key has an
   `OnDelete` or `OnUpdate` other than `Rule.None`. The server would reject both. Failing in
   the generator also covers `--preview` and connectionless runs, which never reach a server.
   Range types and `btree_gist` are left to the server, because the generator cannot see
   column types in an `ALTER TABLE`.

Reversal needs nothing new. `Delete.PrimaryKey`, `Delete.UniqueConstraint` and
`Delete.ForeignKey` drop by name, so `AutoReversingMigration` reverses `Create.PrimaryKey(...)`
and `Create.ForeignKey(...)` with the temporal clauses as it does without them.

## Consequences

**Positive.**
- The SQL in the Context section becomes expressible without `Execute.Sql`.
- The constraints stay in the expression model, so preview output, auto-reversal and
  model-reading tools see them.
- The opt-in is explicit, so no existing migration's output changes.

**Negative.**
- `ForeignKeyDefinition` gains `AdditionalFeatures`. This is additive, but it is a public
  model type that third-party generators may subclass.
- The constraint builder exposes its definition through a new interface.
- Users must create `btree_gist` themselves before using scalar key columns. FluentMigrator
  does not create extensions implicitly: doing so needs privileges the migration user may
  not have.

**Neutral.**
- One more versioned Postgres generator. Which generator the unversioned `Postgres` id
  resolves to is governed by the existing alias rules and is not changed by this ADR.

## Out of Scope

- **`UPDATE` / `DELETE ... FOR PORTION OF`.** It is not in PostgreSQL 18. It was committed
  upstream and reverted again (`a4b26b8f7c`, September 2026), and is in no released version.
  It is data manipulation, not schema, in any case.
- **SQL:2011 `PERIOD FOR` declarations**, and therefore the MariaDB and Db2 forms of
  `WITHOUT OVERLAPS`. Revisit together if FluentMigrator gains a way to declare a period.
- **Typed range columns** (`AsDateRange()`, `AsTimestampTzRange()`, ...). `AsCustom(...)`
  covers this feature; typed range support is separate.
- **Column-level `.ForeignKey(...)` with `PERIOD`.** A temporal foreign key has at least two
  columns, so it cannot hang off one column. An inline temporal foreign key in
  `Create.Table` belongs with [table-level foreign keys](TableLevelForeignKeys.md); if that
  ADR is implemented, `Period(...)` should apply to its builder too.
- **General `EXCLUDE` constraints.** `WITHOUT OVERLAPS` is a special case of one, but a
  general `EXCLUDE USING gist (...)` builder is a larger feature.
- **`Schema.Table(...).Constraint(...).Exists()` and similar checks.** They are unaffected.
  If they ever need to tell temporal keys apart, `pg_constraint.conperiod` is the column.

## Validation Plan

If accepted, the implementation should carry:

- **`Postgres18_0Generator` unit tests** for each route:
  - `Create.Table` with a temporal primary key;
  - `Create.PrimaryKey` and `Create.UniqueConstraint` with `WithoutOverlaps(...)`;
  - `Create.ForeignKey` with `Period(column)` and with `Period(foreign, primary)`;
  - a test that the `WITHOUT OVERLAPS` column is emitted last even when it is declared first.
- **Rejection tests:**
  - every older Postgres generator throws, in strict and loose compatibility mode alike;
  - a temporal key with fewer than two columns throws;
  - `Period(...)` combined with each `Rule` other than `None` throws.
- **Unchanged-output tests:** primary keys, unique constraints and foreign keys without the
  new features produce byte-for-byte the same SQL on every Postgres generator.
- **A `ForeignKeyDefinition.Clone()` test** that the features are copied.
- **Integration on PostgreSQL 18:**
  - an overlapping insert is rejected by the temporal key;
  - a referencing row whose period is not covered is rejected by the foreign key.

  The CI Postgres container is `registry.gitlab.com/dalibo/postgresql_anonymizer:stable`.
  Whether its PostgreSQL major version is 18, and whether it ships `btree_gist`, has to be
  checked. If it doesn't qualify, a PostgreSQL 18 container is needed for these tests.

## References

- [#2453](https://github.com/fluentmigrator/fluentmigrator/issues/2453) — the request
- [PostgreSQL 18: CREATE TABLE, `UNIQUE` / `PRIMARY KEY` / `REFERENCES`](https://www.postgresql.org/docs/18/sql-createtable.html#SQL-CREATETABLE-PARMS-UNIQUE)
- [PostgreSQL 18 release notes, Constraints](https://www.postgresql.org/docs/18/release-18.html#RELEASE-18-CONSTRAINTS)
- [`pg_constraint.conperiod`](https://www.postgresql.org/docs/18/catalog-pg-constraint.html)
- PostgreSQL `REL_18_STABLE`: `src/test/regress/expected/without_overlaps.out` (error messages and `pg_get_constraintdef` output quoted above), `src/backend/commands/tablecmds.c` (referential-action check)
- `src/FluentMigrator.Extensions.Postgres/Postgres/PostgresExtensions.cs` — feature keys and the `IColumnExpressionBuilder` pattern
- `src/FluentMigrator.Runner.Postgres/Generators/Postgres/Postgres15_0Generator.cs` — the versioned-generator pattern to follow
- `src/FluentMigrator.Abstractions/Model/ForeignKeyDefinition.cs`, `ConstraintDefinition.cs` — the models involved
- [Table-level foreign keys ADR](TableLevelForeignKeys.md) — where an inline temporal foreign key would go
