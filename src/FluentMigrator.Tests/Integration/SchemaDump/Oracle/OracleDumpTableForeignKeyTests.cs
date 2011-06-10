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

using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.SchemaDump.SchemaDumpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.SchemaDump.Oracle
{
   [TestFixture]
   public class OracleDumpTableForeignKeyTests : OracleUnitTest
   {
      [Test]
      public void CanCreateForeignKey()
      {
         // Arrange
         var forgeignKey = new ForeignKeyDefinition
                              {
                                 Name = "FK_Foo"
                                 ,PrimaryTable = "Foo"
                                 ,ForeignTable = "Foo2"
                              };
         forgeignKey.PrimaryColumns.Add("Id");
         forgeignKey.ForeignColumns.Add("FooId");

         // Act
         var tables = CreateTable(
            new[]
               {
                  CreateExpression("Foo", new ColumnDefinition {Name = "Id", Type = DbType.Int32, IsPrimaryKey = true})
                  , CreateExpression("Foo2",
                                     new ColumnDefinition {Name = "Id", Type = DbType.Int32}
                                     , new ColumnDefinition {Name = "FooId", Type = DbType.Int32})
               }
            , forgeignKey
            );


         // Assert
         tables.Count.ShouldBe(2);
         tables.Where(t => t.Name == "FOO2").First().ForeignKeys.Count.ShouldBe(1);
      }

      [Test]
      public void CanCreateMultipleForeignKey()
      {
         // Arrange
         var forgeignKey = new ForeignKeyDefinition
         {
            Name = "FK_Foo"
            ,PrimaryTable = "Foo"
            ,ForeignTable = "Foo2"
         };
         forgeignKey.PrimaryColumns.Add("Id");
         forgeignKey.ForeignColumns.Add("FooId");


         var forgeignKeyBar = new ForeignKeyDefinition
         {
            Name = "FK_Bar"
            ,
            PrimaryTable = "Bar"
            ,ForeignTable = "Foo2"
         };

         forgeignKeyBar.PrimaryColumns.Add("Id");
         forgeignKeyBar.ForeignColumns.Add("BarId");

         // Act
         var tables = CreateTable(
            new[]
               {
                  CreateExpression("Foo", new ColumnDefinition {Name = "Id", Type = DbType.Int32, IsPrimaryKey = true})
                  , CreateExpression("Bar", new ColumnDefinition {Name = "Id", Type = DbType.Int32, IsPrimaryKey = true})
                  , CreateExpression("Foo2",
                                     new ColumnDefinition {Name = "Id", Type = DbType.Int32}
                                     , new ColumnDefinition {Name = "FooId", Type = DbType.Int32}
                                     , new ColumnDefinition {Name = "BarId", Type = DbType.Int32})
               }
            , forgeignKey
            , forgeignKeyBar
            );


         // Assert
         tables.Count.ShouldBe(3);
         tables.Where(t => t.Name == "FOO2").First().ForeignKeys.Count.ShouldBe(2);
      }


      private static CreateTableExpression CreateExpression(string name, params ColumnDefinition[] columns)
      {
         var create = new CreateTableExpression {TableName = name};
         foreach (ColumnDefinition columnDefinition in columns)
         {
            columnDefinition.TableName = create.TableName;
            create.Columns.Add(columnDefinition);
         }

         return create;
      }

      private IList<TableDefinition> CreateTable(IEnumerable<CreateTableExpression> expressions,
                                                 params ForeignKeyDefinition[] keys)
      {
         // Act
         var processor = ((OracleProcessor) new OracleProcessorFactory().Create(ConnectionString,
                                                                                new DebugAnnouncer(),
                                                                                new ProcessorOptions()));

         foreach (CreateTableExpression createTableExpression in expressions)
         {
            processor.Process(createTableExpression);

            Assert.IsTrue(processor.TableExists(string.Empty, createTableExpression.TableName), "Oracle");
         }

         foreach (ForeignKeyDefinition key in keys)
         {
            processor.Process(new CreateForeignKeyExpression {ForeignKey = key});
         }

         var dumper = new OracleSchemaDumper(processor, new DebugAnnouncer());
         IList<TableDefinition> tables = dumper.ReadDbSchema();

         processor.CommitTransaction();

         tables.Count.ShouldBe(expressions.Count());

         return tables;
      }
   }
}
