Overview
--------
This app generates a set of C# Migration classes based on a SQL Server database using the Fluent Migrator API.
It can be used to generate migrations for a new database **install** OR an **upgrade** between two database versions.

Generated classes are intended to be added a C# project that outputs a DLL that is executed by a Fluent Migrator Runner (e.g. Migrate.exe, NAnt or MSBuild tasks).

  * https://github.com/schambers/fluentmigrator/wiki/Migration-Runners

Features:
---------

  * Generates a **full** or **upgrade** schema migration (tables, indexes, foriegn keys) based on existing SQL Server 2008+ databases.
    * Generated schema can then be used to install / upgrade other database types supported by Fluent Migrator.
    * Can select included and excluded tables by name or pattern.
  * Generates a class per table ordered by FK dependency constraints. 
  * Migration class are number based a migration version: major.minor.patch.step 
    * You supply the major.minor.patch  (e.g. "3.1.2")
    * The step is generated and defines the execution order of the classes.
    * You can optionally start/end step number to support merging sets of generated classes.
  * Import and embed SQL scripts to perform: 
    * Pre/Post processing.
    * Views, Stored Procedures, Functions
    * Seed Data, Demo Data, Test Data.
    * Per table data migrations.
  * Specify output directory, class namespace, Fluent Migrator [Tag] attrtibutes.
    * Additional support classes are added in a generated project DLL.
    * Uses a MigrationVersion() class that defines the product version.
  * Source databases defined either using full connection string or just a localhost database name.
  * Command line and MSBuild Task support (can load the .EXE as a MSBuild task DLL)
  * Includes several minor enhancements and fixes to Fluent Migrator API.

Schema Upgrade Features:
-----------------------
  * Optionally generates "drop table" and "drop script" classes.
  * Optionally include as comments the definition of objects being deleted. Very useful in diagnosing changes.
  * When a NULL-able table field becomes NOT NULL, optionally emits SQL to set NULL values to the column's DEFAULT value (if defined).
  * Per Table: Can import SQL scripts to be executed after new columns / indexes are added but before old columns are removed.

See the [Options.cs](Options.cs) file for details of command line and MSBuild task options

Known Issues:
------------
 
 * Currently ignores the Schema Name when comparing most schema objects etc.
 * When a field type is altered, we currently don't cope with the cases where this field is part of a foriegn key relation (requires FK to be dropped and two or more tables altered together).
 * There are many complex cases that this generator will not cater for. The goal is to cover the most common cases. The rest needs your input!

Required Libs:
-------------
   PM> Install-Package CommandLineParser

Fluent Migrator API Refs:

  * https://github.com/schambers/fluentmigrator      - Source 
  * https://github.com/schambers/fluentmigrator/wiki - Docs

License:
-------

  * [Apache 2.0](http://www.apache.org/licenses/LICENSE-2.0)


