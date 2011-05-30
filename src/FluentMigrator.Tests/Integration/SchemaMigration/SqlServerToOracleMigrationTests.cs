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
         MigrateTable(create);
         
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
         MigrateTable(create);

      }

      private void MigrateTable(CreateTableExpression create)
      {
         using (var connection = new SqlConnection(sqlContext.ConnectionString))
         {
            var processor = new SqlServerProcessor(connection, new SqlServer2005Generator(), new NullAnnouncer(), new ProcessorOptions());

            processor.Process(create);

            Assert.IsTrue(processor.TableExists(string.Empty, create.TableName), "SqlServer");

            processor.CommitTransaction();
         }

         var settings = new SchemaMigrationSettings()
         {
            FromConnectionString = sqlContext.ConnectionString
            , ToDatabaseType = DatabaseType.Oracle
            , ToConnectionString = oracleContext.ConnectionString
            , MigrationsDirectory = _tempDirectory
         };

         var migrator = new SqlServerSchemaMigrator();

         migrator.Migrate(settings);

         var oracleProcessor = new OracleProcessorFactory().Create(oracleContext.ConnectionString, new NullAnnouncer(),
                                             new ProcessorOptions());

         // Assert
         Assert.IsTrue(oracleProcessor.TableExists(string.Empty, create.TableName), "Oracle");
      }
   }
}
