Over the years, FluentMigrator has accumulated many different ways to filter migrations:

1. `TagsAttribute`, which allows specifying a list of tags.
   The default tag behavior is `TagBehavior.RequireAny`,
   which means that if the user passes in any of the matching tags into the runner,
   then the migration will be run. `TagsAttribute` has an overload that allows `TagBehavior.RequireAll`,
   which means that if that only if the user passes in ALL of the matching tags into the runner,
   then the migration will be run.
2. `ProfileAttribute`, which was a migration that got executed at the end, after all other migrations execute.
3. `MaintenanceMigration`, which was the evolution of the `ProfileAttribute`:
    a migration that could be executed at any `MigrationStage`:
    - `BeforeAll`: Migration will be run before all standard migrations.
    - `BeforeEach`: Migration will be run before each standard migration.
    - `AfterEach`: Migration will be run after each standard migration.
    - `BeforeProfiles`: Migration will be run after all standard migrations, but before profiles.
    - `AfterAll`: Migration will be run after all standard migrations and profiles.
4. `MigrationTraitAttribute`, which allows Xunit-style Traits to be applied to migrations.
5. [`MigrationConstraintAttribute`](https://github.com/fluentmigrator/fluentmigrator/tree/master/src/FluentMigrator.Runner/Constraints),
    which allows specifying a run-time constraint.
    * This feature is kind of non-intuitive and has tight coupling with other pieces of the system.  I don't like it.

Those are all external ways to configure how the runner chooses migrations once an assembly (.dll) has been created.

There are also internal ways to configure how the runner chooses migrations while you're developing the csproj, before the assembly (.dll) has been created:

1. `IfDatabase`, which allows to run different SQL depending on which ProcessorId the runner loaded
    (effectively, a ProcessorId corresponds to a database driver and database version). 
2. `ApplicationContext`, which is obsolete thanks to dependency injection,
    which allows you to write C# conditional logic using some state that is injected into the program at run-time.
3. Dependency Injection, in general, which allows you to inject services into any `Migration` sub-class.
4. Conditionals e.g. testing if an object exists before creating or altering the object

And there are two ways to sequence migrations:

1. `MigrationAttribute`
2. `TimestampedMigrationAttribute`, which delegates to `MigrationAttribute`
