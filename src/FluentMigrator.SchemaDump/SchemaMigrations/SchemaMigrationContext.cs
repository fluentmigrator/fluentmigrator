using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using FluentMigrator.Model;

namespace FluentMigrator.SchemaDump.SchemaMigrations
{
   /// <summary>
   /// Defines common settings that assist with the migration process
   /// </summary>
   public class SchemaMigrationContext
   {
      /// <summary>
      /// Constructs a new instance of a <see cref="SchemaMigrationContext"/>
      /// </summary>
      public SchemaMigrationContext()
      {
         InputDateTimeFormat = "yyyy-MM-dd";
         DateTimeFormat = "yyyy-MM-dd";
         DateTimeDefaultValueFormatter = (columnDefinition, defaultValue) =>
         {
            return string.Format("DateTime.ParseExact(\"{0}\", \"{1}\", null)", DateTime.ParseExact(defaultValue.Replace("\"", ""), DateTimeFormat, null).ToString(DateTimeFormat), DateTimeFormat);
         };
         DefaultMigrationNamespace = "MigrationsDefault";

         ExecuteInMemory = true;

         MigrationClassNamer = (index, table) => string.Format("BaseMigration_{0}_{1}", index, table.Name);

         MigrationViewClassNamer = (index, view) => string.Format("BaseViewMigration_{0}_{1}", index, view.Name);

         MigrationIndex = 0;

         // By default only generate views for SQL Server
         GenerateAlternateMigrationsFor = new List<DatabaseType>();

         ViewConvertor = new Dictionary<DatabaseType, Func<ViewDefinition, string>>
                            {{DatabaseType.Oracle, DefaultSqlServerToOracleViewConvertor}};

         MigrateTables = true; // Migrate tables by default
         ExcludeTables = new List<string>();

         MigrateViews = true; // Migrate views by default
         IncludeViews = new List<string>();
         ExcludeViews = new List<string>();

         MigrateIndexes = true; // Migrate indexes by default

         MigrateForeignKeys = true; // Migrate Foreign keys by default

         MigrationsDirectory = "Migrations";
         ScriptsDirectory = "Scripts";
         DataDirectory = "Data";

         InsertColumnReplacements = new List<InsertColumnReplacement>();

         MigrationEncoding = Encoding.Unicode;

         CaseSenstiveColumns = new List<string>();

      }

      /// <summary>
      /// Basic implementation of SQL Server to Oracle view using string replace
      /// </summary>
      /// <param name="view">The view to be converted</param>
      /// <returns>The altered CREATE VIEW statement</returns>
      private static string DefaultSqlServerToOracleViewConvertor(ViewDefinition view)
      {
         return view.CreateViewSql
               .Replace("dbo.", "")
               .Replace("[dbo].", "")
               .Replace("[", "\"")
               .Replace("]", "\"")
               .Replace("GETDATE()", "sysdate")
               .Replace("ISNULL", "NVL");
      }

      /// <summary>
      /// The type of database tht the schema is being migrated from
      /// </summary>
      public DatabaseType FromDatabaseType { get; set; }

      /// <summary>
      /// The connection string from which the schema is being read
      /// </summary>
      public string FromConnectionString { get; set; }

      /// <summary>
      /// The type of database that the schema is baing migrated to
      /// </summary>
      public DatabaseType ToDatabaseType { get; set; }

      /// <summary>
      /// The connection string of where to place the migrated schema
      /// </summary>
      public string ToConnectionString { get; set; }

      /// <summary>
      /// The directory on the file system where migration files will be created
      /// </summary>
      public string MigrationsDirectory { get; set; }

      /// <summary>
      /// If <c>True</c> indiactes that the generated migrations choudl be compiled to an in memory assembly and executed
      /// </summary>
      public bool ExecuteInMemory { get; set; }

      /// <summary>
      /// The name of the default namesace that migrations should be placed into
      /// </summary>
      public string DefaultMigrationNamespace { get; set; }

      /// <summary>
      /// The index to start the migrations from.
      /// </summary>
      /// <remarks>Migration steps may update this index to ensure that unique migration numbers are generated</remarks>
      public int MigrationIndex { get; set; }

      /// <summary>
      /// Delegate that specifies how migration class name should be named
      /// </summary>
      public Func<int, TableDefinition, string> MigrationClassNamer { get; set; }

      /// <summary>
      /// Delegate that specifies how migration class name should be named for views
      /// </summary>
      public Func<int, ViewDefinition, string> MigrationViewClassNamer { get; set; }

      /// <summary>
      /// Defines the format for parsing date/times input from schema
      /// </summary>
      public string InputDateTimeFormat { get; set; }

      /// <summary>
      /// Defines the format for parsing date/times
      /// </summary>
      public string DateTimeFormat { get; set; }

      /// <summary>
      /// Delegate that will parse default values for <see cref="DbType.DateTime"/>
      /// </summary>
      public Func<ColumnDefinition, string, string> DateTimeDefaultValueFormatter { get; set; }

      /// <summary>
      /// The directory where sql scripts required for schema migration exist or will be created
      /// </summary>
      /// <remarks>If a script file alredy exists it wil not be overwritten</remarks>
      public string ScriptsDirectory { get; set; }

      /// <summary>
      /// A list of alternate migration specific targets that should also be created
      /// </summary>
      public List<DatabaseType> GenerateAlternateMigrationsFor { get; private set; }

      /// <summary>
      /// A list of tables to be excluded from the migration
      /// </summary>
      public List<string> ExcludeTables { get;
         private set;
      }

      /// <summary>
      /// A list of views to be excluded from the migration
      /// </summary>
      public List<string> ExcludeViews
      {
         get;
         private set;
      }



      /// <summary>
      /// Defines datatype specific delegates that will attempt convert from SQL Server view syntax to the specified database
      /// </summary>
      public Dictionary<DatabaseType, Func<ViewDefinition, string>> ViewConvertor { get; private set; }

      /// <summary>
      /// If <c>True</c> then data should be exported and imported into the new database as part of the migration
      /// </summary>
      public bool MigrateData { get; set; }

      /// <summary>
      /// The directory where migration data is saved to/read from
      /// </summary>
      public string DataDirectory { get; set; }

      /// <summary>
      /// The working directory for the migration. <see cref="MigrationsDirectory"/>, <see cref="DataDirectory"/> and <see cref="ScriptsDirectory"/> should be relative to this folder
      /// </summary>
      public string WorkingDirectory { get; set; }

      public List<InsertColumnReplacement> InsertColumnReplacements { get; private set; }

      /// <summary>
      /// Delegate that allows the <see cref="TableDefinition"/> to be altered before the schema migration is generated
      /// </summary>
      public Action<IList<TableDefinition>> PreMigrationTableUpdate { get; set; }

      /// <summary>
      /// The encoding format to use when savsing and migration string data
      /// </summary>
      public Encoding MigrationEncoding { get; set; }

      /// <summary>
      /// A custom delegfate that can be supplied to generate SEQUENCE names
      /// </summary>
      /// <remarks>The name of the table to place a sequence on will be provided. The sequqnce name should be returned</remarks>
      public Func<string, string> OracleSequenceNamer { get; set; }

      /// <summary>
      /// <c>True</c> indicates that columns in the migration should be treated a case senstive
      /// </summary>
      public bool CaseSenstiveColumnNames { get; set; }

      /// <summary>
      /// The list of columns that are case senstive
      /// </summary>
      public List<string> CaseSenstiveColumns { get; private set; }

      /// <summary>
      /// If <c>True</c> then indexes should be included in the migration
      /// </summary>
      public bool MigrateIndexes { get; set; }

      /// <summary>
      /// If <c>True</c> then foreign keys should be included in the migration
      /// </summary>
      public bool MigrateForeignKeys { get; set; }

      /// <summary>
      /// If <c>True</c> then tables should be included in the migration
      /// </summary>
      public bool MigrateTables { get; set; }

      /// <summary>
      /// If <c>True</c> then views should be included in the migration
      /// </summary>
      public bool MigrateViews { get; set; }

      /// <summary>
      /// List of views to migrate. If empty all views are migrated
      /// </summary>
      public List<string> IncludeViews { get; private set; }
   }
}