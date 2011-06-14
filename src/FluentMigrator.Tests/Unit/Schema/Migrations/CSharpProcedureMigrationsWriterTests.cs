using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FluentMigrator.Model;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.SchemaDump.SchemaDumpers;
using FluentMigrator.SchemaDump.SchemaMigrations;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Schema.Migrations
{
   [TestFixture]
   public class CSharpProcedureMigrationsWriterTests 
   {
      private string _tempDirectory;
      private string _scriptsDirectory;

      [SetUp]
      public void Setup()
      {
         _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
         _scriptsDirectory = Path.Combine(_tempDirectory, "Scripts");
         Directory.CreateDirectory(_scriptsDirectory);
      }

      [TearDown]
      public void TearDown()
      {
         Directory.Delete(_tempDirectory, true);
      }

      [Test]
      public void NoMigrationCreatedIfNoProcedures()
      {
         var definitions = new List<ProcedureDefinition>();

         // Arrange
         var settings = GenerateProcedureMigrations(definitions);

         // Assert
         Assert.IsFalse(File.Exists(Path.Combine(_tempDirectory, "Test.cs")));
         Assert.AreEqual(0, settings.MigrationIndex);
      }

      [Test]
      public void CreatesScriptFolderIfNotExist()
      {
         // Arrange
         Directory.Delete(_scriptsDirectory);

         // Act
         GenerateProcedureMigration(new ProcedureDefinition
         {
             Name = "Foo"
            , Sql = "XXX"
         });

         // Assert
         Assert.AreEqual("XXX", File.ReadAllText(Path.Combine(_scriptsDirectory, "CreateProcedureFoo_SqlServer.sql")));
      }

      [Test]
      public void CreatesAndDropsSqlServerProcedure()
      {
         // Arrange

         // Act
         var migration = GenerateProcedureMigration(new ProcedureDefinition
         {
            SchemaName = "dbo"
            ,Name = "Foo"
            ,Sql = "XXX"
         });

         // Assert
         migration.Contains("Execute.WithDatabaseType(DatabaseType.SqlServer).Script(@\"Scripts\\CreateProcedureFoo_SqlServer.sql\");").ShouldBeTrue();
         migration.Contains("Execute.WithDatabaseType(DatabaseType.SqlServer).Sql(\"DROP PROCEDURE [dbo].[Foo]\");").ShouldBeTrue();
         Assert.AreEqual("XXX", File.ReadAllText(Path.Combine(_scriptsDirectory, "CreateProcedureFoo_SqlServer.sql")));
      }

      [Test]
      public void OnlyGeneratesSelectedProcedure()
      {
         // Arrange
         var context = GetDefaultContext();
         context.IncludeProcedures.Add("Foo");

         var migration = GenerateProcedureMigrations(context
            ,   new ProcedureDefinition {Name = "Foo",Sql = "XXX"}
            , new ProcedureDefinition { Name = "Bar", Sql = "YYY" });

         // Assert
         Debug.WriteLine(migration);


         File.Exists(Path.Combine(_scriptsDirectory, "CreateProcedureFoo_SqlServer.sql")).ShouldBeTrue();
         File.Exists(Path.Combine(_scriptsDirectory, "CreateProcedureBar_SqlServer.sql")).ShouldBeFalse();
      }

      [Test]
      public void GenerateOracleProcedure()
      {
         // Arrange
         var context = GetDefaultContext();
         context.FromDatabaseType = DatabaseType.Oracle;


         var migration = GenerateProcedureMigrations(context
            , new ProcedureDefinition
            {
               Name = "Foo"
               ,Sql = "XXX"
            }
            );

         // Assert
         Debug.WriteLine(migration);


         File.Exists(Path.Combine(_scriptsDirectory, "CreateProcedureFoo_Oracle.sql")).ShouldBeTrue();
      }

      [Test]
      public void CreatesAndDropsOracleProcedure()
      {
         // Arrange
         var context = GetDefaultContext();
         context.FromDatabaseType = DatabaseType.Oracle;
         var migration = GenerateProcedureMigrations(context, new ProcedureDefinition
         {
            Name = "Foo"
            ,Sql = "XXX"
         });

         // Assert
         Debug.WriteLine(migration);

         migration.Contains("Execute.WithDatabaseType(DatabaseType.Oracle).Script(@\"Scripts\\CreateProcedureFoo_Oracle.sql\");").ShouldBeTrue();
         migration.Contains("Execute.WithDatabaseType(DatabaseType.Oracle).Sql(\"DROP PROCEDURE Foo\");").ShouldBeTrue();
      }


      private string GenerateProcedureMigration(params ProcedureDefinition[] procedureDefinitions)
      {
         var context = GetDefaultContext();

         GenerateProcedureMigrations(context, procedureDefinitions);

         return File.ReadAllText(Path.Combine(_tempDirectory, @"Migrations\Test.cs"));
      }

      

      /// <summary>
      /// Generates C# files from the supplied procedure definitions
      /// </summary>
      /// <param name="context">The context that defines how the migrations should be created</param>
      /// <param name="procedureDefinitions">The procedure definitions that are the source for thr migration</param>
      private string GenerateProcedureMigrations(SchemaMigrationContext context, params ProcedureDefinition[] procedureDefinitions)
      {
         var mockDumper = new Mock<ISchemaDumper>();
         var writer = new CSharpProcedureMigrationsWriter(new DebugAnnouncer());

         mockDumper.Setup(m => m.ReadProcedures()).Returns(procedureDefinitions);

         writer.GenerateMigrations(context, mockDumper.Object);

         var migrationsFile = Path.Combine(_tempDirectory, @"Migrations\Test.cs");
         return File.Exists(migrationsFile) ? File.ReadAllText(migrationsFile) : string.Empty;
      }

      /// <summary>
      /// Generates C# files from the supplied table migrations
      /// </summary>
      /// <param name="procedureDefinitions">The procedure definitions that are the source for the migration</param>
      private SchemaMigrationContext GenerateProcedureMigrations(IList<ProcedureDefinition> procedureDefinitions)
      {
         var context = GetDefaultContext();

         GenerateProcedureMigrations(context, procedureDefinitions.ToArray());

         return context;
      }

      private SchemaMigrationContext GetDefaultContext()
      {
         return new SchemaMigrationContext
                   {
                        FromDatabaseType = DatabaseType.SqlServer
                      , WorkingDirectory = _tempDirectory
                      ,MigrationProcedureClassNamer = (index, table) => "Test"
                   };
      }
   }
}
