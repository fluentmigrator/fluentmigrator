# FluentMigrator [![(License)](https://img.shields.io/github/license/fluentmigrator/fluentmigrator.svg)](https://github.com/fluentmigrator/fluentmigrator/blob/master/LICENSE.txt)

Fluent Migrator is a migration framework for .NET much like Ruby on Rails Migrations. Migrations are a structured way to alter your database schema and are an alternative to creating lots of sql scripts that have to be run manually by every developer involved. Migrations solve the problem of evolving a database schema for multiple databases (for example, the developer's local database, the test database and the production database). Database schema changes are described in classes written in C# that can be checked into a version control system.

# News

3.0.0 is released and goes full "dependency injection".
We also have a new [documentation website](https://fluentmigrator.github.io)!

Please read the [changelog](https://github.com/fluentmigrator/fluentmigrator/blob/master/CHANGELOG.md)
or the upgrade guide for further information: [2.x to 3.0](https://fluentmigrator.github.io/articles/guides/upgrades/guide-2.0-to-3.0.html?tabs=di).

# Packages

Source              | Status
--------------------|----------
NuGet (Releases)    | [![NuGet](https://img.shields.io/nuget/v/FluentMigrator.svg)](https://www.nuget.org/packages/FluentMigrator/)
MyGet (Prerelease)  | [![MyGet](https://img.shields.io/myget/fluent-migrator/vpre/FluentMigrator.svg)](https://www.myget.org/gallery/fluent-migrator)

The releases are stored on [nuget.org](https://nuget.org)
while the CI builds are stored on [MyGet](https://www.myget.org/feed/Packages/fluent-migrator).

# Project Info

|                           |         |
|---------------------------|---------|
| **Documentation**         | [On our GitHub pages](https://fluentmigrator.github.io) |
| **Discussions**           | [![Gitter](https://img.shields.io/gitter/room/FluentMigrator/fluentmigrator.svg)](https://gitter.im/FluentMigrator/fluentmigrator) |
| **Bug/Feature Tracking**  | [![GitHub issues](https://img.shields.io/github/issues/fluentmigrator/fluentmigrator.svg)](https://github.com/fluentmigrator/fluentmigrator/issues) |
| **Build server**          | [![Travis](https://img.shields.io/travis/fluentmigrator/fluentmigrator.svg)](https://travis-ci.org/fluentmigrator/fluentmigrator) |

# Prerequisites

| Tool                              | Consequences when not installed |
|-----------------------------------|---------------------------------|
| [Multilingual App Toolkit Editor](https://developer.microsoft.com/en-us/windows/develop/multilingual-app-toolkit) | You're unable to create translations. |
| [Multilingual App Toolkit Extension (VS2017+)](https://marketplace.visualstudio.com/items?itemName=MultilingualAppToolkit.MultilingualAppToolkit-18308) | You get a compilation warning and the changed translation doesn't get compiled. |

# Powered by

<span>
<a href="https://www.jetbrains.com"><img src="https://raw.githubusercontent.com/fluentmigrator/fluentmigrator/master/docs/jetbrains/jetbrains.png" alt="JetBrains"  width="15%" /></a>
<a href="https://www.jetbrains.com/resharper"><img src="https://raw.githubusercontent.com/fluentmigrator/fluentmigrator/master/docs/jetbrains/logo.png" alt="ReSharper"  width="15%" /></a>
</span>

<a href="https://travis-ci.com"><img src="https://travis-ci.com/images/logos/TravisCI-Full-Color.png" alt="Travis CI"  width="20%" /></a>

# Contributors

A [long list](https://github.com/fluentmigrator/fluentmigrator/blob/master/CONTRIBUTORS.md) of everyone that has contributed to FluentMigrator. Thanks for all the Pull Requests!

# Contributing

Please see our guide on [how to contribute](https://fluentmigrator.github.io/articles/guides/contribution.html)

# Third Party Contributions / FluentMigrator Ecosystem

FluentMigrator has an actively developed and maintained ecosystem thanks to third party contributions. The following table summarizes some contributions (but are not endorsed):

| GitHub/BitBucket | NuGet Package | Description |
| ---------------- | ------------- | ----------- |
| [EasyMigrator](https://bitbucket.org/quentin-starin/easymigrator/wiki/Home) | [EasyMigrator.FluentMigrator](https://www.nuget.org/packages/EasyMigrator.FluentMigrator) | EasyMigrator allows you to specify database schema using simple POCOs with minimally attributed fields to represent columns. EasyMigrator's core can be adapted to sit on top of various migration libraries. |
| [FluentMigrator-Generator](https://github.com/ritterim/fluentmigrator-generator) | [FluentMigrator.Generator](https://www.nuget.org/packages/FluentMigrator.Generator/) | Adds a command to the package manager console to generate migrations for FluentMigrator. |
| [AspNetBoilerplate](https://github.com/aspnetboilerplate/aspnetboilerplate) | [Abp.FluentMigrator](https://www.nuget.org/packages/Abp.FluentMigrator) | Adds fluent extensions specific to the entity model used by the ASP.NET Boilerplate architecture |
