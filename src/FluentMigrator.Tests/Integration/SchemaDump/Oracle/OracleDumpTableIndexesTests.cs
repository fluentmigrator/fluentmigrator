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
   public class OracleDumpTableIndexTests : OracleUnitTest
   {

      [Test]
      public void CanCreateAcendingIndex()
      {
         // Arrange
         var columns = new List<ColumnDefinition> {new ColumnDefinition {Name = "Data", Type = DbType.String}};

         var index = new CreateIndexExpression { Index = new IndexDefinition { Name = "IDX_Foo", TableName = "Foo" } };
         index.Index.Columns.Add(new IndexColumnDefinition { Name = "Data", Direction = Direction.Ascending});

         // Act
         var table = CreateTable(columns, index);


         // Assert
         table.Indexes.Count.ShouldBe(1); 
         table.Indexes.First().Name.ShouldBe("IDX_FOO");
         table.Indexes.First().Columns.Count.ShouldBe(1);
         table.Indexes.First().Columns.First().Name.ShouldBe("DATA");
         table.Indexes.First().Columns.First().Direction.ShouldBe(Direction.Ascending);
      }

      [Test]
      public void CanCreateDecendingIndex()
      {
         // Arrange
         var columns = new List<ColumnDefinition> { new ColumnDefinition { Name = "Data", Type = DbType.String } };

         var index = new CreateIndexExpression { Index = new IndexDefinition { Name = "IDX_Foo", TableName = "Foo" } };
         index.Index.Columns.Add(new IndexColumnDefinition { Name = "Data", Direction = Direction.Descending });

         // Act
         var table = CreateTable(columns, index);


         // Assert
         table.Indexes.Count.ShouldBe(1);
         table.Indexes.First().Name.ShouldBe("IDX_FOO");
         table.Indexes.First().Columns.Count.ShouldBe(1);
         table.Indexes.First().Columns.First().Name.ShouldBe("DATA");
         table.Indexes.First().Columns.First().Direction.ShouldBe(Direction.Descending);
      }

      [Test]
      public void CanCreateUniqueIndex()
      {
         // Arrange
         var columns = new List<ColumnDefinition> { new ColumnDefinition { Name = "Data", Type = DbType.String } };

         var index = new CreateIndexExpression { Index = new IndexDefinition { Name = "IDX_Foo", TableName = "Foo", IsUnique = true} };
         index.Index.Columns.Add(new IndexColumnDefinition { Name = "Data" });

         // Act
         var table = CreateTable(columns, index);


         // Assert
         table.Indexes.First().IsUnique.ShouldBeTrue();
      }

      [Test]
      public void CanCreateCompositeIndex()
      {
         // Arrange
         var columns = new List<ColumnDefinition>
                          {
                             new ColumnDefinition { Name = "Data", Type = DbType.String }
                             , new ColumnDefinition { Name = "Data2", Type = DbType.String }
                          };

         var index = new CreateIndexExpression { Index = new IndexDefinition { Name = "IDX_Foo", TableName = "Foo", IsUnique = true } };
         index.Index.Columns.Add(new IndexColumnDefinition { Name = "Data" });
         index.Index.Columns.Add(new IndexColumnDefinition { Name = "Data2" });

         // Act
         var table = CreateTable(columns, index);


         // Assert
         table.Indexes.First().Columns.Count.ShouldBe(2);
      }


      private TableDefinition CreateTable(IEnumerable<ColumnDefinition> columns, CreateIndexExpression index)
      {
         
         // Act
         var processor = ((OracleProcessor)new OracleProcessorFactory().Create(ConnectionString,
         new DebugAnnouncer(), new ProcessorOptions()));

         var create = new CreateTableExpression {TableName = "Foo"};
         foreach (var columnDefinition in columns)
         {
            columnDefinition.TableName = create.TableName;
            create.Columns.Add(columnDefinition);
         }

         processor.Process(create);
         processor.Process(index);

         Assert.IsTrue(processor.TableExists(string.Empty, "Foo"), "Oracle");

         var dumper = new OracleSchemaDumper(processor, new DebugAnnouncer());
         var tables = dumper.ReadDbSchema();

         processor.CommitTransaction();

         tables.Count.ShouldBe(1);

         return tables[0];
      }
   }
}
