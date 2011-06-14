using System.IO;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.SchemaDump.SchemaDumpers;

namespace FluentMigrator.SchemaDump.SchemaMigrations
{
   /// <summary>
   /// Responsible for generating C# code that representing procedures in schema information obtained from a <see cref="ISchemaDumper"/>
   /// </summary>
   public class CSharpProcedureMigrationsWriter : CSharpMigrationsWriter
   {
      public CSharpProcedureMigrationsWriter(IAnnouncer announcer)
         : base(announcer)
      {
      }

      /// <summary>
      /// Generates FluentMigration C# files based on the procedures found in <see cref="schemaDumper"/>
      /// </summary>
      /// <param name="context">Defines how, what and where the migrations will be generated</param>
      /// <param name="schemaDumper">The platform specific schema dumper instance to get procedure information from</param>
      public void GenerateMigrations(SchemaMigrationContext context, ISchemaDumper schemaDumper)
      {
         _announcer.Say("Reading procedures");
         var defs = schemaDumper.ReadProcedures();

         SetupMigrationsDirectory(context);

         var migrations = 0;
         foreach (var procedure in defs)
         {
            if ( context.ExcludeProcedures.Contains(procedure.Name))
            {
               _announcer.Say("Excluding procedure " + procedure.Name);
               continue;
            }

            if (context.IncludeProcedures.Count != 0 && !context.IncludeProcedures.Contains(procedure.Name)) continue;

            migrations++;

            var migrationsFolder = Path.Combine(context.WorkingDirectory, context.MigrationsDirectory);
            var csFilename = Path.Combine(migrationsFolder, context.MigrationProcedureClassNamer(context.MigrationIndex + migrations, procedure) + ".cs");


            _announcer.Say("Creating migration " + Path.GetFileName(csFilename));
            using (var writer = new StreamWriter(csFilename))
            {
               WriteToStream(context, procedure, context.MigrationIndex + migrations, writer);
            }
         }

         context.MigrationIndex += migrations;
      }

      /// <summary>
      /// Writes the Migration Up() and Down()
      /// </summary>
      /// <param name="context">The context that controls how the column should be generated</param>
      /// <param name="procedure">the procedure to generate the migration for</param>
      /// <param name="migration">The migration index to apply</param>
      /// <param name="output">The output stream to append the C# code to</param>
      private void WriteToStream(SchemaMigrationContext context, ProcedureDefinition procedure, int migration, StreamWriter output)
      {
         WriteMigration(output, context, migration
            , () => context.MigrationProcedureClassNamer(migration, procedure)
            , () => WriteView(context, procedure, output)
            , () => WriteDeleteView(context, procedure, output));
      }

      private void WriteView(SchemaMigrationContext context, ProcedureDefinition procedure, StreamWriter output)
      {
         var scriptsDirectory = Path.Combine(context.WorkingDirectory, context.ScriptsDirectory);
         var scriptFile = Path.Combine(scriptsDirectory, string.Format("CreateProcedure{0}_{1}.sql", procedure.Name, context.FromDatabaseType));
         if ( !File.Exists(scriptFile))
         {
            if (!Directory.Exists(scriptsDirectory))
               Directory.CreateDirectory(scriptsDirectory);
            File.WriteAllText(scriptFile, procedure.Sql);
         }
         output.WriteLine("\t\t\tExecute.WithDatabaseType(DatabaseType.{0}).Script(@\"{1}\");", context.FromDatabaseType, Path.Combine(context.ScriptsDirectory, Path.GetFileName(scriptFile)));
      }

      private static void WriteDeleteView(SchemaMigrationContext context, ProcedureDefinition procedure, StreamWriter output)
      {
         if ( context.FromDatabaseType == DatabaseType.SqlServer)
            output.WriteLine("\t\t\tExecute.WithDatabaseType(DatabaseType.SqlServer).Sql(\"DROP PROCEDURE [{0}].[{1}]\");", procedure.SchemaName, procedure.Name);
         else
            output.WriteLine("\t\t\tExecute.WithDatabaseType(DatabaseType.{0}).Sql(\"DROP PROCEDURE {1}\");", context.FromDatabaseType, procedure.Name);
      }     
   }
}