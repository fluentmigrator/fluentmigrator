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
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.SchemaDump.SchemaMigrations;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.SchemaMigration
{
   [TestFixture]
   public class SqlServerMigrationTests : BaseSchemaMigrationTests
   {
      private SqlServerUnitTest _source;

      private SqlServerUnitTest _destination;

      private string _tempDirectory;

      [TestFixtureSetUp]
      public void TestFixtureSetUp()
      {
          _source = new SqlServerUnitTest();
         _source.TestFixtureSetUp();

         _destination = new SqlServerUnitTest {DeleteOldDatabases = false};
         _destination.TestFixtureSetUp();
      }

      [TestFixtureTearDown]
      public void TestFixtureTearDown()
      {
      }

      [SetUp]
      public void SetUp()
      {
         _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
         Directory.CreateDirectory(_tempDirectory);

         _source.Setup();
         _destination.Setup();
      }


      [TearDown]
      public void TearDown()
      {
         Directory.Delete(_tempDirectory, true);
      }

      [Test]
      public void WillMigrateTablesDefault()
      {
        GetDefaultContext().MigrationRequired(MigrationType.Tables).ShouldBeTrue();
      }

      [Test]
      public void WillMigrateViewsDefault()
      {
         GetDefaultContext().MigrationRequired(MigrationType.Views).ShouldBeTrue();
      }

      [Test]
      public void WillMigrateIndexesDefault()
      {
         GetDefaultContext().MigrationRequired(MigrationType.Indexes).ShouldBeTrue();
      }

      [Test]
      public void WillMigrateDataDefault()
      {
         GetDefaultContext().MigrationRequired(MigrationType.Data).ShouldBeTrue();
      }

      [Test]
      public void WillMigrateFunctionsDefault()
      {
         GetDefaultContext().MigrationRequired(MigrationType.Functions).ShouldBeTrue();
      }

      [Test]
      public void WillMigrateProceduresDefault()
      {
         GetDefaultContext().MigrationRequired(MigrationType.Procedures).ShouldBeTrue();
      }

      [Test]
      public void WillMigrateForeignKeysDefault()
      {
         GetDefaultContext().MigrationRequired(MigrationType.ForeignKeys).ShouldBeTrue();
      }

      [Test]
      public void WillMigrateAllByDefault()
      {
         GetDefaultContext().MigrationRequired(MigrationType.All).ShouldBeTrue();
      }

      [Test]
      public void CanMigrateTable()
      {
         // Arrange
         var create = new CreateTableExpression
                         {
                            TableName = "Foo"
                            ,Columns = new List<ColumnDefinition> {new ColumnDefinition {Name = "Id", Type = DbType.Int32}}
                         };
         var context = GetDefaultContext();

         // Act
         MigrateTable(context, create);

         // Assert
         context.MigrationIndex.ShouldBe(1);
      }

      [Test]
      public void CanMigrateTableWithIdentityData()
      {
          // Arrange
          var create = new CreateTableExpression
          {
              TableName = "Foo"
             ,
              Columns = new List<ColumnDefinition> { new ColumnDefinition { Name = "Id", Type = DbType.Int32, IsIdentity = true} }
          };
          var context = GetDefaultContext();

          var insert = new InsertDataExpression()
          {
              TableName = "Foo"
          };
          insert.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("Id", 1)});
          insert.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("Id", 100) });
          insert.WithIdentity = true;

          // Act
          MigrateTable(context, create, insert);

          // Assert
          context.MigrationIndex.ShouldBe(1);
      }

      [Test]
      public void CanMigrateTableWithIdentityDataWithSchemaName()
      {
          // Arrange
          var create = new CreateTableExpression
          {
              TableName = "Foo"
             ,
              Columns = new List<ColumnDefinition> { new ColumnDefinition { Name = "Id", Type = DbType.Int32, IsIdentity = true } }
          };
          var context = GetDefaultContext();

          var insert = new InsertDataExpression()
          {
              SchemaName = "dbo",
              TableName = "Foo"
          };
          insert.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("Id", 1) });
          insert.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("Id", 100) });
          insert.WithIdentity = true;

          // Act
          MigrateTable(context, create, insert);

          // Assert
          context.MigrationIndex.ShouldBe(1);
      }

      [Test]
      public void CanMigrateView()
      {
         // Act
         var create = new CreateTableExpression
         {
            TableName = "Foo"
            ,Columns = new List<ColumnDefinition> { new ColumnDefinition { Name = "Id", Type = DbType.Int32 } }
         };

         ExecuteMigrations(create);
         CreateViews(new ViewDefinition { Name = "FooView", CreateViewSql = "CREATE VIEW FooView AS SELECT * FROM Foo"});
         var context = GetDefaultContext();


         // Act
         MigrateTable(context);

         // Assert
         context.MigrationIndex.ShouldBe(2);
      }

      [Test]
      public void WillNotMigrateAnyViews()
      {
         // Act
         var create = new CreateTableExpression
         {
            TableName = "Foo"
            ,
            Columns = new List<ColumnDefinition> { new ColumnDefinition { Name = "Id", Type = DbType.Int32 } }
         };

         ExecuteMigrations(create);
         CreateViews(new ViewDefinition { Name = "FooView", CreateViewSql = "CREATE VIEW FooView AS SELECT * FROM Foo" });
         var context = GetDefaultContext();
         context.Type = MigrationType.Tables;

         // Act
         MigrateTable(context);

         // Assert
         context.MigrationIndex.ShouldBe(1);
      }

      [Test]
      public void WillNotGenerateSpecificView()
      {
         // Act
         var create = new CreateTableExpression
         {
            TableName = "Foo"
            ,Columns = new List<ColumnDefinition> { new ColumnDefinition { Name = "Id", Type = DbType.Int32 } }
         };

         ExecuteMigrations(create);
         CreateViews(
            new ViewDefinition { Name = "FooView", CreateViewSql = "CREATE VIEW FooView AS SELECT * FROM Foo" }
            ,new ViewDefinition { Name = "FooView2", CreateViewSql = "CREATE VIEW FooView2 AS SELECT * FROM Foo" }
            );
         var context = GetDefaultContext();
         context.IncludeViews.Add("FooView");

         // Act
         MigrateTable(context);

         // Assert
         context.MigrationIndex.ShouldBe(2);
      }

      [Test]
      public void WillOnlyGenerateForeignKeyMigration()
      {
          // Act
          var create = new CreateTableExpression
          {
              TableName = "Foo"
             ,Columns = new List<ColumnDefinition> { new ColumnDefinition { Name = "Id", Type = DbType.Int32, IsPrimaryKey = true} }
          };

          var createBar = new CreateTableExpression
          {
              TableName = "FooBar"
             ,Columns = new List<ColumnDefinition> { new ColumnDefinition { Name = "Id", Type = DbType.Int32 } }
          };



          ExecuteMigrations(create, createBar, new CreateForeignKeyExpression { ForeignKey = new ForeignKeyDefinition { Name = "FK_Foo", ForeignTable = "FooBar", ForeignColumns = new [] { "Id"}, PrimaryTable = "Foo", PrimaryColumns = new [] { "Id"}}});


          var context = GetDefaultContext();
          context.Type = MigrationType.ForeignKeys;

          // Act
          GenerateMigrations(context);

          // Assert

          context.MigrationIndex.ShouldBe(1);
      }

      /// <summary>
      /// Obtains the default context that moved from SQL Server to Oracle
      /// </summary>
      /// <returns></returns>
      private SchemaMigrationContext GetDefaultContext()
      {
         return new SchemaMigrationContext
         {
            FromDatabaseType = DatabaseType.SqlServer
            , FromConnectionString = _source.ConnectionString
            ,ToDatabaseType = DatabaseType.SqlServer
            ,ToConnectionString = _destination.ConnectionString
            ,WorkingDirectory = _tempDirectory
         };
      }

      /// <summary>
      /// Creates tables in source oracle database and asserts that they exist
      /// </summary>
      /// <param name="expressions">The tables to be created</param>
      private void ExecuteMigrations(params IMigrationExpression[] expressions)
      {
         if (expressions == null)
            return;

         using (var connection = new SqlConnection(_source.ConnectionString))
         {
            var processor = new SqlServerProcessor(connection, new SqlServer2005Generator(), new DebugAnnouncer(), new ProcessorOptions());

            foreach (var expression in expressions)
            {
                if (expression is CreateTableExpression)
                {
                    var create = (CreateTableExpression)expression;
                    processor.Process(create);
                    Assert.IsTrue(processor.TableExists(string.Empty, create.TableName), "Source " + create.TableName);                    
                }

                if (expression is CreateForeignKeyExpression)
                {
                    processor.Process((CreateForeignKeyExpression)expression);
                }

                if (expression is InsertDataExpression)
                {
                    processor.Process((InsertDataExpression)expression);
                }
            }

            processor.CommitTransaction();
         }
      }

      /// <summary>
      /// Creates views in Oracle using the <see cref="ViewDefinition.CreateViewSql"/>
      /// </summary>
      /// <param name="view">The views to be created</param>
      private void CreateViews(params ViewDefinition[] view)
      {
         using (var connection = new SqlConnection(_source.ConnectionString))
         {
            var processor = new SqlServerProcessor(connection, new SqlServer2005Generator(), new DebugAnnouncer(), new ProcessorOptions());


            foreach (var viewDefinition in view)
            {
               processor.Execute(viewDefinition.CreateViewSql);
            }

            processor.CommitTransaction();
         }
      }

      /// <summary>
      /// Migrates a set of tables using the provided context
      /// </summary>
      /// <param name="context">The migration context that controls how items are migrated between SQL Server and SqlServer</param>
      /// <param name="createTables">The tables to be created in source SqlServer and migrated to SqlServer</param>
      private void MigrateTable(SchemaMigrationContext context, params IMigrationExpression[] createTables)
      {
         var migrator = GenerateMigrations(context, createTables);

          if (Directory.Exists(Path.Combine(_tempDirectory, "Scripts")))
         {
            // Replace Schema from source to destintion
            foreach (var filename in Directory.GetFiles(Path.Combine(_tempDirectory, "Scripts"), "CreateView*.sql"))
            {
               var text = File.ReadAllText(filename);
               File.Delete(filename);
               File.WriteAllText(filename, text.Replace(_source.TestDb.ToUpper(), _destination.TestDb.ToUpper()));
            }   
         }
         

         migrator.Migrate(context);
      }

      private SqlServerSchemaMigrator GenerateMigrations(SchemaMigrationContext context, params IMigrationExpression[] createTables)
       {
           ExecuteMigrations(createTables);

           var migrator = new SqlServerSchemaMigrator(new DebugAnnouncer());

           migrator.Generate(context);
           return migrator;
       }
   }
}
