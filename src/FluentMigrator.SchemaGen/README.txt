This app is used to generate C# class based on a SQL Server database using the Fluent Migrator API.

Code generation options:
  * Generate a schema (tables, indexes, foriegn key relations) based on an existing SQL Server database on localhost
  * Generate schema difference between two SQL Server databases (not yet implemented)

Generated classes are intended to be added to the AMPRO.Migrations C# project that outputs a DLL that is used by a Fluent Migrator Runner (e.g. Migrate.exe, NAnt or MSBuild tasks)
  * https://github.com/schambers/fluentmigrator/wiki/Migration-Runners

Fluent Migrator API Refs:
  * https://github.com/schambers/fluentmigrator      - Source 
  * https://github.com/schambers/fluentmigrator/wiki - Docs

Libs:
   PM> Install-Package CommandLineParser