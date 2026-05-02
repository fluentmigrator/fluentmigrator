# ADR: Map Out ANSI SQL (ISO/IEC 9075) Data Control Language Support for All Providers

## Status
Proposed

## Context
FluentMigrator does not currently provide any top-level fluent syntax for ANSI SQL Data Control Language (DCL) operations. DCL is the subset of SQL that manages access rights and permissions on database objects. The three core DCL statements defined by the ANSI SQL standard are:

- **GRANT** – Grants a privilege on a database object to a principal (user, role, group).
- **REVOKE** – Removes a previously granted (or denied) privilege from a principal.
- **DENY** – Explicitly denies a privilege to a principal, overriding any GRANTs (supported by a subset of providers, notably SQL Server).

These operations are commonly needed during database migrations when:
- Provisioning a new schema and assigning read/write roles.
- Rotating service accounts and updating their permissions.
- Enforcing least-privilege by revoking unnecessary access after structural changes.
- Setting up row-level security roles (PostgreSQL, SQL Server).

Without native FluentMigrator support, users are currently forced to fall back to `Execute.Sql()` with hand-written DCL statements, which is database-specific and bypasses FluentMigrator's compatibility and abstraction layer.

## Research Findings

### Terminology

| Term | ANSI SQL | Description |
|------|----------|-------------|
| **Principal** | Yes | The entity receiving or losing a privilege: a user, role, or group. |
| **Privilege / Permission** | Yes | The right to perform an action on a securable object (e.g., `SELECT`, `INSERT`, `EXECUTE`). |
| **Securable / Object** | Yes | The database object on which the privilege is granted (e.g., table, view, schema, database). |
| **GRANT** | Yes | Gives a privilege to a principal. |
| **REVOKE** | Yes | Removes a GRANT or DENY from a principal. |
| **DENY** | Partial | Explicitly forbids a privilege. Defined by SQL Server; most other providers use `REVOKE` to remove access instead. |
| **GRANT OPTION** | Yes | Allows the grantee to further GRANT the privilege to other principals (`WITH GRANT OPTION`). |
| **ADMIN OPTION** | Yes | Allows a role member to grant role membership to others (`WITH ADMIN OPTION`). |
| **Role Membership** | Yes | `GRANT role TO user` syntax for assigning roles, supported natively by most providers. |

---

### Provider DCL Support Matrix

#### Privilege Types by Provider

The following tables cover the DCL privilege types supported by each provider across the most common object scopes (table, schema/database, routine).

> **Legend:**
> - ✅ Supported
> - ⚠️ Supported with limitations or caveats
> - ❌ Not supported / not applicable

##### Table / View Privileges

| Privilege | SQL Server | PostgreSQL | MySQL / MariaDB | Aurora MySQL | Aurora PostgreSQL | D-SQL | Oracle | SQLite | Firebird | DB2 LUW | DB2 iSeries | Snowflake | SAP HANA | Jet (Access) | Redshift |
|-----------|-----------|------------|-----------------|--------------|-------------------|-------|--------|--------|----------|---------|-------------|-----------|----------|--------------|----------|
| `SELECT` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ (UI only) | ✅ |
| `INSERT` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ (UI only) | ✅ |
| `UPDATE` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ (UI only) | ✅ |
| `DELETE` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ (UI only) | ✅ |
| `REFERENCES` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| `TRIGGER` | ✅ | ✅ | ❌ | ❌ | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| `TRUNCATE` | ❌ | ✅ (PG 14+) | ❌ | ❌ | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| `OWNERSHIP` | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ |

##### Column-Level Privileges

| Privilege | SQL Server | PostgreSQL | MySQL / MariaDB | Oracle | DB2 LUW | Snowflake | SAP HANA | Redshift |
|-----------|-----------|------------|-----------------|--------|---------|-----------|----------|----------|
| `SELECT (col)` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |
| `INSERT (col)` | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ |
| `UPDATE (col)` | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ |
| `REFERENCES (col)` | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ |

##### Schema / Database Privileges

| Privilege | SQL Server | PostgreSQL | MySQL / MariaDB | Oracle | DB2 LUW | Snowflake | SAP HANA | Redshift |
|-----------|-----------|------------|-----------------|--------|---------|-----------|----------|----------|
| `CREATE` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `USAGE` | ❌ | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ | ✅ |
| `CONNECT` | ❌ | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ |
| `TEMPORARY / TEMP` | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| `ALL PRIVILEGES` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |

##### Routine Privileges

| Privilege | SQL Server | PostgreSQL | MySQL / MariaDB | Oracle | DB2 LUW | Snowflake | SAP HANA | Redshift |
|-----------|-----------|------------|-----------------|--------|---------|-----------|----------|----------|
| `EXECUTE` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `ALTER ROUTINE` | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |

---

#### GRANT / REVOKE / DENY Statement Support

| Provider | GRANT | REVOKE | DENY | WITH GRANT OPTION | Role Membership (GRANT role TO user) | Notes |
|----------|-------|--------|------|-------------------|---------------------------------------|-------|
| **SQL Server** | ✅ | ✅ | ✅ | ✅ | ✅ | DENY is SQL Server-specific and overrides GRANT. `WITH GRANT OPTION` supported. |
| **PostgreSQL** | ✅ | ✅ | ❌ | ✅ | ✅ | No DENY; use REVOKE to remove access. `WITH GRANT OPTION` and `WITH ADMIN OPTION` supported. |
| **MySQL 5 / 8** | ✅ | ✅ | ❌ | ✅ | ✅ (MySQL 8+) | MySQL 8+ separates role management from account privileges. `WITH GRANT OPTION` supported. |
| **MariaDB** | ✅ | ✅ | ❌ | ✅ | ✅ | Role support added in MariaDB 10.0.5. Compatible with MySQL syntax. |
| **Aurora MySQL** | ✅ | ✅ | ❌ | ✅ | ✅ (MySQL 8 mode) | Same as MySQL 8 when using Aurora MySQL 3.x (MySQL 8 compatible). |
| **Aurora PostgreSQL** | ✅ | ✅ | ❌ | ✅ | ✅ | Same as PostgreSQL. Aurora-specific extensions do not change DCL. |
| **D-SQL (Amazon)** | ✅ | ✅ | ❌ | ⚠️ | ✅ | D-SQL (Distributed SQL / CockroachDB-style) supports GRANT/REVOKE. WITH GRANT OPTION support varies. |
| **Oracle** | ✅ | ✅ | ❌ | ✅ | ✅ | No DENY. `WITH GRANT OPTION` (object privilege) and `WITH ADMIN OPTION` (system/role privilege) supported. |
| **SQLite** | ❌ | ❌ | ❌ | ❌ | ❌ | SQLite has no user/role concept. All access controlled at the OS filesystem level. |
| **Firebird** | ✅ | ✅ | ❌ | ✅ | ✅ | No DENY. GRANT/REVOKE supported for tables, views, procedures, and roles. `WITH GRANT OPTION` supported. |
| **DB2 LUW** | ✅ | ✅ | ❌ | ✅ | ✅ | No DENY. Full GRANT/REVOKE support. `WITH GRANT OPTION` supported. |
| **DB2 iSeries** | ✅ | ✅ | ❌ | ✅ | ✅ | Uses `GRANT` and `REVOKE`. Object authority model differs slightly from LUW. |
| **Snowflake** | ✅ | ✅ | ❌ | ✅ | ✅ | No DENY. Snowflake uses role-based access control (RBAC) exclusively. `WITH GRANT OPTION` supported. |
| **SAP HANA** | ✅ | ✅ | ❌ | ✅ | ✅ | No DENY. Full GRANT/REVOKE. `WITH GRANT OPTION` supported. |
| **Jet (MS Access)** | ⚠️ | ⚠️ | ❌ | ❌ | ❌ | Very limited: GRANT/REVOKE available only in Jet SQL via Jet ADOX or DAO. Rarely used programmatically and not SQL-standard compliant. |
| **Redshift** | ✅ | ✅ | ❌ | ✅ | ✅ | PostgreSQL-compatible. No DENY. Column-level privileges not supported (table-level only). |

---

### Detailed Provider Analysis

#### SQL Server
- **Syntax**: `GRANT privilege ON object TO principal [WITH GRANT OPTION]`
- **DENY**: `DENY privilege ON object TO principal` — SQL Server-specific; prohibits a principal from receiving the privilege even if it is granted through a role.
- **REVOKE**: `REVOKE [GRANT OPTION FOR] privilege ON object FROM principal [CASCADE]`
- **Role Membership**: `ALTER ROLE role_name ADD MEMBER user_name` (SQL Server 2012+) or `sp_addrolemember` (legacy)
- **Principals**: Logins, Users, Roles, Application Roles, Certificates
- **Scopes**: Table, View, Column, Stored Procedure, Function, Schema, Database, Server
- **References**: [GRANT (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/statements/grant-transact-sql), [DENY (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/statements/deny-transact-sql), [REVOKE (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/statements/revoke-transact-sql)

#### PostgreSQL
- **Syntax**: `GRANT privilege ON object TO role [WITH GRANT OPTION]`
- **Role Membership**: `GRANT role_name TO user_name [WITH ADMIN OPTION]`
- **REVOKE**: `REVOKE [GRANT OPTION FOR] privilege ON object FROM role [CASCADE | RESTRICT]`
- **No DENY**: Use `REVOKE` to remove privileges. Row-level security (RLS) can restrict access further.
- **Principals**: Roles (users and groups are both "roles" in PostgreSQL)
- **Scopes**: Table, Column, Sequence, Database, Domain, Foreign Data Wrapper, Foreign Server, Function, Language, Large Object, Schema, Tablespace, Type
- **Aurora PostgreSQL**: Identical to upstream PostgreSQL. Amazon does not modify DCL syntax.
- **References**: [PostgreSQL GRANT](https://www.postgresql.org/docs/current/sql-grant.html), [PostgreSQL REVOKE](https://www.postgresql.org/docs/current/sql-revoke.html), [Role Membership](https://www.postgresql.org/docs/current/role-membership.html), [Aurora PostgreSQL docs](https://docs.aws.amazon.com/AmazonRDS/latest/AuroraUserGuide/AuroraPostgreSQL.Security.html)

#### MySQL / MariaDB
- **Syntax**: `GRANT privilege ON object TO 'user'@'host' [WITH GRANT OPTION]`
- **Role Membership (MySQL 8+)**: `GRANT role TO user`; `SET DEFAULT ROLE`
- **REVOKE**: `REVOKE privilege ON object FROM 'user'@'host'`
- **No DENY**: No equivalent to SQL Server DENY.
- **Principals**: User accounts identified by `'username'@'host'`, Roles (MySQL 8+, MariaDB 10.0.5+)
- **Scopes**: Global (`*.*`), Database (`db.*`), Table (`db.table`), Column, Routine
- **Aurora MySQL**: Uses MySQL 8 privileges when running Aurora MySQL 3.x; same DCL syntax.
- **References**: [MySQL GRANT (9.4)](https://dev.mysql.com/doc/refman/9.4/en/grant.html), [MySQL REVOKE](https://dev.mysql.com/doc/refman/9.4/en/revoke.html), [MySQL Roles](https://dev.mysql.com/doc/refman/8.0/en/roles.html), [MariaDB GRANT](https://mariadb.com/kb/en/grant/), [Aurora MySQL security](https://docs.aws.amazon.com/AmazonRDS/latest/AuroraUserGuide/AuroraMySQL.Security.html)

#### Oracle
- **Syntax**: `GRANT privilege ON object TO user [WITH GRANT OPTION]`
- **System privileges**: `GRANT CREATE TABLE TO user [WITH ADMIN OPTION]`
- **Role Membership**: `GRANT role TO user [WITH ADMIN OPTION]`
- **REVOKE**: `REVOKE privilege ON object FROM user`
- **No DENY**: No equivalent.
- **Principals**: Users, Roles
- **Scopes**: Table, View, Column, Sequence, Procedure, Function, Package, Type, Directory, Library, Java Objects, Database
- **References**: [Oracle GRANT](https://docs.oracle.com/en/database/oracle/oracle-database/19/sqlrf/GRANT.html), [Oracle REVOKE](https://docs.oracle.com/en/database/oracle/oracle-database/19/sqlrf/REVOKE.html)

#### SQLite
- **GRANT / REVOKE / DENY**: Not supported. SQLite is a serverless, embedded database. Access control is handled entirely at the OS file system level and via application-level logic. There are no users, roles, or privileges.
- **References**: [SQLite FAQ](https://www.sqlite.org/faq.html), [SQLite Features](https://www.sqlite.org/fullsql.html)

#### Firebird
- **Syntax**: `GRANT privilege ON TABLE table_name TO user [WITH GRANT OPTION]`
- **Role Membership**: `GRANT role TO user [WITH ADMIN OPTION]`
- **REVOKE**: `REVOKE privilege ON TABLE table_name FROM user`
- **No DENY**: No equivalent.
- **Scopes**: Table, View, Column, Stored Procedure, Function, Role, Generator/Sequence, Exception
- **References**: [Firebird GRANT](https://firebirdsql.org/refdocs/langrefupd21-grant.html), [Firebird REVOKE](https://firebirdsql.org/refdocs/langrefupd21-revoke.html)

#### DB2 LUW / DB2 iSeries
- **Syntax**: `GRANT privilege ON TABLE table_name TO user [WITH GRANT OPTION]`
- **Role Membership**: `GRANT ROLE role_name TO user`
- **REVOKE**: `REVOKE privilege ON TABLE table_name FROM user`
- **No DENY**: No equivalent.
- **DB2 iSeries**: Uses object-authority model; `GRANT AUTHORITY` instead of `GRANT privilege`. Compatible but semantically different.
- **References**: [DB2 LUW GRANT](https://www.ibm.com/docs/en/db2/11.5?topic=statements-grant-table-privileges), [DB2 LUW REVOKE](https://www.ibm.com/docs/en/db2/11.5?topic=statements-revoke-table-privileges), [DB2 iSeries GRANT](https://www.ibm.com/docs/en/i/7.5?topic=statements-grant-object-privileges)

#### Snowflake
- **Syntax**: `GRANT privilege ON object TO ROLE role_name`
- **Role Membership**: `GRANT ROLE role_name TO ROLE other_role | USER user_name`
- **REVOKE**: `REVOKE privilege ON object FROM ROLE role_name`
- **No DENY**: No equivalent. Snowflake uses RBAC exclusively; every privilege is granted to a role, not a user directly.
- **Scopes**: Database, Schema, Table, View, Column, Stage, Stream, Task, Function, Procedure, Warehouse, Account
- **References**: [Snowflake GRANT](https://docs.snowflake.com/en/sql-reference/sql/grant-privilege), [Snowflake REVOKE](https://docs.snowflake.com/en/sql-reference/sql/revoke-privilege), [Snowflake Access Control Overview](https://docs.snowflake.com/en/user-guide/security-access-control-overview)

#### SAP HANA
- **Syntax**: `GRANT privilege ON TABLE table_name TO user [WITH GRANT OPTION]`
- **Role Membership**: `GRANT role_name TO user [WITH ADMIN OPTION]`
- **REVOKE**: `REVOKE privilege ON TABLE table_name FROM user`
- **No DENY**: No equivalent.
- **Scopes**: Table, View, Procedure, Function, Schema, Database, Package, Structured Privilege
- **References**: [SAP HANA GRANT Statement](https://help.sap.com/docs/SAP_HANA_PLATFORM/4fe29514fd584807ac9f2a04f6754767/20f674e1751910148a8b990d33efbdc5.html), [SAP HANA REVOKE Statement](https://help.sap.com/docs/SAP_HANA_PLATFORM/4fe29514fd584807ac9f2a04f6754767/20fc91e2751910148fc5847c87c8e27a.html)

#### Jet (MS Access)
- **Syntax**: `GRANT privilege ON TABLE table_name TO user` — available in Jet SQL via ADOX/DAO
- **REVOKE**: `REVOKE privilege ON TABLE table_name FROM user`
- **No DENY**: No equivalent.
- **Caveats**: Jet's DCL is minimally documented, rarely used programmatically, and not fully compliant with ANSI SQL. MS Access user-level security has been deprecated since Access 2007 for `.accdb` files. Granting/revoking is only meaningful for legacy `.mdb` (workgroup-secured) files.
- **References**: [Jet SQL GRANT statement (deprecated)](https://support.microsoft.com/en-us/office/grant-statement-microsoft-access-sql-72b8dc01-9fe3-4c49-9d16-6e9a3dd91dac), [User-Level Security FAQ](https://learn.microsoft.com/en-us/office/client-developer/access/desktop-database-reference/grant-statement-microsoft-access-sql)

#### Amazon Redshift
- **Syntax**: `GRANT privilege ON TABLE table_name TO user [WITH GRANT OPTION]`
- **Role Membership**: `GRANT ROLE role_name TO { user_name | ROLE other_role }`
- **REVOKE**: `REVOKE privilege ON TABLE table_name FROM user [CASCADE | RESTRICT]`
- **No DENY**: No equivalent.
- **Caveats**: Column-level privileges not supported (table-level only). Redshift roles (`GRANT ROLE`) were added in 2022; legacy group-based access (`GRANT ... TO GROUP`) still works.
- **References**: [Redshift GRANT](https://docs.aws.amazon.com/redshift/latest/dg/r_GRANT.html), [Redshift REVOKE](https://docs.aws.amazon.com/redshift/latest/dg/r_REVOKE.html), [Redshift Role-Based Access Control](https://docs.aws.amazon.com/redshift/latest/mgmt/redshift-iam-access-control-overview.html)

---

### Summary Statistics

| Category | Providers |
|----------|-----------|
| **Full GRANT + REVOKE support** | SQL Server, PostgreSQL, MySQL 8, MariaDB, Aurora MySQL, Aurora PostgreSQL, Oracle, Firebird, DB2 LUW, DB2 iSeries, Snowflake, SAP HANA, Redshift |
| **GRANT + REVOKE with limitations** | Jet (MS Access) — legacy only, deprecated in modern `.accdb` |
| **DENY support** | SQL Server only |
| **No DCL support** | SQLite |
| **Role membership (GRANT role TO user)** | All except SQLite and Jet |
| **Column-level privileges** | SQL Server, PostgreSQL, MySQL, Oracle, DB2 LUW, SAP HANA, Snowflake (SELECT only) |

---

## Proposed Top-Level Syntax

The following syntax mirrors FluentMigrator's existing fluent API conventions (verb-first, builder-chain pattern) and is designed to be database-agnostic for the core operations.

### Core API

```csharp
// Grant table-level privileges
Grant.Privilege("SELECT").OnTable("Orders").ToUser("reporting_user");
Grant.Privilege("SELECT", "INSERT", "UPDATE").OnTable("Orders").ToRole("app_role");
Grant.AllPrivileges().OnTable("Orders").ToUser("admin_user");

// Grant schema privileges
Grant.Privilege("USAGE").OnSchema("public").ToRole("read_role");
Grant.Privilege("CREATE").OnSchema("app_schema").ToRole("app_role");

// Grant with GRANT OPTION (grantee can re-grant)
Grant.Privilege("SELECT").OnTable("Orders").ToUser("power_user").WithGrantOption();

// Grant role membership
Grant.Role("read_only_role").ToUser("reporting_user");
Grant.Role("read_only_role").ToUser("reporting_user").WithAdminOption();

// Revoke table-level privileges
Revoke.Privilege("INSERT").OnTable("Orders").FromUser("reporting_user");
Revoke.AllPrivileges().OnTable("Orders").FromRole("legacy_role");

// Revoke grant option only (keeps privilege but removes ability to re-grant)
Revoke.GrantOptionFor("SELECT").OnTable("Orders").FromUser("power_user");

// Revoke role membership
Revoke.Role("read_only_role").FromUser("reporting_user");

// Deny (SQL Server only — should use ISupportAdditionalFeatures or IfDatabase())
IfDatabase(ProcessorIdConstants.SqlServer)
    .Deny.Privilege("DELETE").OnTable("Orders").ToUser("readonly_user");
```

### Proposed Expression Classes

```
IGrantPrivilegeExpressionBuilder
  .OnTable(tableName)         → IGrantObjectExpressionBuilder
  .OnSchema(schemaName)       → IGrantObjectExpressionBuilder
  .OnDatabase(databaseName)   → IGrantObjectExpressionBuilder
  .ToUser(userName)           → IGrantFinalBuilder
  .ToRole(roleName)           → IGrantFinalBuilder
  .WithGrantOption()          → terminates chain

IGrantRoleExpressionBuilder
  .ToUser(userName)           → IGrantFinalBuilder
  .ToRole(roleName)           → IGrantFinalBuilder
  .WithAdminOption()          → terminates chain

IRevokePrivilegeExpressionBuilder
  .OnTable(tableName)         → IRevokeObjectExpressionBuilder
  .OnSchema(schemaName)       → IRevokeObjectExpressionBuilder
  .GrantOptionFor(privilege)  → IRevokeObjectExpressionBuilder (removes WITH GRANT OPTION only)
  .FromUser(userName)         → IRevokeFinalBuilder
  .FromRole(roleName)         → IRevokeFinalBuilder

IDenyPrivilegeExpressionBuilder  // SQL Server only
  .OnTable(tableName)         → IDenyObjectExpressionBuilder
  .ToUser(userName)           → IDenyFinalBuilder
  .ToRole(roleName)           → IDenyFinalBuilder
```

### Column-Level Privileges

Column-level privilege support (supported by SQL Server, PostgreSQL, MySQL, Oracle, DB2 LUW, SAP HANA) is more complex and provider-specific. It is recommended to place column-level grants in `ISupportAdditionalFeatures` or behind provider-specific extension methods:

```csharp
// Standard (non-column-level) — works everywhere DCL is supported
Grant.Privilege("SELECT").OnTable("Employees").ToRole("hr_role");

// Column-level — provider-specific extension method approach
Grant.Privilege("SELECT").OnTable("Employees")
    .Columns("FirstName", "LastName")   // extension method on IGrantObjectExpressionBuilder
    .ToRole("hr_role");
```

### ISupportAdditionalFeatures Candidates

The following features are non-standard or provider-specific and should be expressed either through `ISupportAdditionalFeatures`, `IfDatabase()` blocks, or dedicated provider extension methods:

| Feature | Recommended Approach | Reason |
|---------|---------------------|--------|
| **DENY** | `IfDatabase(ProcessorIdConstants.SqlServer)` + extension method | SQL Server-only feature |
| **Column-level privileges** | Extension method `.Columns(...)` on grant builder | Only 6 of 14 providers support it |
| **GRANT OPTION / ADMIN OPTION** | Core API (`.WithGrantOption()`, `.WithAdminOption()`) | Broadly supported, should be part of the main syntax |
| **`ALL PRIVILEGES`** | Core API (`.AllPrivileges()`) | Supported by all DCL-capable providers |
| **`PUBLIC` grantee** | Core API (`.ToPublic()`) | Supported by PostgreSQL, Oracle, DB2, Firebird, Redshift |
| **Cascade revoke** | Extension method `.Cascade()` on revoke builder | Behavior differs across providers |
| **Snowflake RBAC (role-to-role)** | Extension method or `IfDatabase(Snowflake)` block | Snowflake mandates role-based access; direct user grants are uncommon |
| **MySQL host-qualified users** | Provider-specific via `IfDatabase(MySQL)` | MySQL user accounts include `'user'@'host'` syntax |
| **PostgreSQL row-level security** | Separate ADR / provider extension | RLS is a complex PostgreSQL-specific feature beyond DCL |
| **Oracle system privileges** | Provider-specific extension method | System-level privileges (`CREATE TABLE`, `CREATE SESSION`) differ from object privileges |

### Handling SQLite and Jet (No DCL Support)

For providers that do not support DCL (SQLite) or where DCL is deprecated (Jet/MS Access), FluentMigrator should throw a `DatabaseOperationNotSupportedException` when a DCL expression is executed, consistent with how other unsupported operations are currently handled (see `GenericGenerator.NotSupported()`).

---

## Recommendations

1. **Implement core GRANT / REVOKE** first, covering table-level and schema-level privileges for the most widely used providers: SQL Server, PostgreSQL, MySQL 8, MariaDB, Oracle, DB2 LUW, Snowflake, SAP HANA, and Redshift.

2. **Add role membership syntax** (`Grant.Role(...).ToUser(...)`) as part of the initial implementation, since it is broadly supported and commonly needed.

3. **Defer column-level privileges** to a follow-up, keeping the initial surface area manageable. Expose via optional extension method on `IGrantObjectExpressionBuilder`.

4. **Use `IfDatabase()` for DENY** (SQL Server only) rather than polluting the top-level `Deny` root with a misleading cross-database abstraction.

5. **Throw `DatabaseOperationNotSupportedException`** for SQLite; emit a warning or no-op for Jet (deprecated user-level security).

6. **Do not implement DCL for Jet** in any meaningful way—MS Access user-level security was deprecated in 2007. Document this as out of scope.

7. **Keep `GRANT OPTION` / `ADMIN OPTION` in the core API** (`.WithGrantOption()` / `.WithAdminOption()`), since they are supported by all DCL-capable providers.

---

## Decision
**To be decided by maintainers**

This ADR provides the research necessary to make an informed decision about implementing DCL support in FluentMigrator. The key tradeoffs are:

**Pros**:
- Enables database-agnostic migration scripts that include permission management.
- Consistent API reduces provider-specific `Execute.Sql()` calls.
- Supports automated provisioning workflows that co-locate schema and access control changes.

**Cons**:
- DCL semantics differ significantly between providers, especially for roles and column-level privileges.
- DENY is SQL Server-only, creating an asymmetry in the top-level API.
- SQLite and legacy Jet cannot support DCL at all.
- MySQL's `'user'@'host'` model requires special handling.
- Snowflake's exclusive RBAC model may require custom extensions.

---

## References

- [Wikipedia: Data Control Language](https://en.wikipedia.org/wiki/Data_control_language)
- [ANSI SQL ISO/IEC 9075 Standard Overview](https://www.iso.org/standard/63555.html)
- **SQL Server**: [GRANT (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/statements/grant-transact-sql), [DENY (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/statements/deny-transact-sql), [REVOKE (Transact-SQL)](https://learn.microsoft.com/en-us/sql/t-sql/statements/revoke-transact-sql)
- **PostgreSQL**: [GRANT](https://www.postgresql.org/docs/current/sql-grant.html), [REVOKE](https://www.postgresql.org/docs/current/sql-revoke.html), [Role Membership](https://www.postgresql.org/docs/current/role-membership.html)
- **Aurora PostgreSQL**: [Amazon Aurora PostgreSQL Security](https://docs.aws.amazon.com/AmazonRDS/latest/AuroraUserGuide/AuroraPostgreSQL.Security.html)
- **MySQL**: [GRANT (MySQL 9.4)](https://dev.mysql.com/doc/refman/9.4/en/grant.html), [REVOKE (MySQL 9.4)](https://dev.mysql.com/doc/refman/9.4/en/revoke.html), [Account Management Statements](https://dev.mysql.com/doc/refman/9.7/en/account-management-statements.html), [Roles](https://dev.mysql.com/doc/refman/8.0/en/roles.html)
- **MariaDB**: [GRANT](https://mariadb.com/kb/en/grant/), [REVOKE](https://mariadb.com/kb/en/revoke/)
- **Aurora MySQL**: [Amazon Aurora MySQL Security](https://docs.aws.amazon.com/AmazonRDS/latest/AuroraUserGuide/AuroraMySQL.Security.html)
- **Oracle**: [GRANT (Oracle 19c)](https://docs.oracle.com/en/database/oracle/oracle-database/19/sqlrf/GRANT.html), [REVOKE (Oracle 19c)](https://docs.oracle.com/en/database/oracle/oracle-database/19/sqlrf/REVOKE.html)
- **SQLite**: [SQLite Features](https://www.sqlite.org/fullsql.html), [SQLite FAQ](https://www.sqlite.org/faq.html)
- **Firebird**: [Firebird GRANT](https://firebirdsql.org/refdocs/langrefupd21-grant.html), [Firebird REVOKE](https://firebirdsql.org/refdocs/langrefupd21-revoke.html), [Firebird Security](https://firebirdsql.org/file/documentation/html/en/refdocs/fblangref40/firebird-40-language-reference.html#fblangref40-security)
- **DB2 LUW**: [DB2 GRANT (Table Privileges)](https://www.ibm.com/docs/en/db2/11.5?topic=statements-grant-table-privileges), [DB2 REVOKE](https://www.ibm.com/docs/en/db2/11.5?topic=statements-revoke-table-privileges)
- **DB2 iSeries**: [IBM i GRANT Object Privileges](https://www.ibm.com/docs/en/i/7.5?topic=statements-grant-object-privileges)
- **Snowflake**: [GRANT Privilege](https://docs.snowflake.com/en/sql-reference/sql/grant-privilege), [REVOKE Privilege](https://docs.snowflake.com/en/sql-reference/sql/revoke-privilege), [Access Control Overview](https://docs.snowflake.com/en/user-guide/security-access-control-overview)
- **SAP HANA**: [SAP HANA GRANT Statement](https://help.sap.com/docs/SAP_HANA_PLATFORM/4fe29514fd584807ac9f2a04f6754767/20f674e1751910148a8b990d33efbdc5.html), [SAP HANA REVOKE Statement](https://help.sap.com/docs/SAP_HANA_PLATFORM/4fe29514fd584807ac9f2a04f6754767/20fc91e2751910148fc5847c87c8e27a.html)
- **Jet / MS Access**: [GRANT Statement (MS Access SQL)](https://support.microsoft.com/en-us/office/grant-statement-microsoft-access-sql-72b8dc01-9fe3-4c49-9d16-6e9a3dd91dac)
- **Amazon Redshift**: [GRANT](https://docs.aws.amazon.com/redshift/latest/dg/r_GRANT.html), [REVOKE](https://docs.aws.amazon.com/redshift/latest/dg/r_REVOKE.html), [Redshift Role-Based Access Control](https://docs.aws.amazon.com/redshift/latest/mgmt/redshift-iam-access-control-overview.html)

## Conclusion
GRANT and REVOKE are supported by all FluentMigrator providers except SQLite (no user model) and Jet/MS Access (deprecated). DENY is unique to SQL Server. The proposed `Grant` / `Revoke` top-level syntax, modeled after the existing FluentMigrator fluent API, provides a clean and database-agnostic abstraction for the most commonly used DCL operations. Provider-specific features (DENY, column-level grants, Snowflake RBAC, MySQL host-qualified users) should be exposed via `IfDatabase()` blocks, provider extension methods, or `ISupportAdditionalFeatures` to keep the core API portable and uncluttered.
