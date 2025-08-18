# FluentMigrator [![(License)](https://img.shields.io/github/license/fluentmigrator/fluentmigrator.svg)](https://github.com/fluentmigrator/fluentmigrator/blob/main/LICENSE.txt) [![FluentMigrator.GitHub.Io Docs](https://img.shields.io/badge/docs-fluentmigrator-blue.svg)](https://fluentmigrator.github.io) [![Fluent-Migrator tag on Stack Overflow](https://img.shields.io/badge/stackoverflow-fluentmigrator-orange.svg)](https://stackoverflow.com/questions/tagged/fluent-migrator) [![NuGet downloads](https://img.shields.io/nuget/dt/FluentMigrator.svg)](https://www.nuget.org/packages/FluentMigrator/) [![Build Status](https://dev.azure.com/fluentmigrator/fluentmigrator/_apis/build/status%2Ffluentmigrator%20CI%20for%20PRs?branchName=main)](https://dev.azure.com/fluentmigrator/fluentmigrator/_build/latest?definitionId=7&branchName=main) ![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/fluentmigrator/fluentmigrator/1)

test
[Fluent Migrator](https://github.com/fluentmigrator/fluentmigrator) is a migration framework for .NET much like Ruby on Rails Migrations. Migrations are a structured way to alter your database schema and are an alternative to creating lots of sql scripts that have to be run manually by every developer involved. Migrations solve the problem of evolving a database schema for multiple databases (for example, the developer's local database, the test database and the production database). Database schema changes are described in classes written in C# that can be checked into a version control system.

# News

6.2.0 is released and supports .NET 6, .NET 7, and .NET 8.
In 6.0.0, we have begun removing a lot of `[Obsolete]` code. Very little user impact should be seen, other than updating custom VersionTableMetadata to configure CreateWithPrimaryKey setting.
Thanks to @eloekset, our [documentation website](https://fluentmigrator.github.io) now has 5.0.0 API links!

Please read the [changelog](https://github.com/fluentmigrator/fluentmigrator/blob/main/CHANGELOG.md)
or the upgrade guide for further information: [2.x to 3.0](https://fluentmigrator.github.io/articles/guides/upgrades/guide-2.0-to-3.0.html?tabs=di).

There should not be a whole lot to do to upgrade from 3.0 to 5.0. As questions arise, we will consider adding a specific guide.

# Packages

Package Source      | Status   | Source Code Tree
--------------------|----------|-----------------
NuGet (Releases)    | [![NuGet](https://img.shields.io/nuget/v/FluentMigrator.svg)](https://www.nuget.org/packages/FluentMigrator/) | [main](https://github.com/fluentmigrator/fluentmigrator/tree/main)
Azure Artifacts (Prerelease)  | [![Azure Artifacts](https://feeds.dev.azure.com/fluentmigrator/22b31067-b424-416b-b89f-682210a37a55/_apis/public/Packaging/Feeds/55481ff8-c55e-44b4-ad6e-b6638cc22c2b/Packages/d298bf9a-9246-4834-a1ad-a056e046513a/Badge)](https://dev.azure.com/fluentmigrator/fluentmigrator/_packaging?_a=feed&feed=fluentmigrator) | [develop](https://github.com/fluentmigrator/fluentmigrator/tree/develop)

The releases are stored on [nuget.org](https://nuget.org)
while the CI builds are stored on [Azure Artifacts](https://dev.azure.com/fluentmigrator/fluentmigrator/_packaging?_a=feed&feed=fluentmigrator).

:warning: The badge for the Azure Artifacts feed won't display prereleases.  [We're looking into this](https://github.com/fluentmigrator/fluentmigrator/issues/1180#issuecomment-662884030).

# Project Info

|                           |         |
|---------------------------|---------|
| **Documentation**         | [On our GitHub pages](https://fluentmigrator.github.io) |
| **Discussions**           | [![GitHub Discussions](https://img.shields.io/github/discussions/fluentmigrator/fluentmigrator.svg)](https://github.com/fluentmigrator/fluentmigrator/discussions) |
| **Bug/Feature Tracking**  | [![GitHub issues](https://img.shields.io/github/issues/fluentmigrator/fluentmigrator.svg)](https://github.com/fluentmigrator/fluentmigrator/issues) |
| **Build server (new)**    | [![AzureDevOps](https://img.shields.io/azure-devops/build/fluentmigrator/22b31067-b424-416b-b89f-682210a37a55/1)](https://dev.azure.com/fluentmigrator/fluentmigrator/_build?definitionId=1) |

# Prerequisites

| Tool                              | Consequences when not installed |
|-----------------------------------|---------------------------------|
| [Multilingual App Toolkit Editor](https://developer.microsoft.com/en-us/windows/develop/multilingual-app-toolkit) | You're unable to create translations. |
| [Multilingual App Toolkit Extension (VS2017+)](https://marketplace.visualstudio.com/items?itemName=MultilingualAppToolkit.MultilingualAppToolkit-18308) | You get a compilation warning and the changed translation doesn't get compiled. |

# Powered by

<span>
<a href="https://www.jetbrains.com"><img src="https://raw.githubusercontent.com/fluentmigrator/fluentmigrator/main/docs/jetbrains/jetbrains.png" alt="JetBrains"  width="15%" /></a>
<a href="https://www.jetbrains.com/resharper"><img src="https://raw.githubusercontent.com/fluentmigrator/fluentmigrator/main/docs/jetbrains/logo.png" alt="ReSharper"  width="15%" /></a>
</span>

<a href="https://azure.microsoft.com/en-us/services/devops/"><img src="https://azurecomcdn.azureedge.net/cvt-2b18021399b1b3aa2c405a40ce4e9b89f162d9e5b3d6df838d13aae49f3608ea/images/shared/services/devops/pipelines-icon-80.png" alt="Azure DevOps"  width="20%" /></a>

# Contributors

A [long list](https://github.com/fluentmigrator/fluentmigrator/blob/main/CONTRIBUTORS.md) of everyone that has contributed to FluentMigrator. Thanks for all the Pull Requests!

# Contributing

Please see our guide on [how to contribute](https://fluentmigrator.github.io/articles/guides/contribution.html)

# Third Party Contributions / FluentMigrator Ecosystem

FluentMigrator has an actively developed and maintained ecosystem thanks to third party contributions. The following table summarizes some contributions (but are not endorsed):

| GitHub/BitBucket | NuGet Package | Description |
| ---------------- | ------------- | ----------- |
| [EasyMigrator](https://github.com/qstarin/EasyMigrator) | [EasyMigrator.FluentMigrator](https://www.nuget.org/packages/EasyMigrator.FluentMigrator) | EasyMigrator allows you to specify database schema using simple POCOs with minimally attributed fields to represent columns. EasyMigrator's core can be adapted to sit on top of various migration libraries. |
| [FluentMigrator-Generator](https://github.com/ritterim/fluentmigrator-generator) | [FluentMigrator.Generator](https://www.nuget.org/packages/FluentMigrator.Generator/) | Adds a command to the package manager console to generate migrations for FluentMigrator. |
| [AspNetBoilerplate](https://github.com/aspnetboilerplate/aspnetboilerplate) | [Abp.FluentMigrator](https://www.nuget.org/packages/Abp.FluentMigrator) | Adds fluent extensions specific to the entity model used by the ASP.NET Boilerplate architecture |
| [Alt.FluentMigrator.VStudio](https://github.com/crimcol/Alt.FluentMigrator.VStudio) | [Alt.FluentMigrator.VStudio](https://www.nuget.org/packages/Alt.FluentMigrator.VStudio/) | Adds set of commands for Package Manager console:<br> - Add-FluentMigration<br> - Update-FluentDatabase<br> - Rollback-FluentDatabase |
| [FAKE.FluentMigrator](https://github.com/fsharp/FAKE/blob/694f616c97fa242162cfd36db905d7df3156018f/help/markdown/todo-fluentmigrator.md) | [FAKE.FluentMigrator](https://www.nuget.org/packages/FAKE.FluentMigrator/) | FluentMigrator is a .NET library which helps to version database schema using incremental migrations which are described in C#. The basic idea of the FAKE helper is to run FluentMigrator over the existing database using compiled assembly with migrations. |
