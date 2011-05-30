#region License
// 
// Copyright (c) 2011, Grant Archibald
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.SchemaDump.SchemaDumpers;
using Microsoft.CSharp;

namespace FluentMigrator.SchemaDump.SchemaMigrations
{
   /// <summary>
   /// Migrates the schema items from SQL Server to another supprted database type
   /// </summary>
   public class SqlServerSchemaMigrator
   {
      /// <summary>
      /// Perform migration
      /// </summary>
      public void Migrate(SchemaMigrationSettings settings)
      {
         Generate(settings);

         var a = CompileMigrations(GetSourceFiles(settings), typeof(Migration).Assembly.Location);

         MigrateUp(a, settings);
      }

      private string[] GetSourceFiles(SchemaMigrationSettings settings)
      {
         return Directory.GetFiles(settings.MigrationsDirectory, "*.cs");
      }

      private Assembly CompileMigrations(string[] files, params string[] additionalAssemblies)
      {
         var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });
         var parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.dll" }, "foo.dll", true)
         {
            GenerateExecutable = false // Indicate we want dll not exe
            ,GenerateInMemory = true  // And want it in memory
            ,IncludeDebugInformation = false // And do not require debug info to be created

         };

         if (additionalAssemblies != null)
            parameters.ReferencedAssemblies.AddRange(additionalAssemblies);

         var results = csc.CompileAssemblyFromFile(parameters, files);


         if (results.Errors.Count > 0)
         {
            var exception = new StringBuilder();
            exception.Append("Unable to compile code snippet. See Errors");
            foreach (var error in results.Errors)
            {
               exception.Append("\r\n");
               exception.Append(error);
            }
            throw new Exception(exception.ToString());
         }

         return results.CompiledAssembly;
      }

      private void Generate(SchemaMigrationSettings settings)
      {
         using (var connection = new SqlConnection(settings.FromConnectionString))
         {
            var processor = new SqlServerProcessor(connection, new SqlServer2000Generator(), new NullAnnouncer(),
                                                   new ProcessorOptions());
            var schemaDumper = new SqlServerSchemaDumper(processor, new TextWriterAnnouncer(System.Console.Out));

            var defs = schemaDumper.ReadDbSchema();

            settings.MigrationsDirectory = string.IsNullOrEmpty(settings.MigrationsDirectory)
                                        ? Path.GetTempPath() + Guid.NewGuid()
                                        : settings.MigrationsDirectory;

            Directory.CreateDirectory(settings.MigrationsDirectory);

            for (var index = 0; index < defs.Count; index++)
            {
               var table = defs[index];
               using (
                  var writer =
                     new StreamWriter(Path.Combine(settings.MigrationsDirectory,
                                                   string.Format("BaseMigration_{0}_{1}.cs", index, table.Name))))
               {
                  WriteToStream(table, "MigrationsDefault", index, writer);
               }
            }            
         }
      }

      public static void WriteToStream(TableDefinition table, string defaultNamespace, int migration, StreamWriter output)
      {
         //start writing a migration file
         output.WriteLine("using System;");
         output.WriteLine("using FluentMigrator;");
         output.WriteLine(String.Empty);
         output.WriteLine("namespace {0}", defaultNamespace);
         output.WriteLine("{");
         output.WriteLine("\t[Migration({0})]", migration);
         output.WriteLine("\tpublic class BaseMigration_{0}_{1} : Migration", migration, table.Name);
         output.WriteLine("\t{");
         output.WriteLine("\t\tpublic override void Up()");
         output.WriteLine("\t\t{");

         WriteTable(table, output);

         output.WriteLine("\t\t}"); //end Up method

         output.WriteLine("\t\tpublic override void Down()");
         output.WriteLine("\t\t{");

         WriteDeleteTable(table, output);

         output.WriteLine("\t\t}"); //end Down method
         output.WriteLine("\t}"); //end class
         output.WriteLine(String.Empty);
         output.WriteLine("}"); //end namespace
      }

      protected static void WriteTable(TableDefinition table, StreamWriter output)
      {
         output.WriteLine("\t\t\tCreate.Table(\"" + table.Name + "\")");
         foreach (var column in table.Columns)
         {
            WriteColumn(column, output, column == table.Columns.Last());
         }
      }

      protected static void WriteDeleteTable(TableDefinition table, StreamWriter output)
      {
         //Delete.Table("Bar");
         output.WriteLine("\t\t\tDelete.Table(\"" + table.Name + "\");");
      }

      protected static void WriteColumn(ColumnDefinition column, StreamWriter output, bool isLastColumn)
      {
         var columnSyntax = new StringBuilder();
         columnSyntax.AppendFormat(".WithColumn(\"{0}\")", column.Name);
         switch (column.Type)
         {
            case DbType.Boolean:
               columnSyntax.Append(".AsBoolean()");
               break;
            case DbType.DateTime:
               columnSyntax.Append(".AsDateTime()");
               // How handle default .. could be explict value for function get GETDATE()
               break;
            case DbType.Decimal:
               columnSyntax.AppendFormat(".AsDecimal({0},{1})", column.Precision, column.Size);
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
               if ( column.Size > 0 )
                  columnSyntax.AppendFormat(".AsString({0})", column.Size);
               else
                  columnSyntax.Append(".AsString()");
               break;
            case DbType.StringFixedLength:
               columnSyntax.AppendFormat(".AsString({0})", column.Size);
               break;
            default:
               columnSyntax.Append(".AsString()");
               break;
         }
         if (column.IsIdentity)
            columnSyntax.Append(".Identity()");
         else if (column.IsIndexed)
            columnSyntax.Append(".Indexed()");

         if (column.DefaultValue != null && !string.IsNullOrEmpty(column.DefaultValue.ToString()))
         {
            var defaultValue = column.DefaultValue.ToString()
               .Replace("(", "")
               .Replace(")", ""); // HACK - What if default is string and includes ( ?

            //TODO - Test default valeus for boolean
            //TODO - Test functions e.g. GetDate(), NewId()


            if (defaultValue.StartsWith("'") && defaultValue.EndsWith("'"))
            {
               if (column.Type == DbType.DateTime)
               {
                  // TODO Handle GetDate()
                  // Assumes that date in format yyyy-MM-dd
                  // TODO handle 1900-01-01 is this correct ?s 
                  defaultValue = string.Format("DateTime.ParseExact({0}, \"yyyy-MM-dd\", null)", defaultValue);
               }

               defaultValue = defaultValue.Replace("'", "\"");

               // Assume is is a string
               // Hack - Assumes that default does not include '               
               columnSyntax.AppendFormat(".WithDefaultValue({0})", defaultValue.Replace("'", "\""));
            }
            else
            {
               // Insert value as object
               columnSyntax.AppendFormat(".WithDefaultValue({0})", defaultValue);
            }
         }

         if (!column.IsNullable)
            columnSyntax.Append(".NotNullable()");

         if (isLastColumn) columnSyntax.Append(";");
         output.WriteLine("\t\t\t\t" + columnSyntax);
      }


      private void MigrateUp(Assembly assembly, SchemaMigrationSettings settings)
      {
         var announcer = new NullAnnouncer();

         var runnerContext = new RunnerContext(announcer)
                                {
                                   Database = settings.ToDatabaseType.ToString().ToLower(),
                                   Connection = settings.ToConnectionString
                                };

         var factory = ProcessorFactory.GetFactory(settings.ToDatabaseType.ToString());
         IMigrationProcessor toProcessor = null;
         try
         {
            toProcessor = factory.Create(settings.ToConnectionString, announcer, new ProcessorOptions
                                                                                    {
                                                                                       PreviewOnly = false,
                                                                                       Timeout = 30
                                                                                    });
            var runner = new MigrationRunner(assembly, runnerContext, toProcessor);
            runner.MigrateUp(true);
         }
         finally
         {
            if (toProcessor != null && toProcessor is IDisposable)
               ((IDisposable)toProcessor).Dispose();
         }
      }
   }

   public class SchemaMigrationSettings
   {
      public string FromConnectionString { get; set; }

      public DatabaseType ToDatabaseType { get; set; }

      public string ToConnectionString { get; set; }

      public string MigrationsDirectory { get; set; }

      public bool ExecuteInMemory { get; set; }
   }
}