# Migration Runners

FluentMigrator provides several ways to execute your migrations, from in-process execution to command-line tools. We recommend using the in-process runner when possible for better integration with your application.

## Available Runners

| Runner                                  | Use Case                                  | Platform Support  |
|-----------------------------------------|-------------------------------------------|-------------------|
| [In-Process Runner](./in-process)       | Application startup, integrated execution | .NET Core/.NET 5+ |
| [Console Tool (Migrate.exe)](./console) | Build scripts, deployment automation      | .NET Framework    |
| [dotnet-fm](./dotnet-fm)                | .NET Core CLI integration                 | .NET Core/.NET 5+ |

## Choosing the Right Runner

### In-Process Runner ✅ **Recommended**
- **Best for**: Most applications
- **Pros**: Type safety, better error handling, dependency injection support
- **Cons**: Requires code changes

### Console Tool (Migrate.exe)
- **Best for**: Legacy .NET Framework applications, external deployment scripts
- **Pros**: No code changes required, works with any .NET application
- **Cons**: Platform-specific, less flexible

### dotnet-fm CLI Tool
- **Best for**: .NET Core applications, CI/CD pipelines
- **Pros**: Cross-platform, integrates with dotnet CLI
- **Cons**: Requires .NET Core SDK, not recommended in production environments

## Quick Start Examples

### In-Process (Recommended)

```csharp
using var serviceProvider = new ServiceCollection()
    .AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddSqlServer()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(MyMigration).Assembly).For.All())
    .BuildServiceProvider(false);

using var scope = serviceProvider.CreateScope();
var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
runner.MigrateUp();
```

A more complete sample is available in the [Quick Start guide](/intro/quick-start.md#step-3-configure-the-migration-runner).

### dotnet-fm CLI
```bash
# Install globally
dotnet tool install -g FluentMigrator.DotNet.Cli
```

```bash
# Run migrations
dotnet fm migrate -p sqlite -c "Data Source=test.db" -a "MyApp.dll"
```

### Console Tool
```bash
# Install via NuGet
Install-Package FluentMigrator.Console
```

```bash
# Run from tools directory
.\tools\net45\Migrate.exe -p sqlserver -c "Server=.;Database=MyDb;Integrated Security=true" -a "MyApp.dll"
```

## Next Steps

Choose the runner that best fits your scenario and follow the detailed setup guide for that specific runner type.
