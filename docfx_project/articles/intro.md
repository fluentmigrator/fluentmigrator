# Install from Nuget

The project is split into multiple packages:

Package | Description
--------|-------------
FluentMigrator | The base assembly needed to create migrations
FluentMigrator.Runner | The runner classes required for in-process execution of migrations
FluentMigrator.Console | The .NET 4.0/4.5/.NET Core 2.0 executable for out-of-process execution of migrations
FluentMigrator.MSBuild | The .NET 4.0/4.5/.NET Standard 2.0 task for MSBuild
FluentMigrator.DotNet.Cli | The .NET Core 2.1 executable that integrates into the .NET Core CLI tooling (`dotnet` command)

## Getting Started

* Check out the tour of FluentMigrator in our [Quickstart](xref:quickstart.md)

# Usage details

* How to create a [migration](xref:migration-example.md)
* Learn about the [fluent interface](fluent-interface.md)
* [Profiles](profiles.md) can be used to seed test data
* And then choose one of the [migration runners](migration-runners.md) to run your migrations

# More Features

* [Database functions as default value](xref:db-functions)
* [Microsoft SQL Server specific extensions](xref:sql-server-extensions.md)

# Advanced Features and Techniques of FluentMigrator

* [Dealing with multiple database types](multi-db-support.md)

# Current Release

* [Release Notes](https://github.com/fluentmigrator/fluentmigrator/releases)

# Upgrade guides

* [2.x to 3.0](xref:upgrade-guide-2.0-to-3.0)

# Supported databases

For the [current release](https://github.com/fluentmigrator/fluentmigrator/releases/latest) these are the supported databases:

[!include[Supported databases](../snippets/supported-databases.md)]
