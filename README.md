# FluentMigrator [![(License)](https://img.shields.io/github/license/fluentmigrator/fluentmigrator.svg)](https://github.com/fluentmigrator/fluentmigrator/blob/master/LICENSE.txt)

Fluent Migrator is a migration framework for .NET much like Ruby on Rails Migrations. Migrations are a structured way to alter your database schema and are an alternative to creating lots of sql scripts that have to be run manually by every developer involved. Migrations solve the problem of evolving a database schema for multiple databases (for example, the developer's local database, the test database and the production database). Database schema changes are described in classes written in C# that can be checked into a version control system.

# News

Finally, version 2.0.0 is released!

Please read the [changelog](https://github.com/fluentmigrator/fluentmigrator/blob/master/CHANGELOG.md)

# Packages

Source              | Status
--------------------|----------
NuGet (Releases)    | [![NuGet](https://img.shields.io/nuget/v/FluentMigrator.svg)](https://www.nuget.org/packages/FluentMigrator/)
MyGet (Prerelease)  | [![MyGet](https://img.shields.io/myget/fluent-migrator/vpre/FluentMigrator.svg)](https://www.myget.org/feed/Packages/fluent-migrator)

The releases are stored on [nuget.org](https://nuget.org)
while the CI builds are stored on [MyGet](https://www.myget.org/feed/Packages/fluent-migrator).

# Project Info

|                           |         | 
|---------------------------|---------|
| **Documentation**         | [https://github.com/fluentmigrator/fluentmigrator/wiki](https://github.com/fluentmigrator/fluentmigrator/wiki) |
| **Discussions**           | [![Gitter](https://img.shields.io/gitter/room/FluentMigrator/fluentmigrator.svg)](https://gitter.im/FluentMigrator/fluentmigrator) |
| **Bug/Feature Tracking**  | [![GitHub issues](https://img.shields.io/github/issues/fluentmigrator/fluentmigrator.svg)](https://github.com/fluentmigrator/fluentmigrator/issues) |

# Build Status

Build-Server | Status
-------------|----------
TeamCity     | [![TeamCity (full build status)](https://img.shields.io/teamcity/http/teamcity.jetbrains.com/e/FluentMigrator_MasterAndPullRequests.svg)](https://teamcity.jetbrains.com/viewType.html?buildTypeId=FluentMigrator_MasterAndPullRequests&guest=1)
Travis CI    | [![Travis](https://img.shields.io/travis/fluentmigrator/fluentmigrator.svg)](https://travis-ci.org/fluentmigrator/fluentmigrator)


The TeamCity build is generously hosted and run on the [JetBrains TeamCity](https://teamcity.jetbrains.com) infrastructure.

Our Linux build is hosted on Travis CI.

# Building FluentMigrator

The build is based on the .NET Core tooling.

## Prerequisites

* .NET Core 2.1

### Windows

```
dotnet build FluentMigrator.sln
```

### Linux

```
dotnet restore
msbuild ./FluentMigrator.sln
```

## Testing

### Windows

```
dotnet test test\FluentMigrator.Tests\FluentMigrator.Tests.csproj --filter "TestCategory!=Integration"
```

### Linux

```
dotnet vstest test/FluentMigrator.Tests/bin/Debug/net471/FluentMigrator.Tests.dll --TestCaseFilter:'TestCategory!=Integration&TestCategory!=NotWorkingOnMono'
```

## Creating the nuget packages

### Windows

```
dotnet pack .\FluentMigrator.sln --output "C:\fluentmigrator\output"
```

### Linux

```
msbuild ./FluentMigrator.sln /v:m /t:Pack /p:PackageOutputPath="/tmp/fluentmigrator/output"
```

# Powered by

![ReSharper](http://www.jetbrains.com/img/logos/logo_resharper_small.gif)

# Contributors

A [long list](https://github.com/fluentmigrator/fluentmigrator/wiki/ContributorList) of everyone that has contributed to FluentMigrator. Thanks for all the Pull Requests!
