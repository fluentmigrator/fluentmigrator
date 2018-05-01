---
uid: upgrade-guide-2.0-to-3.0
title: Upgrade Guide from 2.0 to 3.0
---

# Upgrading from 2.x to 3.0

The upgrade from 2.x to 3.0 should be very smooth, because the API was kept mostly unchanged.

# What is new?

FluentMigrator now uses dependency injection and other standard libraries from the ASP.NET Core project extensively. This results in a simpler API, well-defined extension points and in general more flexibility.

## New in-process runner initialization

```cs
var serviceProvider = new ServiceCollection()
    // Logging is the replacement for the old IAnnouncer
    .AddLogging(lb => lb.AddFluentMigratorConsole())
    // Registration of all FluentMigrator-specific services
    .AddFluentMigratorCore()
    // Configure the runner
    .ConfigureRunner(
        builder => builder
            // Use SQLite
            .AddSQLite()
            // The SQLite connection string
            .WithGlobalConnectionString("Data Source=test.db")
            // Specify the assembly with the migrations
            .WithMigrationsIn(typeof(MyMigration).Assembly))
    .BuildServiceProvider();

// Put the database update into a scope to ensure
// that all resources will be disposed.
using (var scope = serviceProvider.CreateScope())
{
    // Instantiate the runner
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

    // Execute the migrations
    runner.MigrateUp();
}
```

## `dotnet-fm` as global .NET Core CLI tool

The `dotnet-fm` tool is now a global tool and therefore requires the .NET Core 2.1-preview2 tooling. This allows the usage of `dotnet fm migrate` from other directories than the project directory.

## Connection string handling changes

The library assumes that in `ProcessorOptions.ConnectionString` is either a connection string or
a connection string identifier. This are the steps to load the real connection string.

- Queries all `IConnectionStringReader` implementations
  - When a connection string is returned by one of the readers, then this
    connection string will be used
  - When no connection string is returned, try reading from the next `IConnectionStringReader`
- When no reader returned a connection string, then return `ProcessorOptions.ConnectionString`

The connection string stored in `ProcessorOptions.ConnectionString` might be overridden
by registering the `IConnectionStringReader` instance `PassThroughConnectionStringReader`
as scoped service.

When no connection string could be found, the `SelectingProcessorAccessor` returns
a `ConnectionlessProcessor` instead of the previously selected processor.

## Custom migration expression validation

There is a new service [IMigrationExpressionValidator](xref:FluentMigrator.Validation.IMigrationExpressionValidator) with a default implementation [DefaultMigrationExpressionValidator](xref:FluentMigrator.Validation.DefaultMigrationExpressionValidator) that validates the migration expressions before executing them.

This feature allows - for example - forbidding data deletions in a production environment.

## Using `System.ComponentModel.DataAnnotations` for validation

# Breaking Changes

Version 3.0 dropped support for all .NET Framework versions below 4.6.1 and the timeout values are now stored as [TimeSpan](xref:System.TimeSpan).

## Minimum: .NET Framework 4.6.1

Dropping the support for all .NET Framework versions below 4.6.1 was required, because the package now relies on the following libraries:

* [Microsoft.Extensions.DependencyInjection](https://github.com/aspnet/DependencyInjection/)
* [Microsoft.Extensions.Options](https://github.com/aspnet/Options/)
* [Microsoft.Extensions.Logging](https://github.com/aspnet/Logging/)
* [Microsoft.Extensions.Configuration](https://github.com/aspnet/Configuration/)

## `ProcessorOptions.Timeout` is now a [TimeSpan](xref:System.TimeSpan)

This change is part of the ongoing effort to make the API easier to understand, because it might not be clear if an `int timeout` is the timeout in milliseconds, seconds, etc..

## `ICanBeValidated` not used anymore

The library now uses `System.ComponentModel.DataAnnotations` for validation - for example the `[Required]` attribute for expression fields that are - one might've guessed it - required.

## `MigrationGeneratorFactory` not used anymore

The selection of the SQL generator happens using the [IGeneratorAccessor](xref:FluentMigrator.Runner.Generators.IGeneratorAccessor) service.

## `MigrationProcessorFactoryProvider` not used anymore

The selection of the migration processor is done with the [IProcessorAccessor](xref:FluentMigrator.Runner.Processors.IProcessorAccessor) service.

# Obsolete API

Due to the introduction of dependency injection, some important migration runner related parts of the API have been deprecated. This section convers this topic and shows how to switch to the new dependency injection based API.

## Migration runner initialization

This section shows the runner initialization both with dependency injection and with the `IRunnerContext`.

### [New (with dependency injection)](#tab/di)

```cs
var serviceProvider = new ServiceCollection()
    // Logging is the replacement for the old IAnnouncer
    .AddLogging(lb => lb.AddFluentMigratorConsole())
    // Registration of all FluentMigrator-specific services
    .AddFluentMigratorCore()
    // Configure the runner
    .ConfigureRunner(
        builder => builder
            // Use SQLite
            .AddSQLite()
            // The SQLite connection string
            .WithGlobalConnectionString("Data Source=test.db")
            // Specify the assembly with the migrations
            .WithMigrationsIn(typeof(MyMigration).Assembly))
    .BuildServiceProvider();
```

The runner can now be created and used with:

```cs
// Put the database update into a scope to ensure
// that all resources will be disposed.
using (var scope = serviceProvider.CreateScope())
{
    // Instantiate the runner
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

    // Execute the migrations
    runner.MigrateUp();
}
```

### [Obsolete (with `IRunnerContext`)](#tab/obsolete)

```cs
// Create the announcer to output the migration messages
var announcer = new ConsoleAnnouncer();

// Processor specific options (usually none are needed)
var options = new ProcessorOptions();

// Use SQLite
var processorFactory = new SQLiteProcessorFactory();

// Initialize the processor
using (var processor = processorFactory.Create(
    // The SQLite connection string
    "Data Source=test.db",
    announcer,
    options))
{
    // Configure the runner
    var context = new RunnerContext(announcer);

    // Create the migration runner
    var runner = new MigrationRunner(
        // Specify the assembly with the migrations
        typeof(MyMigration).Assembly,
        context,
        processor);

    // Run the migrations
    runner.MigrateUp();
}
```

***

### Differences explained

#### Dependency Injection

* Allows fluent configuration
* Uses standard libraries
    * Dependency Injection
    * Options
    * Logging
* Uses pluggable APIs
    * May use a different DI container under the hood (AutoFac, etc...)
    * May use standard logging frameworks (log4net, nlog, etc...)

#### Obsolete API

* Clunky
* Re-inventing the wheel

## `IAnnouncer`

The [IAnnouncer](xref:FluentMigrator.Runner.IAnnouncer) interface (and its implementations) were replaced by [ILogger](xref:Microsoft.Extensions.Logging.ILogger) and its implementations.

### Logger registration

You can comfortably register the default [FluentMigratorConsoleLogger](FluentMigrator.Runner.Logging.FluentMigratorConsoleLogger):

```cs
var serviceProvider = new ServiceCollection()
    .AddLogging(lb => lb.AddFluentMigratorConsole())
    .BuildServiceProvider();
```

### Logger usage

You don't use the loggers directly any more. Instead, you just create a constructor parameter with a type of [ILogger](xref:Microsoft.Extensions.Logging.ILogger) or [ILogger](xref:Microsoft.Extensions.Logging.ILogger`1).

```cs
public class MyMigration : ForwardOnlyMigration {
    private readonly ILogger<MyMigration> _logger;

    public MyMigration(ILogger<MyMigration> logger) {
        _logger = logger;
    }

    public void Up() {
        _logger.LogInformation("Creating Up migration expressions");
    }
}
```

### Other loggers

There are several other loggers, like:

* [LogFileFluentMigratorLoggerProvider](xref:FluentMigrator.Runner.Logging.LogFileFluentMigratorLoggerProvider) for logging SQL statements into a file
* [SqlScriptFluentMigratorLoggerProvider](xref:FluentMigrator.Runner.Logging.SqlScriptFluentMigratorLoggerProvider) for logging SQL statements into a [TextWriter](xref:System.IO.TextWriter)

#### Registration of [LogFileFluentMigratorLoggerProvider](xref:FluentMigrator.Runner.Logging.LogFileFluentMigratorLoggerProvider)


```cs
var serviceProvider = new ServiceCollection()
    .AddSingleton<ILoggerProvider, LogFileFluentMigratorLoggerProvider>()
    .Configure<LogFileFluentMigratorLoggerOptions>(opt => {
        opt.OutputFileName = "sqlscript.sql";
    })
    .BuildServiceProvider();
```

## [IMigrationRunnerConventions.GetMigrationInfo](xref:FluentMigrator.Runner.IMigrationRunnerConventions.GetMigrationInfo)

This function was replaced by [IMigrationRunnerConventions.GetMigrationInfoForMigration](xref:FluentMigrator.Runner.IMigrationRunnerConventions.GetMigrationInfoForMigration), because the instantiation will be done using the dependency injection framework.

## [IProfileLoader.ApplyProfiles()](xref:FluentMigrator.Runner.IProfileLoader.ApplyProfiles)

This function was replaced by [IProfileLoader.ApplyProfiles(IMigrationRunner)](xref:FluentMigrator.Runner.IProfileLoader.ApplyProfiles(IMigrationRunner)) to avoid circular dependencies.

## [IProfileLoader.FindProfilesIn(IAssemblyCollection, String)](xref:FluentMigrator.Runner.IProfileLoader.FindProfilesIn(IAssemblyCollection, String))

This function is not used anymore.

## [IMigrationProcessorOptions](xref:FluentMigrator.IMigrationProcessorOptions)

This interface is not used anymore. We use [ProcessorOptions](xref:FluentMigrator.Runner.Processors.ProcessorOptions) instead.

## [IMigrationProcessorFactory](xref:FluentMigrator.Runner.Processors.IMigrationProcessorFactory)

The factories aren't needed anymore. The registered services provide everything that they need for their configuration.

## [IRunnerContext](xref:FluentMigrator.Runner.Initialization.IRunnerContext) and [RunnerContext](xref:FluentMigrator.Runner.Initialization.RunnerContext)

This properties of this interface/class were refactored into several classes.

### Properties moved into [RunnerOptions](xref:FluentMigrator.Runner.Initialization.RunnerOptions)

* [ApplicationContext](xref:FluentMigrator.Runner.Initialization.RunnerOptions.ApplicationContext) (**obsolete!**)
* [AllowBreakingChange](xref:FluentMigrator.Runner.Initialization.RunnerOptions.AllowBreakingChange)
* [NoConnection](xref:FluentMigrator.Runner.Initialization.RunnerOptions.NoConnection)
* [Profile](xref:FluentMigrator.Runner.Initialization.RunnerOptions.Profile)
* [StartVersion](xref:FluentMigrator.Runner.Initialization.RunnerOptions.StartVersion)
* [Steps](xref:FluentMigrator.Runner.Initialization.RunnerOptions.Steps)
* [Tags](xref:FluentMigrator.Runner.Initialization.RunnerOptions.Tags)
* [Task](xref:FluentMigrator.Runner.Initialization.RunnerOptions.Task)
* [TransactionPerSession](xref:FluentMigrator.Runner.Initialization.RunnerOptions.TransactionPerSession)
* [Version](xref:FluentMigrator.Runner.Initialization.RunnerOptions.Version)

### Properties moved into [ProcessorOptions](xref:FluentMigrator.Runner.Processors.ProcessorOptions)

* [ConnectionString](xref:FluentMigrator.Runner.Processors.ProcessorOptions.ConnectionString)
* [PreviewOnly](xref:FluentMigrator.Runner.Processors.ProcessorOptions.PreviewOnly)
* [ProviderSwitches](xref:FluentMigrator.Runner.Processors.ProcessorOptions.ProviderSwitches)
* [Timeout](xref:FluentMigrator.Runner.Processors.ProcessorOptions.Timeout)

### Properties moved into [TypeFilterOptions](xref:FluentMigrator.Runner.Initialization.TypeFilterOptions)

[Namespace](xref:FluentMigrator.Runner.Initialization.TypeFilterOptions.Namespace)
[NestedNamespaces](xref:FluentMigrator.Runner.Initialization.TypeFilterOptions.NestedNamespaces)

### Properties moved into [AppConfigConnectionStringAccessorOptions](xref:FluentMigrator.Runner.Initialization.NetFramework.AppConfigConnectionStringAccessorOptions)

> [!WARNING]
> This class only works under the full .NET Framework and is marked as obsolete!
> Provide access to an [IConfiguration](xref:Microsoft.Extensions.Configuration.IConfiguration) service.
> The FluentMigrator library will use it to call the [GetConnectionString](xref:Microsoft.Extensions.Configuration.ConfigurationExtensions.GetConnectionString(Microsoft.Extensions.Configuration.IConfiguration,System.String)) extension method.

* [ConnectionStringPath -> ConnectionStringConfigPath](xref:FluentMigrator.Runner.Initialization.NetFramework.AppConfigConnectionStringAccessorOptions.ConnectionStringConfigPath)

### Properties moved into [SelectingProcessorAccessorOptions](xref:FluentMigrator.Runner.Processors.SelectingProcessorAccessorOptions)

* [Database -> ProcessorId](xref:FluentMigrator.Runner.Processors.SelectingProcessorAccessorOptions.ProcessorId)

### Properties moved into [AssemblySourceOptions](xref:FluentMigrator.Runner.Initialization.AssemblySourceOptions)

* [Targets -> AssemblyNames](xref:FluentMigrator.Runner.Initialization.AssemblySourceOptions.AssemblyNames)

### Properties with no direct replacement

* `Announcer`: Get your [ILogger](xref:Microsoft.Extensions.Logging.ILogger) with dependency injection instead
* `StopWatch`: Get your [IStopWatch](xref:FluentMigrator.Runner.IStopWatch) with dependency injection instead

### WorkingDirectory

This is set directly by the creation of a DefaultConventionSet and adding it as singleton to the service collection.

```cs
var conventionSet = new DefaultConventionSet(defaultSchemaName: null, WorkingDirectory);
services.AddSingleton<IConventionSet>(conventionSet)
```

### DefaultSchemaName

This is set directly by the creation of a DefaultConventionSet and adding it as singleton to the service collection.

```cs
var conventionSet = new DefaultConventionSet(DefaultSchemaName, workingDirectory: null);
services.AddSingleton<IConventionSet>(conventionSet)
```

## `CompatabilityMode` renamed to [CompatibilityMode](xref:FluentMigrator.Runner.CompatibilityMode)

The spelling has been fixed.

## `ApplicationContext`

It is not needed anymore due to the dependency injection providing all services one may need.

## `ManifestResourceNameWithAssembly` replaced by `ValueTuple`

This class was overkill.

## `MigrationGeneratorFactory`

This isn't needed anymore, because all factories must be added dynamically using the [ConfigureRunner](xref:Microsoft.Extensions.DependencyInjection.FluentMigratorServiceCollectionExtensions.ConfigureRunner(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{FluentMigrator.Runner.IMigrationRunnerBuilder})) extension method.

<a href="factory-example">Example:</a>

```cs
var serviceProvider = new ServiceCollection()
    // Registration of all FluentMigrator-specific services
    .AddFluentMigratorCore()
    // Configure the runner
    .ConfigureRunner(
        builder => builder
            // Add database support
            .AddSQLite()
            .AddSqlServer2008()
            .AddFirebird()
            /* TODO: More configuration */
    )
    /* TODO: Add more services */
    .BuildServiceProvider();
```

## `MigrationProcessorFactoryProvider`

This isn't needed anymore, because all processor factory providers must be added dynamically using the [ConfigureRunner](xref:Microsoft.Extensions.DependencyInjection.FluentMigratorServiceCollectionExtensions.ConfigureRunner(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{FluentMigrator.Runner.IMigrationRunnerBuilder})) extension method.

You can find an example [above](#factory-example).

## `ITypeMap.GetTypeMap(DbType, int, int)`

Sometimes, it is possible that a given database type needs a precision of 0, so we cannot use 0 an indicator for an unspecified value anymore. Therefore, we provide an [overload using nullable integer values](xref:FluentMigrator.Runner.Generators.ITypeMap.GetTypeMap(System.Data.DbType,System.Nullable{System.Int32},System.Nullable{System.Int32})).

## `IDbFactory`

The implementations will remain, but the interface will be gone.

## `ICanBeValidated`

The library now uses `System.ComponentModel.DataAnnotations` for validation - for example the `[Required]` attribute for expression fields that are - one might've guessed it - required.
