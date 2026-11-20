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
| **SQL Server** (2000–2016+) | `ADD CONSTRAINT c CHECK (...)` | `WITH NOCHECK ADD ...` | Add, then `NOCHECK CONSTRAINT c` | `NOCHECK CONSTRAINT c` / `[WITH CHECK] CHECK CONSTRAINT c` | `DROP CONSTRAINT c` (`IF EXISTS` from 2016) | Yes |
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
  `NOT VALID` and `NO INHERIT` for check constraints were added in 9.2, so the base
  `PostgresGenerator` must not emit them and `Postgres92Generator` and later can.
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
  - Drop: `ALTER TABLE t DROP CHECK c`. `DROP CONSTRAINT` works from 8.0.19. Use `DROP CHECK`
    so that every 8.0.16+ server accepts the statement.
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
  MySQL generator emits.
- DDL is not transactional.

#### Oracle
- **Add**: `ALTER TABLE "s"."t" ADD CONSTRAINT "c" CHECK (expr) [ENABLE | DISABLE] [VALIDATE | NOVALIDATE]`.
  The default is `ENABLE VALIDATE`.
- **Enable/disable**: `ALTER TABLE "s"."t" MODIFY CONSTRAINT "c" ENABLE [VALIDATE | NOVALIDATE]`
  and `... MODIFY CONSTRAINT "c" DISABLE`.
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
- **Enable/disable**: `ALTER TABLE "s"."t" ALTER CHECK "c" [NOT] ENFORCED`. Whether going back
  to `ENFORCED` checks the existing rows straight away or puts the table in set integrity
  pending state needs to be verified before implementation.
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

public interface ICreateCheckOptionsSyntax : IFluentSyntax
{
    ICreateCheckOptionsSyntax WithNoCheck();
    ICreateCheckOptionsSyntax Disabled();
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

"compat" means the generator returns `CompatibilityMode.HandleCompatibility("<reason>")`.
In strict mode that throws, and in loose mode the statement is skipped.

| Operation | SQL Server | PostgreSQL 9.2+ | MySQL 8 | Oracle | Firebird | Db2 LUW | Db2 for i | SAP HANA | MySQL 4/5, SQLite, Snowflake, Redshift |
|---|---|---|---|---|---|---|---|---|---|
| `Create.Check(...).Condition(x)` | `ADD CONSTRAINT c CHECK (x)` | same | same | same | same (existing rows not checked) | same | same | same | compat |
| `.WithNoCheck()` | `WITH NOCHECK ADD ...` | `... NOT VALID` | compat | `... ENABLE NOVALIDATE` | same as default | compat | compat | compat | compat |
| `.Disabled()` | `WITH NOCHECK ADD ...` + `NOCHECK CONSTRAINT c` | compat (until PG 18) | `... NOT ENFORCED` | `... DISABLE` | compat | `... NOT ENFORCED` | compat | compat | compat |
| `Alter.Check(...).Disabled()` | `NOCHECK CONSTRAINT c` | compat | `ALTER CHECK c NOT ENFORCED` | `MODIFY CONSTRAINT c DISABLE` | compat | `ALTER CHECK c NOT ENFORCED` | compat | compat | compat |
| `Alter.Check(...).Enabled()` | `WITH CHECK CHECK CONSTRAINT c` | `VALIDATE CONSTRAINT c` | `ALTER CHECK c ENFORCED` | `MODIFY CONSTRAINT c ENABLE VALIDATE` | compat | `ALTER CHECK c ENFORCED` | compat | compat | compat |
| `Alter.Check(...).Enabled().WithNoCheck()` | `CHECK CONSTRAINT c` | nothing to do (a `NOT VALID` constraint is already enforced) | compat | `MODIFY CONSTRAINT c ENABLE NOVALIDATE` | compat | compat | compat | compat | compat |
| `Alter.Check(...).Condition(x)` | drop + add | drop + add | drop + add | drop + add | drop + add | drop + add | drop + add | drop + add | compat |
| `Delete.Check(...)` | `DROP CONSTRAINT c` | `DROP CONSTRAINT c` | `DROP CHECK c` | `DROP CONSTRAINT c` | `DROP CONSTRAINT c` | `DROP CHECK c` | `DROP CHECK c` | `DROP CONSTRAINT c` | compat |

MariaDB is not a column of its own because FluentMigrator has no MariaDB generator (see
[MariaDB](#mariadb)). With a dedicated generator, it would be the same as MySQL 8 except
that `NOT ENFORCED` and enable/disable go through `CompatibilityMode` and `Delete.Check` emits
`DROP CONSTRAINT c`.

Every statement starts with `ALTER TABLE <Quoter.QuoteTableName(table, schema)>`. The schema
comes from `IDefaultSchemaNameResolver` when none is given, as for every other expression.

### Alter.Check with Condition: changing the condition

ANSI SQL cannot change a check constraint in place. `Alter.Check(...).Condition(x)` therefore
emits the `Delete.Check` statement followed by the `Create.Check` statement, using the same
generators. The new constraint is enabled and validated unless `.WithNoCheck()` or
`.Disabled()` is given. Settings of the old constraint, such as SQL Server's disabled state
or `NOT FOR REPLICATION`, are **not** kept, because FluentMigrator does not read the
constraint before dropping it. This needs to be documented.

The two statements are only atomic where DDL is transactional and the processor runs them
in one transaction: SQL Server, PostgreSQL, and Db2. On MySQL 8, Oracle, SAP HANA (with the
default auto-commit) and Firebird (with the default `AutoCommitBehaviour()`), the drop is
committed before the add runs. If an existing row violates the new condition, the add
fails and the table is left with **no constraint**. This is the same problem that
[TransactionalDataDefinitionLanguage.md](TransactionalDataDefinitionLanguage.md) describes
for other operations. Alternatives:

- Add the new constraint under a temporary name, drop the old one, then rename the new one.
  This works only where constraints can be renamed (Oracle, SQL Server, PostgreSQL; see
  [RenamePrimaryKey.md](RenamePrimaryKey.md)). MySQL and MariaDB cannot rename check
  constraints, and they are among the providers that need this most.
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

## Implementation Notes

- **Expressions and definition.** Add `CreateCheckConstraintExpression`,
  `AlterCheckConstraintExpression` and `DeleteCheckConstraintExpression` to
  `FluentMigrator.Abstractions`, backed by a new `CheckConstraintDefinition`
  (`ConstraintName`, `TableName`, `SchemaName`, `Condition`, `NoCheck`, `Enabled`). Do not add
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
  here should be designed so that `CreateTableExpression` can use them later.
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
3. Implement `Alter.Check(...).Condition(...)` as drop + add, and document that it is not
   atomic on providers without transactional DDL.
4. Merge PR #2457 into the core implementation instead of releasing it as a PostgreSQL
   extension.

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
- [Db2 LUW ALTER TABLE](https://www.ibm.com/docs/en/db2/11.5?topic=statements-alter-table)
- [Db2 for i ALTER TABLE](https://www.ibm.com/docs/en/i/7.5?topic=statements-alter-table)
- [SAP HANA ALTER TABLE](https://help.sap.com/docs/SAP_HANA_PLATFORM/4fe29514fd584807ac9f2a04f6754767/20d329a6751910149d5fdbc4800f92ff.html)
- [Snowflake constraints overview](https://docs.snowflake.com/en/sql-reference/constraints-overview)
- [Amazon Redshift CREATE TABLE](https://docs.aws.amazon.com/redshift/latest/dg/r_CREATE_TABLE_NEW.html)
