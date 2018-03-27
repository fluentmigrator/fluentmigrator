# Goals and changes for 2.x

## New features

- Using the .NET Core CLI tooling
- .NET Standard 2.0 support
- .NET Framework 4.5 support

## Breaking changes

- `FluentMigrator` package only contains the core functionality
  - All tools are now only in `FluentMigrator.Tools`
- Removal of deprecated functions
- Moving database specific code from `FluentMigrator.Runner` to `FluentMigrator.Runner.<Database>`
- Removal of .NET Framework 3.5 support
- NAnt build task is published as ZIP
- MSBuild task is available as separate package (with custom .targets file)
- Removal of SchemaDump and T4 experiments

# How to build

## Windows

```
dotnet restore
dotnet build
```

## Linux

```
dotnet restore
msbuild FluentMigrator.sln
```

# How to test

## Windows

```
dotnet vstest test/FluentMigrator.Tests/bin/Debug/net452/FluentMigrator.Tests.dll --TestCaseFilter:"TestCategory!=Integration"
```

## Linux

```
dotnet vstest test/FluentMigrator.Tests/bin/Debug/net452/FluentMigrator.Tests.dll --TestCaseFilter:'TestCategory!=Integration&TestCategory!=NotWorkingOnMono'
```
