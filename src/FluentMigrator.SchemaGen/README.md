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
    * Shows internal migration number as a comment. 
      * Useful when debugging to run a migration up to a previous migration number and then test the failing SQL.
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
  * Adds comments showing changes including: 
    * Renamed/Duplicate indexes and foreign keys (same definition)
    * Previous definition of deleted and updated columns, indexes and foreign keys.
  * When a NULL-able table field becomes NOT NULL, optionally emits SQL to set NULL values to the column's DEFAULT value (if defined).
  * Per Table: Can import SQL scripts to be executed after new columns / indexes are added but before old columns are removed.

Options 
-------
  * See the [Options.cs](Options.cs) for command line options (including help documentation).
  * See the [FmCodeGen.cs](MSBuild/FmCodeGen.cs) for matching MSBuild task options.

Known Issues
------------
 
 * There are many complex cases that this generator is unlikely to ever cater for. 
   * The goal is to cover the most common cases. 
   * The rest invariably needs your knowledge of intended schema and data change! 
   * Example: Migrating recusive data relationships.
 * When a column or table is renamed, we currently emit add/remove or drop/create commands which you may may need to replace these with Rename.Table() or Rename.Column()
   * There is no way that SchemaGen can safely know that this is what was intended. 
 * Currently ignores the Schema Name when comparing schema objects. Should be easy to fix. Many parts of the code already support it.
 * When a field type is altered, we currently don't handle the case where this field is part of a foriegn key relation.
   * Requires one or more FKs to be dropped and two or more tables altered together before FKs are recreated.
 * Currently emits IfDatabase("sqlserver") conditions for foreign key indexes. 
   * This really should be IfNotDatabase("jet") but IfNotDatabase() is not yet implemented in FluentMigrator.
   * In my case I'm generating code for SQL Server and Jet (MS-Access).

To Do
-----
 * Example command line args and MSBuild scripts.
   * An MSBuild script that emits an enitire C# project and compiles it.
 * Unit Tests 
 * Need to check if a Primary Key is non-clustered and emit: 
   * Create.PrimaryKey("PK").OnTable("TestTable").Column("Id").NonClustered();
 * Support the option of emitting a single migration class (not hard to do with current implementation).
Refactoring Wish List:
 * We always know a 'better way' after the deed is done :) . If I get around to it this what needs doing:
 * Split up FmDiffMigrationWriter into smaller component classes.
 * Rewrite FmDiffMigrationWriter.UpdateTable() to use an improved data structure that should make it simpler and more readable.
 * Revise text output implementation. Used IEnumerable<string> an another project to emit lines to great effect. 

Future Ideas
------------
 * Might consider a two phase approach that emits differences as a data structure and then applies an ordering / grouping algorithm.  Groups become classes.
   * This should support more complex cases involing order complexity and make the code a bit more abstracted and readable.
   * We're likely to find that some ordering contraints will depend on the database type.
 * Support selective differences so you can slice the changes into different phases.
   * Currently only support table include/exclude selection.
   * Object renaming only (Can then separate out Index/FK renaming changes from the 'real' schema changes).
   * Map set of tables (selected by pattern) to be assigned a FM Tag.
     * FKs between these tables can get multiple tags.

Required Libs
-------------
   PM> Install-Package CommandLineParser

Fluent Migrator API Refs:

  * https://github.com/schambers/fluentmigrator      - Source 
  * https://github.com/schambers/fluentmigrator/wiki - Docs

License:
-------

  * [Apache 2.0](http://www.apache.org/licenses/LICENSE-2.0)


