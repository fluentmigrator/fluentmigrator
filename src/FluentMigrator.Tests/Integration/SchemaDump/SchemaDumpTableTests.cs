using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.SchemaDump.SchemaDumpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.SchemaDump
{
   public class SchemaDumpTableTests : SqlServerUnitTest
   {
      [Test]
      public void SqlServerImageTypeIsBinary()
      {
         var column = GetTableColumn("Image");

         column.Type.ShouldBe(DbType.Binary);
         column.Size.ShouldBe(int.MaxValue);
      }

      [Test]
      public void SqlServerVarBinary4000IsBinary()
      {
         var column = GetTableColumn("varbinary(4000)");

         column.Type.ShouldBe(DbType.Binary);
         column.Size.ShouldBe(4000);
      }

      [Test]
      public void SqlServerVarBinaryIsBinary()
      {
         var column = GetTableColumn("varbinary(max)");

         column.Type.ShouldBe(DbType.Binary);
         column.Size.ShouldBe(int.MaxValue);
      }

      [Test]
      public void SqlServerTextIsAnsiString()
      {
         var column = GetTableColumn("Text");

         column.Type.ShouldBe(DbType.AnsiString);
         column.Size.ShouldBe(int.MaxValue);
      }

      [Test]
      public void SqlServerNTextIsString()
      {
         var column = GetTableColumn("NText");

         column.Type.ShouldBe(DbType.String);
         column.Size.ShouldBe(int.MaxValue / 2);
      }

      [Test]
      public void SqlServerVarChar8000IsAnsiString()
      {
         var column = GetTableColumn("varchar(8000)");

         column.Type.ShouldBe(DbType.AnsiString);
         column.Size.ShouldBe(8000);
      }

      [Test]
      public void SqlServerNVarChar4000IsString()
      {
         var column = GetTableColumn("nvarchar(4000)");

         column.Type.ShouldBe(DbType.String);
         column.Size.ShouldBe(4000);
      }

      [Test]
      public void SqlServerVarCharMaxIsAnsiString()
      {
         var column = GetTableColumn("varchar(max)");

         column.Type.ShouldBe(DbType.AnsiString);
         column.Size.ShouldBe(int.MaxValue);
      }

      [Test]
      public void SqlServerChar200IsAnsiString()
      {
         var column = GetTableColumn("char(200)");

         column.Type.ShouldBe(DbType.AnsiStringFixedLength);
         column.Size.ShouldBe(200);
      }

      [Test]
      public void SqlServerNChar200IsAnsiString()
      {
         var column = GetTableColumn("nchar(200)");

         column.Type.ShouldBe(DbType.StringFixedLength);
         column.Size.ShouldBe(200);
      }

      [Test]
      public void SqlServerNVarCharMaxIsString()
      {
         var column = GetTableColumn("nvarchar(max)");

         column.Type.ShouldBe(DbType.String);
         column.Size.ShouldBe(int.MaxValue);
      }

      [Test]
      public void SqlServerDateTimeIsDateTime()
      {
         var column = GetTableColumn("datetime");

         column.Type.ShouldBe(DbType.DateTime);
      }

      [Test]
      public void SqlServerSmallDateTimeIsDate()
      {
         var column = GetTableColumn("smalldatetime");

         column.Type.ShouldBe(DbType.Date);
      }

      [Test]
      public void CreateTableWithIndexes()
      {
         var column = GetTableColumnColumns("CREATE TABLE Foo (Id int, Id2 int)"
            , new CreateIndexExpression { Index = new IndexDefinition { TableName = "Foo", Name= "IDX_Id", Columns = new [] { new IndexColumnDefinition { Name = "Id" } } } }
            , new CreateIndexExpression { Index = new IndexDefinition { TableName = "Foo", Name = "IDX_Id2", Columns = new[] { new IndexColumnDefinition { Name = "Id2" } } } }
            , new CreateIndexExpression { Index = new IndexDefinition { TableName = "Foo", Name = "IDX_All", Columns = new[]
                                                                                                                          {
                                                                                                                             new IndexColumnDefinition { Name = "Id" }
                                                                                                                             ,new IndexColumnDefinition { Name = "Id2" }
                                                                                                                          } } });

         column.Indexes.Count.ShouldBe(3);

         column.Indexes.Where(i => i.Name == "IDX_Id").First().Columns.Count.ShouldBe(1);
         column.Indexes.Where(i => i.Name == "IDX_Id2").First().Columns.Count.ShouldBe(1);
         column.Indexes.Where(i => i.Name == "IDX_All").First().Columns.Count.ShouldBe(2);
      }

      [Test]
      public void CreateForeignKey()
      {
          var foo = GetTableColumnColumns(string.Empty, new CreateTableExpression { TableName = "Foo", Columns = new List<ColumnDefinition> { new ColumnDefinition { Name = "Id", Type = DbType.Int32, IsPrimaryKey = true}}});
          var bar = GetTableColumnColumns(string.Empty, "Bar", 
              new CreateTableExpression { TableName = "Bar", Columns = new List<ColumnDefinition> { new ColumnDefinition { Name = "Id2", Type = DbType.Int32, IsPrimaryKey = true}}}
              , new CreateForeignKeyExpression { ForeignKey = new ForeignKeyDefinition { Name = "FK_Foo"
                  , ForeignTable = "Bar", ForeignColumns = new [] { "Id2"}
                  , PrimaryTable = "Foo", PrimaryColumns = new [] { "Id"}}});

          bar.ForeignKeys.Count.ShouldBe(1);

          bar.ForeignKeys.First().Name.ShouldBe("FK_Foo");
          bar.ForeignKeys.First().ForeignTable.ShouldBe("Bar");
          bar.ForeignKeys.First().ForeignColumns.First().ShouldBe("Id2");
          bar.ForeignKeys.First().PrimaryTable.ShouldBe("Foo");
          bar.ForeignKeys.First().PrimaryColumns.First().ShouldBe("Id");
      }

      [Test]
      public void CreateForeignKeyMultipleColumns()
      {
          var foo = GetTableColumnColumns(string.Empty, new CreateTableExpression { TableName = "Foo", Columns = new List<ColumnDefinition>
                                                                                                                     {
                                                                                                                         new ColumnDefinition { Name = "Id", Type = DbType.Int32, IsPrimaryKey = true }
                                                                                                                         , new ColumnDefinition { Name = "Name", Type = DbType.String, IsPrimaryKey = true }
                                                                                                                     } });
          var bar = GetTableColumnColumns(string.Empty, "Bar",
              new CreateTableExpression { TableName = "Bar", Columns = new List<ColumnDefinition>
                                                                           {
                                                                               new ColumnDefinition { Name = "Id2", Type = DbType.Int32, IsPrimaryKey = true }
                                                                               , new ColumnDefinition { Name = "Name2", Type = DbType.String, IsPrimaryKey = true }
                                                                           } }
              , new CreateForeignKeyExpression
              {
                  ForeignKey = new ForeignKeyDefinition
                  {
                      Name = "FK_Foo"
                      ,
                      ForeignTable = "Bar",
                      ForeignColumns = new[] { "Id2", "Name2" }
                      ,
                      PrimaryTable = "Foo",
                      PrimaryColumns = new[] { "Id", "Name" }
                  }
              });

          bar.ForeignKeys.Count.ShouldBe(1);

          bar.ForeignKeys.First().Name.ShouldBe("FK_Foo");
          bar.ForeignKeys.First().ForeignTable.ShouldBe("Bar");
          bar.ForeignKeys.First().ForeignColumns.First().ShouldBe("Id2");
          bar.ForeignKeys.First().ForeignColumns.Last().ShouldBe("Name2");
          bar.ForeignKeys.First().PrimaryTable.ShouldBe("Foo");
          bar.ForeignKeys.First().PrimaryColumns.First().ShouldBe("Id");
          bar.ForeignKeys.First().PrimaryColumns.Last().ShouldBe("Name");
      }

      /// <summary>
      /// Creates a single column table using the spplied type and retruns its <see cref="ColumnDefinition"/>
      /// </summary>
      /// <param name="type">The Sql Server data type to apply to the column</param>
      /// <returns>The translated <see cref="ColumnDefinition"/></returns>
      private ColumnDefinition GetTableColumn(string type, params IMigrationExpression[] expresions)
      {
         return GetTableColumnColumns(string.Format("CREATE TABLE Foo ( Data {0} NULL )", type), expresions).Columns.ToList()[0];
      }

      /// <summary>
      /// Creates a single column table using the spplied type and retruns its <see cref="ColumnDefinition"/>
      /// </summary>
      /// <param name="type">The Sql Server data type to apply to the column</param>
      /// <returns>The translated <see cref="ColumnDefinition"/></returns>
      private TableDefinition GetTableColumnColumns(string createSql, params IMigrationExpression[] expresions)
      {
          return GetTableColumnColumns(createSql, "Foo", expresions);
      }

      /// <summary>
      /// Creates a single column table using the spplied type and retruns its <see cref="ColumnDefinition"/>
      /// </summary>
      /// <param name="type">The Sql Server data type to apply to the column</param>
      /// <returns>The translated <see cref="ColumnDefinition"/></returns>
      private TableDefinition GetTableColumnColumns(string createSql, string name, params IMigrationExpression[] expresions)
      {
          IList<TableDefinition> tables;

          // Act
          using (var connection = new SqlConnection(ConnectionString))
          {
              var processor = new SqlServerProcessor(connection, new SqlServer2005Generator(), new DebugAnnouncer(), new ProcessorOptions());

              if (!string.IsNullOrEmpty(createSql))
                  processor.Execute(createSql);

              foreach (var expresion in expresions)
              {
                  if (expresion is CreateTableExpression)
                      processor.Process((CreateTableExpression)expresion);
                  if (expresion is CreateIndexExpression)
                      processor.Process((CreateIndexExpression)expresion);
                  if (expresion is CreateForeignKeyExpression)
                      processor.Process((CreateForeignKeyExpression)expresion);
              }

              Assert.IsTrue(processor.TableExists(string.Empty, name), "SqlServer");

              var dumper = new SqlServerSchemaDumper(processor, new DebugAnnouncer());
              tables = dumper.ReadDbSchema();

              processor.CommitTransaction();
          }

          if (!string.IsNullOrEmpty(createSql))
              tables.Count.ShouldBe(1);

          return tables.Where(t => t.Name == name).FirstOrDefault();
      }
   }
}
