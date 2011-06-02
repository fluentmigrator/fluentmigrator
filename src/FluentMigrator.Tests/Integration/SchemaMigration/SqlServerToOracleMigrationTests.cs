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
using System.Data.SqlClient;
using System.IO;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.SchemaDump.SchemaMigrations;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.SchemaMigration
{
   [TestFixture]
   public class SqlServerToOracleMigrationTests
   {
      private SqlServerUnitTest sqlContext;
      private OracleUnitTest oracleContext;
      private string _tempDirectory;

      [TestFixtureSetUp]
      public void TestFixtureSetUp()
      {
         sqlContext = new SqlServerUnitTest();
         oracleContext = new OracleUnitTest();
         sqlContext.TestFixtureSetUp();
         oracleContext.TestFixtureSetUp();
      }

      [TestFixtureTearDown]
      public void TestFixtureTearDown()
      {
         sqlContext.TestFixtureTearDown();
      }

      [SetUp]
      public void SetUp()
      {
         _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
         Directory.CreateDirectory(_tempDirectory);
         sqlContext.Setup();
         oracleContext.Setup();
      }


      [TearDown]
      public void TearDown()
      {
         sqlContext.TearDown();
         Directory.Delete(_tempDirectory, true);
      }

      [Test]
      public void CanMigrateIdentityColumn()
      {
         // Arrange
         var create = new CreateTableExpression
                         {
                            TableName = "Foo",
                            Columns = new[]
                                         {
                                            new ColumnDefinition
                                               {
                                                  Name = "Id", Type = DbType.Int32
                                                  , IsIdentity = true
                                               }
                                         }
                         };
         


         // Act
         MigrateTable(GetDefaultContext(), create);
         
      }

      [Test]
      public void CanMigrateGuidAndDateWithDefaultValues()
      {
         // Arrange
         var create = new CreateTableExpression
         {
            TableName = "Foo",
            Columns = new[]
                                         {
                                            new ColumnDefinition
                                               {
                                                  Name = "Id", Type = DbType.Guid
                                                  , DefaultValue = SystemMethods.NewGuid
                                               },
                                               new ColumnDefinition
                                               {
                                                  Name = "Added", Type = DbType.DateTime
                                                  , DefaultValue = SystemMethods.CurrentDateTime
                                               }
                                         }
         };



         // Act
         MigrateTable(GetDefaultContext(), create);

      }

      [Test]
      public void CanMigrateDefaultDate()
      {
         // Arrange
         var create = new CreateTableExpression
         {
            TableName = "Foo",
            Columns = new[]
                                         {
                                               new ColumnDefinition
                                               {
                                                  Name = "Added", Type = DbType.DateTime
                                                  , DefaultValue = "1900-01-01"
                                               }
                                         }
         };

         // Act
         MigrateTable(GetDefaultContext(), create);

      }

      
      [Test]
      public void CanMigrateDecimal()
      {
         // Arrange
         var create = new CreateTableExpression
         {
            TableName = "Foo",
            Columns = new[]
                                         {
                                               new ColumnDefinition
                                               {
                                                  Name = "Value", Type = DbType.Decimal
                                                  , Size = 38
                                                  , Precision = 4
                                               }
                                         }
         };

         // Act
         MigrateTable(GetDefaultContext(), create);

      }

      [Test]
      public void CanMigrateSupportedTypes()
      {
         // Arrange

         // Add in broad coverage of supported data types

         var create = new CreateTableExpression
         {
            TableName = "SupportedTypes",
            Columns = new[]{
                   new ColumnDefinition {Name = "Col1", Type = DbType.Int32 }
                   , new ColumnDefinition {Name = "Col2", Type = DbType.String, Size = 20}
                   , new ColumnDefinition {Name = "Col3", Type = DbType.String}
                   , new ColumnDefinition {Name = "Col4", Type = DbType.Guid }
                   , new ColumnDefinition {Name = "Col5", Type = DbType.Byte}
                   , new ColumnDefinition {Name = "Col6", Type = DbType.DateTime}
                   , new ColumnDefinition {Name = "Col7", Type = DbType.Int64}
                   , new ColumnDefinition {Name = "Col8", Type = DbType.Int16}
                   , new ColumnDefinition {Name = "Col9", Type = DbType.Decimal, Size = 20, Precision = 4}
                   , new ColumnDefinition {Name = "Col10", Type = DbType.Double}
                   , new ColumnDefinition {Name = "Col11", Type = DbType.Boolean}
                    }
         };



         // Act
         MigrateTable(GetDefaultContext(), create);

      }

      [Test]
      [ExpectedException]
      public void ThrowsExceptionIfInvalidCodeGenerated()
      {
         // Arrange

         // Add in broad coverage of supported data types

         var create = new CreateTableExpression
         {
            TableName = "SupportedTypes",
            Columns = new[]{
                   new ColumnDefinition {Name = "Col1", Type = DbType.Int32 }
                    }
         };

         var settings = GetDefaultContext();
         // Modify the context so that invalid class name returned
         // ... in this case make is start with numeric
         settings.MigrationClassNamer = (index, table) => "1";


         // Act
         MigrateTable(settings, create);

      }

      [Test]
      public void CanMigrateTableAndView()
      {
         // Arrange

         var create = new CreateTableExpression
         {
            TableName = "Foo",
            Columns = new[]{
                   new ColumnDefinition {Name = "Id", Type = DbType.Int32 }
                    }
         };

         var context = GetDefaultContext();
         context.GenerateAlternateMigrationsFor.Add(DatabaseType.Oracle);

         // Act
         CreateTables(create);
         CreateViews(new ViewDefinition { Name = "FooView", CreateViewSql = "CREATE VIEW FooView AS SELECT * FROM Foo" });
         MigrateTable(context);

         // Assert
         AssertOracleTablesExist(create);

         context.MigrationIndex.ShouldBe(2);
      }

      [Test]
      public void CanExcludeTable()
      {
         // Arrange

         var create = new CreateTableExpression
         {
            TableName = "Foo",
            Columns = new[]{
                   new ColumnDefinition {Name = "Id", Type = DbType.Int32 }
                    }
         };

         var context = GetDefaultContext();
         context.GenerateAlternateMigrationsFor.Add(DatabaseType.Oracle);
         context.ExcludeTables.Add("Foo");

         // Act
         CreateTables(create);
         MigrateTable(context);

         // Assert
         context.MigrationIndex.ShouldBe(0);
      }

      [Test]
      public void CanMigrateTableAndExcludeView()
      {
         // Arrange

         var create = new CreateTableExpression
         {
            TableName = "Foo",
            Columns = new[]{
                   new ColumnDefinition {Name = "Id", Type = DbType.Int32 }
                    }
         };

         var context = GetDefaultContext();
         context.GenerateAlternateMigrationsFor.Add(DatabaseType.Oracle);
         context.ExcludeViews.Add("FooView");

         // Act
         CreateTables(create);
         CreateViews(new ViewDefinition { Name = "FooView", CreateViewSql = "CREATE VIEW FooView AS SELECT * FROM Foo" });
         MigrateTable(context);

         // Assert
         AssertOracleTablesExist(create);

         context.MigrationIndex.ShouldBe(1);
      }

      [Test]
      public void CanMigrateTableData()
      {
         // Arrange

         var create = new CreateTableExpression
         {
            TableName = "Foo",
            Columns = new[]{
                   new ColumnDefinition {Name = "Id", Type = DbType.Int32 }
                    }
         };

         var row = new InsertDataExpression()
                      {
                         TableName = "Foo"
                      };
         row.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("Id", 1)});

         var context = GetDefaultContext();
         context.GenerateAlternateMigrationsFor.Add(DatabaseType.Oracle);
         context.ExcludeViews.Add("FooView");
         context.MigrateData = true;

         // Act
         CreateTables(create);
         
         InsertData(row);
         CreateViews(new ViewDefinition { Name = "FooView", CreateViewSql = "CREATE VIEW FooView AS SELECT * FROM Foo" });
         MigrateTable(context);
         var data = GetOracleTableData("Foo");

         // Assert
         AssertOracleTablesExist(create);

         context.MigrationIndex.ShouldBe(1);
         data.Tables[0].Rows.Count.ShouldBe(1);
      }

      [Test]
      public void CanMigrateTableWithDate()
      {
         // Arrange

         var create = new CreateTableExpression
         {
            TableName = "Foo",
            Columns = new[]{
                   new ColumnDefinition {Name = "Test", Type = DbType.DateTime }
                    }
         };

         var row = new InsertDataExpression()
         {
            TableName = "Foo"
         };
         row.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("Test", DateTime.Now) });

         var context = GetDefaultContext();
         context.GenerateAlternateMigrationsFor.Add(DatabaseType.Oracle);
         context.MigrateData = true;

         // Act
         CreateTables(create);

         InsertData(row);
         MigrateTable(context);
         var data = GetOracleTableData("Foo");

         // Assert
         AssertOracleTablesExist(create);

         context.MigrationIndex.ShouldBe(1);
         data.Tables[0].Rows.Count.ShouldBe(1);
      }

      [Test]
      public void CanMigrateTableDataWithIdentity()
      {
         // Arrange

         var create = new CreateTableExpression
         {
            TableName = "Foo",
            Columns = new[]{
                   new ColumnDefinition {Name = "Id", Type = DbType.Int32, IsIdentity = true}
                    }
         };

         var row = new InsertDataExpression()
         {
            TableName = "Foo"
         };
         row.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("Id", 1) });
         row.WithIdentity = true;

         var context = GetDefaultContext();
         context.GenerateAlternateMigrationsFor.Add(DatabaseType.Oracle);
         context.ExcludeViews.Add("FooView");
         context.MigrateData = true;

         // Act
         CreateTables(create);

         InsertData(row);
         CreateViews(new ViewDefinition { Name = "FooView", CreateViewSql = "CREATE VIEW FooView AS SELECT * FROM Foo" });
         MigrateTable(context);
         var data = GetOracleTableData("Foo");

         // Assert
         AssertOracleTablesExist(create);

         context.MigrationIndex.ShouldBe(1);
         data.Tables[0].Rows.Count.ShouldBe(1);
      }

      private void InsertData(InsertDataExpression row)
      {
         using (var connection = new SqlConnection(sqlContext.ConnectionString))
         {
            var processor = new SqlServerProcessor(connection, new SqlServer2005Generator(), new DebugAnnouncer(), new ProcessorOptions());

            processor.Process(row);

            processor.CommitTransaction();
         }
      }

      private void MigrateTable(SchemaMigrationContext context, params CreateTableExpression[] createTables)
      {
         CreateTables(createTables);

         var migrator = new SqlServerSchemaMigrator(new DebugAnnouncer());

         migrator.Migrate(context);

         AssertOracleTablesExist(createTables);
      }

      private void AssertOracleTablesExist(params CreateTableExpression[] createTables)
      {
         if (createTables == null)
            return;
         var oracleProcessor = new OracleProcessorFactory().Create(oracleContext.ConnectionString, new NullAnnouncer(),
                                                                   new ProcessorOptions());

         foreach ( var create in createTables)
            Assert.IsTrue(oracleProcessor.TableExists(string.Empty, create.TableName), "Oracle");   
      }

      private DataSet GetOracleTableData(string tableName)
      {
         var oracleProcessor = new OracleProcessorFactory().Create(oracleContext.ConnectionString, new NullAnnouncer(),
                                                                   new ProcessorOptions());

         return oracleProcessor.ReadTableData(string.Empty, tableName);
      }

      private void CreateTables(params CreateTableExpression[] createTables)
      {
         if (createTables == null)
            return;

         using (var connection = new SqlConnection(sqlContext.ConnectionString))
         {
            var processor = new SqlServerProcessor(connection, new SqlServer2005Generator(), new DebugAnnouncer(), new ProcessorOptions());

            foreach (var create in createTables)
            {
               processor.Process(create);
               Assert.IsTrue(processor.TableExists(string.Empty, create.TableName), "SqlServer " + create.TableName);
            }

            processor.CommitTransaction();
         }
      }

      private void CreateViews(params ViewDefinition[] view)
      {
         using (var connection = new SqlConnection(sqlContext.ConnectionString))
         {
            var processor = new SqlServerProcessor(connection, new SqlServer2005Generator(), new DebugAnnouncer(), new ProcessorOptions());

            foreach (var viewDefinition in view)
            {
               processor.Execute(viewDefinition.CreateViewSql);
            }

            processor.CommitTransaction();
         }
      }

      private SchemaMigrationContext GetDefaultContext()
      {
         return new SchemaMigrationContext
                   {
                      FromConnectionString = sqlContext.ConnectionString
                      , ToDatabaseType = DatabaseType.Oracle
                      , ToConnectionString = oracleContext.ConnectionString
                      , WorkingDirectory = _tempDirectory
                   };
      }
   }
}
