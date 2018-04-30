---
uid: migration-runners.md
title: Migration runners
---

# Migration runners

We have three migration runners you can choose from.

Please use the in-process runner if possible.

# In-Process

This is an example of using the in-process migration runner:

[!code-cs[Program.cs](quickstart/Program.cs "Migrating the database")]

# `Migrate.exe` (FluentMigrator.Console package)

This is a console tool that works also with the .NET Framework outside of the .NET Core ecosystem.

## [Visual Studio: Install with the package manager console](#tab/vs-pkg-manager-console)

Install the package with:

```
Install-Package FluentMigrator.Console
```

## [NuGet: Install with the nuget.exe tool](#tab/nuget)

Install the package with:

```
nuget install FluentMigrator.Console -ExcludeVersion
```

***

Now, you can find the tool in the path `FluentMigrator[package-version]/tools/<target-framework>/[optional-platform/]Migrate.exe`.

> [!NOTE]
> The `package-verion` is only part of the path when the tool was installed using the Visual Studio package manager console.

> [!IMPORTANT]
> Choose the correct `target-framework`. Otherwise, the tool might not be able to load your assembly.

`target-framework` | platform | `optional-platform` exists? | path
-------------------|----------|-----------------------------|---------
`net40` | `x86`     | yes | `tools/net40/x86/Migrate.exe`
`net40` | `x64`     | yes | `tools/net40/x64/Migrate.exe`
`net40` | `AnyCPU`  | no  | `tools/net40/Migrate.exe`
`net45` | `x86`     | yes | `tools/net45/x86/Migrate.exe`
`net45` | `x64`     | yes | `tools/net45/x64/Migrate.exe`
`net45` | `AnyCPU`  | no  | `tools/net45/Migrate.exe`
`net452` | `x86`     | yes | `tools/net452/x86/Migrate.exe`
`net452` | `x64`     | yes | `tools/net452/x64/Migrate.exe`
`net452` | `AnyCPU`  | yes | `tools/net452/any/Migrate.exe`

> [!IMPORTANT]
> On non-Windows platforms, you have to install/use mono.

# `dotnet fm` (FluentMigrator.DotNet.Cli tool)

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
