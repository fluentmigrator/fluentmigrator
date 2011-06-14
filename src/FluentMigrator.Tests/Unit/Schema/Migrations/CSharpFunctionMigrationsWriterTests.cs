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
   public class CSharpFunctionMigrationsWriterTests 
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
      public void NoMigrationCreatedIfNoFunctions()
      {
         var definitions = new List<FunctionDefinition>();

         // Arrange
         var settings = GenerateFunctionMigrations(definitions);

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
         GenerateFunctionMigration(new FunctionDefinition
         {
             Name = "Foo"
            , Sql = "XXX"
         });

         // Assert
         Assert.AreEqual("XXX", File.ReadAllText(Path.Combine(_scriptsDirectory, "CreateFunctionFoo_SqlServer.sql")));
      }

      [Test]
      public void CreatesAndDropsSqlServerFunction()
      {
         // Arrange

         // Act
         var migration = GenerateFunctionMigration(new FunctionDefinition
         {
            SchemaName = "dbo"
            ,Name = "Foo"
            ,Sql = "XXX"
         });

         // Assert
         migration.Contains("Execute.WithDatabaseType(DatabaseType.SqlServer).Script(@\"Scripts\\CreateFunctionFoo_SqlServer.sql\");").ShouldBeTrue();
         migration.Contains("Execute.WithDatabaseType(DatabaseType.SqlServer).Sql(\"DROP FUNCTION [dbo].[Foo]\");").ShouldBeTrue();
         Assert.AreEqual("XXX", File.ReadAllText(Path.Combine(_scriptsDirectory, "CreateFunctionFoo_SqlServer.sql")));
      }

      [Test]
      public void OnlyGeneratesSelectedFunction()
      {
         // Arrange
         var context = GetDefaultContext();
         context.IncludeFunctions.Add("Foo");

         var migration = GenerateFunctionMigrations(context
            ,   new FunctionDefinition {Name = "Foo",Sql = "XXX"}
            , new FunctionDefinition { Name = "Bar", Sql = "YYY" });

         // Assert
         Debug.WriteLine(migration);


         File.Exists(Path.Combine(_scriptsDirectory, "CreateFunctionFoo_SqlServer.sql")).ShouldBeTrue();
         File.Exists(Path.Combine(_scriptsDirectory, "CreateFunctionBar_SqlServer.sql")).ShouldBeFalse();
      }

      [Test]
      public void GenerateOracleFunction()
      {
         // Arrange
         var context = GetDefaultContext();
         context.FromDatabaseType = DatabaseType.Oracle;


         var migration = GenerateFunctionMigrations(context
            , new FunctionDefinition
            {
               Name = "Foo"
               ,Sql = "XXX"
            }
            );

         // Assert
         Debug.WriteLine(migration);


         File.Exists(Path.Combine(_scriptsDirectory, "CreateFunctionFoo_Oracle.sql")).ShouldBeTrue();
      }

      [Test]
      public void CreatesAndDropsOracleFunction()
      {
         // Arrange
         var context = GetDefaultContext();
         context.FromDatabaseType = DatabaseType.Oracle;
         var migration = GenerateFunctionMigrations(context, new FunctionDefinition
         {
            Name = "Foo"
            ,Sql = "XXX"
         });

         // Assert
         Debug.WriteLine(migration);

         migration.Contains("Execute.WithDatabaseType(DatabaseType.Oracle).Script(@\"Scripts\\CreateFunctionFoo_Oracle.sql\");").ShouldBeTrue();
         migration.Contains("Execute.WithDatabaseType(DatabaseType.Oracle).Sql(\"DROP FUNCTION Foo\");").ShouldBeTrue();
      }


      private string GenerateFunctionMigration(params FunctionDefinition[] functionDefinitions)
      {
         var context = GetDefaultContext();

         GenerateFunctionMigrations(context, functionDefinitions);

         return File.ReadAllText(Path.Combine(_tempDirectory, @"Migrations\Test.cs"));
      }

      

      /// <summary>
      /// Generates C# files from the supplied function definitions
      /// </summary>
      /// <param name="context">The context that defines how the migrations should be created</param>
      /// <param name="functionDefinitions">The function definitions that are the source for thr migration</param>
      private string GenerateFunctionMigrations(SchemaMigrationContext context, params FunctionDefinition[] functionDefinitions)
      {
         var mockDumper = new Mock<ISchemaDumper>();
         var writer = new CSharpFunctionMigrationsWriter(new DebugAnnouncer());

         mockDumper.Setup(m => m.ReadFunctions()).Returns(functionDefinitions);

         writer.GenerateMigrations(context, mockDumper.Object);

         var migrationsFile = Path.Combine(_tempDirectory, @"Migrations\Test.cs");
         return File.Exists(migrationsFile) ? File.ReadAllText(migrationsFile) : string.Empty;
      }

      /// <summary>
      /// Generates C# files from the supplied table migrations
      /// </summary>
      /// <param name="functionDefinitions">The function definitions that are the source for the migration</param>
      private SchemaMigrationContext GenerateFunctionMigrations(IList<FunctionDefinition> functionDefinitions)
      {
         var context = GetDefaultContext();

         GenerateFunctionMigrations(context, functionDefinitions.ToArray());

         return context;
      }

      private SchemaMigrationContext GetDefaultContext()
      {
         return new SchemaMigrationContext
                   {
                        FromDatabaseType = DatabaseType.SqlServer
                      , WorkingDirectory = _tempDirectory
                      ,MigrationFunctionClassNamer = (index, table) => "Test"
                   };
      }
   }
}
