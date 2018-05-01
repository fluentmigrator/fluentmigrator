---
uid: migration-filter-tags
title: Fluent Interface
---

# Filter migrations run based on Tags

Scenario: multiple database once had the same schema but over the years have had differences introduced.

Because of this the majority of migrations, contained in one assembly, can be run against all the databases but at the same time you need to be able to filter certain migrations to run against specific databases based on the country (UK, DK, etc) and environment (Staging, Production, etc) it serves.

To facilitate this you can tag migrations (much like cucumber scenarios) and then filter these by passing tags into the runner. A migration is then run if it has:

* No Tags
* OR Has Tags that match ALL those passed into the runner.

Tags are assigned to migrations with an attribute/s:

```cs
[Tags("DK", "NL", "UK")]
[Tags("Staging", "Production")]
[Migration(1)]
public class DoSomeStuffToEuropeanStagingAndProdDbs : Migration { /* ... etc ... */ }
```

# Filtering

## [`IMigrationRunner`](#tab/runner-internal)

```cs
var serviceProvider = new ServiceCollection()
    .AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddSqlServer2008()
        .WithGlobalConnectionString("server=.\\SQLEXPRESS;uid=testfm;pwd=test;Trusted_Connection=yes;database=FluentMigrator")
        .WithMigrationsIn(typeof(DoSomeStuffToEuropeanStagingAndProdDbs).Assembly))
    .AddLogging(lb => lb.AddFluentMigratorConsole())
    // Start of type filter configuration
    .Configure<RunnerOptions>(opt => {
        opt.Tags = new[] { "UK", "Production" }
    })
    .BuildServiceProvider(false);

var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
runner.MigrateUp();
```

## [`Migrate.exe` tool](#tab/runner-external-migrate)

And the tags are entered into the command line like so: (Migrations with UK AND Production tags executed)

<pre><code><migrate.exe command> --tag UK --tag Production</code></pre>

## [MSBuild task](#tab/runner-external-msbuid)

For the Msbuild runner, there is a Tags option and tags are passed in as a comma-separated string.

```xml
<Migrate Database="sqlserver2008"
    Connection="server=.\SQLEXPRESS;uid=testfm;pwd=test;Trusted_Connection=yes;database=FluentMigrator"
	Target="Migrations" 
    Tags="UK,Production" />
```

***
