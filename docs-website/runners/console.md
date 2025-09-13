# Console Migration Runner (Migrate.exe)

The console migration runner is a standalone executable that works with .NET Framework applications and can be used in build scripts and deployment automation.

## Installation

### Via Package Manager Console
```powershell
Install-Package FluentMigrator.Console
```

### Via NuGet CLI
```bash
nuget install FluentMigrator.Console -ExcludeVersion
```

## Location and Platform Support

After installation, find the tool at:
```
FluentMigrator[.version]/tools/<target-framework>/[platform/]Migrate.exe
```

### Available Platforms

| Target Framework | Platform | Path                          |
|------------------|----------|-------------------------------|
| net48            | x86      | `tools/net48/x86/Migrate.exe` |
| net48            | x64      | `tools/net48/x64/Migrate.exe` |
| net48            | AnyCPU   | `tools/net48/any/Migrate.exe` |

::: warning Platform Compatibility
Choose the correct target framework that matches your migration assembly. Otherwise, the tool might not be able to load your assembly.
:::

::: tip Cross-Platform
On non-Windows platforms, you need to install and use Mono to run the console tool.
:::

## Command Line Configuration

The console runner provides extensive command-line options for configuration. For detailed configuration concepts and advanced scenarios, see the [Configuration Guide](/intro/configuration.md).

### Basic Usage

### Migrate Up (Latest)
```bash
Migrate.exe -p sqlserver -c "Server=.;Database=MyDb;Integrated Security=true" -a "MyApp.dll"
```

### Migrate to Specific Version
```bash
Migrate.exe -p sqlserver -c "Server=.;Database=MyDb;Integrated Security=true" -a "MyApp.dll" --version 20200801120000
```

### Rollback Migrations
```bash
Migrate.exe -p sqlserver -c "Server=.;Database=MyDb;Integrated Security=true" -a "MyApp.dll" --version 0
```

## Command Line Parameters

### Required Parameters

| Parameter            | Short | Description                | Example                          |
|----------------------|-------|----------------------------|----------------------------------|
| `--provider`         | `-p`  | Database provider          | `sqlserver`, `postgres`, `mysql` |
| `--connectionString` | `-c`  | Database connection string | `"Server=.;Database=MyDb;..."`   |
| `--assembly`         | `-a`  | Path to migration assembly | `"MyApp.dll"`                    |

### Optional Parameters

| Parameter                | Short | Description                           | Default         |
|--------------------------|-------|---------------------------------------|-----------------|
| `--version`              | `-v`  | Target version (0 for rollback)       | Latest          |
| `--profile`              |       | Profile to execute                    | None            |
| `--tag`                  | `-t`  | Filter migrations by tag              | None            |
| `--output`               | `-o`  | Output generated SQL to file          | None            |
| `--outputFileName`       |       | Name of output file                   | `migration.sql` |
| `--preview`              |       | Preview SQL without execution         | `false`         |
| `--steps`                | `-s`  | Number of versions to migrate         | All             |
| `--workingDirectory`     | `-w`  | Working directory                     | Current         |
| `--timeout`              |       | Command timeout in seconds            | `60`            |
| `--context`              |       | Application context                   | None            |
| `--transaction-mode`     |       | Transaction behavior                  | `true`          |
| `--allowDirtyAssemblies` |       | Allow loading different .NET versions | `false`         |

## Configuration Options

### Database Provider Configuration

The console runner supports all FluentMigrator database providers through command-line switches. See the [Configuration Guide](/intro/configuration.md#database-provider-configuration) for comprehensive provider setup.

### Database Providers

### SQL Server
```bash
# SQL Server 2016+
Migrate.exe -p sqlserver2016 -c "Server=.;Database=MyDb;Integrated Security=true" -a "MyApp.dll"

# Older SQL Server versions
Migrate.exe -p sqlserver2008 -c "..." -a "MyApp.dll"
```

### PostgreSQL
```bash
Migrate.exe -p postgres -c "Host=localhost;Database=mydb;Username=user;Password=pass" -a "MyApp.dll"
```

### MySQL
```bash
Migrate.exe -p mysql5 -c "Server=localhost;Database=mydb;Uid=user;Pwd=password;" -a "MyApp.dll"
```

### SQLite
```bash
Migrate.exe -p sqlite -c "Data Source=mydb.db" -a "MyApp.dll"
```

### Oracle
```bash
Migrate.exe -p oracle -c "Data Source=localhost:1521/xe;User Id=user;Password=pass;" -a "MyApp.dll"
```

## Advanced Usage Examples

### Output SQL Without Execution
```bash
Migrate.exe -p sqlserver -c "..." -a "MyApp.dll" --output --outputFileName "migration-script.sql"
```

### Execute Specific Profile
```bash
Migrate.exe -p sqlserver -c "..." -a "MyApp.dll" --profile Development
```

### Filter by Tags
```bash
Migrate.exe -p sqlserver -c "..." -a "MyApp.dll" --tag Production
```

### Preview Changes
```bash
Migrate.exe -p sqlserver -c "..." -a "MyApp.dll" --preview
```

### Migrate Specific Number of Steps
```bash
# Migrate up 3 versions
Migrate.exe -p sqlserver -c "..." -a "MyApp.dll" --steps 3

# Rollback 2 versions
Migrate.exe -p sqlserver -c "..." -a "MyApp.dll" --steps -2
```

### Custom Working Directory
```bash
Migrate.exe -p sqlserver -c "..." -a "bin\Debug\MyApp.dll" --workingDirectory "C:\MyProject"
```

## Batch Scripts

### Windows Batch File
```batch
@echo off
set PROVIDER=sqlserver
set CONNECTION_STRING=Server=.;Database=MyDb;Integrated Security=true
set ASSEMBLY=MyApp.dll

echo Running FluentMigrator migrations...
Migrate.exe -p %PROVIDER% -c "%CONNECTION_STRING%" -a "%ASSEMBLY%"

if %ERRORLEVEL% neq 0 (
    echo Migration failed with error code %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)

echo Migrations completed successfully!
```

### PowerShell Script
```powershell
$provider = "sqlserver"
$connectionString = "Server=.;Database=MyDb;Integrated Security=true"
$assembly = "MyApp.dll"

Write-Host "Running FluentMigrator migrations..." -ForegroundColor Green

& .\tools\net48\Migrate.exe -p $provider -c $connectionString -a $assembly

if ($LASTEXITCODE -ne 0) {
    Write-Host "Migration failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Migrations completed successfully!" -ForegroundColor Green
```

## CI/CD Integration

### Azure DevOps Pipeline
```yaml
- task: PowerShell@2
  displayName: 'Run Database Migrations'
  inputs:
    targetType: 'inline'
    script: |
      $migratePath = "$(Build.SourcesDirectory)\packages\FluentMigrator.Console\tools\net48\Migrate.exe"
      $connectionString = "$(DatabaseConnectionString)"
      $assembly = "$(Build.ArtifactStagingDirectory)\MyApp.dll"

      & $migratePath -p sqlserver -c $connectionString -a $assembly

      if ($LASTEXITCODE -ne 0) {
        throw "Migration failed with exit code $LASTEXITCODE"
      }
```

### Jenkins Pipeline
```groovy
stage('Database Migration') {
    steps {
        bat """
            tools\\net48\\Migrate.exe ^
            -p sqlserver ^
            -c "${DATABASE_CONNECTION_STRING}" ^
            -a "bin\\Release\\MyApp.dll"
        """
    }
}
```

## Troubleshooting

### Common Issues

**Assembly Loading Errors**
```
Could not load file or assembly 'MyApp'
```
- Ensure all dependencies are in the same directory as your assembly
- Use the correct target framework version of Migrate.exe
- Check that the assembly path is correct

**Database Connection Errors**
```
Unable to connect to database
```
- Verify the connection string is correct
- Ensure the database server is accessible
- Check database user permissions

**No Migrations Found**
```
No migrations found
```
- Verify migrations are public classes
- Ensure migrations inherit from `Migration`
- Check that migrations have the `[Migration]` attribute

### Verbose Logging
```bash
# Enable verbose output for debugging
Migrate.exe -p sqlserver -c "..." -a "MyApp.dll" --verbose
```

### Dry Run Testing
```bash
# Test without making changes
Migrate.exe -p sqlserver -c "..." -a "MyApp.dll" --preview
```

## Best Practices

### ✅ Do
- Use absolute paths for assemblies in automated scripts
- Test migration scripts in a staging environment first
- Include error handling in your deployment scripts
- Use specific database provider versions when possible
- Keep migration assemblies and their dependencies together

### ❌ Don't
- Hardcode sensitive connection strings in scripts
- Run migrations without testing the rollback
- Mix target framework versions
- Ignore exit codes in automated deployments

## Migration to Modern Alternatives

::: tip Consider Upgrading
For new projects, consider using:
- [In-Process Runner](./in-process) for better integration
- [dotnet-fm CLI](./dotnet-fm) for .NET Core/.NET 5+ projects

The console runner is primarily maintained for legacy .NET Framework applications.
:::
