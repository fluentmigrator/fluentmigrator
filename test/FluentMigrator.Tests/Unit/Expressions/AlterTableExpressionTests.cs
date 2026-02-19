#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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
using FluentMigrator.Runner;
using FluentMigrator.Tests.Helpers;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    [Category("Expression")]
    [Category("AlterTable")]
    public class AlterTableExpressionTests
    {
        [Test]
        public void ErrorIsReturnedWhenTableNameIsEmptyString()
        {
            var expression = new AlterTableExpression { TableName = string.Empty };

            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsNotReturnedWhenTableNameIsSet()
        {
            var expression = new AlterTableExpression { TableName = "table1" };

            var errors = ValidationHelper.CollectErrors(expression);
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsNotSetThenSchemaShouldBeNull()
        {
            var expression = new AlterTableExpression { TableName = "table1" };

            var processed = expression.Apply(ConventionSets.NoSchemaName);

            Assert.That(processed.SchemaName, Is.Null);
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsSetThenSchemaShouldNotBeChanged()
        {
            var expression = new AlterTableExpression { SchemaName = "testschema", TableName = "table1" };

            var processed = expression.Apply(ConventionSets.WithSchemaName);

            Assert.That(processed.SchemaName, Is.EqualTo("testschema"));
        }

        [Test]
        public void WhenDefaultSchemaConventionIsChangedAndSchemaIsNotSetThenSetSchema()
        {
            var expression = new AlterTableExpression { TableName = "table1" };

            var processed = expression.Apply(ConventionSets.WithSchemaName);

            Assert.That(processed.SchemaName, Is.EqualTo("testdefault"));
        }

        [Test]
        public void ReverseReturnsAlterTableExpression()
        {
            var expression = new AlterTableExpression
            {
                TableName = "TestTable",
                SchemaName = "TestSchema",
                TableDescription = "TestDescription"
            };

            var reversed = expression.Reverse();

            Assert.That(reversed, Is.TypeOf<AlterTableExpression>());
            var reversedExpression = (AlterTableExpression)reversed;
            Assert.That(reversedExpression.TableName, Is.EqualTo("TestTable"));
            Assert.That(reversedExpression.SchemaName, Is.EqualTo("TestSchema"));
            Assert.That(reversedExpression.TableDescription, Is.EqualTo("TestDescription"));
        }
    }
}
