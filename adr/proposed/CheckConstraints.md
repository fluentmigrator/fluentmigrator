# ADR: Create.Check / Alter.Check / Delete.Check Top-Level Syntax

## Status
Proposed

Written for [#2458](https://github.com/fluentmigrator/fluentmigrator/issues/2458), as the
cross-provider design step for [#1021](https://github.com/fluentmigrator/fluentmigrator/issues/1021).
PR [#2457](https://github.com/fluentmigrator/fluentmigrator/pull/2457) by @ScottishTapWater adds
a PostgreSQL-only `Create.CheckConstraint(...)` extension. This ADR sets out what every
supported provider can do, so that the feature can be designed once, in the core API, rather
than once per provider.

No implementation is proposed here; the intent is to settle the syntax and the per-provider
behavior first.

## Context

ANSI SQL defines `CHECK` constraints as table constraints. They can be declared inside
`CREATE TABLE`, added to an existing table, and dropped:

```sql
ALTER TABLE SchemaName.TableName ADD CONSTRAINT ConstraintName CHECK (Quantity > 0);
ALTER TABLE SchemaName.TableName DROP CONSTRAINT ConstraintName;
```

ANSI SQL has **no** statement that changes the condition of an existing check constraint. To
change it, you drop the constraint and add it again.

Several databases add two more operations on top of the standard:

1. **Not validating existing rows** when the constraint is added. The constraint applies to
   rows inserted or updated afterwards, and rows already in the table are not checked.
   SQL Server calls this `WITH NOCHECK`, PostgreSQL `NOT VALID`, and Oracle `NOVALIDATE`.
2. **Disabling and enabling** a constraint without dropping it. SQL Server does this with
   `ALTER TABLE ... NOCHECK CONSTRAINT` / `CHECK CONSTRAINT`, Oracle with `DISABLE` /
   `ENABLE`, and MySQL and Db2 with `NOT ENFORCED` / `ENFORCED`:

```sql
ALTER TABLE SchemaName.TableName NOCHECK CONSTRAINT ConstraintName;
```

These are two separate settings. In this ADR, **validated** means that the existing rows were
checked, and **enforced** (or enabled) means that new and changed rows are checked. SQL
Server, for example, can have a constraint that is enabled but not validated: it was added
`WITH NOCHECK`, so `sys.check_constraints.is_not_trusted = 1`.

### Current state

FluentMigrator has no API for check constraints:

- `ConstraintType` (`src/FluentMigrator.Abstractions/Model/ConstraintType.cs`) has only
  `PrimaryKey` and `Unique`.
- `GenericGenerator.Generate(CreateConstraintExpression)` writes
  `IsPrimaryKeyConstraint ? "PRIMARY KEY" : "UNIQUE"` followed by a column list, and the
  provider generators make the same assumption. A new `ConstraintType.Check` member would
  therefore not fail. Every existing generator, including third-party ones, would emit
  `ADD CONSTRAINT ... UNIQUE (...)` for it, which is wrong SQL. So check constraints need
  their own expressions and cannot reuse `CreateConstraintExpression`.
- `Delete.UniqueConstraint(name)` drops a check constraint by name on the providers that
  use `DROP CONSTRAINT` for both, but this only works by accident. On MySQL it emits
  `DROP INDEX`, and on SQLite it emits `DROP INDEX` or rejects the operation.
- Users write raw SQL with `Execute.Sql(...)`, as described in
  [#1021](https://github.com/fluentmigrator/fluentmigrator/issues/1021#issuecomment-487993268).
  This does not get the runner's quoting or default schema, and it cannot be reversed
  automatically.

`Schema.Table(t).Constraint(c).Exists()` already finds check constraints on every provider
except SQLite. These providers query catalogs that include `CHECK` constraints:
`INFORMATION_SCHEMA.TABLE_CONSTRAINTS` (SQL Server, PostgreSQL, MySQL, Snowflake, Db2 for i),
`SYSCAT.TABCONST` (Db2 LUW), `ALL_CONSTRAINTS` (Oracle), `rdb$relation_constraints`
(Firebird), and `CONSTRAINTS` (SAP HANA). SQLite looks only for unique indexes.

## Provider Support

### Capability matrix

| Provider | Add to existing table | Add without validating existing rows | Add as not enforced | Disable / enable later | Drop | Transactional DDL |
|---|---|---|---|---|---|---|
| **SQL Server** (2000–2016+) | `ADD CONSTRAINT c CHECK (...)` | `WITH NOCHECK ADD ...` | `WITH NOCHECK ADD ...`, then `NOCHECK CONSTRAINT c` | `NOCHECK CONSTRAINT c` / `[WITH CHECK] CHECK CONSTRAINT c` | `DROP CONSTRAINT c` (`IF EXISTS` from 2016) | Yes |
| **PostgreSQL** (9.2+) | `ADD CONSTRAINT c CHECK (...)` | `... NOT VALID`, later `VALIDATE CONSTRAINT c` | Only from PostgreSQL 18 (`NOT ENFORCED`) | **No** (see notes) | `DROP CONSTRAINT [IF EXISTS] c` | Yes |
| **MySQL** 8.0.16+ (`MySql8`) | `ADD CONSTRAINT c CHECK (...)` | **No** | `... NOT ENFORCED` | `ALTER CHECK c [NOT] ENFORCED` | `DROP CHECK c` (`DROP CONSTRAINT c` from 8.0.19) | No |
| **MySQL** < 8.0.16 (`MySql4`, `MySql5`) | Parsed and **ignored** | — | — | — | — | No |
| **MariaDB** 10.2.1+ | `ADD CONSTRAINT c CHECK (...)` | **No** | **No** | **No** (only the session-wide `check_constraint_checks`) | `DROP CONSTRAINT [IF EXISTS] c` | No |
| **Oracle** | `ADD CONSTRAINT c CHECK (...)` | `... ENABLE NOVALIDATE` | `... DISABLE` | `MODIFY CONSTRAINT c ENABLE [NO]VALIDATE` / `DISABLE` | `DROP CONSTRAINT c` | No (implicit commit) |
| **SQLite** | **No** (only inside `CREATE TABLE`) | — | — | **No** (only the connection-wide `PRAGMA ignore_check_constraints`) | **No** | n/a |
| **Firebird** | `ADD CONSTRAINT c CHECK (...)` | **Always** (existing rows are never validated) | **No** | **No** | `DROP CONSTRAINT c` | Yes, but `AddFirebird()` commits after every statement by default |
| **Db2 LUW** | `ADD CONSTRAINT c CHECK (...)` | Only through `SET INTEGRITY` (see notes) | `... NOT ENFORCED` | `ALTER CHECK c [NOT] ENFORCED` | `DROP CHECK c` / `DROP CONSTRAINT c` | Yes |
| **Db2 for i** | `ADD CONSTRAINT c CHECK (...)` | **No** | **No** | Not in SQL (CL command `CHGPFCST`) | `DROP CHECK c` / `DROP CONSTRAINT c` | Yes |
| **SAP HANA** | `ADD CONSTRAINT c CHECK (...)` | To be verified | To be verified | To be verified (see notes) | `DROP CONSTRAINT c` | DDL auto-commits by default |
| **Snowflake** | **No** — `CHECK` is not a supported constraint type | — | — | — | — | — |
| **Redshift** | **No** — `CHECK` is not a supported constraint type | — | — | — | — | — |

Summary:

- **Create and Delete** are supported by SQL Server, PostgreSQL, MySQL 8, MariaDB, Oracle,
  Firebird, Db2 LUW, Db2 for i, and SAP HANA. They are not available on SQLite (outside
  `CREATE TABLE`), Snowflake, Redshift, or MySQL before 8.0.16. MariaDB also needs generator
  work in FluentMigrator before it can be supported (see [MariaDB](#mariadb)).
- **Disable/enable** works natively on SQL Server, Oracle, MySQL 8 and Db2 LUW only.
- **Not validating existing rows** works natively on SQL Server, PostgreSQL and Oracle. Firebird
  always skips the existing rows, whether or not that was requested.

### Detailed provider analysis

#### SQL Server
- **Add**: `ALTER TABLE [s].[t] [WITH CHECK | WITH NOCHECK] ADD CONSTRAINT [c] CHECK (expr)`.
  `WITH CHECK` is the default for a new constraint.
- **Disable**: `ALTER TABLE [s].[t] NOCHECK CONSTRAINT [c]`. This sets `is_disabled = 1` and
  `is_not_trusted = 1`.
- **Enable**: `ALTER TABLE [s].[t] WITH CHECK CHECK CONSTRAINT [c]` checks the existing rows
  again, so the constraint is trusted afterwards. A plain `ALTER TABLE [s].[t] CHECK CONSTRAINT [c]`
  enables the constraint but leaves it untrusted, and the query optimizer ignores untrusted
  constraints.
- **Drop**: `ALTER TABLE [s].[t] DROP CONSTRAINT [c]`. SQL Server 2016 adds `DROP CONSTRAINT IF EXISTS`.
- **Catalog**: `sys.check_constraints` (`definition`, `is_disabled`, `is_not_trusted`).
- **Other**: `NOT FOR REPLICATION` is SQL Server-specific. If it is supported, it should be an
  option in `FluentMigrator.Extensions.SqlServer`, not in the core syntax. Constraint names are
  unique within the schema, not only within the table. The condition cannot contain
  subqueries.

#### PostgreSQL
- **Add**: `ALTER TABLE "s"."t" ADD CONSTRAINT "c" CHECK (expr) [NO INHERIT] [NOT VALID]`.
  `NOT VALID` and `NO INHERIT` for check constraints were added in 9.2.
- **Generator hierarchy**: `Postgres92Generator` and `Postgres10_0Generator` both derive from
  `PostgresGenerator`, and `Postgres11_0Generator` and `Postgres15_0Generator` (which
  `AddPostgres()` uses) derive from `Postgres10_0Generator`. Support added only to
  `Postgres92Generator` would therefore never reach the default generator. This ADR recommends
  putting it in `PostgresGenerator`. PostgreSQL 9.1 reached end of life in 2016, and a
  pre-9.2 server rejects `NOT VALID` with a clear syntax error. The alternative is a protected
  virtual property that `Postgres92Generator` and `Postgres10_0Generator` both override.
- **Validate**: `ALTER TABLE "s"."t" VALIDATE CONSTRAINT "c"`. A `NOT VALID` constraint is
  still enforced for new and updated rows.
- **Disable**: Versions before 18 cannot disable a check constraint. PostgreSQL 18 allows a
  check constraint to be created `NOT ENFORCED`. Whether 18 can change an *existing* check
  constraint with `ALTER CONSTRAINT ... [NOT] ENFORCED` needs to be verified before
  implementation: the release notes describe this for foreign keys. FluentMigrator has no
  PostgreSQL 18 generator yet.
- **Drop**: `ALTER TABLE "s"."t" DROP CONSTRAINT [IF EXISTS] "c"`.
- **Catalog**: `pg_constraint` (`contype = 'c'`, `convalidated`).
- **Other**: Constraint names are unique per table. The condition cannot contain subqueries
  and should be immutable.

#### MySQL
- **Before 8.0.16** (the `MySql4` and `MySql5` generators target these versions), `CHECK` is
  parsed and silently ignored by every storage engine. Emitting it would give users a
  constraint that does nothing, so these generators should report the operation through
  `CompatibilityMode`.
- **8.0.16 and later** (`MySql8`):
  - Add: `ALTER TABLE t ADD CONSTRAINT c CHECK (expr) [NOT] ENFORCED`. An enforced
    constraint is checked against the existing rows when it is added, and the statement fails
    if a row violates it. MySQL cannot add an enforced constraint without checking the
    existing rows.
  - Enable/disable: `ALTER TABLE t ALTER CHECK c [NOT] ENFORCED` (8.0.16). `ALTER CONSTRAINT`
    works from 8.0.19.
  - Drop: `ALTER TABLE t DROP CHECK c`. `DROP CONSTRAINT` works from 8.0.19. Use
    `DROP CONSTRAINT`: `MySql8Generator` also answers to the `MariaDB` generator id, and MariaDB
    accepts `DROP CONSTRAINT` but rejects `DROP CHECK`. The cost is MySQL 8.0.16 to 8.0.18,
    which reject `DROP CONSTRAINT`. Those releases are long superseded within 8.0, which itself
    reached end of life in April 2026.
  - Catalog: `INFORMATION_SCHEMA.CHECK_CONSTRAINTS`, and the `ENFORCED` column of
    `INFORMATION_SCHEMA.TABLE_CONSTRAINTS`.
  - Other: Check constraint names are unique per **schema**, not per table. The condition
    cannot reference `AUTO_INCREMENT` columns or use non-deterministic functions or subqueries.
    A column used in a foreign key referential action (`ON UPDATE CASCADE`, ...) cannot be
    referenced.
- DDL is not transactional.

#### MariaDB
- Check constraints are enforced from 10.2.1.
- **Add**: `ALTER TABLE t ADD CONSTRAINT c CHECK (expr)`. The existing rows are checked.
- **Disable**: Individual constraints cannot be disabled. `SET check_constraint_checks = 0`
  turns off all check constraints for the session. That is a session setting, not DDL, so it
  must not be used to implement `Disabled()`.
- **Drop**: `ALTER TABLE t DROP CONSTRAINT [IF EXISTS] c`.
- **Generator**: FluentMigrator has no MariaDB generator. `MariaDB` is an alias on the
  MySQL 4, 5 and 8 processors, and a generator id of `MySql5Generator` and `MySql8Generator`,
  so which MySQL generator a MariaDB migration gets depends on how the runner was configured.
  MariaDB and MySQL need different SQL: MySQL 5.x ignores `CHECK` and rejects
  `DROP CONSTRAINT`, and MySQL 8 supports `NOT ENFORCED` and `DROP CHECK`, which MariaDB
  does not. Supporting MariaDB properly needs a dedicated MariaDB generator and id. Until
  then, the MySQL generators cannot tell MariaDB apart, so MariaDB gets whatever the selected
  MySQL generator emits. With `MySql8Generator`, `Create.Check`, `Delete.Check` and changing the
  condition work on MariaDB, because they use `ADD CONSTRAINT` and `DROP CONSTRAINT`. `Disabled()`
  and `Enabled()` emit `NOT ENFORCED` and `ALTER CHECK`, which MariaDB rejects with a syntax
  error. That fails loudly rather than silently, and should be documented.
- DDL is not transactional.

#### Oracle
- **Add**: `ALTER TABLE "s"."t" ADD CONSTRAINT "c" CHECK (expr) [ENABLE | DISABLE] [VALIDATE | NOVALIDATE]`.
  The default is `ENABLE VALIDATE`.
- **Enable/disable**: `ALTER TABLE "s"."t" MODIFY CONSTRAINT "c" ENABLE [VALIDATE | NOVALIDATE]`
  and `... MODIFY CONSTRAINT "c" DISABLE`. `DISABLE` means `DISABLE NOVALIDATE`, so a disabled
  constraint is also not validated. `ENABLE NOVALIDATE` marks a constraint `NOT VALIDATED` even
  if it was validated before, and the optimizer then stops relying on it.
- **Drop**: `ALTER TABLE "s"."t" DROP CONSTRAINT "c"`.
- **Catalog**: `ALL_CONSTRAINTS` (`CONSTRAINT_TYPE = 'C'`, `STATUS`, `VALIDATED`,
  `SEARCH_CONDITION`). `NOT NULL` columns are also stored there as type `C` constraints.
- **Other**: Constraint names are unique per schema. They are limited to 30 bytes before
  12.2. The condition cannot use subqueries, `SYSDATE`, `USER`, `USERENV`, `ROWNUM`, or
  sequence `CURRVAL`/`NEXTVAL`. `ALTER TABLE ... RENAME CONSTRAINT` is available.
- DDL commits implicitly, so it is not transactional.

#### SQLite
- `CHECK` can be declared only inside `CREATE TABLE`, as a column or table constraint. SQLite
  has no `ALTER TABLE ... ADD CONSTRAINT` or `DROP CONSTRAINT`. Adding, changing or removing a
  check on an existing table means rebuilding the table, which FluentMigrator does not do for
  any other operation either.
- `PRAGMA ignore_check_constraints = ON` turns off all checks for the connection. It is not a
  per-constraint setting.
- `Create.Check`, `Alter.Check` and `Delete.Check` go through `CompatibilityMode`. An inline
  `CHECK` inside `Create.Table` is a separate feature (see [Out of scope](#out-of-scope)).
  For SQLite, that is the only way to get a check constraint.

#### Firebird
- **Add**: `ALTER TABLE "t" ADD CONSTRAINT "c" CHECK (expr)`. **The existing rows are not
  checked**: the constraint applies only to rows inserted or updated afterwards. A migration
  that asks for the default (validate the existing rows) cannot get it on Firebird. This
  should be documented. Checking the rows first, for example with a generated `EXECUTE BLOCK`
  that raises an exception, is a possible later improvement.
- **Disable**: Not supported.
- **Drop**: `ALTER TABLE "t" DROP CONSTRAINT "c"`.
- **Other**: Check constraints are implemented with triggers, and the condition may contain
  subqueries. Constraint names are unique in the whole database. They are limited to 31
  characters before Firebird 4 and 63 characters from Firebird 4. There is no
  `RENAME CONSTRAINT`.
- DDL is transactional, but `AddFirebird()` uses `FirebirdOptions.AutoCommitBehaviour()` by
  default, so the FluentMigrator Firebird processor commits after every statement. Firebird
  needs metadata changes to be committed before later statements can use them.

#### Db2 LUW
- **Add**: `ALTER TABLE "s"."t" ADD CONSTRAINT "c" CHECK (expr) [ENFORCED | NOT ENFORCED] [ENABLE | DISABLE QUERY OPTIMIZATION]`.
  An enforced constraint is checked against the existing rows when it is added, and the
  statement fails if a row violates it. To skip that check, the table must first be put in
  set integrity pending state with `SET INTEGRITY FOR t OFF`. That affects the whole table, so
  `WithNoCheck()` should go through `CompatibilityMode` on Db2.
- **Enable/disable**: `ALTER TABLE "s"."t" ALTER CHECK "c" [NOT] ENFORCED`. Going back to
  `ENFORCED` checks the existing rows straight away, and the statement fails if a row violates
  the constraint:

  > altering the existing check or referential constraint from NOT ENFORCED to ENFORCED […]
  > causes the existing rows in the table to be immediately evaluated against the constraint.
  > If the verification fails, an error is returned (SQLSTATE 23512).
  >
  > — [Source](https://www.ibm.com/docs/en/db2/11.5.x?topic=statements-alter-table#:~:text=altering%20the%20existing%20check%20or%20referential%20constraint%20from%20NOT%20ENFORCED%20to%20ENFORCED,an%20error%20is%20returned%20%28SQLSTATE%2023512%29)

  So `Enabled()` emits `ALTER CHECK "c" ENFORCED` and means the same as on the other
  providers: enforced, with the existing rows validated. The one exception is a table that is
  already in set integrity pending state:

  > If a table is in set integrity pending state, adding a check or referential constraint, or
  > altering a constraint from NOT ENFORCED to ENFORCED does not immediately lead to the
  > enforcement of the constraint. Issue the SET INTEGRITY statement with the IMMEDIATE CHECKED
  > option to begin enforcing the constraint.
  >
  > — [Source](https://www.ibm.com/docs/en/db2/11.5.x?topic=statements-alter-table#:~:text=If%20a%20table%20is%20in%20set%20integrity%20pending%20state%2C%20adding%20a%20check,option%20to%20begin%20enforcing%20the%20constraint)

  FluentMigrator never puts a table in that state, because `WithNoCheck()` goes through
  `CompatibilityMode`. So the table is only in that state if something outside FluentMigrator put
  it there, and the documentation for `Enabled()` should mention this case. Because `ENFORCED`
  always checks the existing rows, `Enabled().WithNoCheck()` goes through `CompatibilityMode`.
- **Query optimization**: The default for a constraint is `ENABLE QUERY OPTIMIZATION`, which
  stays in effect after `NOT ENFORCED`, so the optimizer still assumes the rows satisfy the
  condition. After `Disabled()` they may not, and queries could return wrong results, for
  example `WHERE Quantity <= 0` returning no rows. So `Disabled()` must also emit
  `DISABLE QUERY OPTIMIZATION`, and `Enabled()` must emit `ENABLE QUERY OPTIMIZATION`.
  One `ALTER CHECK` clause takes both settings, for example
  `ALTER CHECK "c" NOT ENFORCED DISABLE QUERY OPTIMIZATION`. The constraint-alteration syntax
  diagram lets its options repeat, and its footnote rules out only giving the same option twice:

  > The same clause must not be specified more than once.
  >
  > — [Source](https://www.ibm.com/docs/en/db2/11.5.x?topic=statements-alter-table#:~:text=The%20same%20clause%20must%20not%20be%20specified%20more%20than%20once)
- **Trusted**: A `NOT ENFORCED` constraint is `TRUSTED` by default, which means the data is
  assumed to satisfy it. `NOT TRUSTED` would describe a disabled check better, but Db2 allows it
  only for foreign keys:

  > NOT TRUSTED can be specified only for foreign keys (SQLSTATE 42601).
  >
  > — [Source](https://www.ibm.com/docs/en/db2/11.5.x?topic=statements-alter-table#:~:text=NOT%20TRUSTED%20can%20be%20specified%20only%20for%20foreign%20keys)

  So `Disabled()` does not emit it. `DISABLE QUERY OPTIMIZATION` already stops the optimizer
  from relying on the constraint.
- **Drop**: `ALTER TABLE "s"."t" DROP CHECK "c"`, or `DROP CONSTRAINT "c"`.
- **Catalog**: `SYSCAT.CHECKS`, and the `ENFORCED` column of `SYSCAT.TABCONST`.
- **Other**: The condition cannot contain subqueries.

#### Db2 for i
- **Add**: `ALTER TABLE s.t ADD CONSTRAINT c CHECK (expr)`.
- **Disable**: There is no SQL statement for this. The CL command `CHGPFCST ... STATE(*DISABLED)`
  can disable a constraint, but a migration cannot run CL commands. The operation goes through
  `CompatibilityMode`.
- **Drop**: `ALTER TABLE s.t DROP CHECK c`, or `DROP CONSTRAINT c`.
- **Catalog**: `QSYS2.CHECK_CST`, and `INFORMATION_SCHEMA.TABLE_CONSTRAINTS`.

#### SAP HANA
- **Add**: `ALTER TABLE "s"."t" ADD CONSTRAINT "c" CHECK (expr)`.
- **Drop**: `ALTER TABLE "s"."t" DROP CONSTRAINT "c"`.
- **Enable/disable, validation**: HANA documents `ALTER TABLE ... ALTER CONSTRAINT ... {ENABLE | DISABLE} [VALIDATED | NOT VALIDATED]`
  for referential constraints. Whether this also works for `CHECK` constraints needs to be
  verified before implementation. Until then, these options go through `CompatibilityMode`.
- **Catalog**: `SYS.CONSTRAINTS` (`CHECK_CONDITION`).
- DDL auto-commits by default.

#### Snowflake
- Snowflake supports `PRIMARY KEY`, `UNIQUE`, `FOREIGN KEY` and `NOT NULL` constraints only.
  It does not support `CHECK`, so all three operations go through `CompatibilityMode`.

#### Redshift
- Redshift supports `PRIMARY KEY`, `UNIQUE` and `FOREIGN KEY` table constraints only, and it
  does not enforce them. It does not support `CHECK`, so all three operations go through
  `CompatibilityMode`.

## Proposed Syntax

The syntax uses the existing constraint vocabulary: `OnTable` for create and alter,
`FromTable` for delete, and `InSchema`. `Condition(sql)` takes the boolean expression and
writes it into the statement exactly as given, the way `Execute.Sql` does. It is not quoted
or rewritten. That keeps the syntax provider-neutral, but the expression is not: `ISJSON(x) = 1`
(SQL Server) or `code ~ '^[a-z]{3}$'` (PostgreSQL) work only on their own provider. A
migration that runs on several providers should put a non-portable condition inside
`IfDatabase(...)`.

### Create

```csharp
Create.Check("CK_Items_Quantity")
    .OnTable("Items").InSchema("inventory")
    .Condition("Quantity > 0");

// Leave existing rows unchecked: SQL Server WITH NOCHECK, PostgreSQL NOT VALID, Oracle ENABLE NOVALIDATE
Create.Check("CK_Items_Quantity")
    .OnTable("Items")
    .Condition("Quantity > 0")
    .WithNoCheck();

// Create the constraint without enforcing it: SQL Server NOCHECK CONSTRAINT, Oracle DISABLE, MySQL/Db2 NOT ENFORCED
Create.Check("CK_Items_Quantity")
    .OnTable("Items")
    .Condition("Quantity > 0")
    .Disabled();
```

### Alter

`Alter.Check` has two forms, as described in #2458:

```csharp
// 1. With Condition(): change the condition by dropping the constraint and adding it again
Alter.Check("CK_Items_Quantity")
    .OnTable("Items").InSchema("inventory")
    .Condition("Quantity >= 0");          // .WithNoCheck() and .Disabled() are also available

// 2. Without Condition(): change whether the constraint is enforced
Alter.Check("CK_Items_Quantity").OnTable("Items").Disabled();
Alter.Check("CK_Items_Quantity").OnTable("Items").Enabled();                 // check existing rows again
Alter.Check("CK_Items_Quantity").OnTable("Items").Enabled().WithNoCheck();   // enforce for new rows only
```

### Delete

```csharp
Delete.Check("CK_Items_Quantity").FromTable("Items").InSchema("inventory");
```

### Interface sketch

```csharp
// ICreateExpressionRoot
ICreateCheckOnTableSyntax Check(string constraintName);
// IAlterExpressionRoot
IAlterCheckOnTableSyntax Check(string constraintName);
// IDeleteExpressionRoot
IDeleteCheckFromTableSyntax Check(string constraintName);

public interface ICreateCheckOnTableSyntax : IFluentSyntax
{
    ICreateCheckInSchemaOrConditionSyntax OnTable(string tableName);
}

public interface ICreateCheckInSchemaOrConditionSyntax : ICreateCheckConditionSyntax
{
    ICreateCheckConditionSyntax InSchema(string schemaName);
}

public interface ICreateCheckConditionSyntax : IFluentSyntax
{
    ICreateCheckOptionsSyntax Condition(string sqlCondition);
}

// One option or none: Disabled() already leaves the existing rows unchecked, so the two
// cannot be combined.
public interface ICreateCheckOptionsSyntax : IFluentSyntax
{
    void WithNoCheck();
    void Disabled();
}

public interface IAlterCheckOnTableSyntax : IFluentSyntax
{
    IAlterCheckInSchemaOrActionSyntax OnTable(string tableName);
}

public interface IAlterCheckActionSyntax : IFluentSyntax
{
    ICreateCheckOptionsSyntax Condition(string sqlCondition);   // drop and add again
    IAlterCheckEnableOptionsSyntax Enabled();
    void Disabled();
}

public interface IAlterCheckInSchemaOrActionSyntax : IAlterCheckActionSyntax
{
    IAlterCheckActionSyntax InSchema(string schemaName);
}

public interface IAlterCheckEnableOptionsSyntax : IFluentSyntax
{
    void WithNoCheck();
}

public interface IDeleteCheckFromTableSyntax : IFluentSyntax
{
    IInSchemaSyntax FromTable(string tableName);
}
```

### Generated SQL per provider

`Enabled()` and `Disabled()` describe the state the constraint is in afterwards, not a
statement. `Enabled()` means enforced and validated, `Enabled().WithNoCheck()` means enforced
(validated or not), and `Disabled()` means not enforced. On PostgreSQL a constraint is always
enforced, so `Enabled()` only has to validate it, and `Enabled().WithNoCheck()` has nothing to
do.

`Enabled().WithNoCheck()` does not leave a constraint in the same state everywhere. On SQL
Server, `CHECK CONSTRAINT c` leaves a trusted constraint trusted. On Oracle,
`ENABLE NOVALIDATE` marks a validated constraint `NOT VALIDATED`. That does not matter for the
usual pattern of disabling a constraint for a bulk load and enabling it again, because
`DISABLE` has already left it not validated. It should be documented for the other cases.

`Disabled()` leaves the existing rows unchecked by definition, so `WithNoCheck()` and
`Disabled()` are alternatives and cannot be chained (see the interface sketch).

"compat" means the generator returns `CompatibilityMode.HandleCompatibility("<reason>")`.
In strict mode that throws, and in loose mode the statement is skipped.

| Operation | SQL Server | PostgreSQL 9.2+ | MySQL 8 | Oracle | Firebird | Db2 LUW | Db2 for i | SAP HANA | MySQL 4/5, SQLite, Snowflake, Redshift |
|---|---|---|---|---|---|---|---|---|---|
| `Create.Check(...).Condition(x)` | `ADD CONSTRAINT c CHECK (x)` | same | same | same | same (existing rows not checked) | same | same | same | compat |
| `.WithNoCheck()` | `WITH NOCHECK ADD ...` | `... NOT VALID` | compat | `... ENABLE NOVALIDATE` | same as default | compat | compat | compat | compat |
| `.Disabled()` | `WITH NOCHECK ADD ...` + `NOCHECK CONSTRAINT c` | compat (until PG 18) | `... NOT ENFORCED` | `... DISABLE` | compat | `... NOT ENFORCED DISABLE QUERY OPTIMIZATION` | compat | compat | compat |
| `Alter.Check(...).Disabled()` | `NOCHECK CONSTRAINT c` | compat | `ALTER CHECK c NOT ENFORCED` | `MODIFY CONSTRAINT c DISABLE` | compat | `ALTER CHECK c NOT ENFORCED DISABLE QUERY OPTIMIZATION` | compat | compat | compat |
| `Alter.Check(...).Enabled()` | `WITH CHECK CHECK CONSTRAINT c` | `VALIDATE CONSTRAINT c` | `ALTER CHECK c ENFORCED` | `MODIFY CONSTRAINT c ENABLE VALIDATE` | compat | `ALTER CHECK c ENFORCED ENABLE QUERY OPTIMIZATION` | compat | compat | compat |
| `Alter.Check(...).Enabled().WithNoCheck()` | `CHECK CONSTRAINT c` | nothing to do (a `NOT VALID` constraint is already enforced) | compat | `MODIFY CONSTRAINT c ENABLE NOVALIDATE` | compat | compat | compat | compat | compat |
| `Alter.Check(...).Condition(x)` | drop + add | drop + add | one `ALTER TABLE` with both clauses (to be verified) | drop + add in a PL/SQL block | drop + add, run separately | drop + add, run separately | drop + add, run separately | drop + add in a block | compat |
| `Delete.Check(...)` | `DROP CONSTRAINT c` | `DROP CONSTRAINT c` | `DROP CONSTRAINT c` | `DROP CONSTRAINT c` | `DROP CONSTRAINT c` | `DROP CHECK c` | `DROP CHECK c` | `DROP CONSTRAINT c` | compat |

MariaDB is not a column of its own because FluentMigrator has no MariaDB generator (see
[MariaDB](#mariadb)). With a dedicated generator, it would be the same as MySQL 8 except
that `NOT ENFORCED` and enable/disable go through `CompatibilityMode`.

Every statement starts with `ALTER TABLE <Quoter.QuoteTableName(table, schema)>`. The schema
comes from `IDefaultSchemaNameResolver` when none is given, as for every other expression.

### Alter.Check with Condition: changing the condition

ANSI SQL cannot change a check constraint in place. `Alter.Check(...).Condition(x)` therefore
emits the `Delete.Check` statement followed by the `Create.Check` statement, using the same
generators. The new constraint is enabled and validated unless `.WithNoCheck()` or
`.Disabled()` is given. Settings of the old constraint, such as SQL Server's disabled state
or `NOT FOR REPLICATION`, are **not** kept, because FluentMigrator does not read the
constraint before dropping it. This needs to be documented.

#### How the statements are run

Some processors run the generated SQL as one command, and not every database accepts two
statements in one command. Oracle, for example, fails with ORA-00911. The output therefore
depends on the provider:

- **SQL Server and PostgreSQL** run both statements in one command. The same applies to SQL
  Server's `Create.Check(...).Disabled()`, which is also two statements.
- **MySQL 8 (and MariaDB)** accept both changes in one statement:
  `ALTER TABLE t DROP CONSTRAINT c, ADD CONSTRAINT c CHECK (x)`. This makes the change atomic
  there too. Whether both servers accept dropping and adding a constraint of the same name in one
  statement needs to be verified. If they don't, the change uses two statements, as on the
  other providers.
- **Oracle and SAP HANA** wrap the statements in a block, as `OracleGenerator.WrapInBlock` and
  `HanaGenerator.WrapInBlock` already do for other multi-statement output.
- **Firebird and Db2** run the drop and the add as separate statements. One way is for the
  processor to run them one after the other, as `FirebirdProcessor` already does for an identity
  column's sequence after `CREATE TABLE`. Db2 LUW may also accept both changes in one
  `ALTER TABLE`, which is to be verified.

The alternative is for the builder to add a `Delete.Check` expression and a `Create.Check`
expression to the migration. Each is then a single statement on every provider, and no
wrapping is needed. The cost is that MySQL can no longer make the change in one atomic
statement.

#### Atomicity

The change is atomic where DDL is transactional and the processor runs both statements in one
transaction (SQL Server, PostgreSQL and Db2), and on MySQL 8 and MariaDB if the single
`ALTER TABLE` above works. On Oracle and SAP HANA (with the default auto-commit), the drop is
committed before the add runs. If an existing row violates the new condition, the add fails
and the table is left with **no constraint**.

Firebird behaves differently. `ADD CONSTRAINT` never checks the existing rows, so a violating
row does not make the add fail. The change succeeds, and the rows that break the new condition
stay in the table. The drop-then-fail risk remains only for an add that fails for another
reason, such as an invalid condition, because the default `AutoCommitBehaviour()` has
committed the drop by then. The documentation should describe both outcomes.

This is the same problem that
[TransactionalDataDefinitionLanguage.md](TransactionalDataDefinitionLanguage.md) describes for
other operations. Alternatives for the providers that are left:

- Add the new constraint under a temporary name, drop the old one, then rename the new one.
  This works only where constraints can be renamed: Oracle, SQL Server and PostgreSQL (see
  [RenamePrimaryKey.md](RenamePrimaryKey.md)).
- Accept the risk and document it, and advise users to check their data first. This ADR
  recommends this option to start with.

### Reverse() and AutoReversingMigration

- `Create.Check(...).Reverse()` returns a `Delete.Check` with the constraint name, table and
  schema only. As the code-review guidance requires for `Delete*` expressions, the reverse
  does not carry the condition or the creation options.
- `Delete.Check(...)` cannot be reversed, because the condition is not known. It throws, like
  `Delete.Table(...)`.
- `Alter.Check(...).Condition(x)` cannot be reversed, because the old condition is not known.
- `Alter.Check(...).Disabled()` and `Enabled()` cannot be reversed either, because the state
  before the migration is not known. Reversing `Disabled()` to `Enabled()` would validate
  rows that may never have been validated, and on PostgreSQL `Enabled()` runs
  `VALIDATE CONSTRAINT`, which has no inverse. A migration that disables a constraint for a
  bulk load and enables it again should write `Down()` itself (see
  [Open questions](#open-questions)).

## Naming Convention

`docs-website/intro/configuration.md` already lists `CK_<TableName>_<ColumnName>` as the
default name of a check constraint, but nothing produces that name. PR #2457 requires a name,
and its expressions don't go through any naming convention. This section makes the documented
name real.

### Syntax

The name becomes optional when the constraint names its columns, as with
`Create.UniqueConstraint()`:

```csharp
Create.Check().OnTable("Items").Column("Quantity").Condition("Quantity > 0");
// CK_Items_Quantity

Create.Check().OnTable("Bookings").Columns("StartDate", "EndDate").Condition("EndDate > StartDate");
// CK_Bookings_StartDate_EndDate

Alter.Check().OnTable("Items").Column("Quantity").Disabled();
Delete.Check().FromTable("Items").Column("Quantity");
```

Each root gets a `Check()` overload next to `Check(string constraintName)`. After `OnTable` (or
`FromTable`) and `InSchema`, the overload without a name requires `Column(string)` or
`Columns(params string[])` before `Condition` (create), before the action (alter), or at the end
of the chain (delete). The overload with a name doesn't offer them.

- **The columns are used only for the name.** They are not checked against the condition, and
  they don't appear in the SQL. `CheckConstraintDefinition` gets a `Columns` collection, which
  `Clone()` copies.
- **An explicit name always wins.** The convention fills the name only when it is empty, as
  `DefaultConstraintNameConvention` does.
- **A constraint with neither a name nor a column is rejected.** The validation message is
  "A CHECK constraint requires a name or at least one column." Conventions run before
  validation (`MigrationValidator.ApplyConventionsToAndValidateExpressions`), so a custom
  convention can still name such a constraint, for example from the condition.
- **`Alter.Check()` and `Delete.Check()` must give the same columns as `Create.Check()`**, so that
  the convention produces the same name, as with `Delete.UniqueConstraint()`.
- **Two checks on one column get the same name**, for example `Quantity > 0` and
  `Quantity <= 1000`. The second one fails with the provider's duplicate-name error, so one of
  them needs an explicit name.
- **The convention doesn't shorten long names**, like the other naming conventions. Oracle
  before 12.2 allows 30 bytes, and PostgreSQL truncates names to 63 bytes.

### Convention

A check constraint gets its own convention, not a `ConstraintType.Check` handled by
`DefaultConstraintNameConvention`:

- `DefaultConstraintNameConvention` works on `IConstraintExpression`, whose
  `ConstraintDefinition` is a primary key or unique constraint with a column list.
  `CheckConstraintDefinition` is a different type, and [Current state](#current-state) explains
  why there is no `ConstraintType.Check`.
- Foreign keys and indexes already have their own conventions (`IForeignKeyConvention`,
  `IIndexConvention`). 9.0 moves primary keys out of `DefaultConstraintNameConvention` into
  `IPrimaryKeyNameConvention` (Issue #1800). Unique constraints are the last kind left in the
  shared convention.

```csharp
// FluentMigrator.Abstractions, next to IConstraintExpression
public interface ICheckConstraintExpression : IMigrationExpression
{
    CheckConstraintDefinition Constraint { get; }
}

// FluentMigrator.Runner.Conventions
public interface ICheckConstraintConvention
{
    ICheckConstraintExpression Apply(ICheckConstraintExpression expression);
}

public class DefaultCheckConstraintNameConvention : ICheckConstraintConvention
{
    public ICheckConstraintExpression Apply(ICheckConstraintExpression expression)
    {
        var constraint = expression.Constraint;
        if (string.IsNullOrEmpty(constraint.ConstraintName) && constraint.Columns.Count > 0)
        {
            constraint.ConstraintName = "CK_" + constraint.TableName + "_" + string.Join("_", constraint.Columns);
        }

        return expression;
    }
}

// IConventionSet
IList<ICheckConstraintConvention> CheckConstraintConventions { get; }
```

`CheckConstraintExpressionBase` implements `ICheckConstraintExpression`, and
`ConventionSetExtensions.Apply` runs `CheckConstraintConventions` on it, as it does the other
lists. `DefaultConventionSet` puts `DefaultCheckConstraintNameConvention` in the list, and
`ConventionSet` starts with an empty list. The default schema still comes from
`SchemaConvention` through `ISchemaExpression`, as in PR #2457.

### Breaking change to IConventionSet

Adding `CheckConstraintConventions` to `IConventionSet` breaks every class that implements the
interface. Such a class no longer compiles, and an assembly built against 8.x fails to load
with a `TypeLoadException`. The pattern in `docs-website/intro/configuration.md`
(`YourConventionSet`, which wraps an inner convention set) is exactly such a class. `net48` and
`netstandard2.0` have no default interface methods, so, as with `IMigrationGenerator`, the
break is accepted in the 9.0 major release, with a `CompatibilitySuppressions.xml` entry. The
alternative, an optional interface checked at run time, avoids the break but is hard to find.

The fix for an affected class is one line:

```csharp
CheckConstraintConventions = innerConventionSet.CheckConstraintConventions;
// or, without an inner convention set:
CheckConstraintConventions = new List<ICheckConstraintConvention> { new DefaultCheckConstraintNameConvention() };
```

### Tasks for the implementation PR (#2457)

1. **`CHANGELOG.md`**, under `### Breaking` in `9.0.0-prerelease`:

   > `IConventionSet` has a new member, `CheckConstraintConventions`. Custom implementations
   > must add it. To keep the default name for an unnamed check constraint
   > (`CK_<Table>_<Column>`, from the new `DefaultCheckConstraintNameConvention`), set it to the
   > inner convention set's list, as the configuration docs show, or to a list containing
   > `new DefaultCheckConstraintNameConvention()` (PR #2457)

   The `### New` entry for `Create.Check` should also say that a constraint given columns but no
   name is named by `DefaultCheckConstraintNameConvention`, which a custom
   `ICheckConstraintConvention` can replace.
2. **Release notes draft.** The maintainer who merges the PR adds it to the draft release in the
   format of [ReleaseManagement.md](ReleaseManagement.md), and states the break in the same line:

   > PR #2457 : `Create.Check` / `Alter.Check` / `Delete.Check` for CHECK constraints
   > (@ScottishTapWater) [Fixes Issue #1021 (@sariyandath)]. **Breaking:** custom
   > `IConventionSet` implementations must add `CheckConstraintConventions`. The default,
   > `DefaultCheckConstraintNameConvention`, names a check constraint without a name
   > `CK_<Table>_<Column>`.
3. **`docs-website/intro/configuration.md`.**
   - Add `CheckConstraintConventions` to the `YourConventionSet` example, which no longer
     compiles without it.
   - Add check constraints to the list of things a convention can change.
   - Show how to implement `ICheckConstraintConvention`, for example a lower-case
     `ck_<table>_<column>` for PostgreSQL. Show how to register it in place of
     `DefaultCheckConstraintNameConvention` in `CheckConstraintConventions`.
   - Keep the existing `CK_<TableName>_<ColumnName>` line, which becomes true.
4. **`docs-website/basics/constraints.md`.** Show the form without a name, say that the columns
   are used only for the name, and warn about two checks on one column.
5. **Tests.** Unit tests for `DefaultCheckConstraintNameConvention`: one column, several columns,
   and an explicit name kept. Also tests for the validation message, for `Clone()` copying
   `Columns`, and for `DefaultConventionSet` containing the convention
   (`test/FluentMigrator.Tests/ConventionSets.cs`).

## Implementation Notes

- **Expressions and definition.** Add `CreateCheckConstraintExpression`,
  `AlterCheckConstraintExpression` and `DeleteCheckConstraintExpression` to
  `FluentMigrator.Abstractions`, backed by a new `CheckConstraintDefinition`
  (`ConstraintName`, `TableName`, `SchemaName`, `Columns`, `Condition`, `NoCheck`, `Enabled`;
  see [Naming Convention](#naming-convention) for `Columns`). Do not add
  `ConstraintType.Check`, for the reason given in [Current state](#current-state).
  `CheckConstraintDefinition.Clone()` must copy every member, with a test in
  `Unit/Definitions/`. `AlterCheckConstraintExpression` needs an explicit action
  (change condition, enable, disable), so that validation can be done per action: every
  expression rejects an empty name or table, `Create` and `Alter ... Condition(...)` reject an
  empty condition, and `Enabled()`/`Disabled()` must not have one.
- **Generators and processors.** Add `Generate(...)` overloads for the three expressions to
  `IMigrationGenerator`, and `Process(...)` overloads to `IMigrationProcessor`.
  `GenericGenerator` provides the ANSI implementation, and each provider generator overrides
  it as the table above shows. Adding interface members breaks third-party implementations.
  `net48` and `netstandard2.0` have no default interface methods, so this cannot be avoided
  that way. It is acceptable in the 9.0 major release, with a `CompatibilitySuppressions.xml`
  entry and a `### Breaking` entry in `CHANGELOG.md`. The alternative is an optional
  capability interface (`ICheckConstraintGenerator`), which avoids the break but makes the
  feature hard to find and needs a runtime type check.
- **Do not build SQL in `ExecuteWith`.** PR #2457 builds its statement in
  `CreateCheckConstraintExpression.ExecuteWith` with `processor.Execute(...)`. That skips the
  generator, so `CompatibilityMode`, preview and connectionless output, and the generator
  pipeline extensions (see `docs-website/advanced/generator-pipeline-extensions.md`) do not
  apply to it. The core implementation should go through the generator like every other
  expression.
- **Fluent roots.** `ICreateExpressionRoot`, `IAlterExpressionRoot` and `IDeleteExpressionRoot`
  get a `Check(string)` member each. `IfDatabaseExpressionRoot` gets the same members.
- **Relationship to PR #2457.** If core adds `ICreateExpressionRoot.CheckConstraint(string)`,
  that instance member takes precedence over the PostgreSQL extension method with the same
  signature, and the extension can no longer be called. If core adds `Check(string)`, users
  get two APIs that do the same thing. In both cases the extension should not be released on
  its own. Its PostgreSQL integration tests (quoting, default schema, `Force Quote=false`)
  should become tests of the core implementation.
- **Tests.** Add generator unit tests per provider for every row of the SQL table, including
  the compat cases. Add expression tests for `Reverse()`, and integration tests on the
  TestContainers-backed providers that show a violating row is rejected, as PR #2457 does for
  PostgreSQL.
- **Docs.** Add a "Check constraints" section to `docs-website/basics/constraints.md`, with
  the capability matrix and the warning about non-atomic changes.

## Out of scope

- **Inline `CHECK` in `Create.Table`**, either on a column (`.WithColumn("Quantity").AsInt32().Check("Quantity > 0")`)
  or at table level (`.WithCheck("CK_...", "...")`). This works on every provider that supports
  `CHECK`, and on SQLite it is the only option. It is a separate feature, and it raises the same
  design questions about chaining column and table constraints as
  [TableLevelForeignKeys.md](TableLevelForeignKeys.md). The expressions and definition proposed
  here should be designed so that `CreateTableExpression` can use them later. A check declared on
  a column would be named by the same convention, with that column.
- **A separate convention for unique constraints.** `DefaultConstraintNameConvention` keeps
  naming them `UC_<Table>_<Column>`.
- **`IF EXISTS` on `Delete.Check`.** It is available on SQL Server 2016+, PostgreSQL and MariaDB,
  and can be added later in the same way as `Delete.Table(...).IfExists()`.
- **Rebuilding SQLite tables** to emulate `ALTER TABLE ... ADD/DROP CONSTRAINT`.

## Open questions

1. **Naming.** This ADR uses `Check`, as in #2458. `CheckConstraint` would match
   `UniqueConstraint` and `DefaultConstraint` and is the name PR #2457 uses. Its drawback is
   that it is longer, and `Alter.CheckConstraint(...).Enabled()` reads less naturally.
2. **Option names.** `WithNoCheck()` uses SQL Server's term. A provider-neutral name such as
   `NotValidated()` or `WithoutValidatingExistingRows()` describes the behavior better on
   PostgreSQL and Oracle.
3. **Firebird default.** Should `Create.Check` without `WithNoCheck()` go through
   `CompatibilityMode` on Firebird, because the existing rows cannot be validated? Or should it
   emit the plain `ADD CONSTRAINT` and document the difference? This ADR suggests the latter,
   because the constraint itself works the same.
4. **Reverse of Enabled/Disabled.** This ADR makes them irreversible. They could be made
   reversible if the migration states the previous state explicitly, for example
   `Disabled().WasEnabled()`, but that adds syntax for a narrow case.
5. **MariaDB generator.** Should MariaDB get its own generator as part of this work, or later?
6. **Validation as its own action.** On PostgreSQL, `Enabled()` only validates. A separate
   `Alter.Check(...).Validate()` would say that more directly, at the cost of a second way to
   do the same thing on SQL Server and Oracle.

## Decision
**To be decided by maintainers**

Recommended:

1. Add `Create.Check`, `Alter.Check` and `Delete.Check` to the core API, with dedicated
   expressions, and generate them on every provider. Providers that cannot express an
   operation report it through `CompatibilityMode`.
2. Support `WithNoCheck()`, `Disabled()` and `Enabled()` where the database supports them
   natively, as the table above shows, and do not emulate them anywhere else.
3. Implement `Alter.Check(...).Condition(...)` as drop + add. Use a single `ALTER TABLE` on
   MySQL 8 if that is verified, and block or separate statements where a processor runs one
   statement per command. Document that the change is not atomic on Oracle, SAP HANA and
   Firebird.
4. Merge PR #2457 into the core implementation instead of releasing it as a PostgreSQL
   extension.
5. Name a check constraint that has columns but no name `CK_<Table>_<Column>`, through a new
   `ICheckConstraintConvention` in `IConventionSet`. Accept the break to custom
   `IConventionSet` implementations in 9.0, and document it in `CHANGELOG.md`, the release
   notes and the configuration docs.

## References
- [#1021 — How to add a CHECK constraint using fluentmigrator?](https://github.com/fluentmigrator/fluentmigrator/issues/1021)
- [#2458 — ADR: Create.Check/Alter.Check/Delete.Check top-level syntax](https://github.com/fluentmigrator/fluentmigrator/issues/2458)
- [PR #2457 — Add PostgreSQL CHECK constraint support](https://github.com/fluentmigrator/fluentmigrator/pull/2457)
- [SQL Server ALTER TABLE](https://learn.microsoft.com/en-us/sql/t-sql/statements/alter-table-transact-sql)
- [SQL Server sys.check_constraints](https://learn.microsoft.com/en-us/sql/relational-databases/system-catalog-views/sys-check-constraints-transact-sql)
- [PostgreSQL ALTER TABLE](https://www.postgresql.org/docs/current/sql-altertable.html)
- [MySQL 8.0 CHECK Constraints](https://dev.mysql.com/doc/refman/8.0/en/create-table-check-constraints.html)
- [MariaDB CONSTRAINT](https://mariadb.com/kb/en/constraint/)
- [Oracle constraint clause](https://docs.oracle.com/en/database/oracle/oracle-database/19/sqlrf/constraint.html)
- [SQLite CREATE TABLE — CHECK constraints](https://www.sqlite.org/lang_createtable.html#check_constraints)
- [Firebird 4.0 Language Reference — ALTER TABLE](https://firebirdsql.org/file/documentation/html/en/refdocs/fblangref40/firebird-40-language-reference.html#fblangref40-ddl-tbl-alter)
- [Db2 LUW ALTER TABLE](https://www.ibm.com/docs/en/db2/11.5.x?topic=statements-alter-table)
- [Db2 for i ALTER TABLE](https://www.ibm.com/docs/en/i/7.5?topic=statements-alter-table)
- [SAP HANA ALTER TABLE](https://help.sap.com/docs/SAP_HANA_PLATFORM/4fe29514fd584807ac9f2a04f6754767/20d329a6751910149d5fdbc4800f92ff.html)
- [Snowflake constraints overview](https://docs.snowflake.com/en/sql-reference/constraints-overview)
- [Amazon Redshift CREATE TABLE](https://docs.aws.amazon.com/redshift/latest/dg/r_CREATE_TABLE_NEW.html)
