using System;
using System.Collections.Generic;
using System.Data;
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
   public class CSharpTableMigrationsWriterTests 
   {
      private string _tempDirectory;
      private DataSet _testData;

      [SetUp]
      public void Setup()
      {
         _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
         Directory.CreateDirectory(_tempDirectory);

         _testData = new DataSet();
         var table = new DataTable("Foo");
         table.Columns.Add(new DataColumn("Id", typeof (int)));
         table.Rows.Add(1);
         _testData.Tables.Add(table);
      }

      [TearDown]
      public void TearDown()
      {
         Directory.Delete(_tempDirectory, true);
      }

      [Test]
      public void NoMigrationCreatedIfNoTables()
      {
         var tableDefinitions = new List<TableDefinition>();

         // Arrange
         var context = GenerateTableMigrations(tableDefinitions);

         // Assert
         Assert.IsFalse(File.Exists(Path.Combine(_tempDirectory, "Test.cs")));
         Assert.AreEqual(0, context.MigrationIndex);
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
         var context = GenerateTableMigrations(tableDefinitions);

         // Assert
         Assert.IsTrue(File.Exists(Path.Combine(_tempDirectory, @"Migrations\Test.cs")));
         Assert.AreEqual(1, context.MigrationIndex);
      }

      [Test]
      public void AddColumnInsertReplacement()
      {
         // Arrange
         var tableDefinitions = new List<TableDefinition>
                                   {
                                      new TableDefinition
                                         {
                                            Name = "Foo",
                                            Columns = new[] {new ColumnDefinition {Name = "Id", Type = DbType.String}}
                                         }
                                   };


         var context = GetDefaultContext();
         context.MigrateData = true;
         context.WorkingDirectory = _tempDirectory;
         context.InsertColumnReplacements.Add(new InsertColumnReplacement() { 
            ColumnDataToMatch = new ColumnDefinition { Type = DbType.String }
            ,OldValue = ""
            , NewValue = " "})
         ;
      
         // Act
         GenerateTableMigrations(context, tableDefinitions);

         // Assert
         var migration = File.ReadAllText(Path.Combine(_tempDirectory, @"Migrations\Test.cs"));

         File.Exists(Path.Combine(_tempDirectory, @"Data\Foo.xml")).ShouldBeTrue();
         File.Exists(Path.Combine(_tempDirectory, @"Data\Foo.xsd")).ShouldBeTrue();
         migration.Contains("Insert.IntoTable(\"Foo\").DataTable(@\"Data\\Foo.xml\").WithReplacementValue(\"\", \" \");").ShouldBeTrue();
      }

      [Test]
      public void DataFileCreated()
      {
         // Arrange
         var tableDefinitions = new List<TableDefinition>
                                   {
                                      new TableDefinition
                                         {
                                            Name = "Foo",
                                            Columns = new[] {new ColumnDefinition {Name = "Id", Type = DbType.Int32}}
                                         }
                                   };

         
         var context = GetDefaultContext();
         context.MigrateData = true;
         context.WorkingDirectory = _tempDirectory;

         // Act
         GenerateTableMigrations(context,tableDefinitions);

         // Assert
         var migration = File.ReadAllText(Path.Combine(_tempDirectory, @"Migrations\Test.cs"));

         File.Exists(Path.Combine(_tempDirectory, @"Data\Foo.xml")).ShouldBeTrue();
         File.Exists(Path.Combine(_tempDirectory, @"Data\Foo.xsd")).ShouldBeTrue();
         migration.Contains("Insert.IntoTable(\"Foo\").DataTable(@\"Data\\Foo.xml\");").ShouldBeTrue();
      }

      [Test]
      public void DataFileNotCreatedIfNoRows()
      {
         // Arrange
         var tableDefinitions = new List<TableDefinition>
                                   {
                                      new TableDefinition
                                         {
                                            Name = "Foo",
                                            Columns = new[] {new ColumnDefinition {Name = "Id", Type = DbType.Int32}}
                                         }
                                   };
         var context = GetDefaultContext();
         context.MigrateData = true;
         context.WorkingDirectory = _tempDirectory;
         _testData.Tables[0].Rows.Clear();

         // Act
         GenerateTableMigrations(context, tableDefinitions);

         // Assert
         var migration = File.ReadAllText(Path.Combine(_tempDirectory, @"Migrations\Test.cs"));

         File.Exists(Path.Combine(_tempDirectory, @"Data\Foo.xml")).ShouldBeFalse();
         File.Exists(Path.Combine(_tempDirectory, @"Data\Foo.xsd")).ShouldBeFalse();
         migration.Contains("Insert.IntoTable(\"Foo\").DataTable(@\"Data\\Foo.xml\");").ShouldBeTrue();
      }

      [Test]
      public void MigrateIdentity()
      {
         var tableDefinitions = new List<TableDefinition>
                                   {
                                      new TableDefinition
                                         {
                                            Name = "Foo",
                                            Columns = new[] {new ColumnDefinition
                                                                {
                                                                   Name = "Id", Type = DbType.Int32
                                                                   , IsIdentity = true
                                                                }}
                                         }
                                   };

         // Arrange
         var context = GetDefaultContext();
         context.MigrateData = true;
         context.WorkingDirectory = _tempDirectory;
         context.GenerateAlternateMigrationsFor.Add(DatabaseType.Oracle);
         GenerateTableMigrations(context, tableDefinitions);

         // Assert
         var migration = File.ReadAllText(Path.Combine(_tempDirectory, @"Migrations\Test.cs"));

         migration.Contains("Insert.IntoTable(\"Foo\").DataTable(@\"Data\\Foo.xml\").WithIdentity();").ShouldBeTrue();
      }

      [Test]
      public void TestMigrationDataImport()
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
         var context = GetDefaultContext();
         context.MigrateData = true;
         GenerateTableMigrations(context, tableDefinitions);

         // Assert
         var dataDirectory = Path.Combine(context.WorkingDirectory, context.DataDirectory);
         Assert.IsTrue(File.Exists(Path.Combine(dataDirectory, "Foo.xml")));
         Assert.AreEqual(1, context.MigrationIndex);
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
      public void CreatesNotNullable()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.Int32 });

         //Assert
         migration.Contains(".NotNullable()").ShouldBeTrue();
      }

      [Test]
      public void CreatesNullable()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.Int32, IsNullable = true});

         //Assert
         migration.Contains(".Nullable()").ShouldBeTrue();
      }


      [Test]
      public void CreatesIdentityColumn()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.Int32, IsIdentity = true });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsInt32().Identity()").ShouldBeTrue();
      }

      [Test]
      public void CreatesIndexedColumn()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.Int32, IsIndexed = true});

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsInt32().Indexed()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnBinary()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.Binary, Size = 20 });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsBinary(20)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnBoolean()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.Boolean });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsBoolean()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnByte()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.Byte });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsByte()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnCurrency()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.Currency });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsCurrency()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnDate()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.Date });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsDate()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnDateTime()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.DateTime });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsDateTime()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnDateTimeAndDefault()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.DateTime, DefaultValue = "'1900-01-01'"});

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsDateTime().WithDefaultValue(\"TO_DATE('1900-01-01', 'YYYY-MM-DD')\")").ShouldBeTrue();
      }

      [Test]
      public void WithColumnDateTimeAndDefaultCustomFormat()
      {
         // Arrange
         var settings = GetDefaultContext();
         settings.DateTimeDefaultValueFormatter = (columnDefinition, defaultValue) => "XXX";

         // Act
         var migration = GetTestMigration(settings, new ColumnDefinition { Name = "Id", Type = DbType.DateTime, DefaultValue = "'1900-01-01'" });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsDateTime().WithDefaultValue(XXX)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnDouble()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.Double });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsDouble()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnDecimal()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.Decimal, Precision = 19, Scale = 4});

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsDecimal(19,4)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnGuid()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.Guid });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsGuid()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnGuidWithDefaultNewId()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.Guid, DefaultValue = SystemMethods.NewGuid});

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsGuid().WithDefaultValue(SystemMethods.NewGuid)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnInt16()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.Int16 });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsInt16()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnInt32()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.Int32 });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsInt32()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnInt64()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.Int64 });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsInt64()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnString()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.String });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsString()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnStringWithSize()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.String, Size = 20});

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsString(20)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnStringAnsiFixedLength()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.AnsiStringFixedLength, Size = 20 });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsFixedLengthAnsiString(20)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnStringFixedLength()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.StringFixedLength, Size = 20 });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsFixedLengthString(20)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnAnsiString()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.AnsiString });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsAnsiString()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnAnsiStringWithLength()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.AnsiString, Size = 20 });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsAnsiString(20)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnXml()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Id", Type = DbType.Xml });

         //Assert
         migration.Contains(".WithColumn(\"Id\").AsXml()").ShouldBeTrue();
      }

      private string GetTestMigration(params ColumnDefinition[] columns)
      {
         return GetTestMigration(GetDefaultContext(), columns);
      }

      private string GetTestMigration(SchemaMigrationContext context, params ColumnDefinition[] columns)
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

         return File.ReadAllText(Path.Combine(_tempDirectory, @"Migrations\Test.cs"));
      }

      /// <summary>
      /// Generates C# files from the supplied table migrations
      /// </summary>
      /// <param name="context">The context that defines how the migrations should be created</param>
      /// <param name="tableDefinitions">The table definitions that are the source for the migration</param>
      private void GenerateTableMigrations(SchemaMigrationContext context, IList<TableDefinition> tableDefinitions)
      {
         var mockDumper = new Mock<ISchemaDumper>();
         var writer = new CSharpTableMigrationsWriter(new DebugAnnouncer());

         mockDumper.Setup(m => m.ReadDbSchema()).Returns(tableDefinitions);
         mockDumper.Setup(m => m.ReadTableData(It.IsAny<string>(), It.IsAny<string>())).Returns(_testData);

         writer.GenerateMigrations(context, mockDumper.Object);
      }

      /// <summary>
      /// Generates C# files from the supplied table migrations
      /// </summary>
      /// <param name="tableDefinitions">The table definitions that are the source for the migration</param>
      private SchemaMigrationContext GenerateTableMigrations(IList<TableDefinition> tableDefinitions)
      {
         SchemaMigrationContext context = GetDefaultContext();

         GenerateTableMigrations(context, tableDefinitions);

         return context;
      }

      private SchemaMigrationContext GetDefaultContext()
      {
         return new SchemaMigrationContext
                   {
                      WorkingDirectory = _tempDirectory
                      ,MigrationClassNamer = (index, table) => "Test"
                   };
      }
   }
}
