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

using System.Data;
using System.Linq;

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
    [Category("CreateColumn")]
    public class CreateColumnExpressionTests
    {
        [Test]
        public void ModificationTypeShouldBeSetToCreate()
        {
            var expression = new CreateColumnExpression();
            Assert.That(expression.Column.ModificationType, Is.EqualTo(ColumnModificationType.Create));
        }

        [Test]
        public void ErrorIsReturnedWhenOldNameIsNull()
        {
            var expression = new CreateColumnExpression { TableName = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenOldNameIsEmptyString()
        {
            var expression = new CreateColumnExpression { TableName = string.Empty };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsNotReturnedWhenOldNameIsNotNullEmptyString()
        {
            var expression = new CreateColumnExpression { TableName = "Bacon" };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldNotContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ReverseReturnsDeleteColumnExpression()
        {
            var expression = new CreateColumnExpression { TableName = "Bacon", Column = { Name = "BaconId" } };
            var reverse = expression.Reverse();
            reverse.ShouldBeOfType<DeleteColumnExpression>();
        }

        [Test]
        public void ReverseSetsTableNameAndColumnNameOnGeneratedExpression()
        {
            var expression = new CreateColumnExpression { TableName = "Bacon", Column = { Name = "BaconId" } };
            var reverse = (DeleteColumnExpression)expression.Reverse();
            reverse.TableName.ShouldBe("Bacon");
            reverse.ColumnNames.ElementAt(0).ShouldBe("BaconId");
        }

        [Test]
        public void ToStringIsDescriptive()
        {
            var expression = new CreateColumnExpression { TableName = "Bacon", Column = { Name = "BaconId", Type = DbType.Int32 } };
            expression.ToString().ShouldBe("CreateColumn Bacon BaconId Int32");
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsNotSetThenSchemaShouldBeNull()
        {
            var expression = new CreateColumnExpression { TableName = "Bacon", Column = { Name = "BaconId", Type = DbType.Int32 } };

            var processed = expression.Apply(ConventionSets.NoSchemaName);

            Assert.That(processed.SchemaName, Is.Null);
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsSetThenSchemaShouldNotBeChanged()
        {
            var expression = new CreateColumnExpression { SchemaName = "testschema", TableName = "Bacon", Column = { Name = "BaconId", Type = DbType.Int32 } };

            var processed = expression.Apply(ConventionSets.WithSchemaName);

            Assert.That(processed.SchemaName, Is.EqualTo("testschema"));
        }

        [Test]
        public void WhenDefaultSchemaConventionIsChangedAndSchemaIsNotSetThenSetSchema()
        {
            var expression = new CreateColumnExpression { TableName = "Bacon", Column = { Name = "BaconId", Type = DbType.Int32 } };

            var processed = expression.Apply(ConventionSets.WithSchemaName);

            Assert.That(processed.SchemaName, Is.EqualTo("testdefault"));
        }
    }
}
