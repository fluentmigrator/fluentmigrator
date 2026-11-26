#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Tests.Helpers;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    [Category("Expression")]
    [Category("CreateTable")]
    public class CreateTableExpressionTests
    {
        [Test]
        public void ToStringIsDescriptive()
        {
            new CreateTableExpression
                {
                    TableName = "Table"
                }.ToString().ShouldBe("CreateTable Table");
        }

        [Test]
        public void ErrorIsReturnedWhenTableNameIsEmptyString()
        {
            var expression = new CreateTableExpression { TableName = string.Empty };

            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsNotReturnedWhenTableNameIsSet()
        {
            var expression = new CreateTableExpression { TableName = "table1" };

            var errors = ValidationHelper.CollectErrors(expression);
            Assert.That(errors, Is.Empty);
        }

        // With IfNotExists(), the table's indexes and foreign keys are in ExpressionsForNewTable, not in the
        // migration's own list of expressions, so the table must validate them.

        [Test]
        public void ErrorIsReturnedWhenAnIndexForTheNewTableIsInvalid()
        {
            var index = new CreateIndexExpression { Index = { Name = null, TableName = "table1" } };
            index.Index.Columns.Add(new IndexColumnDefinition { Name = "Column1" });
            var expression = new CreateTableExpression { TableName = "table1", ExpressionsForNewTable = { index } };

            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.IndexNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenAForeignKeyForTheNewTableIsInvalid()
        {
            var foreignKey = new CreateForeignKeyExpression
            {
                ForeignKey =
                {
                    Name = "FK_table1_table2",
                    ForeignTable = "table1",
                    PrimaryTable = null,
                    ForeignColumns = { "Column1" },
                    PrimaryColumns = { "Id" },
                },
            };
            var expression = new CreateTableExpression { TableName = "table1", ExpressionsForNewTable = { foreignKey } };

            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.PrimaryTableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsNotReturnedWhenTheIndexesAndForeignKeysForTheNewTableAreValid()
        {
            var index = new CreateIndexExpression { Index = { Name = "IX_table1_Column1", TableName = "table1" } };
            index.Index.Columns.Add(new IndexColumnDefinition { Name = "Column1" });
            var foreignKey = new CreateForeignKeyExpression
            {
                ForeignKey =
                {
                    Name = "FK_table1_table2",
                    ForeignTable = "table1",
                    PrimaryTable = "table2",
                    ForeignColumns = { "Column1" },
                    PrimaryColumns = { "Id" },
                },
            };
            var expression = new CreateTableExpression { TableName = "table1", ExpressionsForNewTable = { index, foreignKey } };

            var errors = ValidationHelper.CollectErrors(expression);
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void ReverseThrowsWhenCreateIsConditional()
        {
            var expression = new CreateTableExpression
            {
                TableName = "table1",
                IfNotExists = true
            };

            Should.Throw<System.NotSupportedException>(() => expression.Reverse());
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsNotSetThenSchemaShouldBeNull()
        {
            var expression = new CreateTableExpression { TableName = "table1" };

            var processed = expression.Apply(ConventionSets.NoSchemaName);

            Assert.That(processed.SchemaName, Is.Null);
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsSetThenSchemaShouldNotBeChanged()
        {
            var expression = new CreateTableExpression { SchemaName = "testschema", TableName = "table1" };

            var processed = expression.Apply(ConventionSets.WithSchemaName);

            Assert.That(processed.SchemaName, Is.EqualTo("testschema"));
        }

        [Test]
        public void WhenDefaultSchemaConventionIsChangedAndSchemaIsNotSetThenSetSchema()
        {
            var expression = new CreateTableExpression { TableName = "table1" };

            var processed = expression.Apply(ConventionSets.WithSchemaName);

            Assert.That(processed.SchemaName, Is.EqualTo("testdefault"));
        }
    }
}
