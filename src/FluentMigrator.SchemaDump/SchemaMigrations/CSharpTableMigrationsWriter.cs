using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.SchemaDump.SchemaDumpers;

namespace FluentMigrator.SchemaDump.SchemaMigrations
{
   /// <summary>
   /// Responsible for generating C# code that representing tables using schema information obtained from a <see cref="ISchemaDumper"/>
   /// </summary>
   public class CSharpTableMigrationsWriter : CSharpMigrationsWriter
   {
      public CSharpTableMigrationsWriter(IAnnouncer announcer) : base(announcer)
      {
         MapDatabaseTypes = new Dictionary<DbType, DbType>();
      }

      public Dictionary<DbType, DbType> MapDatabaseTypes { get; private set; }

      /// <summary>
      /// Generates C# Migrations that Create tables Migrations
      /// </summary>
      /// <param name="context">The context that define how parts of the C# will be formatted</param>
      /// <param name="schemaDumper">The schema dump to use as the source of the schema migration</param>
      public void GenerateMigrations(SchemaMigrationContext context, ISchemaDumper schemaDumper)
      {
         _announcer.Say("Reading database schema");
         var defs = schemaDumper.ReadDbSchema();

         if (context.PreMigrationTableUpdate != null)
            context.PreMigrationTableUpdate(defs);

         SetupMigrationsDirectory(context);

         var migrations = 0;

         var foreignkeyTables = new List<TableDefinition>();

         foreach (var table in defs)
         {
            // Check if we want to exclude this table
            if ( context.ExcludeTables.Contains(table.Name))
            {
               _announcer.Say("Exluding table " + table.Name);
               continue;
            }

            if (context.MigrationRequired(MigrationType.Tables) || (context.MigrationRequired(MigrationType.Indexes) && table.Indexes.Count > 0))
            {
               migrations++;

               var migrationsFolder = Path.Combine(context.WorkingDirectory, context.MigrationsDirectory);
               var csFilename = Path.Combine(migrationsFolder, context.MigrationClassNamer(context.MigrationIndex + migrations, table) + ".cs");
               _announcer.Say("Creating migration " + Path.GetFileName(csFilename));
               using (var writer = new StreamWriter(csFilename))
               {
                  WriteToStream(context, table, context.MigrationIndex + migrations, writer);
               }   
            }

            if (context.MigrationRequired(MigrationType.Data))
            {
               var data = schemaDumper.ReadTableData(table.SchemaName, table.Name);

               if (data != null && data.Tables.Count > 0 && data.Tables[0].Rows.Count > 0)
               {
                  var dataDirectory = Path.Combine(context.WorkingDirectory, context.DataDirectory);
                  if (!Directory.Exists(dataDirectory))
                     Directory.CreateDirectory(dataDirectory);
                  data.Tables[0].WriteXmlSchema(Path.Combine(dataDirectory, table.Name + ".xsd"));

                  using (var writer = new XmlTextWriter(Path.Combine(dataDirectory, table.Name + ".xml"), context.MigrationEncoding))
                  {
                     data.Tables[0].WriteXml(writer);   
                  }
                  
               }
            }

            if (context.MigrationRequired(MigrationType.ForeignKeys) && table.ForeignKeys.Count > 0)
            {
               // Add to list of tables to apply foreign key
               // ... done as two part process as may me interdepdancies between tables
               foreignkeyTables.Add(table); 
            }
               
         }

         context.MigrationIndex += migrations;

         if (context.MigrationRequired(MigrationType.ForeignKeys))
            GenerateForeignKeyMigrations(context, foreignkeyTables);
      }

      private void GenerateForeignKeyMigrations(SchemaMigrationContext context, List<TableDefinition> tables)
      {
         if (!context.MigrationRequired(MigrationType.ForeignKeys) || tables.Count <= 0)
         {
            return;
         }

         _announcer.Say(string.Format("Found {0} tables with foreign keys", tables.Count));

         var migrations = 0;

         foreach (var foreignkeyTable in tables)
         {
            var migrationsFolder = Path.Combine(context.WorkingDirectory, context.MigrationsDirectory);

            migrations++;

            var migrationIndex = context.MigrationIndex+ migrations;
            var table = foreignkeyTable;

            var csFilename = Path.Combine(migrationsFolder,
                                          context.MigrationClassNamer(migrationIndex, table) + "ForeignKey.cs");
            _announcer.Say("Creating migration " + Path.GetFileName(csFilename));
            using (var writer = new StreamWriter(csFilename))
            {
               WriteMigration(writer, context, migrationIndex
                              , () => context.MigrationClassNamer(migrationIndex, table) + "ForeignKey"
                              , () => WriteForeignKey(table, writer)
                              , () => WriteDeleteForeignKey(table, writer));
            }
         }

         context.MigrationIndex +=  migrations;
      }

      private void WriteDeleteForeignKey(TableDefinition table, StreamWriter writer)
      {
         foreach (var foreignKey in table.ForeignKeys)
         {
            writer.WriteLine(string.Format("\t\t\tDelete.ForeignKey(\"{0}\");", foreignKey.Name));
         }
      }

      private void WriteForeignKey(TableDefinition table, StreamWriter writer)
      {
         foreach (var foreignKey in table.ForeignKeys)
         {
            writer.WriteLine(string.Format("\t\t\tCreate.ForeignKey(\"{0}\")", foreignKey.Name));
            writer.WriteLine(string.Format("\t\t\t\t.FromTable(\"{0}\")", foreignKey.ForeignTable));
            foreach (var column in foreignKey.ForeignColumns)
            {
               writer.WriteLine(string.Format("\t\t\t\t\t.ForeignColumn(\"{0}\")", column));
            }
            writer.WriteLine(string.Format("\t\t\t\t.ToTable(\"{0}\")", foreignKey.PrimaryTable));
            foreach (var column in foreignKey.PrimaryColumns)
            {
               writer.WriteLine();
               writer.Write(string.Format("\t\t\t\t\t.PrimaryColumn(\"{0}\")", column));
            }
            writer.WriteLine(";");
         }
      }

      /// <summary>
      /// Writes the Migration Up() and Down()
      /// </summary>
      /// <param name="context">The context that controls how the column should be generated</param>
      /// <param name="table">the table to generate the migration for</param>
      /// <param name="migration">The migration index to apply</param>
      /// <param name="output">The output stream to append the C# code to</param>
      private void WriteToStream(SchemaMigrationContext context, TableDefinition table, int migration, StreamWriter output)
      {
         WriteMigration(output, context, migration
            , () => context.MigrationClassNamer(migration, table)
            ,() => WriteTable(context, table, output)
            , () => WriteDeleteTable(context, table, output));
      }

      /// <summary>
      /// Creates a table and optionally inserts data into the table if <see cref="SchemaMigrationContext.MigrateData"/> is <c>True</c>
      /// </summary>
      /// <param name="context">The context that controls how the column should be generated</param>
      /// <param name="table">the table to generate the migration for</param>
      /// <param name="output">The output stream to append the C# code to</param>
      private void WriteTable(SchemaMigrationContext context, TableDefinition table, StreamWriter output)
      {
          if ( context.MigrationRequired(MigrationType.Tables) )
          {
              output.WriteLine("\t\t\tif (Schema.Table(\"{0}\").Exists()) return;", table.Name);
              output.WriteLine("");

              output.WriteLine("\t\t\tCreate.Table(\"" + table.Name + "\")");
              foreach (var column in table.Columns)
              {
                  WriteColumn(context, column, output, column == table.Columns.Last());
              }
              if (context.MigrationRequired(MigrationType.Data))
              {
                  WriteInsertData(output, table, context);
              }
          }

          if ( context.MigrationRequired(MigrationType.Indexes) && table.Indexes.Count > 0 )
         {
             var primaryKey = table.Columns.Where(c => c.IsPrimaryKey);
             var primaryKeyName = string.Empty;
             if ( primaryKey.Count() > 0 )
             {
                 primaryKeyName = primaryKey.First().PrimaryKeyName;
             }


            foreach (var index in table.Indexes.Where(i => i.Name != primaryKeyName))
               WriteIndex(output, index, context);
            
         }
      }

       private void WriteIndex(StreamWriter output, IndexDefinition index, SchemaMigrationContext context)
      {
         output.WriteLine("\t\t\tCreate.Index(\"" + index.Name + "\").OnTable(\"" + index.TableName + "\")");
         foreach (var column in index.Columns)
         {
            output.Write("\t\t\t\t.OnColumn(\"" + column.Name + "\")");   
            if ( column.Direction == Direction.Ascending)
               output.Write(".Ascending()");
            if (column.Direction == Direction.Descending)
               output.Write(".Descending()");
         }

         if ( index.IsUnique || index.IsClustered )
         {
            output.Write(".WithOptions()");

            if ( index.IsUnique )
               output.Write(".Unique()");

            if (index.IsClustered)
               output.Write(".Clustered()");
         }
                     
         output.WriteLine(";");
      }

       private static void WriteDeleteIndex(StreamWriter output, IndexDefinition index)
       {
           output.WriteLine("\t\t\tDelete.Index(\"" + index.Name + "\");");
       }

      private void WriteInsertData(StreamWriter output, TableDefinition table, SchemaMigrationContext context)
      {
         output.Write(string.Format("\t\t\tInsert.IntoTable(\"{0}\").DataTable(@\"{1}\\{0}.xml\")",table.Name, context.DataDirectory));

         // Check if the column contains an identity column
          var identityColumn = ColumnWithIdentitySpecified(table);
          if (!string.IsNullOrEmpty(identityColumn))
         {
            // It does lets add on extra handling for inserting the identity value into the destination database
            output.Write(".WithIdentity().OnColumn(\"");
            output.Write(identityColumn);
            output.Write("\")");
         }

         // Add any replacement values
         foreach (var replacement in MatchingReplacements(context.InsertColumnReplacements, table))
         {
            output.Write(string.Format(".WithReplacementValue({0}, {1})", FormatValue(replacement.OldValue), FormatValue(replacement.NewValue)));   
         }

         // Check if we need to honour case senstive column names
         if ( context.CaseSenstiveColumnNames && context.CaseSenstiveColumns.Count() == 0)
         {
            // We do update insert statement
            output.Write(".WithCaseSensitiveColumnNames()");   
         }

         if ( context.CaseSenstiveColumns.Count() > 0 )
         {
            foreach (var column in context.CaseSenstiveColumns)
            {
               // We do update insert statement
               output.Write(string.Format(".WithCaseSensitiveColumn(\"{0}\")",column));    
            }
                  
         }
              
         output.Write(";");
      }

      /// <summary>
      /// Formats a .Net CLR type as a C# string representation
      /// </summary>
      /// <param name="value">The value to be formatted</param>
      /// <returns>The text version of the value</returns>
      private static string FormatValue(object value)
      {
         if ( value is string)
         {
            return string.Format("\"{0}\"", value);
         }

         if (value is DateTime)
         {
            return string.Format("DateTime.ParseExact(\"{0}\", \"yyyy-MM-dd\", null)", ((DateTime)value).ToString("yyyy-MM-dd"));
         }

         throw new NotSupportedException(string.Format("Value type {0} not supported for migration replacement", value.GetType().Name));
      }

      /// <summary>
      /// Searched for replacements that match the <see cref="InsertColumnReplacement.ColumnDataToMatch"/>
      /// </summary>
      /// <param name="replacements">The replacements to be searched</param>
      /// <param name="table">The table to compare the replacements against</param>
      /// <returns>Any matching replacements</returns>
      private static IEnumerable<InsertColumnReplacement> MatchingReplacements(IEnumerable<InsertColumnReplacement> replacements, TableDefinition table)
      {
         return
            replacements.Where(
               possibleReplacement => (table.Columns.Where(
                  columnDefinition =>
                  columnDefinition.Type == possibleReplacement.ColumnDataToMatch.Type &&
                  columnDefinition.IsNullable == possibleReplacement.ColumnDataToMatch.IsNullable).Count() > 0)).ToList();
      }

      private static string ColumnWithIdentitySpecified(TableDefinition table)
      {
         var column = table.Columns.Where(c => c.IsIdentity);

          if ( column.Count() == 1)
          {
              return column.First().Name;
          }

          return string.Empty;
      }

      private static void WriteDeleteTable(SchemaMigrationContext context, TableDefinition table, StreamWriter output)
      {
          if (context.MigrationRequired(MigrationType.Indexes) && table.Indexes.Count > 0)
          {
              var primaryKey = table.Columns.Where(c => c.IsPrimaryKey);
              var primaryKeyName = string.Empty;
              if (primaryKey.Count() > 0)
              {
                  primaryKeyName = primaryKey.First().PrimaryKeyName;
              }


              foreach (var index in table.Indexes.Where(i => i.Name != primaryKeyName))
                  WriteDeleteIndex(output, index);
          }

          if (context.MigrationRequired(MigrationType.Tables))
          {
              output.WriteLine("\t\t\tDelete.Table(\"" + table.Name + "\");");
          }
      }

      /// <summary>
      /// Convert the column definition into a fluent WithColumn.AsXXXX() syntax
      /// </summary>
      /// <param name="context">The context that controls how the column should be generated</param>
      /// <param name="column">The column to generate</param>
      /// <param name="output">The stream to write the C# to</param>
      /// <param name="isLastColumn">If <c>True</c> indicates that this is the last column and a ; should be appened</param>
      private void WriteColumn(SchemaMigrationContext context, ColumnDefinition column, StreamWriter output, bool isLastColumn)
      {
         var columnSyntax = new StringBuilder();
         columnSyntax.AppendFormat(".WithColumn(\"{0}\")", column.Name);

         if (column.Type.HasValue)
         {
            var columnType = column.Type.Value;
            if (MapDatabaseTypes.ContainsKey(columnType))
            {
               columnType = MapDatabaseTypes[columnType];
            }

            switch (columnType)
            {
               case DbType.AnsiString:
                  if (column.Size > 0)
                     columnSyntax.AppendFormat(".AsAnsiString({0})", column.Size);
                  else
                     columnSyntax.Append(".AsAnsiString()");
                  break;
               case DbType.AnsiStringFixedLength:
                  columnSyntax.AppendFormat(".AsFixedLengthAnsiString({0})", column.Size);
                  break;
               case DbType.Binary:
                  columnSyntax.AppendFormat(".AsBinary({0})", column.Size);
                  break;
               case DbType.Byte:
                  columnSyntax.Append(".AsByte()");
                  break;
               case DbType.Boolean:
                  columnSyntax.Append(".AsBoolean()");
                  break;
               case DbType.Currency:
                  columnSyntax.Append(".AsCurrency()");
                  break;
               case DbType.Date:
                  columnSyntax.Append(".AsDate()");
                  break;
               case DbType.DateTime:
                  columnSyntax.Append(".AsDateTime()");
                  break;
               case DbType.Decimal:
                  columnSyntax.AppendFormat(".AsDecimal({0},{1})", column.Precision, column.Scale);
                  break;
               case DbType.Double:
                  columnSyntax.Append(".AsDouble()");
                  break;
               case DbType.Guid:
                  columnSyntax.Append(".AsGuid()");
                  break;
               case DbType.Int16:
                  columnSyntax.Append(".AsInt16()");
                  break;
               case DbType.Int32:
                  columnSyntax.Append(".AsInt32()");
                  break;
               case DbType.Int64:
                  columnSyntax.Append(".AsInt64()");
                  break;
               case DbType.String:
                  if (column.Size > 0)
                     columnSyntax.AppendFormat(".AsString({0})", column.Size);
                  else
                     columnSyntax.Append(".AsString()");
                  break;

               case DbType.StringFixedLength:
                  columnSyntax.AppendFormat(".AsFixedLengthString({0})", column.Size);
                  break;
               case DbType.Xml:
                  columnSyntax.Append(".AsXml()");
                  break;
               default:
                  _announcer.Error(string.Format("Unsupported type {0} for column {1}", column.Type, column.Name));
                  columnSyntax.Append(".AsString()");
                  break;
            }
         }
            
         if (column.IsIdentity)
            columnSyntax.Append(".Identity()");
         
         ApplyDefaultValue(columnSyntax, column, context);

         if (!column.IsNullable)
            columnSyntax.Append(".NotNullable()");

         if (column.IsNullable)
            columnSyntax.Append(".Nullable()");

         if (column.IsPrimaryKey)
             columnSyntax.Append(".PrimaryKey()");

         if (isLastColumn) columnSyntax.Append(";");
         output.WriteLine("\t\t\t\t" + columnSyntax);
      }

      /// <summary>
      /// Conditioanlly appends a default value clause is the column has a default value
      /// </summary>
      /// <param name="fluentColumnCode">The current fluent column being constructed</param>
      /// <param name="column">The column to check if it has a default value</param>
      /// <param name="context">The context that controls how the column should be generated</param>
      private static void ApplyDefaultValue(StringBuilder fluentColumnCode, ColumnDefinition column, SchemaMigrationContext context)
      {
         if (column.DefaultValue is ColumnDefinition.UndefinedDefaultValue)
            return;

         // Special case handle system methods
         if (column.DefaultValue != null && column.DefaultValue.ToString().ToLower().Equals("(newid())") || column.DefaultValue.ToString().ToLower().Equals("newguid"))
         {
            // Append the enum value 
            fluentColumnCode.AppendFormat(".WithDefaultValue(SystemMethods.{0})", SystemMethods.NewGuid);
            return;
         }

         if (column.DefaultValue != null && column.DefaultValue.ToString().ToLower().Equals("(getdate())") || column.DefaultValue.ToString().ToLower().Equals("currentdatetime"))
         {
            // Append the enum value 
            fluentColumnCode.AppendFormat(".WithDefaultValue(SystemMethods.{0})", SystemMethods.CurrentDateTime);
            return;
         }

         // Check if we have a value
         if (column.DefaultValue == null || string.IsNullOrEmpty(column.DefaultValue.ToString()))
            return; // No return
         
         // Convert to string and remove any enclosing ( and )
         var defaultValue = column.DefaultValue.ToString()
            .Replace("(", "")
            .Replace(")", ""); // HACK - What if default is string and includes ( ?

         // Check if it is a string value
         if (defaultValue.StartsWith("'") && defaultValue.EndsWith("'"))
         {
            defaultValue = defaultValue.Replace("'", "\"");

            if (column.Type == DbType.DateTime || column.Type == DbType.Date)
            {
               defaultValue = context.DateTimeDefaultValueFormatter(column, defaultValue);
            }

            fluentColumnCode.AppendFormat(".WithDefaultValue({0})", defaultValue);
            return;
         }

         if (!defaultValue.StartsWith("\""))
            defaultValue = "\"" + defaultValue;

         if (!defaultValue.EndsWith("\""))
            defaultValue = defaultValue + "\"";
         
         fluentColumnCode.AppendFormat(".WithDefaultValue({0})", defaultValue);
      }      
   }
}