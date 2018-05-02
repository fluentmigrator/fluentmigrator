---
uid: contribution
title: How to contribute
---

# How to contribute

1. [Fork on GitHub](https://github.com/fluentmigrator/fluentmigrator)
2. [Create a branch](https://git-scm.com/book/en/v2/Git-Branching-Branches-in-a-Nutshell)
3. Code (and add tests)
4. Create a pull request on GitHub
    * Target the `develop` branch for API-breaking changes
    * Target the `master` branch for non-API-breaking changes and/or hotfixes
5. Get the pull request merged
6. Done

# How to build the code

> [!NOTE]
> You must install [mono](http://www.mono-project.com/) on non-Windows platforms!

```
dotnet build FluentMigrator.sln
```

# How to test the code

## Windows

```
dotnet test test\FluentMigrator.Tests\FluentMigrator.Tests.csproj
```

## Linux, MacOS

```
dotnet test test/FluentMigrator.Tests/FluentMigrator.Tests.csproj --filter "TestCategory!=NotWorkingOnMono"
```

## Enabling integration tests

Only integration tests for two databases are enabled by default:

* SQL Server Compact Edition
    * Only runs on Windows due to inability to load the needed DLLs
* SQLite
    * This requires an installed `Mono.Data.Sqlite` package on Linux

Every database can be configured using the `dotnet user-secrets` tool.

Enabling PostgreSQL integration tests might look like this:

1. Create a user `fluentmigrator` with password `fluentmigrator`
2. Add a database `FluentMigrator` with the owner `fluentmigrator`
3. Type the following instructions:

```bash
# change into the test project directory
cd test/FluentMigrator.Tests
# Set the PostgreSQL connection string
dotnet user-secrets set "TestConnectionStrings:Postgres:ConnectionString" "Server=127.0.0.1;Port=5432;Database=FluentMigrator;User Id=fluentmigrator;Password=fluentmigrator"
# Enable the PostgreSQL integration tests
dotnet user-secrets set "TestConnectionStrings:Postgres:IsEnabled" True
```

When you run the unit tests, the integration tests for PostgreSQL are run as well.

# How to create NuGet packages

```
dotnet pack ./FluentMigrator.sln --output "absolute-path-to-output-directory"
```

# ReSharper/Rider specific support

There is a new template `ctorc` which creates a StyleCop-compatible constructor summary.

# Code style

Please use an editor that supports the [.editorconfig](https://raw.githubusercontent.com/fluentmigrator/fluentmigrator/master/.editorconfig)
and/or the ReSharper/Rider settings [FluentMigrator.sln.DotSettings](https://raw.githubusercontent.com/fluentmigrator/fluentmigrator/master/FluentMigrator.sln.DotSettings).

## Generic

* Use spaces for indention
* Add a "new line" character when the last line is not empty
* Remove trailing whitespace characters

## For C#

* Indent size is 4 characters
* Use `var` for built-in types
* Use `var` when the type is apparent
* Prefer braces
* Sort using directives
* `System` using directives first
* Empty line between using directive groups
* Line break before open brace
* Add a license header region to every *.cs file

```cs
#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion
```
