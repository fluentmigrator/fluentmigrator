How to use this library:

1. Add your migrations to your assembly as desired or use an assembly dedicated to migrations.

2. Execute the following code in your startup sequence, e.g. in a static initializer:

         var databaseConnectionString = ... // your database connection string
         var assembly = ... // assembly containing migrations, e.g. typeof(YourMigration).Assembly
         new Migrator(new MigratorContext(Console.Out) {
            Database = "postgres",
            Connection = databaseConnectionString,
            MigrationsAssembly = assembly
         }).MigrateUp();

If you have further questions please visit https://github.com/ManfredLange/fluentmigrator

