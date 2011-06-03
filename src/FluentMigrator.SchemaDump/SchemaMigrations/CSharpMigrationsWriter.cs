using System;
using System.IO;
using FluentMigrator.Runner;

namespace FluentMigrator.SchemaDump.SchemaMigrations
{
   public abstract class CSharpMigrationsWriter
   {
      protected readonly IAnnouncer _announcer;

      protected CSharpMigrationsWriter(IAnnouncer announcer)
      {
         _announcer = announcer;
      }

      protected void SetupMigrationsDirectory(SchemaMigrationContext context)
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

      protected void WriteMigration(StreamWriter output, SchemaMigrationContext context, int migration, Func<string> generateName, Action upStatement, Action downStatement)
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
   }
}