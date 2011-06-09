using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FluentMigrator.Model;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.SchemaDump.SchemaDumpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.SchemaDump
{
   [TestFixture]
   public class OracleDumpTableTests : OracleUnitTest
   {

      [Test]
      public void OracleVarCharIsString()
      {
         var column = GetTableColumn("varchar(50)");

         column.Type.ShouldBe(DbType.AnsiString);
         column.Size.ShouldBe(50);
      }

      [Test]
      public void OracleNVarCharIsString()
      {
         var column = GetTableColumn("nvarchar2(50)");

         column.Type.ShouldBe(DbType.String);
         column.Size.ShouldBe(50);
      }

      [Test]
      public void OracleNClobIsString()
      {
         var column = GetTableColumn("nclob");

         column.Type.ShouldBe(DbType.String);
         column.Size.ShouldBe(Int32.MaxValue);
      }

      [Test]
      public void OracleClobString()
      {
         var column = GetTableColumn("clob");

         column.Type.ShouldBe(DbType.String);
         column.Size.ShouldBe(Int32.MaxValue);
      }


      [Test]
      public void OracleNumberToInt16()
      {
         var column = GetTableColumn("NUMBER(5,0)");

         column.Type.ShouldBe(DbType.Int16);
      }

      [Test]
      public void OracleNumberToInt32()
      {
         var column = GetTableColumn("NUMBER(10,0)");

         column.Type.ShouldBe(DbType.Int32);
      }

      [Test]
      public void OracleNumberToInt64()
      {
         var column = GetTableColumn("NUMBER(20,0)");

         column.Type.ShouldBe(DbType.Int64);
      }

      [Test]
      public void OracleRaw16ToGuid()
      {
         var column = GetTableColumn("RAW(16)");

         column.Type.ShouldBe(DbType.Guid);
      }

      [Test]
      public void OracleBlobToBinary()
      {
         var column = GetTableColumn("blob");

         column.Type.ShouldBe(DbType.Binary);
         column.Size.ShouldBe(Int32.MaxValue);
      }

      [Test]
      public void OracleNumber1ToBoolean()
      {
         var column = GetTableColumn("NUMBER(1,0)");

         column.Type.ShouldBe(DbType.Boolean);
         column.Size.ShouldBe(1);
      }

      [Test]
      public void OracleNumber3ToByte()
      {
         var column = GetTableColumn("NUMBER(3,0)");

         column.Type.ShouldBe(DbType.Byte);
      }

      [Test]
      public void OracleTimestampToDateTime()
      {
         var column = GetTableColumn("TIMESTAMP(4)");

         column.Type.ShouldBe(DbType.DateTime);
      }

      [Test]
      public void OracleDateToDate()
      {
         var column = GetTableColumn("DATE");

         column.Type.ShouldBe(DbType.Date);
      }

      [Test]
      public void OracleCharToAnsiStringFixedLength()
      {
         var column = GetTableColumn("CHAR(255)");

         column.Type.ShouldBe(DbType.AnsiStringFixedLength);
         column.Size.ShouldBe(255);
      }

      [Test]
      public void OracleNumberToDecimal()
      {
         var column = GetTableColumn("NUMBER(19,5)");

         column.Type.ShouldBe(DbType.Decimal);
      }

      [Test]
      public void OracleDoublePrescisionToDouble()
      {
         var column = GetTableColumn("DOUBLE PRECISION");

         column.Type.ShouldBe(DbType.Double);
      }

      [Test]
      public void OracleFloatToSingle()
      {
         var column = GetTableColumn("FLOAT(24)");

         column.Type.ShouldBe(DbType.Single);
      }

      [Test]
      public void OracleNCharToStringFixedLength()
      {
         var column = GetTableColumn("NCHAR(255)");

         column.Type.ShouldBe(DbType.StringFixedLength);
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
         var processor = ((OracleProcessor)new OracleProcessorFactory().Create(ConnectionString,
         new DebugAnnouncer(), new ProcessorOptions()));


         processor.Execute("CREATE TABLE Foo ( Data {0} NULL )", type);

         Assert.IsTrue(processor.TableExists(string.Empty, "Foo"), "Oracle");

         var dumper = new OracleSchemaDumper(processor, new DebugAnnouncer());
         tables = dumper.ReadDbSchema();

         processor.CommitTransaction();

         tables.Count.ShouldBe(1);

         return tables[0].Columns.ToList()[0];
      }
   }
}
