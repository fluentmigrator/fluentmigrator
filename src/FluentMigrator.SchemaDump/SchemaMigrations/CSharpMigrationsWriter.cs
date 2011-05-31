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

         context.MigrationsDirectory = string.IsNullOrEmpty(context.MigrationsDirectory)
                                           ? Path.GetTempPath() + Guid.NewGuid()
                                           : context.MigrationsDirectory;

         _announcer.Say("Writing migrations to " + context.MigrationsDirectory);

         Directory.CreateDirectory(context.MigrationsDirectory);

         for (var index = 0; index < defs.Count; index++)
         {
            var table = defs[index];
            var csFilename = Path.Combine(context.MigrationsDirectory, context.MigrationClassNamer(index, table) + ".cs");
            _announcer.Say("Creating migration " + Path.GetFileName(csFilename));
            using (var writer = new StreamWriter(csFilename))
            {
               WriteToStream(context, table, index, writer);
            }
         }

         context.MigrationIndex += defs.Count;
      }

      public void WriteToStream(SchemaMigrationContext context, TableDefinition table, int migration, StreamWriter output)
      {
         //start writing a migration file
         output.WriteLine("using System;");
         output.WriteLine("using FluentMigrator;");
         output.WriteLine(String.Empty);
         output.WriteLine("namespace {0}", context.DefaultMigrationNamespace);
         output.WriteLine("{");
         output.WriteLine("\t[Migration({0})]", migration);
         output.WriteLine("\tpublic class {0} : Migration", context.MigrationClassNamer(migration, table));
         output.WriteLine("\t{");
         output.WriteLine("\t\tpublic override void Up()");
         output.WriteLine("\t\t{");

         WriteTable(context, table, output);

         output.WriteLine("\t\t}"); //end Up method

         output.WriteLine("\t\tpublic override void Down()");
         output.WriteLine("\t\t{");

         WriteDeleteTable(table, output);

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
      }

      protected static void WriteDeleteTable(TableDefinition table, StreamWriter output)
      {
         //Delete.Table("Bar");
         output.WriteLine("\t\t\tDelete.Table(\"" + table.Name + "\");");
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