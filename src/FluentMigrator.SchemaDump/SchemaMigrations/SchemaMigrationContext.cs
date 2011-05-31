using System;
using System.Data;
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
            return string.Format("\"TO_DATE('{0}', '{1}')\"", DateTime.ParseExact(defaultValue.Replace("\"",""), DateTimeFormat, null).ToString(DateTimeFormat), DateTimeFormat.ToUpper());
         };
         DefaultMigrationNamespace = "MigrationsDefault";

         ExecuteInMemory = true;

         MigrationClassNamer = (index, table) => string.Format("BaseMigration_{0}_{1}", index, table.Name);
         MigrationIndex = 0;
      }

      /// <summary>
      /// The connection string from which the schema is being read
      /// </summary>
      public string FromConnectionString { get; set; }

      /// <summary>
      /// The type of database taht the schema is baing migrated to
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
   }
}