using System.IO;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.SchemaDump.SchemaDumpers;

namespace FluentMigrator.SchemaDump.SchemaMigrations
{
   /// <summary>
   /// Responsible for generating C# code that representing views in schema information obtained from a <see cref="ISchemaDumper"/>
   /// </summary>
   public class CSharpViewMigrationsWriter : CSharpMigrationsWriter
   {
      public CSharpViewMigrationsWriter(IAnnouncer announcer) : base(announcer)
      {
      }

      /// <summary>
      /// Generates FluentMigration C# files based on the views found in <see cref="schemaDumper"/>
      /// </summary>
      /// <param name="context">Defines how, what and where the migrations will be generated</param>
      /// <param name="schemaDumper">The platform specific schema dumper instance to get view information from</param>
      public void GenerateMigrations(SchemaMigrationContext context, ISchemaDumper schemaDumper)
      {
         _announcer.Say("Reading views");
         var defs = schemaDumper.ReadViews();

         SetupMigrationsDirectory(context);

         //TODO: Think about adding custom sort order for view definitions as there may be
         // dependancies between views.
         // if ( context.CustomViewSorter != null )
         //   defs = context.CustomViewSorter(defs);

         var migrations = 0;
         foreach (var view in defs)
         {
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

      /// <summary>
      /// Writes the Migration Up() and Down()
      /// </summary>
      /// <param name="context">The context that controls how the column should be generated</param>
      /// <param name="view">the view to generate the migration for</param>
      /// <param name="migration">The migration index to apply</param>
      /// <param name="output">The output stream to append the C# code to</param>
      private void WriteToStream(SchemaMigrationContext context, ViewDefinition view, int migration, StreamWriter output)
      {
         WriteMigration(output, context, migration
            , () => context.MigrationViewClassNamer(migration, view)
            , () => WriteView(context, view, output)
            , () => WriteDeleteView(context, view, output));
      }

      private void WriteView(SchemaMigrationContext context, ViewDefinition view, StreamWriter output)
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

      private static void WriteDeleteView(SchemaMigrationContext context, ViewDefinition view, StreamWriter output)
      {
         output.WriteLine("\t\t\tExecute.WithDatabaseType(DatabaseType.SqlServer).Sql(\"DROP VIEW [{0}].[{1}]\");", view.SchemaName, view.Name);

         foreach (var databaseType in context.GenerateAlternateMigrationsFor)
         {
            output.WriteLine("\t\t\tExecute.WithDatabaseType(DatabaseType.{0}).Sql(\"DROP VIEW {1}\");", databaseType, view.Name);
         }
      }     
   }
}