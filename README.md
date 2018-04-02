# Contribution

Every contributor implicitly assigns the copyright for its changes of the project (e.g. pull requests or patch sets) to the "Fluent Migrator Project" itself.

# Goals and changes for 2.x

## New features

- Using the .NET Core CLI tooling
- .NET Standard 2.0 support
- .NET Framework 4.5 support

## Breaking changes

- Moving database specific code from `FluentMigrator.Runner` to `FluentMigrator.Runner.<Database>`
- `FluentMigrator` is now `FluentMigrator.Core`
- Extension methods for - e.g. SqlServer - are now in `FluentMigrator.Extensions.SqlServer`
- Removal of deprecated functions
- Removal of .NET Framework 3.5 support
- Removal of NAnt build task
- MSBuild task is available as separate package (with custom .targets file)
- Removal of SchemaDump and T4 experiments

# How to build/test/pack

## Windows

```
dotnet build FluentMigrator.sln
dotnet test test\FluentMigrator.Tests\FluentMigrator.Tests.csproj --filter "TestCategory!=Integration"
dotnet pack .\FluentMigrator.sln --output "C:\fluentmigrator\output"
```

## Linux

```
dotnet restore
msbuild ./FluentMigrator.sln
dotnet vstest test/FluentMigrator.Tests/bin/Debug/net471/FluentMigrator.Tests.dll --TestCaseFilter:'TestCategory!=Integration&TestCategory!=NotWorkingOnMono'
msbuild ./FluentMigrator.sln /v:m  /t:Pack /p:PackageOutputPath="/tmp/fluentmigrator/output"
```

# TODO

* [ ] Port sample application
* [ ] Test [PR #798](https://github.com/fluentmigrator/fluentmigrator/pull/798)
* [x] Implement Distinct NULL unique constraints for SQL Anywhere, affected by:
  * [x] [Support SQLAnywhere](https://github.com/fluentmigrator/fluentmigrator/pull/659)
  * [x] [Unique index with support for multiple null values per column](https://github.com/fluentmigrator/fluentmigrator/pull/717)
  * [x] [Unique index with support for multiple null values](https://github.com/fluentmigrator/fluentmigrator/pull/716)
* [x] [Travis CI](https://travis-ci.org/fluentmigrator/fluentmigrator)
* [x] [JetBrains TeamCity Build Server](https://teamcity.jetbrains.com/project.html?projectId=FluentMigrator)

# Pull Requests for 2.0

## High priority

### New providers

- [x] [Redshift provider](https://github.com/fluentmigrator/fluentmigrator/pull/605)
- [x] [Support SQLAnywhere](https://github.com/fluentmigrator/fluentmigrator/pull/659)
- [x] [SQL Server 2016](https://github.com/fluentmigrator/fluentmigrator/pull/833)

### New features

* [x] [Add db type datetime2](https://github.com/fluentmigrator/fluentmigrator/pull/657)
* [ ] [Remove ExplicitUnicodeString and add NonUnicodeString](https://github.com/fluentmigrator/fluentmigrator/pull/642)
* [x] [Create Foreign Key with SQLite](https://github.com/fluentmigrator/fluentmigrator/pull/638)
* [x] [DBNull for Insert/Update criteria expression](https://github.com/fluentmigrator/fluentmigrator/pull/672)
* [x] [Overload IfDatabase to accept predicate](https://github.com/fluentmigrator/fluentmigrator/pull/683)
* [x] [TaskExecutor got method HasMigrationsToApply](https://github.com/fluentmigrator/fluentmigrator/pull/701)
* [x] [Enable delegation to methods for IfDatabase expressions](https://github.com/fluentmigrator/fluentmigrator/pull/707)
* [x] [case insensitive arguments and give more instructive messages when required options are not given](https://github.com/fluentmigrator/fluentmigrator/pull/719)
* [x] [ListMigrations says "(not applied)" for migrations not applied](https://github.com/fluentmigrator/fluentmigrator/pull/750)
* [x] [Syntax for creating indexes with nonkey columns](https://github.com/fluentmigrator/fluentmigrator/pull/759)
* [x] [Ability to specify the Default Schema Name](https://github.com/fluentmigrator/fluentmigrator/pull/772)
* [x] [Add MySQL alter and drop of defaults](https://github.com/fluentmigrator/fluentmigrator/pull/783)
* [x] [SQL Server 2005 support WITH (ONLINE = ON|OFF)](https://github.com/fluentmigrator/fluentmigrator/pull/788)
* [x] [MySqlProcessor was not announcing the SQL of embedded scripts](https://github.com/fluentmigrator/fluentmigrator/pull/793)
* [x] [Added StopOnError flag in order to pause console if some migration fails](https://github.com/fluentmigrator/fluentmigrator/pull/795)
* [x] [Add SystemMethods.CurrentDateTimeOffset](https://github.com/fluentmigrator/fluentmigrator/pull/803)
* [x] [Remove default connection timeout. Use provider's default instead.](https://github.com/fluentmigrator/fluentmigrator/pull/811)
* [x] [64 bit identity seed for SQL Server](https://github.com/fluentmigrator/fluentmigrator/pull/816)

### Bug fixes

* [x] [allowed for TagsAttribute to match by inheritance](https://github.com/fluentmigrator/fluentmigrator/pull/643)
* [x] [AsString() -> Nvarchar() in MySql](https://github.com/fluentmigrator/fluentmigrator/pull/725):
      `MySql` is now `MySql4` and this feature requires `MySql5`
* [x] [[Hana] Fix drop primary key](https://github.com/fluentmigrator/fluentmigrator/pull/745)
* [x] [IntegrationTestBase Assert on configuration error](https://github.com/fluentmigrator/fluentmigrator/pull/751)
* [x] [Update Max Decimal for MySql](https://github.com/fluentmigrator/fluentmigrator/pull/825)

### Optimization

* [x] [Add StringBuilder to ExecuteBatchNonQuery()](https://github.com/fluentmigrator/fluentmigrator/pull/798)
* [x] [Avoid confusion reported in issue #748](https://github.com/fluentmigrator/fluentmigrator/pull/808):
  may be a breaking change.

## Medium priority

### New features

* [x] [Add Delete.UniqueConstraint() based on conventions name](https://github.com/fluentmigrator/fluentmigrator/pull/754)
  is interesting. Does it have a relation to PR #753?
* [ ] [Pass arguments to scripts](https://github.com/fluentmigrator/fluentmigrator/pull/666)
* [ ] [add delete.Table.IfExists() syntax and support generators](https://github.com/fluentmigrator/fluentmigrator/pull/684/files)
  seems to be a more generic take than PR #664.
* [ ] [add breaking change identification](https://github.com/fluentmigrator/fluentmigrator/pull/829)

## Not sure about priority/effort/correctness

* [Execute.Sql option to enable echoing command; changed timestamps](https://github.com/fluentmigrator/fluentmigrator/pull/742):
  This PR changes two things at once and I'm not sure if using CURRENT_TIMESTAMP works everywhere. Investigation needed.
* [use unique constraint instead of unique index in ColumnExpressionBuilderHelper.Unique](https://github.com/fluentmigrator/fluentmigrator/pull/753)
  might be a breaking change
* [Refactor tests](https://github.com/fluentmigrator/fluentmigrator/pull/784)
