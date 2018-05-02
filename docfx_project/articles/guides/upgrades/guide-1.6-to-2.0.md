---
uid: upgrade-guide-1.6-to-2.0
title: Upgrade Guide from 1.6 to 2.0
---

# Upgrading from 1.6 to 2.0

# What is new?

FluentMigrator finally gained .NET Standard 2.0 support and added serveral new database and migration expression features.

## .NET Standard 2.0

This finally allows the usage of FluentMigrator together with ASP.NET Core 2.0!

## New databases

* SAP SQL Anywhere 16
    * With Unique Constraints: Non-Distinct NULL support
* Microsoft SQL Server 2016
* Oracle MySQL 5
* Amazon Redshift (experimental)

## Several enhancements

* MySQL
    * `ALTER DEFAULT`, `DROP DEFAULT` support
* SQL Server 2005
    * `WITH (ONLINE=ON/OFF)` support
    * 64 bit identity support
* SQL Server 2008
    * Unique Constraints: Non-Distinct NULL support
* Firebird
    * New provider option: `Force Quote=true` to enforce quotes
* SQLite
    * Foreign key support
* Streamlined table/index schema quoting
* Types: `DateTime2` support
* Expression:
    * IfDatabase: Predicate support
    * IfDatabase: Method delegation support
    * Index: Creation with non*key columns
    * Conventions: Default schema name support
    * `SetExistingRowsTo` supports `SystemMethods`
    * Insert/Update/Delete: `DbNull` support
    * Passing arguments to embedded SQL scripts
* Runner:
    * TaskExecutor: HasMigrationsToApply support
    * Case insensitive arguments support
    * `StopOnError` flag

# What did change?

## DB-specific code moved

The database specific code is now in its own assembly (one per database family). This will allow trimming dependencies in the future.

## MySQL announcing SQL scripts

Some other DB processors already do this.

## Runner improvements

* Better error messages
* ListMigrations: showing `(not applied)` for unapplied migrations
* Show `(BREAKING)` for migrations with breaking changes
* MSBuild task is available as separate package (with custom .targets file)
* Use provider default command timeout when no global timeout is set

# Breaking changes

## DB-specific extensions

The extension methods are now in their own assembly and namespace (e.g. for SqlServer - `FluentMigrator.Extensions.SqlServer`).

## .NET Framework 3.5 support removed

We now require at least .NET Framework 4.0.

## NAnt build runner removed

NAnt itself isn't a living project anymore.
