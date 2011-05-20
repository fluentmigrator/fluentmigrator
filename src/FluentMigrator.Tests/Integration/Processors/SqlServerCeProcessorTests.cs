#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// Copyright (c) 2011, Grant Archibald
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors
{
   [TestFixture]
   public class SqlServerCeProcessorTests
   {
      public string ConnectionString;
      public string TempDb;
      private string _tempFolder;
      private SqlCeConnection _connection;
      private SqlServerCeProcessor _processor;

      [SetUp]
      public virtual void Setup()
      {
         _tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

         Directory.CreateDirectory(_tempFolder);

         TempDb = Path.Combine(_tempFolder, "test.sdf");

         ConnectionString = "Data Source=" + TempDb;

         var engine = new SqlCeEngine(ConnectionString);
         engine.CreateDatabase();

         _connection = new SqlCeConnection(ConnectionString);
         _processor = new SqlServerCeProcessor(_connection, new SqlServerCeGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions());
      }

      [TearDown]
      public virtual void TearDown()
      {
         if (_processor != null)
         {
            _processor.Dispose();
            _processor = null;
         }
         Directory.Delete(_tempFolder, true);
      }

      [Test]
      public void DatabaseCreated() {
         File.Exists(TempDb);
      }

      [Test]
      public void CallingTableExistsReturnsFalseIfTableDoesNotExist()
      {
         _processor.TableExists(null, "DoesNotExist").ShouldBeFalse();
      }

      [Test]
      public void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
      {
         _processor.ColumnExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
      }

      [Test]
      public void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
      {
         CreateTestTable();

         _processor.ColumnExists(null, "Foo", "DoesNotExist").ShouldBeFalse();
      }

      [Test]
      public void CanReadDataFromTestTable()
      {
         CreateTestTable();

         var table = _processor.ReadTableData(string.Empty, "Foo");

         Assert.AreEqual(0, table.Tables[0].Rows.Count);
      }

      [Test]
      public void CanDeleteConstraint()
      {
         CreateTestTable();

         var table = _processor.ReadTableData(string.Empty, "Foo");

         Assert.AreEqual(0, table.Tables[0].Rows.Count);
      }

      [Test]
      public void CanInsertAndReadDataFromTestTable()
      {
         CreateTestTable();

         _processor.Execute("INSERT INTO Foo VALUES ( {0} )", 1);

         var table = _processor.ReadTableData(string.Empty, "Foo");

         Assert.AreEqual(1, table.Tables[0].Rows.Count);
      }

      [Test]
      public void CanCheckIndexExists()
      {
         CreateTestTableWithIndex();

         _processor.IndexExists(string.Empty, "Foo", "IDX_Id").ShouldBeTrue();
      }

      [Test]
      public void CanCheckContraintExists()
      {
         CreateTestTableWithConstraint();

         _processor.ConstraintExists(string.Empty, "Foo", "CK_Test").ShouldBeTrue();
      }

      [Test]
      public void CanCheckSchemaExists()
      {
         
         _processor.SchemaExists(string.Empty).ShouldBeTrue();
      }

      [Test]
      public void CanCheckDboSchemaExists()
      {
         _processor.SchemaExists("dbo").ShouldBeTrue();
         _processor.SchemaExists("DBO").ShouldBeTrue();
      }

      [Test]
      [ExpectedException(typeof(NotSupportedException), ExpectedMessage = "Schemas not supported by SQL Compact")]
      public void ThrowsExceptionIfSelectNonDboSchema()
      {
         _processor.SchemaExists("DoesNotExist");
      }

      [Test]
      public void CanCommitTransaction()
      {
         _processor.CommitTransaction();
      }

      [Test]
      public void CanRollbackTransaction()
      {
         _processor.RollbackTransaction();
      }

      [Test]
      public void CanCreateTableWithDecimalPrecision()
      {
         _processor.Process(new CreateTableExpression
         {
            TableName = "Foo",
            Columns = { new ColumnDefinition { Name = "Bar", Type = DbType.Decimal, Precision = 4, Size = 38} }
         });
      }

      [Test]
      public void SqlCeSupportsDecimalPrecisionAbove19()
      {
         _processor.Execute("CREATE TABLE Foo ( Value Numeric(38,4) )");

         _processor.Exists("SELECT * FROM Foo");
         var schema = _processor.GetTableSchema(string.Empty, "Foo");
         Assert.IsNotNull(schema, "Table not found");

         Assert.AreEqual(38, schema.Rows[0]["NumericPrecision"]);
         Assert.AreEqual(4, schema.Rows[0]["NumericScale"]);
      }

      [Test]
      public void CanCreateTableWithDecimal()
      {
         _processor.Process(new CreateTableExpression
         {
            TableName = "Foo",
            Columns = { new ColumnDefinition { Name = "Bar", Type = DbType.Decimal } }
         });
      }

      [Test]
      public void CanInsertRowIntoTable()
      {
         _processor.Process(new CreateTableExpression
         {
            TableName = "Foo",
            Columns = { new ColumnDefinition { Name = "Bar", Type = DbType.Decimal } }
         });

         var insert = new InsertDataExpression {TableName = "Foo"};
         insert.Rows.Add(new InsertionDataDefinition() { new KeyValuePair<string, object>("Bar", 12)});
         _processor.Process(insert);
      }

      [Test]
      [ExpectedException(typeof(Exception))]
      public void CanInsertMultipleRowsIntoTable()
      {
         _processor.Process(new CreateTableExpression
         {
            TableName = "Foo",
            Columns = { new ColumnDefinition { Name = "Bar", Type = DbType.Decimal } }
         });

         var insert = new InsertDataExpression { TableName = "Foo" };
         insert.Rows.Add(new InsertionDataDefinition() { new KeyValuePair<string, object>("Bar", 41) });
         insert.Rows.Add(new InsertionDataDefinition() { new KeyValuePair<string, object>("Bar", 42) });
         _processor.Process(insert);
      }

      [Test]
      public void CanCreateAndDropIndex()
      {
         _processor.Process(new CreateTableExpression
         {
            TableName = "Foo",
            Columns = { new ColumnDefinition { Name = "Bar", Type = DbType.Decimal } }
         });

         var index = new CreateIndexExpression()
                        {
                           Index = new IndexDefinition()
                                      {
                                         TableName = "Foo"
                                         ,Name = "IDX_Bar"
                                         ,Columns = new[] {new IndexColumnDefinition() {Name = "Bar"}}
                                      }
                        };
         _processor.Process(index);

         var dropIndex = new DeleteIndexExpression()
                           {
                              Index = new IndexDefinition()
                                         {
                                            TableName = "Foo"
                                            ,Name = "IDX_Bar"
                                         }
                           };

         _processor.Process(dropIndex);
      }

      [Test]
      public void CanCreateAndDropColumn()
      {
         _processor.Process(new CreateTableExpression
         {
            TableName = "Foo",
            Columns =
               {
                  new ColumnDefinition { Name = "Bar", Type = DbType.Decimal }
                  , new ColumnDefinition { Name = "Buzz", Type = DbType.Decimal }
               }
         });

         _processor.Process(new DeleteColumnExpression()
         {
            TableName = "Foo"
            , ColumnName = "Buzz"
         });
      }

      private void CreateTestTable()
      {
         _processor.Process(new CreateTableExpression
                               {
                                  TableName = "Foo",
                                  Columns = { new ColumnDefinition { Name = "Id", Type = DbType.Int32 } }
                               });
      }

      private void CreateTestTableWithIndex()
      {
         CreateTestTable();

         _processor.Process(
            new CreateIndexExpression
               {                    
               Index = new IndexDefinition
                          {
                      Name = "IDX_Id"
                       , Columns = new[] { new IndexColumnDefinition { Name = "Id", Direction = Direction.Ascending}}         
                      , TableName = "Foo"
                   }
         });
      }

      private void CreateTestTableWithConstraint()
      {
         CreateTestTable();

         _processor.Execute("ALTER TABLE Foo ADD CONSTRAINT CK_TEST PRIMARY KEY ( Id )");
      }

     
   }
}
