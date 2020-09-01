The below table summarizes portability of default schema across different databases.

One end-product of this document is to allow end-users can force a consistent default schema across all generators, via overriding the Convention Set interface.
A consistent default schema would allow schema-qualified object names in procedural code like stored procedures and triggers.

| Database   | Official Magic String for Schema  | How To Get Current Schema of Session | How to Override Value Inside Session |
| ------------ | ------------------------------------ | ------------------------------------------- | ---------------------------------------- |
| SQL Server | dbo                                               | `SCHEMA_NAME()` [1]                             | User Impersonation                            | 
| SyBase ASE | dbo                                               | `USER_NAME()` [2]                                  | Change User                                       |
| Sqlite          | n/a                                                | n/a                                                          | Call ATTACH DATABASE [3]                  |
| Postgres     | public                                            | CURRENT_SCHEMA() [4]                         | `SET search_path TO val1, val2`; [5]     |
| SQL Anywhere |  ?                                             | ?                                                               | `setuser [ "username" ]` [6]                  |
| Oracle        | `user()` or `sys_context('USERENV', 'SESSION_USER')` | `select sys_context( 'userenv', 'current_schema' ) from dual;`      | `alter session set current_schema = xx`   |
| DB2            |         |       |    |
| Firebird       |        |       |    |
| MySQL       | _default database for current session; set via connection string_ | `DATABASE()`      |  `USE db_name`  |

1. [`SCHEMA_NAME()`](https://docs.microsoft.com/en-us/sql/t-sql/functions/schema-name-transact-sql?view=sql-server-ver15)
2. In SyBase, database objects are scoped to their owner and the [`CREATE SCHEMA`](http://infocenter.sybase.com/help/index.jsp?topic=/com.sybase.help.sqlanywhere.12.0.1/dbreference/create-schema-statement.html) statement specifies the authorization of the current user.  The _owner_ value is equal to [`USER_NAME()`](http://infocenter.sybase.com/help/index.jsp?topic=/com.sybase.infocenter.dc36271.1570/html/blocks/X22909.htm)
3. In SQLite, a schema name is the name of an [attached database](https://www.sqlite.org/lang_attach.html). So it is not possible to have multiple schemata within the same database.
4. See [`current_schema()`](https://www.postgresql.org/docs/9.6/functions-info.html)
5. When using FluentMigrator's TransactionPerSession mode, the end user can call  [`DISCARD ALL`](https://www.postgresql.org/docs/9.4/sql-discard.html) in a maintenace migration attributed with `Maintenance(MigrationStage.AfterEach)` to reset the search path, such that in a team of end users, each end user can write a self-contained migration that does not affect other migrations.
6. [`setuser [ "username" ]`](http://dcx.sybase.com/1200/en/dbreference/setuser-statement.html)
