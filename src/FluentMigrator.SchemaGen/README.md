Overview
--------
This app generates a set of C# Migration classes based on a SQL Server database using the [Fluent Migrator API](https://github.com/schambers/fluentmigrator/wiki).

It can be used to generate migrations for a new database **install** OR an **upgrade** between two database versions.

Generated classes are intended to be added a C# project that outputs a DLL that is executed by a [Fluent Migrator Runner](https://github.com/schambers/fluentmigrator/wiki/Migration-Runners).

Main Features:
--------------
  * Generates a **full** Install migration (tables, indexes, foreign keys) based on an existing database.
  * OR Generates an **upgrade** schema migration based on the differences between two database.
    * Reads schema from SQL Server 2008+
    * Generated migrations can then be used to install / upgrade other database types supported by Fluent Migrator.
    * Can select included and excluded tables by name or pattern.
  * Schema migrations are [auto reversing](https://github.com/schambers/fluentmigrator/wiki/Auto-Reversing-Migrations) unless objects are deleted.
  * Generates a class per table ordered by FK dependency constraints. 
  * Import and embed SQL scripts to perform: 
    * Pre/Post processing.
    * Views, Stored Procedures, Functions
    * Seed Data, Demo Data, Test Data.
    * Per table data migrations.
  * Specify output directory, class namespace, Fluent Migrator [Tag] attributes (applied to all classes).
    * Some additional support classes are required for in a generated project DLL.
    * Uses a MigrationVersion() class that defines the product version.
  * Generated classes inherit from local project classes: MigratorExt and AutoReversingMigrationExt 
    * All you to customise inherited behaviour.
  * Generated migration class are each number based on a migration version: **Major.Minor.Patch.Step** 
    * You supply the product version: **Major.Minor.Patch**  (e.g. **3.1.2**)
    * The step is generated and defines the execution order of the migration classes.
    * You can optionally define first and last step numbers.
	  * Useful when sequencing merging sets of generated classes or ensuring that Install and Upgrade migrations both reach a matching step final number.
  * Source databases defined either using full connection string or just a localhost database name.
  * Command line and MSBuild Task support (can load the .EXE as a MSBuild task DLL)
  * Includes several minor enhancements and fixes to Fluent Migrator API.

Schema Upgrade Features:
-----------------------
  * Optionally generates "drop table" migration that removes tables and related foreign keys in dependency order.
  * Optionally generates "drop script" migration that removes functions/views/stored procedures .  
  * Optionally include as comments the definition of objects being deleted. Very useful in diagnosing changes.
  * When a NULL-able table field becomes NOT NULL, optionally emits SQL to set NULL values to the column's DEFAULT value (if defined).
  * Per Table: Can import SQL scripts to be executed after new columns / indexes are added but before old columns are removed.

See the [Options.cs](Options.cs) file for details of command line and MSBuild task options

Known Issues:
------------
 
 * Currently ignores the Schema Name when comparing most schema objects etc.
 * When a field type is altered, we currently don't cope with the cases where this field is part of a foreign key relation (requires FK to be dropped and two or more tables altered together).
 * There are many complex cases that this generator will not cater for. The goal is to cover the most common cases. The rest needs your input!

Required Libs:
-------------
   PM> Install-Package CommandLineParser

Fluent Migrator API Refs:

  * https://github.com/schambers/fluentmigrator      - Source 
  * https://github.com/schambers/fluentmigrator/wiki - Docs

License:
-------

Apache 2.0


