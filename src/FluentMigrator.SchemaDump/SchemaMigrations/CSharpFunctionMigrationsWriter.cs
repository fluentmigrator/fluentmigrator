using System.IO;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.SchemaDump.SchemaDumpers;

namespace FluentMigrator.SchemaDump.SchemaMigrations
{
   /// <summary>
   /// Responsible for generating C# code that representing functions in schema information obtained from a <see cref="ISchemaDumper"/>
   /// </summary>
   public class CSharpFunctionMigrationsWriter : CSharpMigrationsWriter
   {
      public CSharpFunctionMigrationsWriter(IAnnouncer announcer)
         : base(announcer)
      {
      }

      /// <summary>
      /// Generates FluentMigration C# files based on the functions found in <see cref="schemaDumper"/>
      /// </summary>
      /// <param name="context">Defines how, what and where the migrations will be generated</param>
      /// <param name="schemaDumper">The platform specific schema dumper instance to get Function information from</param>
      public void GenerateMigrations(SchemaMigrationContext context, ISchemaDumper schemaDumper)
      {
         _announcer.Say("Reading Functions");
         var defs = schemaDumper.ReadFunctions();

         SetupMigrationsDirectory(context);

         var migrations = 0;
         foreach (var function in defs)
         {
            if (context.ExcludeFunctions.Contains(function.Name))
            {
               _announcer.Say("Excluding Function " + function.Name);
               continue;
            }

            if (context.IncludeFunctions.Count != 0 && !context.IncludeFunctions.Contains(function.Name)) continue;

            migrations++;

            var migrationsFolder = Path.Combine(context.WorkingDirectory, context.MigrationsDirectory);
            var csFilename = Path.Combine(migrationsFolder, context.MigrationFunctionClassNamer(context.MigrationIndex + migrations, function) + ".cs");


            _announcer.Say("Creating migration " + Path.GetFileName(csFilename));
            using (var writer = new StreamWriter(csFilename))
            {
               WriteToStream(context, function, context.MigrationIndex + migrations, writer);
            }
         }

         context.MigrationIndex += migrations;
      }

      /// <summary>
      /// Writes the Migration Up() and Down()
      /// </summary>
      /// <param name="context">The context that controls how the column should be generated</param>
      /// <param name="function">the Function to generate the migration for</param>
      /// <param name="migration">The migration index to apply</param>
      /// <param name="output">The output stream to append the C# code to</param>
      private void WriteToStream(SchemaMigrationContext context, FunctionDefinition function, int migration, StreamWriter output)
      {
         WriteMigration(output, context, migration
            , () => context.MigrationFunctionClassNamer(migration, function)
            , () => WriteView(context, function, output)
            , () => WriteDeleteView(context, function, output));
      }

      private void WriteView(SchemaMigrationContext context, FunctionDefinition function, StreamWriter output)
      {
         var scriptsDirectory = Path.Combine(context.WorkingDirectory, context.ScriptsDirectory);
         var scriptFile = Path.Combine(scriptsDirectory, string.Format("CreateFunction{0}_{1}.sql", function.Name, context.FromDatabaseType));
         if ( !File.Exists(scriptFile))
         {
            if (!Directory.Exists(scriptsDirectory))
               Directory.CreateDirectory(scriptsDirectory);
            File.WriteAllText(scriptFile, function.Sql);
         }
         output.WriteLine("\t\t\tExecute.WithDatabaseType(DatabaseType.{0}).Script(@\"{1}\");", context.FromDatabaseType, Path.Combine(context.ScriptsDirectory, Path.GetFileName(scriptFile)));
      }

      private static void WriteDeleteView(SchemaMigrationContext context, FunctionDefinition function, StreamWriter output)
      {
         if ( context.FromDatabaseType == DatabaseType.SqlServer)
            output.WriteLine("\t\t\tExecute.WithDatabaseType(DatabaseType.SqlServer).Sql(\"DROP FUNCTION [{0}].[{1}]\");", function.SchemaName, function.Name);
         else
            output.WriteLine("\t\t\tExecute.WithDatabaseType(DatabaseType.{0}).Sql(\"DROP FUNCTION {1}\");", context.FromDatabaseType, function.Name);
      }     
   }
}