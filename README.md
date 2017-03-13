# FluentMigrator [![http://badge.fury.io/nu/fluentmigrator](https://badge.fury.io/nu/fluentmigrator.png)](http://badge.fury.io/nu/fluentmigrator)

Fluent Migrator is a migration framework for .NET much like Ruby on Rails Migrations. Migrations are a structured way to alter your database schema and are an alternative to creating lots of sql scripts that have to be run manually by every developer involved. Migrations solve the problem of evolving a database schema for multiple databases (for example, the developer's local database, the test database and the production database). Database schema changes are described in classes written in C# that can be checked into a version control system.

## Project Info

* **Documentation**: [https://github.com/fluentmigrator/fluentmigrator/wiki](https://github.com/fluentmigrator/fluentmigrator/wiki)
* **Discussions**: [fluentmigrator-google-group@googlegroups.com](http://groups.google.com/group/fluentmigrator-google-group)
* **Bug/Feature Tracking**: [http://github.com/fluentmigrator/fluentmigrator/issues](http://github.com/fluentmigrator/fluentmigrator/issues)
* **TeamCity sources**: [http://teamcity.codebetter.com/viewType.html?buildTypeId=bt82&tab=buildTypeStatusDiv](http://teamcity.codebetter.com/viewType.html?buildTypeId=bt82&tab=buildTypeStatusDiv)
  * Click the "Login as guest" link in the footer of the page.

## Build Status

The build is generously hosted and run on the [CodeBetter TeamCity](http://codebetter.com/codebetter-ci/) infrastructure.
Latest build status: [![TeamCity status](http://teamcity.codebetter.com/app/rest/builds/buildType:(id:bt82)/statusIcon)](http://teamcity.codebetter.com/viewType.html?buildTypeId=bt82&guest=1)

Our Mono build is hosted on Travis CI.
Latest Mono build status: [![Travis CI status](https://travis-ci.org/fluentmigrator/fluentmigrator.svg?branch=master)](https://travis-ci.org/fluentmigrator/fluentmigrator)

## Build instructions

### Prerequisites

* Ruby 2.2.4

Gems:

* Rake 10.5.0
* albacore
* version_bumper

### Creating the nuget packages

```
tools\NuGet.exe restore FluentMigrator.sln
rake nuget:create_nugets
```

This will also build the whole solution.

## Powered by

![ReSharper](http://www.jetbrains.com/img/logos/logo_resharper_small.gif)

## Contributors

A [long list](https://github.com/fluentmigrator/fluentmigrator/wiki/ContributorList) of everyone that has contributed to FluentMigrator. Thanks for all the Pull Requests!

## License

[Apache 2 License](https://github.com/fluentmigrator/fluentmigrator/blob/master/LICENSE.txt)
