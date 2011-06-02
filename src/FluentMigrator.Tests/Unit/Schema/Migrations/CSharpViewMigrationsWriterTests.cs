using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
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
   public class CSharpViewMigrationsWriterTests 
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
      public void NoMigrationCreatedIfNoViews()
      {
         var viewDefinitions = new List<ViewDefinition>();

         // Arrange
         var settings = GenerateViewMigrations(viewDefinitions);

         // Assert
         Assert.IsFalse(File.Exists(Path.Combine(_tempDirectory, "Test.cs")));
         Assert.AreEqual(0, settings.MigrationIndex);
      }

      [Test]
      public void CreatsScriptFolderIfNotExist()
      {
         // Arrange
         Directory.Delete(_scriptsDirectory);

         // Act
         var migration = GetTestMigration(new ViewDefinition
         {
            SchemaName = "dbo"
            , Name = "Foo"
            , CreateViewSql = "XXX"
         });

         // Assert
         Assert.AreEqual("XXX", File.ReadAllText(Path.Combine(_scriptsDirectory, "CreateViewFoo_SqlServer.sql")));
      }

      [Test]
      public void CreatesAndDropsSqlServerView()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ViewDefinition
         {
            SchemaName = "dbo"
            ,
            Name = "Foo"
            ,
            CreateViewSql = "XXX"
         });

         // Assert
         migration.Contains("Execute.WithDatabaseType(DatabaseType.SqlServer).Script(@\"Scripts\\CreateViewFoo_SqlServer.sql\");").ShouldBeTrue();
         migration.Contains("Execute.WithDatabaseType(DatabaseType.SqlServer).Sql(\"DROP VIEW [dbo].[Foo]\");").ShouldBeTrue();
         Assert.AreEqual("XXX", File.ReadAllText(Path.Combine(_scriptsDirectory, "CreateViewFoo_SqlServer.sql")));
      }

      [Test]
      public void CreatesAndDropsOracleView()
      {
         // Arrange
         var context = GetDefaultContext();
         context.GenerateAlternateMigrationsFor.Add(DatabaseType.Oracle);
         var migration = GetTestMigration(context, new ViewDefinition
         {
            SchemaName = "dbo"
            ,Name = "Foo"
            ,CreateViewSql = "XXX"
         });

         // Assert
         Debug.WriteLine(migration);

         migration.Contains("Execute.WithDatabaseType(DatabaseType.Oracle).Script(@\"Scripts\\CreateViewFoo_Oracle.sql\");").ShouldBeTrue();
         migration.Contains("Execute.WithDatabaseType(DatabaseType.Oracle).Sql(\"DROP VIEW Foo\");").ShouldBeTrue();
         Assert.AreEqual("XXX", File.ReadAllText(Path.Combine(_scriptsDirectory, "CreateViewFoo_Oracle.sql")));
      }

      [Test]
      public void OracleViewRemovesDboSchemaWithBrackets()
      {
         // Arrange
         var context = GetDefaultContext();
         context.GenerateAlternateMigrationsFor.Add(DatabaseType.Oracle);
         var migration = GetTestMigration(context, new ViewDefinition
                                                      {
                                                         SchemaName = "dbo"
                                                         ,Name = "Foo"
                                                         ,CreateViewSql =
                                                            "CREATE VIEW [dbo].FooTest AS SELECT Id FROM [dbo].Foo"
         });

         // Assert
         Debug.WriteLine(migration);

         Assert.AreEqual("CREATE VIEW FooTest AS SELECT Id FROM Foo", File.ReadAllText(Path.Combine(_scriptsDirectory, "CreateViewFoo_Oracle.sql")));
      }

      [Test]
      public void OracleViewRemovesDboSchema()
      {
         // Arrange
         var context = GetDefaultContext();
         context.GenerateAlternateMigrationsFor.Add(DatabaseType.Oracle);
         var migration = GetTestMigration(context, new ViewDefinition
         {
            SchemaName = "dbo"
            ,
            Name = "Foo"
            ,
            CreateViewSql =
              "CREATE VIEW dbo.FooTest AS SELECT Id FROM dbo.Foo"
         });

         // Assert
         Debug.WriteLine(migration);

         Assert.AreEqual("CREATE VIEW FooTest AS SELECT Id FROM Foo", File.ReadAllText(Path.Combine(_scriptsDirectory, "CreateViewFoo_Oracle.sql")));
      }

      [Test]
      public void OracleViewRemovesConvertsISNULLtoNVL()
      {
         // Arrange
         var context = GetDefaultContext();
         context.GenerateAlternateMigrationsFor.Add(DatabaseType.Oracle);
         var migration = GetTestMigration(context, new ViewDefinition
         {
            SchemaName = "dbo"
            ,
            Name = "Foo"
            ,
            CreateViewSql =
              "CREATE VIEW dbo.FooTest AS SELECT ISNULL(Id,1) FROM dbo.Foo"
         });

         // Assert
         Debug.WriteLine(migration);

         Assert.AreEqual("CREATE VIEW FooTest AS SELECT NVL(Id,1) FROM Foo", File.ReadAllText(Path.Combine(_scriptsDirectory, "CreateViewFoo_Oracle.sql")));
      }

      private string GetTestMigration(params ViewDefinition[] views)
      {
         var context = GetDefaultContext();

         GenerateViewMigrations(context, views);

         return File.ReadAllText(Path.Combine(_tempDirectory, @"Migrations\Test.cs"));
      }

      private string GetTestMigration(SchemaMigrationContext context, params ViewDefinition[] views)
      {
         GenerateViewMigrations(context, views);

         return File.ReadAllText(Path.Combine(Path.Combine( _tempDirectory, "Migrations"), "Test.cs"));
      }

      /// <summary>
      /// Generates C# files from the supplied view definitions
      /// </summary>
      /// <param name="context">The context that defines how the migrations should be created</param>
      /// <param name="viewDefinitions">The view definitions that are the source for thr migration</param>
      private static void GenerateViewMigrations(SchemaMigrationContext context, IList<ViewDefinition> viewDefinitions)
      {
         var mockDumper = new Mock<ISchemaDumper>();
         var writer = new CSharpMigrationsWriter(new DebugAnnouncer());

         mockDumper.Setup(m => m.ReadViews()).Returns(viewDefinitions);

         writer.GenerateViewMigrations(context, mockDumper.Object);
      }

      /// <summary>
      /// Generates C# files from the supplied table migrations
      /// </summary>
      /// <param name="viewDefinitions">The view definitions that are the source for the migration</param>
      private SchemaMigrationContext GenerateViewMigrations(IList<ViewDefinition> viewDefinitions)
      {
         var context = GetDefaultContext();

         GenerateViewMigrations(context, viewDefinitions);

         return context;
      }

      private SchemaMigrationContext GetDefaultContext()
      {
         return new SchemaMigrationContext
                   {
                       WorkingDirectory = _tempDirectory
                      ,MigrationViewClassNamer = (index, table) => "Test"
                   };
      }
   }
}
