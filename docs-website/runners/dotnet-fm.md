# dotnet-fm CLI Tool

The `dotnet-fm` CLI tool provides cross-platform migration execution for .NET Core and .NET 5+ applications, integrating seamlessly with the .NET CLI toolchain.

## Installation

### Global Installation (Recommended)
```bash
dotnet tool install -g FluentMigrator.DotNet.Cli
```

### Local Installation
```bash
# Create tool manifest (if not exists)
dotnet new tool-manifest

# Install locally
dotnet tool install FluentMigrator.DotNet.Cli

# Use via dotnet run
dotnet tool run dotnet-fm --help
```

### Update Tool
```bash
# Update global tool
dotnet tool update -g FluentMigrator.DotNet.Cli

# Update local tool
dotnet tool update FluentMigrator.DotNet.Cli
```

::: tip Requirements
You need at least .NET Core 2.1 preview 2 SDK or later to use this tool.
:::

## Configuration

The dotnet-fm CLI tool provides comprehensive configuration options through command-line parameters. For configuration concepts and advanced scenarios, see the [Configuration Guide](/intro/configuration.md).

## Basic Usage

### Migrate to Latest Version
```bash
dotnet fm migrate -p sqlite -c "Data Source=test.db" -a "MyApp.dll"
```

### Migrate to Specific Version
```bash
dotnet fm migrate -p sqlserver -c "Server=.;Database=MyDb;Integrated Security=true" -a "MyApp.dll" --version 20200801120000
```

### List Available Migrations
```bash
dotnet fm list migrations -p sqlserver -c "Server=.;Database=MyDb;Integrated Security=true" -a "MyApp.dll"
```

## Command Reference

### migrate Command
Executes database migrations.

```bash
dotnet fm migrate [options]
```

**Options:**
| Option | Short | Description | Required |
|--------|-------|-------------|----------|
| `--provider` | `-p` | Database provider | Yes |
| `--connectionString` | `-c` | Connection string | Yes |
| `--assembly` | `-a` | Migration assembly path | Yes |
| `--version` | `-v` | Target version | No |
| `--profile` | | Profile name to execute | No |
| `--tag` | `-t` | Filter by tag | No |
| `--output` | `-o` | Output SQL to console | No |
| `--outputFileName` | | Output file name | No |
| `--preview` | | Preview without execution | No |
| `--steps` | `-s` | Number of migration steps | No |
| `--workingDirectory` | `-w` | Working directory | No |
| `--timeout` | | Command timeout (seconds) | No |
| `--transaction-mode` | | Transaction behavior | No |
| `--allowDirtyAssemblies` | | Allow version mismatches | No |
| `--context` | | Application context | No |
| `--startVersion` | | Start from version | No |
| `--noConnection` | | Validate without connecting | No |
| `--help` | `-h` | Show help | No |

### list Command
Lists migrations and their status.

```bash
# List all migrations
dotnet fm list migrations -p sqlserver -c "..." -a "MyApp.dll"

# List applied migrations only
dotnet fm list applied -p sqlserver -c "..." -a "MyApp.dll"

# List unapplied migrations only
dotnet fm list unapplied -p sqlserver -c "..." -a "MyApp.dll"
```

### rollback Command
Rollback migrations to a specific version.

```bash
# Rollback to specific version
dotnet fm rollback -p sqlserver -c "..." -a "MyApp.dll" --version 20200701000000

# Rollback by steps
dotnet fm rollback -p sqlserver -c "..." -a "MyApp.dll" --steps 3
```

### validate Command
Validates migrations without executing them.

```bash
dotnet fm validate -p sqlserver -c "..." -a "MyApp.dll"
```

## Database Providers

### SQL Server
```bash
# SQL Server 2016+
dotnet fm migrate -p sqlserver2016 -c "Server=.;Database=MyDb;Integrated Security=true" -a "MyApp.dll"

# SQL Server with specific version
dotnet fm migrate -p sqlserver2014 -c "..." -a "MyApp.dll"
```

### PostgreSQL
```bash
# PostgreSQL
dotnet fm migrate -p postgres -c "Host=localhost;Database=mydb;Username=user;Password=pass" -a "MyApp.dll"

# PostgreSQL with specific version
dotnet fm migrate -p postgresql15_0 -c "..." -a "MyApp.dll"
```

### MySQL
```bash
# MySQL 5.x
dotnet fm migrate -p mysql5 -c "Server=localhost;Database=mydb;Uid=user;Pwd=password;" -a "MyApp.dll"

# MySQL 8.x
dotnet fm migrate -p mysql8 -c "..." -a "MyApp.dll"
```

### SQLite
```bash
dotnet fm migrate -p sqlite -c "Data Source=mydb.db" -a "MyApp.dll"
```

### Oracle
```bash
# Oracle managed driver
dotnet fm migrate -p oraclemanaged -c "Data Source=localhost:1521/xe;User Id=user;Password=pass;" -a "MyApp.dll"
```

## Project Integration

### .NET Core Project Setup

**MyApp.csproj**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="FluentMigrator" Version="3.3.2" />
    <PackageReference Include="FluentMigrator.Runner" Version="3.3.2" />
    <PackageReference Include="FluentMigrator.Runner.SqlServer" Version="3.3.2" />
  </ItemGroup>
  
  <!-- Local tool installation -->
  <ItemGroup>
    <DotNetCliToolReference Include="FluentMigrator.DotNet.Cli" Version="3.3.2" />
  </ItemGroup>
</Project>
```

### Build Script Integration
```bash
#!/bin/bash

# Build the project
dotnet build --configuration Release

# Run migrations
dotnet fm migrate \
  -p sqlserver \
  -c "$CONNECTION_STRING" \
  -a "./bin/Release/net6.0/MyApp.dll"
```

## Advanced Usage Examples

### Environment-Specific Migrations
```bash
# Development environment
dotnet fm migrate -p sqlite -c "Data Source=dev.db" -a "MyApp.dll" --profile Development

# Production environment
dotnet fm migrate -p sqlserver -c "$PROD_CONNECTION_STRING" -a "MyApp.dll" --profile Production
```

### Output SQL Script
```bash
# Generate SQL script without execution
dotnet fm migrate -p sqlserver -c "..." -a "MyApp.dll" --output --outputFileName "migration-script.sql"
```

### Preview Mode
```bash
# Preview what would be executed
dotnet fm migrate -p sqlserver -c "..." -a "MyApp.dll" --preview
```

### Step-by-Step Migration
```bash
# Migrate up 2 versions
dotnet fm migrate -p sqlserver -c "..." -a "MyApp.dll" --steps 2

# Rollback 1 version
dotnet fm rollback -p sqlserver -c "..." -a "MyApp.dll" --steps 1
```

### Working with Assembly Dependencies
```bash
# Specify working directory for dependency resolution
dotnet fm migrate -p sqlserver -c "..." -a "MyApp.dll" --workingDirectory "./bin/Release/net6.0/"
```

## CI/CD Integration

### GitHub Actions
```yaml
name: Database Migration

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  migrate:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 6.0.x
        
    - name: Install dotnet-fm
      run: dotnet tool install -g FluentMigrator.DotNet.Cli
      
    - name: Build
      run: dotnet build --configuration Release
      
    - name: Run migrations
      run: |
        dotnet fm migrate \
          -p sqlserver \
          -c "${{ secrets.DATABASE_CONNECTION_STRING }}" \
          -a "./bin/Release/net6.0/MyApp.dll"
```

### Azure DevOps Pipeline
```yaml
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'

steps:
- task: UseDotNet@2
  inputs:
    version: '6.0.x'

- script: dotnet tool install -g FluentMigrator.DotNet.Cli
  displayName: 'Install dotnet-fm'

- script: dotnet build --configuration $(buildConfiguration)
  displayName: 'Build project'

- script: |
    dotnet fm migrate \
      -p sqlserver \
      -c "$(DatabaseConnectionString)" \
      -a "./bin/$(buildConfiguration)/net6.0/MyApp.dll"
  displayName: 'Run database migrations'
```

### Docker Integration
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Install dotnet-fm
RUN dotnet tool install -g FluentMigrator.DotNet.Cli
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy and build
COPY . .
RUN dotnet build --configuration Release

# Migration entrypoint
ENTRYPOINT ["dotnet", "fm", "migrate"]
```

**Usage:**
```bash
docker run --rm my-migration-image \
  -p sqlserver \
  -c "Server=db;Database=MyDb;User=sa;Password=MyPass;" \
  -a "./bin/Release/net6.0/MyApp.dll"
```

## Configuration Files

### appsettings.json Integration
While `dotnet-fm` doesn't directly read configuration files, you can use environment variables:

```bash
# Load from environment
export CONNECTION_STRING="Server=.;Database=MyDb;Integrated Security=true"
dotnet fm migrate -p sqlserver -c "$CONNECTION_STRING" -a "MyApp.dll"
```

### Custom Scripts
```bash
#!/bin/bash
# migrate.sh - Custom migration script

set -e

PROVIDER="sqlserver"
ASSEMBLY="./bin/Release/net6.0/MyApp.dll"

# Load connection string from config
CONNECTION_STRING=$(dotnet run --project ./ConfigReader/ -- --connectionString)

echo "Running migrations with provider: $PROVIDER"
echo "Assembly: $ASSEMBLY"

dotnet fm migrate -p "$PROVIDER" -c "$CONNECTION_STRING" -a "$ASSEMBLY" "$@"

echo "Migration completed successfully!"
```

## Troubleshooting

### Version Compatibility Issues
```bash
# Allow loading assemblies with different .NET versions
dotnet fm migrate -p sqlserver -c "..." -a "MyApp.dll" --allowDirtyAssemblies
```

### Assembly Loading Problems
```bash
# Specify working directory for dependency resolution
dotnet fm migrate -p sqlserver -c "..." -a "MyApp.dll" -w "./bin/Release/net6.0/"
```

### Connection Testing
```bash
# Validate migrations without connecting to database
dotnet fm validate -p sqlserver -c "..." -a "MyApp.dll" --noConnection
```

### Verbose Output
```bash
# Enable detailed logging (if supported by specific version)
dotnet fm migrate -p sqlserver -c "..." -a "MyApp.dll" --verbose
```

## Common Issues

### `FileLoadException`
If you encounter version conflicts:
```bash
# Use allowDirtyAssemblies flag
dotnet fm migrate --allowDirtyAssemblies -p sqlserver -c "..." -a "MyApp.dll"
```

### Missing Dependencies
Ensure all dependencies are in the same directory:
```bash
# Publish to single directory first
dotnet publish --configuration Release --output ./publish
dotnet fm migrate -p sqlserver -c "..." -a "./publish/MyApp.dll"
```

### Target Framework Mismatches
Use the correct .NET version:
```bash
# Check your target framework
dotnet --info
# Ensure dotnet-fm tool matches your project's target framework
```

## Best Practices

### ✅ Do
- Use global tool installation for consistent CLI experience
- Include dotnet-fm in your CI/CD pipelines
- Test migrations with `--preview` flag first
- Use environment variables for sensitive connection strings
- Specify working directory when dealing with complex dependencies

### ❌ Don't
- Hardcode sensitive information in command lines
- Skip testing migrations in staging environments
- Ignore tool version compatibility
- Mix local and global tool installations without purpose

## Migration from Other Tools

### From Migrate.exe
```bash
# Old: Migrate.exe -p sqlserver -c "..." -a "MyApp.dll"
# New: 
dotnet fm migrate -p sqlserver -c "..." -a "MyApp.dll"
```

### From In-Process Runner
Consider keeping both approaches - use `dotnet-fm` for deployment scripts and in-process for application integration.