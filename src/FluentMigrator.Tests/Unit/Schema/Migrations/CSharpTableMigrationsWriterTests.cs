using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
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

         SetupTestData(typeof(int),1);
      }

      private void SetupTestData(Type columnType, object value)
      {
         _testData = new DataSet();
         var table = new DataTable("Foo");
         table.Columns.Add(new DataColumn("Data", columnType));
         table.Rows.Add(value);
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
                                            Columns = new[] {new ColumnDefinition {Name = "Data", Type = DbType.Int32}}
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
                                            Columns = new[] {new ColumnDefinition {Name = "Data", Type = DbType.String}}
                                         }
                                   };


         var context = GetDefaultContext();
         context.Type = MigrationType.Tables | MigrationType.Data;
         context.WorkingDirectory = _tempDirectory;
         context.InsertColumnReplacements.Add(new InsertColumnReplacement
                                                 { 
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
      public void AddColumnInsertCaseSensitiveAll()
      {
         // Arrange
         var tableDefinitions = new List<TableDefinition>
                                   {
                                      new TableDefinition
                                         {
                                            Name = "Foo",
                                            Columns = new[] {new ColumnDefinition {Name = "Data", Type = DbType.String}}
                                         }
                                   };


         var context = GetDefaultContext();
         context.CaseSenstiveColumnNames = true;
         context.Type = MigrationType.Tables | MigrationType.Data;
         context.WorkingDirectory = _tempDirectory;

         // Act
         GenerateTableMigrations(context, tableDefinitions);

         // Assert
         var migration = File.ReadAllText(Path.Combine(_tempDirectory, @"Migrations\Test.cs"));

         File.Exists(Path.Combine(_tempDirectory, @"Data\Foo.xml")).ShouldBeTrue();
         File.Exists(Path.Combine(_tempDirectory, @"Data\Foo.xsd")).ShouldBeTrue();
         migration.Contains("Insert.IntoTable(\"Foo\").DataTable(@\"Data\\Foo.xml\").WithCaseSensitiveColumnNames();").ShouldBeTrue();
      }

      [Test]
      public void AddColumnInsertCaseSenstiveColumn()
      {
         // Arrange
         var tableDefinitions = new List<TableDefinition>
                                   {
                                      new TableDefinition
                                         {
                                            Name = "Foo",
                                            Columns = new[] {new ColumnDefinition {Name = "Data", Type = DbType.String}}
                                         }
                                   };


         var context = GetDefaultContext();
         context.CaseSenstiveColumnNames = true;
         context.CaseSenstiveColumns.Add("Data");
         context.Type = MigrationType.Tables | MigrationType.Data;
         context.WorkingDirectory = _tempDirectory;

         // Act
         GenerateTableMigrations(context, tableDefinitions);

         // Assert
         var migration = File.ReadAllText(Path.Combine(_tempDirectory, @"Migrations\Test.cs"));

         File.Exists(Path.Combine(_tempDirectory, @"Data\Foo.xml")).ShouldBeTrue();
         File.Exists(Path.Combine(_tempDirectory, @"Data\Foo.xsd")).ShouldBeTrue();
         migration.Contains("Insert.IntoTable(\"Foo\").DataTable(@\"Data\\Foo.xml\").WithCaseSensitiveColumn(\"Data\");").ShouldBeTrue();
      }

      [Test]
      public void AddColumnInsertDateTimeReplacement()
      {
         // Arrange
         var tableDefinitions = new List<TableDefinition>
                                   {
                                      new TableDefinition
                                         {
                                            Name = "Foo",
                                            Columns = new[] {new ColumnDefinition {Name = "Data", Type = DbType.DateTime}}
                                         }
                                   };


         var context = GetDefaultContext();
         context.Type = MigrationType.Tables | MigrationType.Data;
         context.WorkingDirectory = _tempDirectory;
         context.InsertColumnReplacements.Add(new InsertColumnReplacement
                                                 {
            ColumnDataToMatch = new ColumnDefinition { Type = DbType.DateTime }
            ,OldValue = DateTime.ParseExact("1900-01-01", "yyyy-MM-dd",null)
            ,NewValue = DateTime.ParseExact("2000-01-01", "yyyy-MM-dd", null)
         })
         ;

         // Act
         GenerateTableMigrations(context, tableDefinitions);

         // Assert
         var migration = File.ReadAllText(Path.Combine(_tempDirectory, @"Migrations\Test.cs"));

         File.Exists(Path.Combine(_tempDirectory, @"Data\Foo.xml")).ShouldBeTrue();
         File.Exists(Path.Combine(_tempDirectory, @"Data\Foo.xsd")).ShouldBeTrue();
         migration.Contains("Insert.IntoTable(\"Foo\").DataTable(@\"Data\\Foo.xml\").WithReplacementValue(DateTime.ParseExact(\"1900-01-01\", \"yyyy-MM-dd\", null), DateTime.ParseExact(\"2000-01-01\", \"yyyy-MM-dd\", null));").ShouldBeTrue();
      }

      [Test]
      public void CanMigrateString()
      {
         // Arrange
         var tableDefinitions = new List<TableDefinition>
                                   {
                                      new TableDefinition
                                         {
                                            Name = "Foo",
                                            Columns = new[] {new ColumnDefinition {Name = "Data", Type = DbType.String}}
                                         }
                                   };


         var context = GetDefaultContext();
         context.Type = MigrationType.Tables | MigrationType.Data;
         context.WorkingDirectory = _tempDirectory;

         SetupTestData(typeof (string), "Ä Test");

         // Act
         GenerateTableMigrations(context, tableDefinitions);

         // Assert
         var data = File.ReadAllText(Path.Combine(_tempDirectory, @"Data\Foo.xml"));

         data.Contains("Ä Test").ShouldBeTrue();
      }

      [Test]
      public void CanMigrateStringAsciiEncoding()
      {
         // Arrange
         var tableDefinitions = new List<TableDefinition>
                                   {
                                      new TableDefinition
                                         {
                                            Name = "Foo",
                                            Columns = new[] {new ColumnDefinition {Name = "Data", Type = DbType.String}}
                                         }
                                   };


         var context = GetDefaultContext();
         context.Type = MigrationType.Tables | MigrationType.Data;
         context.WorkingDirectory = _tempDirectory;
         context.MigrationEncoding = Encoding.ASCII;

         SetupTestData(typeof(string), "Ä Test");

         // Act
         GenerateTableMigrations(context, tableDefinitions);

         // Assert
         var data = File.ReadAllText(Path.Combine(_tempDirectory, @"Data\Foo.xml"));

         Debug.WriteLine(data);
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
                                            Columns = new[] {new ColumnDefinition {Name = "Data", Type = DbType.Int32}}
                                         }
                                   };

         
         var context = GetDefaultContext();
         context.Type = MigrationType.Tables | MigrationType.Data;
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
                                            Columns = new[] {new ColumnDefinition {Name = "Data", Type = DbType.Int32}}
                                         }
                                   };
         var context = GetDefaultContext();
         context.Type = MigrationType.Tables | MigrationType.Data;
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
                                                                   Name = "Data", Type = DbType.Int32
                                                                   , IsIdentity = true
                                                                }}
                                         }
                                   };

         // Arrange
         var context = GetDefaultContext();
         context.Type = MigrationType.Tables | MigrationType.Data;
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
                                            Columns = new[] {new ColumnDefinition {Name = "Data", Type = DbType.Int32}}
                                         }
                                   };

         // Arrange
         var context = GetDefaultContext();
         context.Type = MigrationType.Tables | MigrationType.Data;
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
         var migration = GetTestMigration(new[] {new ColumnDefinition {Name = "Data", Type = DbType.Int32}});

         //Assert
         migration.Contains("Create.Table(\"Foo\")").ShouldBeTrue();
         migration.Contains("Delete.Table(\"Foo\");").ShouldBeTrue();
      }

      [Test]
      public void CreatesNotNullable()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.Int32 });

         //Assert
         migration.Contains(".NotNullable()").ShouldBeTrue();
      }

      [Test]
      public void CreatesNullable()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.Int32, IsNullable = true});

         //Assert
         migration.Contains(".Nullable()").ShouldBeTrue();
      }


      [Test]
      public void CreatesIdentityColumn()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.Int32, IsIdentity = true });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsInt32().Identity()").ShouldBeTrue();
      }

      /// <summary>
      /// Shluld not use Indexes syntax as this should be done using <see cref="MigrationType.Indexes"/>
      /// </summary>
      [Test]
      public void DoesNotCreateIndexedColumn()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.Int32, IsIndexed = true});

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsInt32().NotNullable();").ShouldBeTrue();
      }

      [Test]
      public void WithColumnBinary()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.Binary, Size = 20 });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsBinary(20)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnBoolean()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.Boolean });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsBoolean()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnByte()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.Byte });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsByte()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnCurrency()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.Currency });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsCurrency()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnDate()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.Date });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsDate()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnDateTime()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.DateTime });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsDateTime()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnStringAndUndefinedDefault()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.String, DefaultValue = new ColumnDefinition.UndefinedDefaultValue() });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsString()").ShouldBeTrue();
         migration.Contains(".WithDefaultValue").ShouldBeFalse();
      }


      [Test]
      public void WithColumnStringAndDefault()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.String, DefaultValue = " " });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsString().WithDefaultValue(\" \")").ShouldBeTrue();
      }

      [Test]
      public void WithColumnDateTimeAndDefault()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.DateTime, DefaultValue = "'1900-01-01'"});

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsDateTime().WithDefaultValue(DateTime.ParseExact(\"1900-01-01\", \"yyyy-MM-dd\", null))").ShouldBeTrue();
      }

      [Test]
      public void WithColumnDateTimeAndDefaultCustomFormat()
      {
         // Arrange
         var settings = GetDefaultContext();
         settings.DateTimeDefaultValueFormatter = (columnDefinition, defaultValue) => "XXX";

         // Act
         var migration = GetTestMigration(settings, new ColumnDefinition { Name = "Data", Type = DbType.DateTime, DefaultValue = "'1900-01-01'" });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsDateTime().WithDefaultValue(XXX)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnDouble()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.Double });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsDouble()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnDecimal()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.Decimal, Precision = 19, Scale = 4});

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsDecimal(19,4)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnGuid()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.Guid });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsGuid()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnGuidWithDefaultNewId()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.Guid, DefaultValue = SystemMethods.NewGuid});

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsGuid().WithDefaultValue(SystemMethods.NewGuid)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnInt16()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.Int16 });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsInt16()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnInt32()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.Int32 });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsInt32()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnInt64()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.Int64 });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsInt64()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnString()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.String });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsString()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnStringWithSize()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.String, Size = 20});

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsString(20)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnStringAnsiFixedLength()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.AnsiStringFixedLength, Size = 20 });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsFixedLengthAnsiString(20)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnStringFixedLength()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.StringFixedLength, Size = 20 });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsFixedLengthString(20)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnAnsiString()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.AnsiString });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsAnsiString()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnAnsiStringWithLength()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.AnsiString, Size = 20 });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsAnsiString(20)").ShouldBeTrue();
      }

      [Test]
      public void WithColumnXml()
      {
         // Arrange

         // Act
         var migration = GetTestMigration(new ColumnDefinition { Name = "Data", Type = DbType.Xml });

         //Assert
         migration.Contains(".WithColumn(\"Data\").AsXml()").ShouldBeTrue();
      }

      [Test]
      public void WithColumnStringAndIndex()
      {
         // Arrange
         var context = GetDefaultContext();
         context.Type = MigrationType.Tables | MigrationType.Indexes;

         // Act
         var migration = GetTestMigrationWithIndex(context
            , new IndexDefinition {Name = "IDX_Foo", TableName = "Foo", Columns = new[] { new IndexColumnDefinition { Name="Data"}}}
            , new ColumnDefinition { Name = "Data", Type = DbType.String, IsIndexed = true});

         //Assert

         Debug.WriteLine(migration);

         migration.Contains(".WithColumn(\"Data\").AsString().NotNullable();").ShouldBeTrue();

         migration.Contains("Create.Index(\"IDX_Foo\").OnTable(\"Foo\")").ShouldBeTrue();
         migration.Contains(".OnColumn(\"Data\").Ascending();").ShouldBeTrue();
      }

      [Test]
      public void WithColumnStringAndUniqueIndex()
      {
         // Arrange
         var context = GetDefaultContext();
         context.Type = MigrationType.Tables | MigrationType.Indexes;

         // Act
         var migration = GetTestMigrationWithIndex(context
            , new IndexDefinition { Name = "IDX_Foo", TableName = "Foo", IsUnique = true, Columns = new[] { new IndexColumnDefinition { Name = "Data" } } }
            , new ColumnDefinition { Name = "Data", Type = DbType.Xml });

         //Assert

         Debug.WriteLine(migration);

         migration.Contains(".WithColumn(\"Data\").AsXml()").ShouldBeTrue();

         migration.Contains("Create.Index(\"IDX_Foo\").OnTable(\"Foo\")").ShouldBeTrue();
         migration.Contains(".OnColumn(\"Data\").Ascending().WithOptions().Unique();").ShouldBeTrue();
      }

      [Test]
      public void WithColumnStringAndClusteredIndex()
      {
         // Arrange

         // Act
         var migration = GetTestMigrationWithIndex(GetDefaultContext()
            , new IndexDefinition { Name = "IDX_Foo", TableName = "Foo", IsClustered = true, Columns = new[] { new IndexColumnDefinition { Name = "Data" } } }
            , new ColumnDefinition { Name = "Data", Type = DbType.Xml });

         //Assert

         Debug.WriteLine(migration);

         migration.Contains(".WithColumn(\"Data\").AsXml()").ShouldBeTrue();

         migration.Contains("Create.Index(\"IDX_Foo\").OnTable(\"Foo\")").ShouldBeTrue();
         migration.Contains(".OnColumn(\"Data\").Ascending().WithOptions().Clustered();").ShouldBeTrue();
      }

      [Test]
      public void WithColumnForeignKey()
      {
         // Arrange

         // Act
         GetTestMigrationWithForeignKey(GetDefaultContext()
            , new ForeignKeyDefinition { Name = "FK_Foo", ForeignTable = "Foo", ForeignColumns = new[] { "BarId" }, PrimaryTable = "Bar", PrimaryColumns = new[] { "Id" } }
            , new ColumnDefinition { Name = "Id", Type = DbType.Int32 }
            , new ColumnDefinition { Name = "BarId", Type = DbType.Int32 });

         //Assert

         var foreignKeyMigration = File.ReadAllText(Path.Combine(_tempDirectory, @"Migrations\TestForeignKey.cs"));

         Debug.WriteLine(foreignKeyMigration);

         foreignKeyMigration.Contains("Create.ForeignKey(\"FK_Foo\")").ShouldBeTrue();
         foreignKeyMigration.Contains(".FromTable(\"Foo\")").ShouldBeTrue();
         foreignKeyMigration.Contains(".ForeignColumn(\"BarId\")").ShouldBeTrue();
         foreignKeyMigration.Contains(".ToTable(\"Bar\")").ShouldBeTrue();
         foreignKeyMigration.Contains(".PrimaryColumn(\"Id\")").ShouldBeTrue();
      }

      private string GetTestMigration(params ColumnDefinition[] columns)
      {
         return GetTestMigration(GetDefaultContext(), columns);
      }

      private string GetTestMigration(SchemaMigrationContext context, params ColumnDefinition[] columns)
      {
         return GetTestMigrationWithIndex(context, null, columns);
      }

      private string GetTestMigrationWithIndex(SchemaMigrationContext context, IndexDefinition index, params ColumnDefinition[] columns)
      {
                 
            var tableDefinitions = new List<TableDefinition>
                                   {
                                      new TableDefinition
                                         {
                                            Name = "Foo",
                                            Columns = columns
                                         }
                                   };


            if (index != null)
               tableDefinitions[0].Indexes = new[] {index};

         GenerateTableMigrations(context, tableDefinitions);

         return File.ReadAllText(Path.Combine(_tempDirectory, @"Migrations\Test.cs"));

      }

      private string GetTestMigrationWithForeignKey(SchemaMigrationContext context, ForeignKeyDefinition foreignKey, params ColumnDefinition[] columns)
      {

         context.Type = context.Type | MigrationType.ForeignKeys;

         var tableDefinitions = new List<TableDefinition>
                                   {
                                      new TableDefinition
                                         {
                                            Name = "Foo",
                                            Columns = columns
                                         }
                                   };


         if (foreignKey != null)
            tableDefinitions[0].ForeignKeys = new[] { foreignKey };

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
