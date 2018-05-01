---
uid: app-context
title: "ApplicationContext: Passing parameters to Migrations"
---

> [!CAUTION]
> The `ApplicationContext` is obsolete. Use dependency injection instead!

# Obsolete: ApplicationContext: Passing parameters to Migrations

ApplicationContext is an object passed in by the [Migration Runner](xref:migration-runners.md) that is available to migrations to be used as a switch.

## To set the ApplicationContext when running migrate.exe

Run `migrate.exe ... --context MyArgument`.  This will set the application context to the string `MyArgument`.

## To set the ApplicationContext when running migration in code

Set [migratorConsole.ApplicationContext](xref:FluentMigrator.Runner.Initialization.RunnerContext.ApplicationContext) to an arbitrary C# object, such as a string: `migratorConsole.ApplicationContext = "MyArgument";` or when creating the `RunnerContext`:

```cs
var migrationContext = new FluentMigrator.Runner.Initialization.RunnerContext(announcer)
{
    ApplicationContext = "MyArgument"
};
```

## To use the ApplicationContext

Inside your migration, you can access the context via `this.ApplicationContext`.  So for instance:

```cs
if ((string)this.ApplicationContext == "MyArgument")
    this.Delete.Column("BadColumn").FromTable("MyTable");
```

# Alternative: DependencyInjection

Just register your service and use it in your migration:

```c#
interface IMyService {
    string MyStringData { get; }
}

class MyService : IMyService {
    public string MyStringData { get; set; }
}

static void Main() {
    var serviceProvider = new ServiceCollection()
        .AddFluentMigratorCore()
        // TODO: Configure database and connection
        .AddSingleton<IMyService>(new MyService() { MyStringData = "MyArgument" })
        .BuildServiceProvider();

    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
    // TODO: Use the runner
}

[Migration(1234)]
public class MyMigration : ForwardOnlyMigration {
    private readonly IMyService _service;

    public MyMigration(IMyService service) {
        _service = service;
    }

    public void Up() {
        if (_service.MyStringData == "MyArgument")
            Delete.Column("BadColumn").FromTable("MyTable");
    }
}
```
