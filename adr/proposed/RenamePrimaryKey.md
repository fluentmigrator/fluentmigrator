# ADR: Rename.PrimaryKey Top-Level Syntax

## Status
Proposed

## Context
FluentMigrator currently supports renaming tables and columns through the `Rename` fluent API. However, there is no top-level syntax for renaming primary key constraints specifically. This ADR investigates the feasibility and portability of adding a `Rename.PrimaryKey()` syntax across all supported database providers.

## Research Findings

### Current State
FluentMigrator supports the following rename operations:
- `Rename.Table(oldName).To(newName)` - Supported by most providers
- `Rename.Column(oldName).OnTable(tableName).To(newName)` - Supported by most providers

When a table is renamed, most database systems automatically rename associated primary key constraints, but the behavior varies significantly across providers.

### Provider Support for Renaming Primary Keys

The following table summarizes the capabilities of each supported database provider for renaming primary key constraints:

| Provider | Syntax for Renaming PK | Level of Support | Notes |
|----------|------------------------|------------------|-------|
| **SQL Server** | `sp_rename 'old_constraint_name', 'new_constraint_name', 'OBJECT'` | **Supported** | Direct constraint renaming via system stored procedure; works independently of table/column renames |
| **PostgreSQL** | `ALTER TABLE table_name RENAME CONSTRAINT old_name TO new_name` | **Supported** | Native SQL support since PostgreSQL 9.2; straightforward and reliable |
| **MySQL** | `ALTER TABLE table_name RENAME INDEX PRIMARY TO new_name` (MySQL 5.7+) | **Supported But Not If Certain Other Features Are Enabled** | Can rename using RENAME INDEX syntax, but PRIMARY KEY name is fixed as "PRIMARY" by default. Renaming may fail if foreign keys reference the table. Named primary keys are uncommon in MySQL |
| **Oracle** | `ALTER INDEX old_pk_name RENAME TO new_pk_name` | **Supported** | Primary keys are backed by unique indexes; rename the index to rename the PK |
| **SQLite** | Not supported | **Not Supported** | SQLite does not support `ALTER TABLE ... RENAME CONSTRAINT`. Would require full table recreation |
| **Firebird** | Not supported directly | **Not Supported** | No native support for constraint renaming. Would require dropping and recreating the constraint |
| **DB2** | `RENAME TABLE old_constraint TO new_constraint` for constraints (DB2 11.5+) | **Supported But Version-Dependent** | Support added in DB2 11.5 for z/OS and 11.5.4 for LUW; older versions not supported |
| **Snowflake** | `ALTER TABLE table_name RENAME CONSTRAINT old_name TO new_name` | **Supported** | Native support similar to PostgreSQL syntax |
| **SAP HANA** | `RENAME CONSTRAINT old_name TO new_name` on table | **Supported** | Uses `ALTER TABLE table_name RENAME CONSTRAINT` syntax |
| **Jet (MS Access)** | Not supported | **Not Supported** | MS Access has very limited ALTER TABLE capabilities; no constraint renaming |
| **Redshift** | `ALTER TABLE table_name RENAME CONSTRAINT old_name TO new_name` | **Supported** | PostgreSQL-compatible syntax; supported in recent versions |

### Detailed Provider Analysis

#### SQL Server
- **Syntax**: `EXEC sp_rename N'schema.old_name', N'new_name', N'OBJECT'`
- **Support**: Full support via stored procedure
- **Considerations**: Must use qualified names with schema. The constraint must exist and be uniquely identifiable
- **Auto-rename on table rename**: No - constraints retain their original names when tables are renamed

#### PostgreSQL
- **Syntax**: `ALTER TABLE table_name RENAME CONSTRAINT old_pk_name TO new_pk_name`
- **Support**: Native support since version 9.2 (2012)
- **Considerations**: Straightforward and transaction-safe
- **Auto-rename on table rename**: No - constraints retain their original names

#### MySQL/MariaDB
- **Syntax**: `ALTER TABLE table_name RENAME INDEX old_pk_name TO new_pk_name`
- **Support**: Supported with significant limitations
- **Considerations**: 
  - The `RENAME INDEX` syntax was added in MySQL 5.7 (2015) and works for regular indexes
  - MySQL primary keys are automatically named "PRIMARY" (a reserved name)
  - Attempting to rename a PRIMARY KEY fails with MySQL error 1828 if foreign keys from other tables reference this primary key
  - MySQL doesn't allow custom PRIMARY KEY constraint names during creation like other databases
  - Custom primary key names are uncommon in MySQL practice; most schemas treat the PRIMARY name as immutable
  - Alternative workaround: Drop and recreate, but requires knowing column list and risks FK violations
- **Auto-rename on table rename**: No - the PRIMARY index retains its name

#### Oracle
- **Syntax**: `ALTER INDEX old_pk_index_name RENAME TO new_pk_index_name`
- **Support**: Full support via index renaming
- **Considerations**: 
  - Primary keys are implemented as unique indexes
  - Renaming the backing index effectively renames the PK
  - Must identify the correct index name (system-generated if not explicitly named)
- **Auto-rename on table rename**: No - constraint and index names remain unchanged

#### SQLite
- **Syntax**: N/A
- **Support**: Not supported
- **Considerations**: 
  - SQLite's ALTER TABLE is very limited
  - Would require creating new table, copying data, dropping old table, renaming new table
  - No native constraint management for rename operations
- **Auto-rename on table rename**: N/A

#### Firebird
- **Syntax**: N/A
- **Support**: Not supported
- **Considerations**: 
  - No direct constraint renaming support
  - Would need to drop and recreate constraint
  - Potential for complexity with foreign key relationships
- **Auto-rename on table rename**: No

#### DB2
- **Syntax**: `RENAME TABLE old_constraint_name TO new_constraint_name` (DB2 11.5+)
- **Support**: Version-dependent (11.5 and later)
- **Considerations**: 
  - Only supported in recent versions (11.5+ for z/OS, 11.5.4+ for LUW)
  - Syntax differs from table rename syntax
  - Not supported in DB2 for i (iSeries)
- **Auto-rename on table rename**: No

#### Snowflake
- **Syntax**: `ALTER TABLE table_name RENAME CONSTRAINT old_name TO new_name`
- **Support**: Full support
- **Considerations**: 
  - Modern cloud database with good DDL support
  - PostgreSQL-compatible syntax
- **Auto-rename on table rename**: No

#### SAP HANA
- **Syntax**: `ALTER TABLE table_name RENAME CONSTRAINT old_name TO new_name`
- **Support**: Full support
- **Considerations**: 
  - Straightforward implementation
  - Part of standard ALTER TABLE syntax
- **Auto-rename on table rename**: No

#### Jet (MS Access)
- **Syntax**: N/A
- **Support**: Not supported
- **Considerations**: 
  - MS Access has minimal DDL capabilities
  - No programmatic constraint renaming
- **Auto-rename on table rename**: N/A

#### Redshift
- **Syntax**: `ALTER TABLE table_name RENAME CONSTRAINT old_name TO new_name`
- **Support**: Full support (added in recent versions)
- **Considerations**: 
  - PostgreSQL fork with compatible syntax
  - Requires relatively recent Redshift version
- **Auto-rename on table rename**: No

## Summary Statistics

- **Fully Supported**: 7 providers (SQL Server, PostgreSQL, Oracle, Snowflake, SAP HANA, Redshift, and conditionally DB2)
- **Supported with Limitations**: 1 provider (MySQL - RENAME INDEX syntax exists but primary keys are typically named "PRIMARY" and cannot be renamed if foreign keys reference them)
- **Not Supported**: 3 providers (SQLite, Firebird, Jet)

## Recommendations

### Option 1: Implement with Compatibility Checking
Implement `Rename.PrimaryKey()` syntax that:
- Generates appropriate SQL for providers that support it
- Throws a `DatabaseOperationNotSupportedException` for unsupported providers (SQLite, Firebird, Jet)
- Uses `RENAME INDEX` for MySQL 5.7+ with validation for foreign key constraints
- Checks version requirements for DB2

### Option 2: Delay Implementation
Given that 4 out of 11 providers either don't support or have significant limitations with primary key renaming, consider whether this feature provides sufficient value given its limited portability.

### Option 3: Document Workarounds
Instead of adding top-level syntax, document provider-specific approaches:
- Users can use `Execute.Sql()` for providers that support direct renaming
- For MySQL, note that `RENAME INDEX` works but with FK constraint limitations
- For unsupported providers, document that primary keys should be dropped and recreated

## Decision
**To be decided by maintainers**

This ADR provides the research necessary to make an informed decision about implementing `Rename.PrimaryKey()` syntax. The key tradeoffs are:

**Pros**:
- Consistent API for a common operation
- Abstracts provider-specific syntax differences
- Reduces need for Execute.Sql() calls

**Cons**:
- Limited portability (only 7-8 out of 11 providers have full support)
- MySQL implementation requires checking for foreign key constraints (error 1828)
- May give false impression of universal support
- Requires significant testing across all providers

## References
- [PostgreSQL ALTER TABLE Documentation](https://www.postgresql.org/docs/current/sql-altertable.html)
- [SQL Server sp_rename Documentation](https://docs.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-rename-transact-sql)
- [MySQL ALTER TABLE Documentation (8.0)](https://dev.mysql.com/doc/refman/8.0/en/alter-table.html)
- [MySQL ALTER TABLE Documentation (9.4) - RENAME INDEX](https://dev.mysql.com/doc/refman/9.4/en/alter-table.html)
- [Oracle ALTER INDEX Documentation](https://docs.oracle.com/en/database/oracle/oracle-database/19/sqlrf/ALTER-INDEX.html)
- [Snowflake ALTER TABLE Documentation](https://docs.snowflake.com/en/sql-reference/sql/alter-table-constraint.html)
- [DB2 RENAME Documentation](https://www.ibm.com/docs/en/db2/11.5?topic=statements-rename)
- [SAP HANA ALTER TABLE Documentation](https://help.sap.com/docs/HANA_SERVICE_CF/7c78579ce9b14a669c1f3295b0d8ca16/20d329b6751910149985f1c1a3c1679b.html)

## Conclusion
Renaming primary key constraints is supported by most modern database providers but with significant variations in syntax and capability. A top-level `Rename.PrimaryKey()` API is technically feasible but would require careful handling of provider-specific differences and limitations. The decision to implement this feature should weigh the API consistency benefits against the portability challenges and implementation complexity.
