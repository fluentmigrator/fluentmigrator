#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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
    public class AlterColumnExpressionTests
    {
        [Test]
        public void ModificationTypeShouldBeSetToAlter()
        {
            var expression = new CreateColumnExpression();
            Assert.AreEqual(ColumnModificationType.Create, expression.Column.ModificationType);
        }

        [Test]
        public void ErrorIsReturnedWhenOldNameIsNull()
        {
            var expression = new AlterColumnExpression { TableName = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenOldNameIsEmptyString()
        {
            var expression = new AlterColumnExpression { TableName = string.Empty };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsNotReturnedWhenOldNameIsNotNullEmptyString()
        {
            var expression = new AlterColumnExpression { TableName = "Bacon" };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldNotContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ToStringIsDescriptive()
        {
            var expression = new AlterColumnExpression { TableName = "Bacon", Column = { Name = "BaconId", Type = DbType.Int32 } };
            expression.ToString().ShouldBe("AlterColumn Bacon BaconId Int32");
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsNotSetThenSchemaShouldBeNull()
        {
            var expression = new AlterColumnExpression { TableName = "Bacon", Column = { Name = "BaconId", Type = DbType.Int32 } };

            var processed = expression.Apply(ConventionSets.NoSchemaName);

            Assert.That(processed.SchemaName, Is.Null);
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsSetThenSchemaShouldNotBeChanged()
        {
            var expression = new AlterColumnExpression { SchemaName = "testschema", TableName = "Bacon", Column = { Name = "BaconId", Type = DbType.Int32 } };

            var processed = expression.Apply(ConventionSets.WithSchemaName);

            Assert.That(processed.SchemaName, Is.EqualTo("testschema"));
        }

        [Test]
        public void WhenDefaultSchemaConventionIsChangedAndSchemaIsNotSetThenSetSchema()
        {
            var expression = new AlterColumnExpression { TableName = "Bacon", Column = { Name = "BaconId", Type = DbType.Int32 } };

            var processed = expression.Apply(ConventionSets.WithSchemaName);

            Assert.That(processed.SchemaName, Is.EqualTo("testdefault"));
        }
    }
}
