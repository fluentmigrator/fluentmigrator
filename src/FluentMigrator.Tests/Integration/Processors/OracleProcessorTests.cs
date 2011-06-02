#region License
// 
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
using System.Data.Common;
using System.Diagnostics;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors
{
   /// <summary>
   /// Note: May receive exception "ORA-12520: TNS:listener could not find available handler for requested type"
   /// Cause XE only has 40 connections by default need to increase
   /// See http://dbtricks.com/?p=30 
   /// CONNECT / As SYSDBA
   /// Alter system set processes=100 scope=spfile;
   /// shutdown immediate
   /// Startup
   /// </summary>
	[TestFixture]
	public class OracleProcessorTests : OracleUnitTest
	{
	   private IMigrationProcessor _processor;

      [SetUp]
      public override void Setup()
      {
         base.Setup();
         _processor = new OracleProcessorFactory().Create(ConnectionString, new DebugAnnouncer(), new ProcessorOptions());
      }

      [TearDown]
      public void Teardown()
      {
         if ( (_processor != null) && (_processor is OracleProcessor))
         {
            var oracleProcessor = (OracleProcessor) _processor;
            if ( oracleProcessor.Connection.State == ConnectionState.Open )
               oracleProcessor.Connection.Close();


            oracleProcessor.Connection.Dispose();

         }         
      }


		[Test]
		public void TestQuery()
		{
			DbConnection connection = OracleFactory.GetOpenConnection
            (ConnectionString);

			string sql = "Select * from Dual";
			DataSet ds = new DataSet();
			using (var command = OracleFactory.GetCommand(connection,sql ))
			using (DbDataAdapter adapter = OracleFactory.GetDataAdapter(command))
			{
				adapter.Fill(ds);
			}

			Assert.Greater(ds.Tables.Count,0);
			Assert.Greater(ds.Tables[0].Columns.Count,0);

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
      public void CanInsertAndReadDataFromTestTable()
      {
         CreateTestTable();


         _processor.Process(new InsertDataExpression()
         {
            TableName = "Foo",
            Rows = { new InsertionDataDefinition() { new KeyValuePair<string, object>("Id", 1) } }
         });

         var table = _processor.ReadTableData(string.Empty, "Foo");

         table.Tables[0].Rows.Count.ShouldBe(1);
      }

      [Test]
      public void CanInsertAndReadDateFromTestTable()
      {
         _processor.Process(new CreateTableExpression
         {
            TableName = "Foo",
            Columns = { new ColumnDefinition { Name = "Test", Type = DbType.DateTime } }
         });

         var testDate = DateTime.ParseExact("2011-12-31 22:59", "yyyy-MM-dd HH:mm", null);
         _processor.Process(new InsertDataExpression()
         {
            TableName = "Foo",
            Rows = { new InsertionDataDefinition() { new KeyValuePair<string, object>("Test", testDate) } }
         });

         var table = _processor.ReadTableData(string.Empty, "Foo");

         table.Tables[0].Rows.Count.ShouldBe(1);
         table.Tables[0].Rows[0][0].ShouldBe(testDate);
      }

      [Test]
      public void CanInsertMultipleRowsSeparately()
      {
         CreateTestTable();

         _processor.Process(new InsertDataExpression()
                               {
                                  TableName = "Foo",
                                  Rows =
                                     {
                                        new InsertionDataDefinition() {new KeyValuePair<string, object>("Id", 1)}
                                        ,new InsertionDataDefinition() {new KeyValuePair<string, object>("Id", 2)}
                                     }
                                  ,InsertRowsSeparately = true
         });

         var table = _processor.ReadTableData(string.Empty, "Foo");

         Assert.AreEqual(2, table.Tables[0].Rows.Count);
      }

      [Test]
      public void CanInsertWithIdentity()
      {
         CreateTestTableWithIdentity();

         _processor.Process(new InsertDataExpression()
         {
            TableName = "Foo"
            , Rows = { new InsertionDataDefinition() { new KeyValuePair<string, object>("Id", 1) }}
            , WithIdentity = true
         });

         var table = _processor.ReadTableData(string.Empty, "Foo");

         Assert.AreEqual(1, table.Tables[0].Rows.Count);

         var nextVal = _processor.Read("SELECT FOOSEQ.nextval from dual");

         Assert.AreEqual(2, nextVal.Tables[0].Rows[0][0], "Next Val");
      }

      [Test]
      public void CanInsertBinaryData()
      {
         _processor.Process(new CreateTableExpression
         {
            TableName = "Foo",
            Columns = { new ColumnDefinition { Name = "Data", Type = DbType.Binary, Size = 2000} }
         });

         _processor.Process(new InsertDataExpression()
         {
            TableName = "Foo"
            ,Rows = { new InsertionDataDefinition() { new KeyValuePair<string, object>("Data", System.Text.Encoding.ASCII.GetBytes("HELLO WORLD")) } }
         });

         var table = _processor.ReadTableData(string.Empty, "Foo");

         Assert.AreEqual(1, table.Tables[0].Rows.Count);
      }

      [Test]
      public void CanDeleteTestFromTable()
      {
         CanInsertAndReadDataFromTestTable();

         var delete = new DeleteDataExpression {TableName = "Foo"};
         delete.Rows.Add(new DeletionDataDefinition {
            new KeyValuePair<string, object>("Id", 1)
            });

         _processor.Process(delete);

         var table = _processor.ReadTableData(string.Empty, "Foo");

         Assert.AreEqual(0, table.Tables[0].Rows.Count);
      }

      [Test]
      public void CanUpdateTable()
      {
         CanInsertAndReadDataFromTestTable();

         var expression = new UpdateDataExpression { TableName = "Foo"
                                                     , Set = new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>("Id", 2) }
                                                     , Where = new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>("Id", 1) }
         };

         _processor.Process(expression);

         var table = _processor.ReadTableData(string.Empty, "Foo");

         Assert.AreEqual(1, table.Tables[0].Rows.Count, "Count");
         Assert.AreEqual(2, table.Tables[0].Rows[0]["Id"], "Value");
      }

      [Test]
      public void CanRenameTable()
      {
         CreateTestTable();


         _processor.Process(new RenameTableExpression()
         { 
            OldName = "Foo",
            NewName = "Bar"
             
            
         });

         _processor.TableExists(string.Empty, "Bar").ShouldBeTrue();
      }

      [Test]
      public void CanDeleteTable()
      {
         CreateTestTable();


         _processor.TableExists(string.Empty, "Foo").ShouldBeTrue();

         _processor.Process(new DeleteTableExpression()
         {
            TableName = "Foo"
         });

         _processor.TableExists(string.Empty, "Bar").ShouldBeFalse();
      }


      [Test]
      public void CanRenameColumn()
      {
         CreateTestTable();


         _processor.Process(new RenameColumnExpression()
         {
            TableName = "Foo",
            OldName = "Id",
            NewName = "Bar"


         });

         _processor.ColumnExists(string.Empty, "Foo", "Bar").ShouldBeTrue();
      }

      [Test]
      public void CanCreateColumn()
      {
         CreateTestTable();


         _processor.Process(new CreateColumnExpression()
         {
            TableName = "Foo"
            , Column = new ColumnDefinition()
                          {
                             Name = "Bar",
                             Type = DbType.Int32
                          }
         });

         _processor.ColumnExists(string.Empty, "Foo", "Bar").ShouldBeTrue();
      }

      [Test]
      public void CanDeleteColumn()
      {
         _processor.Process(new CreateTableExpression
         {
            TableName = "Foo",
            Columns = { new ColumnDefinition
                           {
                              Name = "Id"
                              , Type = DbType.StringFixedLength
                              , Size= 10
                              , IsNullable = true
                           },
            new ColumnDefinition
                           {
                              Name = "Bar"
                              , Type = DbType.Int32
                           }}
         });


         _processor.Process(new DeleteColumnExpression()
         {
            TableName = "Foo"
            , ColumnName = "Bar"
         });

         _processor.ColumnExists(string.Empty, "Foo", "Bar").ShouldBeFalse();
      }

      [Test]
      public void CanAlterStringColumnLength()
      {
         _processor.Process(new CreateTableExpression
         {
            TableName = "Foo",
            Columns = { new ColumnDefinition
                           {
                              Name = "Id"
                              , Type = DbType.StringFixedLength
                              , Size= 10
                           } }
         });


         _processor.Process(new AlterColumnExpression()
         {
            TableName = "Foo"
            ,
            Column = new ColumnDefinition()
            {
               Name = "Id",
               Type = DbType.StringFixedLength,
               Size = 100
            }
         });

         _processor.ColumnExists(string.Empty, "Foo", "Id").ShouldBeTrue();
      }

      [Test]
      public void CanAlterStringNullColumnLength()
      {
         _processor.Process(new CreateTableExpression
         {
            TableName = "Foo",
            Columns = { new ColumnDefinition
                           {
                              Name = "Id"
                              , Type = DbType.StringFixedLength
                              , Size= 10
                              , IsNullable = true
                           } }
         });


         _processor.Process(new AlterColumnExpression()
         {
            TableName = "Foo"
            ,
            Column = new ColumnDefinition()
            {
               Name = "Id"
               , Type = DbType.StringFixedLength
               , Size = 100
               , IsNullable = true
            }
         });

         _processor.ColumnExists(string.Empty, "Foo", "Id").ShouldBeTrue();
      }

      [Test]
      public void CanAlterStringLengthWithSameDefaultColumn()
      {
         _processor.Process(new CreateTableExpression
         {
            TableName = "Foo",
            Columns = { new ColumnDefinition
                           {
                              Name = "Id"
                              , Type = DbType.StringFixedLength
                              , Size= 10
                              , IsNullable = true
                              , DefaultValue = 0
                           } }
         });


         _processor.Process(new AlterColumnExpression()
         {
            TableName = "Foo"
            ,
            Column = new ColumnDefinition()
            {
               Name = "Id"
               , Type = DbType.StringFixedLength
               , Size = 100
               , IsNullable = true
               , DefaultValue = 0
            }
         });

         _processor.ColumnExists(string.Empty, "Foo", "Id").ShouldBeTrue();
      }

      [Test]
      public void CanAlterColumnNullToNotNullable()
      {
         _processor.Process(new CreateTableExpression
         {
            TableName = "Foo",
            Columns = { new ColumnDefinition
                           {
                              Name = "Id"
                              , Type = DbType.StringFixedLength
                              , Size = 10
                              , IsNullable = true
                           } }
         });


         _processor.Process(new AlterColumnExpression()
         {
            TableName = "Foo"
            ,
            Column = new ColumnDefinition()
            {
               Name = "Id"
               , Type = DbType.StringFixedLength
               , Size = 10
               , IsNullable = false
            }
         });

         _processor.ColumnExists(string.Empty, "Foo", "Id").ShouldBeTrue();
      }

      [Test]
      public void CanCheckIndexExists()
      {
         CreateTestTableWithIndex();

         _processor.IndexExists(string.Empty, "Foo", "IDX_Id").ShouldBeTrue();
      }

      [Test]
      public void DropIndex()
      {
         CanCheckIndexExists();

         _processor.Process(new DeleteIndexExpression() { Index = new IndexDefinition() { TableName = "Foo", Name = "IDX_Id"}});

         _processor.IndexExists(string.Empty, "Foo", "IDX_Id").ShouldBeFalse();
      }

      [Test]
      public void CanCheckContraintExists()
      {
         CreateTestTableWithConstraint();

         _processor.ConstraintExists(string.Empty, "Foo", "CK_Test").ShouldBeTrue();
      }

      [Test]
      public void CreateForiegnKey()
      {
         _processor.Process(new CreateTableExpression
         {
            TableName = "Foo",
            Columns = { new ColumnDefinition { Name = "Id", Type = DbType.Int32, IsPrimaryKey = true} }
         });

         _processor.Process(new CreateTableExpression
         {
            TableName = "Bar",
            Columns =
               {
                  new ColumnDefinition { Name = "Id", Type = DbType.Int32, IsUnique = true }
                  , new ColumnDefinition { Name = "FooId", Type = DbType.Int32, IsUnique = true }
               }
         });



         _processor.Process(new CreateForeignKeyExpression()
                               {
                                  ForeignKey = new ForeignKeyDefinition()
                                                  {
                                                     PrimaryTable = "Foo"
                                                     , PrimaryColumns = new[] { "Id" }
                                                     , ForeignTable = "Bar"
                                                     , ForeignColumns = new[] { "FooId"}
                                                     , Name = "FK_Foo_Bar"
                                                  }
                                   
                               });
      }

      [Test]
      public void DropForiegnKey()
      {
         CreateForiegnKey();

         _processor.Process(new DeleteForeignKeyExpression() { ForeignKey = new ForeignKeyDefinition()
                                                                               {
                                                                                  ForeignTable = "Bar"
                                                                                  , Name = "FK_Foo_Bar"
                                                                               }
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

      private void CreateTestTableWithIdentity()
      {
         _processor.Process(new CreateTableExpression
         {
            TableName = "Foo",
            Columns = { new ColumnDefinition { Name = "Id", Type = DbType.Int32, IsIdentity = true} }
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
                  , Columns = new[] { new IndexColumnDefinition { Name = "Id", Direction = Direction.Ascending } }
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
