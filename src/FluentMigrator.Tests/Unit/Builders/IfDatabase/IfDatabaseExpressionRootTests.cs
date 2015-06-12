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
using System.Data.OleDb;
using FluentMigrator.Builders.IfDatabase;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Processors.Jet;
using Moq;
using NUnit.Framework;
using NUnit.Should;
using System.Collections.Generic;
using System.Linq;

namespace FluentMigrator.Tests.Unit.Builders.IfDatabase
{
    [TestFixture]
    public class IfDatabaseExpressionRootTests
    {

        [Test]
        public void WillAddExpressionIfDatabaseTypeApplies()
        {
            var context = ExecuteTestMigration("Jet");

            context.Expressions.Count.ShouldBe(1);
        }

        [Test]
        public void WillNotAddExpressionIfDatabaseTypeApplies()
        {
            var context = ExecuteTestMigration("Unknown");

            context.Expressions.Count.ShouldBe(0);
        }

        [Test]
        public void WillNotAddExpressionIfProcessorNotMigrationProcessor()
        {
            var mock = new Mock<IQuerySchema>();
            var context = ExecuteTestMigration(new List<string>() { "Jet" }, mock.Object);

            context.Expressions.Count.ShouldBe(0);
        }

        [Test]
        public void WillAddExpressionIfOneDatabaseTypeApplies()
        {
            var context = ExecuteTestMigration("Jet", "Unknown");

            context.Expressions.Count.ShouldBe(1);
        }

        [Test]
        public void WillAddAlterExpression()
        {
            var context = ExecuteTestMigration(new List<string>() { "Jet" }, m => m.Alter.Table("Foo").AddColumn("Blah").AsString());

            context.Expressions.Count.ShouldBeGreaterThan(0);
        }

        [Test]
        public void WillAddCreateExpression()
        {
            var context = ExecuteTestMigration(new List<string>() { "Jet" }, m => m.Create.Table("Foo").WithColumn("Blah").AsString());

            context.Expressions.Count.ShouldBeGreaterThan(0);
        }

        [Test]
        public void WillAddDeleteExpression()
        {
            var context = ExecuteTestMigration(new List<string>() { "Jet" }, m => m.Delete.Table("Foo"));

            context.Expressions.Count.ShouldBeGreaterThan(0);
        }

        [Test]
        public void WillAddExecuteExpression()
        {
            var context = ExecuteTestMigration(new List<string>() { "Jet" }, m => m.Execute.Sql("DROP TABLE Foo"));

            context.Expressions.Count.ShouldBeGreaterThan(0);
        }

        [Test]
        public void WillAddInsertExpression()
        {
            var context = ExecuteTestMigration(new List<string>() { "Jet" }, m => m.Insert.IntoTable("Foo").Row(new { Id = 1 }));

            context.Expressions.Count.ShouldBeGreaterThan(0);
        }

        [Test]
        public void WillAddRenameExpression()
        {
            var context = ExecuteTestMigration(new List<string>() { "Jet" }, m => m.Rename.Table("Foo").To("Foo2"));

            context.Expressions.Count.ShouldBeGreaterThan(0);
        }

        [Test]
        public void WillAddSchemaExpression()
        {
            var databaseTypes = new List<string>() { "Unknown" };
            // Arrange
            var unknownProcessorMock = new Mock<IMigrationProcessor>(MockBehavior.Loose);

            unknownProcessorMock.SetupGet(x => x.DatabaseType).Returns(databaseTypes.First());

            var context = ExecuteTestMigration(databaseTypes, unknownProcessorMock.Object, m => m.Schema.Table("Foo").Exists());

            context.Expressions.Count.ShouldBe(0);

            unknownProcessorMock.Verify(x => x.TableExists(null, "Foo"));
        }

        [Test]
        public void WillAddUpdateExpression()
        {
            var context = ExecuteTestMigration(new List<string>() { "Jet" }, m => m.Update.Table("Foo").Set(new { Id = 1 }));

            context.Expressions.Count.ShouldBeGreaterThan(0);
        }

        private MigrationContext ExecuteTestMigration(params string[] databaseType)
        {
            return ExecuteTestMigration(databaseType, (IQuerySchema)null);
        }

        private MigrationContext ExecuteTestMigration(IEnumerable<string> databaseType, params Action<IIfDatabaseExpressionRoot>[] fluentEpression)
        {
            return ExecuteTestMigration(databaseType, null, fluentEpression);
        }

        private MigrationContext ExecuteTestMigration(IEnumerable<string> databaseType, IQuerySchema processor, params Action<IIfDatabaseExpressionRoot>[] fluentExpression)
        {
            // Arrange

            var context = new MigrationContext(new MigrationConventions(), processor ?? new JetProcessor(new OleDbConnection(), null, null, null), new SingleAssembly(GetType().Assembly), null, "");


            var expression = new IfDatabaseExpressionRoot(context, databaseType.ToArray());

            // Act
            if (fluentExpression == null || fluentExpression.Length == 0)
                expression.Create.Table("Foo").WithColumn("Id").AsInt16();
            else
            {
                foreach (var action in fluentExpression)
                {
                    action(expression);
                }

            }

            return context;
        }
    }

}
