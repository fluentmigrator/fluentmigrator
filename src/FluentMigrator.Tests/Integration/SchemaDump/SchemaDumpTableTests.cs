using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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

         column.Type.ShouldBe(DbType.AnsiString);
         column.Size.ShouldBe(200);
      }

      [Test]
      public void SqlServerNVarCharMaxIsString()
      {
         var column = GetTableColumn("nvarchar(max)");

         column.Type.ShouldBe(DbType.String);
         column.Size.ShouldBe(int.MaxValue);
      }

      /// <summary>
      /// Creates a single column table using the spplied type and retruns its <see cref="ColumnDefinition"/>
      /// </summary>
      /// <param name="type">The Sql Server data type to apply to the column</param>
      /// <returns>The translated <see cref="ColumnDefinition"/></returns>
      private ColumnDefinition GetTableColumn(string type)
      {
         IList<TableDefinition> tables;

         // Act
         using (var connection = new SqlConnection(ConnectionString))
         {
            var processor = new SqlServerProcessor(connection, new SqlServer2005Generator(), new DebugAnnouncer(), new ProcessorOptions());

            processor.Execute("CREATE TABLE Foo ( Data {0} NULL )", type);

            Assert.IsTrue(processor.TableExists(string.Empty, "Foo"), "SqlServer");

            var dumper = new SqlServerSchemaDumper(processor, new DebugAnnouncer());
            tables = dumper.ReadDbSchema();

            processor.CommitTransaction();
         }

         tables.Count.ShouldBe(1);

         return tables[0].Columns.ToList()[0];
      }
   }
}
