This app is used to generate C# class based on a SQL Server database using the Fluent Migrator API.

Code generation options:
  * Generate a schema (tables, indexes, foriegn keys) based on an existing SQL Server database on localhost
  * Generate schema difference between two SQL Server databases (not yet implemented)

Generated classes are intended to be added to the AMPRO.Migrations C# project that outputs a DLL that is used by a Fluent Migrator Runner (e.g. Migrate.exe, NAnt or MSBuild tasks)
  * https://github.com/schambers/fluentmigrator/wiki/Migration-Runners

Fluent Migrator API Refs:
  * https://github.com/schambers/fluentmigrator      - Source 
  * https://github.com/schambers/fluentmigrator/wiki - Docs

Libs:
   PM> Install-Package CommandLineParser

Notes:
 * I avoided use of T4 templates as I found that the code was simpler and cleaner to leave it in the C# code (same reasons as FluentMigrator).
 * Added a few additional properties to the FM schema model to support code generation.
 * Several FM projects are set to x86 to ensure that Jet 4.0 driver will load.

Planned refactoring / fixes ...

 * FmDiffMigrationWriter - generates schema differences. 
     - Still some bugs to sort out (not yet generating PrimaryKeys properly). 
	 - Generates a class per table ordered by FK constrains.

 * FmInitialMigrationWriter + FmSchemaWriterBase 
     - Generates a schema for single database as one class but can probably now be all replaced by the code in 
	   FmDiffMigrationWriter as I think it can cover both cases by simulating an empty database.