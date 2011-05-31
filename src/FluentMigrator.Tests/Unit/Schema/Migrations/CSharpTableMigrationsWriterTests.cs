using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using FluentMigrator.Model;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.SchemaDump.SchemaDumpers;
using FluentMigrator.SchemaDump.SchemaMigrations;
using FluentMigrator.Tests.Integration;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Schema.Migrations
{
   [TestFixture]
   public class CSharpTableMigrationsWriterTests : SqlServerUnitTest
   {
      private string _tempDirectory;

      [SetUp]
      public override void Setup()
      {
         base.Setup();
         _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
         Directory.CreateDirectory(_tempDirectory);
      }

      [TearDown]
      public override void TearDown()
      {
         base.TearDown();
         Directory.Delete(_tempDirectory, true);
      }

      [Test]
      public void NoMigrationCreatedIfNoTables()
      {
         var tableDefinitions = new List<TableDefinition>();

         // Arrange
         var settings = GenerateTableMigrations(tableDefinitions);

         // Assert
         Assert.IsFalse(File.Exists(Path.Combine(_tempDirectory, "Test.cs")));
         Assert.AreEqual(0, settings.MigrationIndex);
      }

      [Test]
      public void TestMigrationFileCreated()
      {
         var tableDefinitions = new List<TableDefinition>
                                   {
                                      new TableDefinition
                                         {
                                            Name = "Foo",
                                            Columns = new[] {new ColumnDefinition {Name = "Id", Type = DbType.Int32}}
                                         }
                                   };

         // Arrange
         var settings = GenerateTableMigrations(tableDefinitions);

         // Assert
         Assert.IsTrue(File.Exists(Path.Combine(_tempDirectory, "Test.cs")));
         Assert.AreEqual(1, settings.MigrationIndex);
      }

      [Test]
      public void CreatesAndDeletesTableFoo()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] {new ColumnDefinition {Name = "Id", Type = DbType.Int32}});

         //Assert
         migration.Contains("Create.Table(\"Foo\")").ShouldBeTrue();
         migration.Contains("Delete.Table(\"Foo\");").ShouldBeTrue();
      }

      [Test]
      public void CreatesIdentityColumn()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.Int32, IsIdentity = true } });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsInt32().Identity()").ShouldBeTrue();
      }

      [Test]
      public void CreatesIndexedColumn()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.Int32, IsIndexed = true} });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsInt32().Indexed()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnBinary()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.Binary, Size = 20 } });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsBinary(20)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnBoolean()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.Boolean } });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsBoolean()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnByte()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.Byte } });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsByte()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnCurrency()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.Currency } });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsCurrency()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnDate()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.Date } });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsDate()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnDateTime()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.DateTime } });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsDateTime()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnDateTimeAndDefault()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.DateTime, DefaultValue = "'1900-01-01'"} });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsDateTime().WithDefaultValue(\"TO_DATE('1900-01-01', 'YYYY-MM-DD')\")").ShouldBeTrue();
      }

      [Test]
      public void WithColumnDateTimeAndDefaultCustomFormat()
      {
         // Arrange
         var settings = new SchemaMigrationContext
                           {
                              MigrationsDirectory = _tempDirectory
                              ,MigrationClassNamer = (index, table) => "Test"
                              , DateTimeDefaultValueFormatter = (columnDefinition, defaultValue) => "XXX"
                           };

         // Act
         var migration = GetTestMigration(settings, new[] { new ColumnDefinition { Name = "Id", Type = DbType.DateTime, DefaultValue = "'1900-01-01'" } });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsDateTime().WithDefaultValue(XXX)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnDouble()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.Double } });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsDouble()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnDecimal()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.Decimal, Precision = 19, Scale = 4} });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsDecimal(19,4)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnGuid()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.Guid } });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsGuid()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnGuidWithDefaultNewId()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.Guid, DefaultValue = SystemMethods.NewGuid} });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsGuid().WithDefaultValue(SystemMethods.NewGuid)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnInt16()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.Int16 } });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsInt16()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnInt32()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.Int32 } });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsInt32()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnInt64()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.Int64 } });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsInt64()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnString()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.String } });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsString()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnStringWithSize()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.String, Size = 20} });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsString(20)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnStringAnsiFixedLength()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.AnsiStringFixedLength, Size = 20 } });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsFixedLengthAnsiString(20)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnStringFixedLength()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.StringFixedLength, Size = 20 } });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsFixedLengthString(20)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnAnsiString()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.AnsiString } });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsAnsiString()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnAnsiStringWithLength()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.AnsiString, Size = 20 } });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsAnsiString(20)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnXml()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new[] { new ColumnDefinition { Name = "Id", Type = DbType.Xml } });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsXml()").ShouldBeTrue();
      }

      private string GetTestMigration(ICollection<ColumnDefinition> columns)
      {
         var settings = new SchemaMigrationContext
         {
            MigrationsDirectory = _tempDirectory
            ,MigrationClassNamer = (index, table) => "Test"
         };

         return GetTestMigration(settings, columns);
      }

      private string GetTestMigration(SchemaMigrationContext context, ICollection<ColumnDefinition> columns)
      {
         var tableDefinitions = new List<TableDefinition>
                                   {
                                      new TableDefinition
                                         {
                                            Name = "Foo",
                                            Columns = columns
                                         }
                                   };

         GenerateTableMigrations(context, tableDefinitions);

         return File.ReadAllText(Path.Combine(_tempDirectory, "Test.cs"));
      }

      /// <summary>
      /// Generates C# files from the supplied table migrations
      /// </summary>
      /// <param name="context">The context that defines how the migrations should be created</param>
      /// <param name="tableDefinitions">The table definitions that are the source from the migration</param>
      private static void GenerateTableMigrations(SchemaMigrationContext context, IList<TableDefinition> tableDefinitions)
      {
         var mockDumper = new Mock<ISchemaDumper>();
         var writer = new CSharpMigrationsWriter(new DebugAnnouncer());

         mockDumper.Setup(m => m.ReadDbSchema()).Returns(tableDefinitions);

         writer.GenerateTableMigrations(context, mockDumper.Object);
      }

      /// <summary>
      /// Generates C# files from the supplied table migrations
      /// </summary>
      /// <param name="tableDefinitions">The table definitions that are the source from the migration</param>
      private SchemaMigrationContext GenerateTableMigrations(IList<TableDefinition> tableDefinitions)
      {
         var settings = new SchemaMigrationContext
         {
            MigrationsDirectory = _tempDirectory
            ,MigrationClassNamer = (index, table) => "Test"
         };

         GenerateTableMigrations(settings, tableDefinitions);

         return settings;
      }
   }
}
