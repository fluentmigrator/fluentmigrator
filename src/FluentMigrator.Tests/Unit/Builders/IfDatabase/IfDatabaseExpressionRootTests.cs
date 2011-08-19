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
using System.Data;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Builders.IfDatabase;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Processors.Jet;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Builders.IfDatabase
{
    [TestFixture]
    public class IfDatabaseExpressionRootTests
    {
        [Test]
        public void WillAddExpressionIfDatabaseTypeApplies()
        {
            var context = ExecuteTestMigration(DatabaseType.Jet);

            context.Expressions.Count.ShouldBe(1);
        }

        [Test]
        public void WillNotAddExpressionIfDatabaseTypeApplies()
        {
            var context = ExecuteTestMigration(DatabaseType.Unknown);

            context.Expressions.Count.ShouldBe(0);
        }

        [Test]
        public void WillNotAddExpressionIfProcessorNotMigrationProcessor()
        {
            var mock = new Mock<IQuerySchema>();
            var context = ExecuteTestMigration(DatabaseType.Jet, mock.Object);

            context.Expressions.Count.ShouldBe(0);
        }

        [Test]
        public void WillAddExpressionIfOneDatabaseTypeApplies()
        {
            var context = ExecuteTestMigration(DatabaseType.Jet | DatabaseType.Unknown);

            context.Expressions.Count.ShouldBe(1);
        }

        [Test]
        public void WillAddAlterExpression()
        {
            var context = ExecuteTestMigration(DatabaseType.Jet, m => m.Alter.Table("Foo").AddColumn("Blah").AsString());

            context.Expressions.Count.ShouldBeGreaterThan(0);
        }

        [Test]
        public void WillAddCreateExpression()
        {
            var context = ExecuteTestMigration(DatabaseType.Jet, m => m.Create.Table("Foo").WithColumn("Blah").AsString());

            context.Expressions.Count.ShouldBeGreaterThan(0);
        }

        [Test]
        public void WillAddDeleteExpression()
        {
            var context = ExecuteTestMigration(DatabaseType.Jet, m => m.Delete.Table("Foo"));

            context.Expressions.Count.ShouldBeGreaterThan(0);
        }

        [Test]
        public void WillAddExecuteExpression()
        {
            var context = ExecuteTestMigration(DatabaseType.Jet, m => m.Execute.Sql("DROP TABLE Foo"));

            context.Expressions.Count.ShouldBeGreaterThan(0);
        }

        [Test]
        public void WillAddInsertExpression()
        {
            var context = ExecuteTestMigration(DatabaseType.Jet, m => m.Insert.IntoTable("Foo").Row(new { Id =  1}));

            context.Expressions.Count.ShouldBeGreaterThan(0);
        }

        [Test]
        public void WillAddRenameExpression()
        {
            var context = ExecuteTestMigration(DatabaseType.Jet, m => m.Rename.Table("Foo").To("Foo2"));

            context.Expressions.Count.ShouldBeGreaterThan(0);
        }

        [Test]
        public void WillAddSchemaExpression()
        {
            // Arrange
            var processor = new UnknownProcessor();

            var context = ExecuteTestMigration(DatabaseType.Unknown, processor,  m => m.Schema.Table("Foo").Exists());

            context.Expressions.Count.ShouldBe(0);
            processor.CalledTableExists.ShouldBe(true);
        }

        [Test]
        public void WillAddUpdateExpression()
        {
            var context = ExecuteTestMigration(DatabaseType.Jet, m => m.Update.Table("Foo").Set(new { Id = 1}));

            context.Expressions.Count.ShouldBeGreaterThan(0);
        }

        private MigrationContext ExecuteTestMigration(DatabaseType databaseType, params Action<IIfDatabaseExpressionRoot>[] fluentEpression)
        {
            return ExecuteTestMigration(databaseType, null, fluentEpression);
        }

        private MigrationContext ExecuteTestMigration(DatabaseType databaseType, IQuerySchema processor, params Action<IIfDatabaseExpressionRoot>[] fluentEpression)
        {
            // Arrange
            
            var context = new MigrationContext(new MigrationConventions(), processor ?? new JetProcessor(null, null, null, null),
                                               GetType().Assembly);


            var expression = new IfDatabaseExpressionRoot(context, databaseType);

            // Act
            if (fluentEpression == null || fluentEpression.Length == 0)
                expression.Create.Table("Foo").WithColumn("Id").AsInt16();
            else
            {
                foreach (var action in fluentEpression)
                {
                    action(expression);
                }

            }

            return context;
        }
    }

    /// <summary>
    /// Specail test Fake implementation of Unknown processor used by unit test
    /// </summary>
    public class UnknownProcessor : IMigrationProcessor
    {
        public bool SchemaExists(string schemaName)
        {
            throw new NotImplementedException();
        }

        public bool TableExists(string schemaName, string tableName)
        {
            CalledTableExists = true;
            return true;
        }

        public bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            throw new NotImplementedException();
        }

        public bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            throw new NotImplementedException();
        }

        public bool IndexExists(string schemaName, string tableName, string indexName)
        {
            throw new NotImplementedException();
        }

        public IMigrationProcessorOptions Options
        {
            get { throw new NotImplementedException(); }
        }

        public bool CalledTableExists { get; set; }

        public void Execute(string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public DataSet ReadTableData(string schemaName, string tableName)
        {
            throw new NotImplementedException();
        }

        public DataSet Read(string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string template, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public void CommitTransaction()
        {
            throw new NotImplementedException();
        }

        public void RollbackTransaction()
        {
            throw new NotImplementedException();
        }

        public void Process(CreateSchemaExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Process(DeleteSchemaExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Process(AlterTableExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Process(AlterColumnExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Process(CreateTableExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Process(CreateColumnExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Process(DeleteTableExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Process(DeleteColumnExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Process(CreateForeignKeyExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Process(DeleteForeignKeyExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Process(CreateIndexExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Process(DeleteIndexExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Process(RenameTableExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Process(RenameColumnExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Process(InsertDataExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Process(AlterDefaultConstraintExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Process(PerformDBOperationExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Process(DeleteDataExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Process(UpdateDataExpression expression)
        {
            throw new NotImplementedException();
        }

        public void Process(AlterSchemaExpression expression)
        {
            throw new NotImplementedException();
        }
    }
}
