using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.SchemaDump.SchemaDumpers;

namespace FluentMigrator.SchemaDump.SchemaMigrations
{
   /// <summary>
   /// Responsible for generating C# code that represents schema information obtained from a <see cref="ISchemaDumper"/>
   /// </summary>
   public class CSharpMigrationsWriter
   {
      private readonly IAnnouncer _announcer;

      public CSharpMigrationsWriter(IAnnouncer announcer)
      {
         _announcer = announcer;
      }

      /// <summary>
      /// Generates C# Migrations that Create tables Migrations
      /// </summary>
      /// <param name="context">The context that define how parts of the C# will be formatted</param>
      /// <param name="schemaDumper">The schema dump to use as the source of the schema migration</param>
      public void GenerateTableMigrations(SchemaMigrationContext context, ISchemaDumper schemaDumper)
      {
         _announcer.Say("Reading database schema");
         var defs = schemaDumper.ReadDbSchema();

         SetupMigrationsDirectory(context);

         int migrations = 0;

         for (var index = 0; index < defs.Count; index++)
         {
            var table = defs[index];

            // Check if we want to exclude this table
            if ( context.ExcludeTables.Contains(table.Name))
            {
               _announcer.Say("Exluding table " + table.Name);
               continue;
            }

            migrations++;

            var migrationsFolder = Path.Combine(context.WorkingDirectory, context.MigrationsDirectory);
            var csFilename = Path.Combine(migrationsFolder, context.MigrationClassNamer(context.MigrationIndex + migrations, table) + ".cs");
            _announcer.Say("Creating migration " + Path.GetFileName(csFilename));
            using (var writer = new StreamWriter(csFilename))
            {
               WriteToStream(context, table, context.MigrationIndex + migrations, writer);
            }

            if ( context.MigrateData )
            {
               var data = schemaDumper.ReadTableData(table.SchemaName, table.Name);

               if (data != null && data.Tables.Count > 0 && data.Tables[0].Rows.Count > 0)
               {
                  var dataDirectory = Path.Combine(context.WorkingDirectory, context.DataDirectory);
                  if (!Directory.Exists(dataDirectory))
                     Directory.CreateDirectory(dataDirectory);
                  data.Tables[0].WriteXmlSchema(Path.Combine(dataDirectory, table.Name + ".xsd"));
                  data.Tables[0].WriteXml(Path.Combine(dataDirectory, table.Name + ".xml"));
               }
            }
         }

         context.MigrationIndex += migrations;
      }

      

      /// <summary>
      /// Generates FluentMigration C# files based on the views found in <see cref="schemaDumper"/>
      /// </summary>
      /// <param name="context">Defines how, what and where the migrations will be generated</param>
      /// <param name="schemaDumper">The platform specific schema dumper instance to get view information from</param>
      public void GenerateViewMigrations(SchemaMigrationContext context, ISchemaDumper schemaDumper)
      {
         _announcer.Say("Reading views");
         var defs = schemaDumper.ReadViews();

         SetupMigrationsDirectory(context);

         //TODO: Think about adding custom sort order for view definitions as there may be
         // dependancies between views.
         // if ( context.CustomViewSorter != null )
         //   defs = context.CustomViewSorter(defs);

         int migrations = 0;
         for (var index = 0; index < defs.Count; index++)
         {
            var view = defs[index];

            if ( context.ExcludeViews.Contains(view.Name))
            {
               _announcer.Say("Excluding view " + view.Name);
               continue;
            }

            migrations++;

            var migrationsFolder = Path.Combine(context.WorkingDirectory, context.MigrationsDirectory);
            var csFilename = Path.Combine(migrationsFolder, context.MigrationViewClassNamer(context.MigrationIndex + migrations, view) + ".cs");


            _announcer.Say("Creating migration " + Path.GetFileName(csFilename));
            using (var writer = new StreamWriter(csFilename))
            {
               WriteToStream(context, view, context.MigrationIndex + migrations, writer);
            }
         }

         context.MigrationIndex += migrations;
      }

      private void SetupMigrationsDirectory(SchemaMigrationContext context)
      {
         if (string.IsNullOrEmpty(context.WorkingDirectory))
            context.WorkingDirectory = Path.GetTempPath() + Guid.NewGuid();

         context.MigrationsDirectory = string.IsNullOrEmpty(context.MigrationsDirectory)
                                          ? @".\Migrations"
                                          : context.MigrationsDirectory;

         var migrationsPath = Path.Combine(context.WorkingDirectory, context.MigrationsDirectory);
         if (!Directory.Exists(migrationsPath))
            Directory.CreateDirectory(migrationsPath);

         _announcer.Say("Writing migrations to " + migrationsPath);

         Directory.CreateDirectory(context.MigrationsDirectory);
      }

      public void WriteToStream(SchemaMigrationContext context, TableDefinition table, int migration, StreamWriter output)
      {
         WriteMigration(output, context, migration
            , () => context.MigrationClassNamer(migration, table)
            ,() => WriteTable(context, table, output)
            , () => WriteDeleteTable(table, output));
      }

     

      public void WriteToStream(SchemaMigrationContext context, ViewDefinition view, int migration, StreamWriter output)
      {
         WriteMigration(output, context, migration
            , () => context.MigrationViewClassNamer(migration, view)
            , () => WriteView(context, view, output)
            , () => WriteDeleteView(context, view, output));
      }

      private void WriteMigration(StreamWriter output, SchemaMigrationContext context, int migration, Func<string> generateName, Action upStatement, Action downStatement)
      {
         //start writing a migration file
         output.WriteLine("using System;");
         output.WriteLine("using FluentMigrator;");
         output.WriteLine(String.Empty);
         output.WriteLine("namespace {0}", context.DefaultMigrationNamespace);
         output.WriteLine("{");
         output.WriteLine("\t[Migration({0})]", migration);
         output.WriteLine("\tpublic class {0} : Migration", generateName());
         output.WriteLine("\t{");
         output.WriteLine("\t\tpublic override void Up()");
         output.WriteLine("\t\t{");

         upStatement();

         output.WriteLine("\t\t}"); //end Up method

         output.WriteLine("\t\tpublic override void Down()");
         output.WriteLine("\t\t{");

         downStatement();

         output.WriteLine("\t\t}"); //end Down method
         output.WriteLine("\t}"); //end class
         output.WriteLine(String.Empty);
         output.WriteLine("}"); //end namespace
      }

      protected void WriteTable(SchemaMigrationContext context, TableDefinition table, StreamWriter output)
      {
         output.WriteLine("\t\t\tCreate.Table(\"" + table.Name + "\")");
         foreach (var column in table.Columns)
         {
            WriteColumn(context, column, output, column == table.Columns.Last());
         }
         if ( context.MigrateData)
         {
            output.Write(string.Format("\t\t\tInsert.IntoTable(\"{0}\").DataTable(@\"{1}\\{0}.xml\")",table.Name, context.DataDirectory));

            // Check if the column contains an identity column
            if ( table.Columns.Where(c => c.IsIdentity).Count() == 1 )
            {
               // It does lets add on extra handling for inserting the identity value into the destination database
               output.Write(".WithIdentity()");
            }
              
            output.Write(";");
         }
      }

      protected static void WriteDeleteTable(TableDefinition table, StreamWriter output)
      {
         //Delete.Table("Bar");
         output.WriteLine("\t\t\tDelete.Table(\"" + table.Name + "\");");
      }

      protected void WriteView(SchemaMigrationContext context, ViewDefinition view, StreamWriter output)
      {
         var scriptsDirectory = Path.Combine(context.WorkingDirectory, context.ScriptsDirectory);
         var scriptFile = Path.Combine(scriptsDirectory, string.Format("CreateView{0}_SqlServer.sql", view.Name));
         if ( !File.Exists(scriptFile))
         {
            if (!Directory.Exists(scriptsDirectory))
               Directory.CreateDirectory(scriptsDirectory);
            File.WriteAllText(scriptFile, view.CreateViewSql);
         }
         output.WriteLine("\t\t\tExecute.WithDatabaseType(DatabaseType.SqlServer).Script(@\"{0}\");",  Path.Combine(context.ScriptsDirectory, Path.GetFileName(scriptFile)));

         foreach (var databaseType in context.GenerateAlternateMigrationsFor)
         {
            if (!context.ViewConvertor.ContainsKey(databaseType)) continue;

            var alterternateScriptFile = Path.Combine(scriptsDirectory, string.Format("CreateView{0}_{1}.sql", view.Name, databaseType));
            if (!File.Exists(alterternateScriptFile))
            {
               File.WriteAllText(alterternateScriptFile, context.ViewConvertor[databaseType](view));
            }
            output.WriteLine("\t\t\tExecute.WithDatabaseType(DatabaseType.{0}).Script(@\"{1}\");", databaseType, Path.Combine(context.ScriptsDirectory, Path.GetFileName(alterternateScriptFile)));
         }
      }

      protected static void WriteDeleteView(SchemaMigrationContext context, ViewDefinition view, StreamWriter output)
      {
         output.WriteLine("\t\t\tExecute.WithDatabaseType(DatabaseType.SqlServer).Sql(\"DROP VIEW [{0}].[{1}]\");", view.SchemaName, view.Name);

         foreach (var databaseType in context.GenerateAlternateMigrationsFor)
         {
            output.WriteLine("\t\t\tExecute.WithDatabaseType(DatabaseType.{0}).Sql(\"DROP VIEW {1}\");", databaseType, view.Name);
         }
      }

      protected void WriteColumn(SchemaMigrationContext context, ColumnDefinition column, StreamWriter output, bool isLastColumn)
      {
         var columnSyntax = new StringBuilder();
         columnSyntax.AppendFormat(".WithColumn(\"{0}\")", column.Name);
         switch (column.Type)
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
         if (column.IsIdentity)
            columnSyntax.Append(".Identity()");
         else if (column.IsIndexed)
            columnSyntax.Append(".Indexed()");

         ApplyDefaultValue(columnSyntax, column, context);

         if (!column.IsNullable)
            columnSyntax.Append(".NotNullable()");

         if (isLastColumn) columnSyntax.Append(";");
         output.WriteLine("\t\t\t\t" + columnSyntax);
      }

      private void ApplyDefaultValue(StringBuilder columnSyntax, ColumnDefinition column, SchemaMigrationContext context)
      {
         // Special case handle system methods
         if (column.DefaultValue != null && column.DefaultValue.ToString().ToLower().Equals("(newid())") || column.DefaultValue.ToString().ToLower().Equals("newguid"))
         {
            // Append the enum value 
            columnSyntax.AppendFormat(".WithDefaultValue(SystemMethods.{0})", SystemMethods.NewGuid);
            return;
         }

         if (column.DefaultValue != null && column.DefaultValue.ToString().ToLower().Equals("(getdate())") || column.DefaultValue.ToString().ToLower().Equals("currentdatetime"))
         {
            // Append the enum value 
            columnSyntax.AppendFormat(".WithDefaultValue(SystemMethods.{0})", SystemMethods.CurrentDateTime);
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

            if (column.Type == DbType.DateTime)
            {
               defaultValue = context.DateTimeDefaultValueFormatter(column, defaultValue);
            }

            columnSyntax.AppendFormat(".WithDefaultValue({0})", defaultValue);
            return;
         }
         
         columnSyntax.AppendFormat(".WithDefaultValue({0})", defaultValue);
      }

      
   }
}