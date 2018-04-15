# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Breaking changes

- `IQuerySchema.DatabaseType` now returns `SqlServer2016`, etc... and not `SqlServer` any more
- Database specific code was moved into its own assemblies
- `IMigrationConventions` was renamed to `IMigrationRunnerConventions`
- `IMigrationContext` doesn't contain the `IMigrationConventions` any more
  - Expression conventions are now bundled in the new `IConventionSet`
- `ICanBeConventional` was removed during the overhaul of the expression convention system
- Strings are now Unicode by default. Use `NonUnicodeString` for ANSI strings

### Added

- Framework: .NET Standard 2.0 support
- Database:
  - SQL Anywhere 16 support
  - SQL Server 2016 support
  - MySQL:
    - `ALTER/DROP DEFAULT` value support
  - MySQL 5:
    - New dialect
    - `NVARCHAR` for `AsString`
  - SQL Server 2005
    - `WITH (ONLINE=ON/OFF)` support
    - 64 bit identity support
  - Redshift (Amazon, experimental)
  - Firebird
    - New provider option: `Force Quote=true` to enforce quotes
- Unique Constraints: Non-Distinct NULL support (SQL Server 2008 and SQL Anywhere 16)
- Types: DateTime2 support
- Dialect: SQLite foreign key support
- Insert/Update/Delete: DbNull support
- Expression:
  - IfDatabase: Predicate support
  - IfDatabase: Method delegation support
  - Index: Creation with non-key columns
  - Conventions: Default schema name support
  - `SetExistingRowsTo` supports `SystemMethods`
  - Passing arguments to embedded SQL scripts
- Runner:
  - TaskExecutor: HasMigrationsToApply support
  - Case insensitive arguments support
  - `StopOnError` flag

### Changed

- Project:
  - Moving database specific code from `FluentMigrator.Runner` to `FluentMigrator.Runner.<Database>`
  - Extension methods for - e.g. SqlServer - are now in `FluentMigrator.Extensions.SqlServer`
- Database:
  - MySQL: Now announcing SQL scripts
- Runner:
  - Better error messages
  - ListMigrations: showing `(not applied)` for unapplied migrations
  - Show `(BREAKING)` for migrations with breaking changes
  - MSBuild task is available as separate package (with custom .targets file)
  - Use provider default command timeout when no global timeout is set

### Deprecated

- Generic:
  - `IAnnouncer.Write`

### Removed

- Generic:
  - Deprecated functions
  - SchemaDump experiment
  - T4 experiment
- Framework:
  - .NET Framework 3.5 support
- Runner:
  - NAnt build task

### Fixed

- Runner:
  - Match `TagAttribute` by inheritance
- Processors (database specific processing of expressions):
  - Using the new `SqlBatchParser` to parse batches of SQL statements (`GO` statement support)
- Database:
  - Hana: Fixed syntax for dropping a primary key
- Tests:
  - Mark integration tests as ignored when no active processor could be found
