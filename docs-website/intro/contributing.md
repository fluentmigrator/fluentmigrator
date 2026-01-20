# How to contribute

1. [Fork on GitHub](https://github.com/fluentmigrator/fluentmigrator)
2. [Create a branch](https://git-scm.com/book/en/v2/Git-Branching-Branches-in-a-Nutshell)
3. Code (and add tests)
4. Create a pull request on GitHub
    * Target the `main` branch for all changes
5. Get the pull request merged
6. Done

# How to build the code

```bash
dotnet build
```

# How to test the code

```bash
dotnet test
```

## Enabling integration tests

The easiest way to enable integration tests is to install Docker. The test suite uses
[Testcontainers](https://dotnet.testcontainers.org/) to create containerized databases for the integration tests,
on demand.

By default, only SQLite integration tests are enabled, you can enable the others explicitly, either via user secrets
or via environment variables.

When using containerized databases, the integration tests will create and destroy databases as needed, and no
connection string changes are required.

Integration tests for the following databases can be enabled:

* SQLite (enabled by default)
* PostgreSQL (`TestConnectionStrings:Postgres`)
* SQL Server (`TestConnectionStrings:SqlServer2016`)
* MySQL (`TestConnectionStrings:MySql`)
* Oracle (`TestConnectionStrings:Oracle`)

### User secrets usage

```bash
# Change into the test project directory
cd test/FluentMigrator.Tests

# Enable PostgreSQL integration tests on Docker
dotnet user-secrets set "TestConnectionStrings:Postgres:ContainerEnabled" True
dotnet user-secrets set "TestConnectionStrings:Postgres:IsEnabled" True
```

### Environment variables usage

```bash
# Enable PostgreSQL integration tests on Docker, this is the way to do it on Linux/macOS
# The environment variables are case-insensitive and must be prefixed with "FM_"
export FM_TestConnectionStrings__Postgres__ContainerEnabled=true
export FM_TestConnectionStrings__Postgres__IsEnabled=true
```

When you run the unit tests, the enabled integration tests are run as well.

# How to create NuGet packages

```bash
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
// Copyright (c) 2026, Fluent Migrator Project
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
