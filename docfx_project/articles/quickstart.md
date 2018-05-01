---
uid: quickstart.md
title: Starting with FluentMigrator
---

# Creating a project

```bash
mkdir test
cd test
dotnet new console
```

# Adding the NuGet packages

```bash
# For migration development
dotnet add package FluentMigrator

# For migration execution
dotnet add package FluentMigrator.Runner

# For database support
dotnet add package FluentMigrator.Runner.SQLite

# ADO.NET support for the database
dotnet add package Microsoft.Data.Sqlite
```

# Creating your first migration

Create a file called `20180430_AddLogTable.cs` with the following contents:

[!code-cs[20180430121800_AddLogTable.cs](quickstart/20180430121800_AddLogTable.cs "Your first migration")]

This will create a table named `Log` with the columns `Id`, and `Text`.

# Running your first migration

You have two options to execute your migration:

* Using an in-process runner (preferred)
* Using an out-of-process runner (for some corporate requirements)

## [In-Process (preferred)](#tab/runner-in-process)

Change your `Program.cs` to the following code:

[!code-cs[Program.cs](quickstart/Program.cs "Migrating the database")]

As you can see, instantiating the [migration runner](xref:FluentMigrator.Runner.IMigrationRunner) (in `UpdateDatabase`) becomes
very simple and updating the database is straight-forward.

## [Out-of-process (for some corporate requirements)](#tab/runner-dotnet-fm)

> [!IMPORTANT]
> You need at least the .NET Core 2.1 preview 2 SDK for this tool.

Install the `dotnet-fm` tool:

```bash
dotnet tool install -g FluentMigrator.DotNet.Cli
```

Execute the migration:

```bash
dotnet fm migrate -p sqlite -c "Data Source=test.db" -a ".\bin\Debug\netcoreapp2.1\test.dll"
```

> [!TIP]
> You probably have to replace `netcoreapp2.1` with the correct target framework. You can find it in the `csproj` file, XML element `TargetFramework`.

***
