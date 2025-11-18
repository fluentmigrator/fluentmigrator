# Installation

FluentMigrator can be installed in several ways depending on your project type and preferences.

## Package Installation

### Core Packages

All projects need these base packages:

* [FluentMigrator](https://www.nuget.org/packages/FluentMigrator/)
* [FluentMigrator.Runner](https://www.nuget.org/packages/FluentMigrator.Runner/)

### Database Provider Packages

Choose the package for your database provider:

* [SQL Server](https://www.nuget.org/packages/FluentMigrator.Runner.SqlServer/)
* [PostgreSQL](https://www.nuget.org/packages/FluentMigrator.Runner.Postgres/)
* [MySQL / MariaDB](https://www.nuget.org/packages/FluentMigrator.Runner.MySql/)
* [SQLite](https://www.nuget.org/packages/FluentMigrator.Runner.SQLite/)
* [Oracle](https://www.nuget.org/packages/FluentMigrator.Runner.Oracle/)
* [Firebird](https://www.nuget.org/packages/FluentMigrator.Runner.Firebird/)
* [IBM DB2](https://www.nuget.org/packages/FluentMigrator.Runner.Db2/)
* [SAP HANA](https://www.nuget.org/packages/FluentMigrator.Runner.Hana/)
* [Snowflake](https://www.nuget.org/packages/FluentMigrator.Runner.Snowflake/)
* [Amazon Redshift](https://www.nuget.org/packages/FluentMigrator.Runner.Redshift/)

## Installation Methods

### .NET CLI
```bash
dotnet add package FluentMigrator
dotnet add package FluentMigrator.Runner
# And then the provider package you need ...
```

### Package Manager Console (Visual Studio)
```powershell
Install-Package FluentMigrator
Install-Package FluentMigrator.Runner
# And then the provider package you need ...
```

### Package Manager UI
1. Right-click on your project in Solution Explorer
2. Select "Manage NuGet Packages"
3. Search for "FluentMigrator" and install the required packages

## Command Line Tools

### .NET Tool
Install the FluentMigrator .NET tool for command-line usage:

```bash
dotnet tool install -g FluentMigrator.DotNet.Cli
```

Usage:
```bash
dotnet fm migrate -p sqlserver -c "Server=.;Database=MyApp;Trusted_Connection=true;" -a "MyApp.dll"
```

### MSBuild Integration
For MSBuild integration, add the MSBuild package.

```bash
dotnet add package FluentMigrator.MSBuild
```

```powershell
Install-Package FluentMigrator.MSBuild
```

## Project Templates

### Console Application Template

Create a new console application for running migrations:

```bash
dotnet new console -n MyApp.Migrations
cd MyApp.Migrations
dotnet add package FluentMigrator
dotnet add package FluentMigrator.Runner
dotnet add package FluentMigrator.Runner.SqlServer
```

### Web Application Integration

For ASP.NET Core applications, add migration support to your existing web project:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="FluentMigrator" Version="7.2.0" /> <!-- Use the latest stable version -->
    <PackageReference Include="FluentMigrator.Runner" Version="7.2.0" />
    <PackageReference Include="FluentMigrator.Runner.SqlServer" Version="7.2.0" />
  </ItemGroup>
</Project>
```

## Framework Support

FluentMigrator supports multiple .NET frameworks:

- **.NET 8.0+** ✅ (Recommended)
- **.NET Framework 4.8** ✅

## Version Compatibility

| FluentMigrator Version | .NET Version | Status      |
|------------------------|--------------|-------------|
| 7+                     | .NET 8+      | Current     |
| 5+                     | .NET 5+      | Legacy      |
| 3.x                    | .NET Core 2+ | Unsupported |

## Verification

After installation, verify that FluentMigrator is properly installed by creating a simple migration:

```csharp
using FluentMigrator;

[Migration(1)]
public class TestMigration : Migration
{
    public override void Up()
    {
        // Migration code here
    }

    public override void Down()
    {
        // Rollback code here
    }
}
```

If the code compiles without errors, FluentMigrator is correctly installed.

## Common Installation Issues

### Package Conflicts
If you encounter package conflicts, ensure all FluentMigrator packages are the same version:

```xml
<PackageReference Include="FluentMigrator" Version="7.2.0" /> <!-- Use the latest stable version -->
<PackageReference Include="FluentMigrator.Runner" Version="7.2.0" />
<PackageReference Include="FluentMigrator.Runner.SqlServer" Version="7.2.0" />
```

### Missing Database Provider
Error: "No database provider registered"

Solution: Install the appropriate database provider package for your database.

### Assembly Loading Issues
If you get assembly loading errors, ensure your target framework is compatible and all dependencies are properly installed.

Se e the [FAQ](/intro/faq.md) for more help.

## Next Steps

Once FluentMigrator is installed, proceed to:
- [Configuration](./configuration.md) - Comprehensive configuration guide for all scenarios
- [Quick Start Guide](./quick-start.md) - Create your first migration
- [Creating Tables](/operations/create-tables.md) - Learn the table creation API
- [Database Providers](/providers/sql-server.md) - Provider-specific configuration
