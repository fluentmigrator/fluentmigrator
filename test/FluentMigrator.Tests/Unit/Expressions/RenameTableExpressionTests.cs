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
using FluentMigrator.Runner;
using FluentMigrator.Tests.Helpers;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    [Category("Expression")]
    [Category("RenameTable")]
    public class RenameTableExpressionTests
    {
        [Test]
        public void ErrorIsReturnedWhenOldNameIsNull()
        {
            var expression = new RenameTableExpression { OldName = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.OldTableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenOldNameIsEmptyString()
        {
            var expression = new RenameTableExpression { OldName = string.Empty };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.OldTableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsNotReturnedWhenOldNameIsNotNullEmptyString()
        {
            var expression = new RenameTableExpression { OldName = "Bacon" };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldNotContain(ErrorMessages.OldTableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenNewNameIsNull()
        {
            var expression = new RenameTableExpression { NewName = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.NewTableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenNewNameIsEmptyString()
        {
            var expression = new RenameTableExpression { NewName = string.Empty };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.NewTableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsNotReturnedWhenNewNameIsNotNullOrEmptyString()
        {
            var expression = new RenameTableExpression { NewName = "Bacon" };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldNotContain(ErrorMessages.NewTableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ReverseReturnsRenameTableExpression()
        {
            var expression = new RenameTableExpression { OldName = "Bacon", NewName = "ChunkyBacon" };
            var reverse = expression.Reverse();
            reverse.ShouldBeOfType<RenameTableExpression>();
        }

        [Test]
        public void ReverseSetsOldNameAndNewNameOnGeneratedExpression()
        {
            var expression = new RenameTableExpression { OldName = "Bacon", NewName = "ChunkyBacon" };
            var reverse = (RenameTableExpression)expression.Reverse();
            reverse.OldName.ShouldBe("ChunkyBacon");
            reverse.NewName.ShouldBe("Bacon");
        }

        [Test]
        public void ToStringIsDescriptive()
        {
            var expression = new RenameTableExpression { OldName = "Bacon", NewName = "ChunkyBacon" };
            expression.ToString().ShouldBe("RenameTable Bacon ChunkyBacon");
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsNotSetThenSchemaShouldBeNull()
        {
            var expression = new RenameTableExpression { OldName = "Bacon", NewName = "ChunkyBacon" };

            var processed = expression.Apply(ConventionSets.NoSchemaName);

            Assert.That(processed.SchemaName, Is.Null);
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsSetThenSchemaShouldNotBeChanged()
        {
            var expression = new RenameTableExpression { SchemaName = "testschema", OldName = "Bacon", NewName = "ChunkyBacon" };

            var processed = expression.Apply(ConventionSets.WithSchemaName);

            Assert.That(processed.SchemaName, Is.EqualTo("testschema"));
        }

        [Test]
        public void WhenDefaultSchemaConventionIsChangedAndSchemaIsNotSetThenSetSchema()
        {
            var expression = new RenameTableExpression { OldName = "Bacon", NewName = "ChunkyBacon" };

            var processed = expression.Apply(ConventionSets.WithSchemaName);

            Assert.That(processed.SchemaName, Is.EqualTo("testdefault"));
        }
    }
}
