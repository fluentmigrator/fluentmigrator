# Fluent migrations framework for .NET [![(Apache 2.0 License)](https://img.shields.io/github/license/fluentmigrator/fluentmigrator.svg)](https://github.com/fluentmigrator/fluentmigrator/blob/master/LICENSE.txt)

Fluent Migrator is a migration framework for .NET much like Ruby on Rails Migrations. Migrations are a structured way to alter your database schema and are an alternative to creating lots of sql scripts that have to be run manually by every developer involved. Migrations solve the problem of evolving a database schema for multiple databases (for example, the developer's local database, the test database and the production database). Database schema changes are described in classes written in C# that can be checked into a version control system.

# How to use it

* [Quickstart](xref:quickstart.md)

# What does it look like?

This is an example of a database-agnostic migration:

[!code-cs[20180430121800_AddLogTable.cs](articles/quickstart/20180430121800_AddLogTable.cs "Your first migration")]

# Current Release

* [Release Notes](https://github.com/fluentmigrator/fluentmigrator/releases)

# Upgrade guides

* [2.x to 3.0](xref:upgrade-guide-2.0-to-3.0)

# Supported databases

For the [current release](https://github.com/fluentmigrator/fluentmigrator/releases/latest) these are the supported databases:

[!include[Supported databases](snippets/supported-databases.md)]

# More Information on FluentMigrator

* Sean Chambers on the [Herding Code podcast](http://herdingcode.com/herding-code-70)
