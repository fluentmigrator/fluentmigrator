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

namespace FluentMigrator.Tests.Unit.Definitions
{
    [TestFixture]
    [Category("Definition")]
    public class IndexColumnDefinitionTests
    {
        [Test]
        public void DirectionIsAscendingIfNotSpecified()
        {
            var column = new IndexColumnDefinition();
            column.Direction.ShouldBe(Direction.Ascending);
        }

        [Test]
        public void ErrorIsReturnedWhenColumnNameIsNull()
        {
            var column = new IndexColumnDefinition { Name = null };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenColumnNameIsEmptyString()
        {
            var column = new IndexColumnDefinition { Name = string.Empty };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsNotReturnedWhenColumnNameIsNotNullOrEmptyString()
        {
            var column = new IndexColumnDefinition { Name = "Bacon" };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldNotContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenIncludeNameIsNull()
        {
            var column = new IndexIncludeDefinition { Name = null };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.IndexIncludeColumnNameMustNotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenIncludeNameIsEmptyString()
        {
            var column = new IndexIncludeDefinition { Name = string.Empty };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.IndexIncludeColumnNameMustNotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsNotReturnedWhenIncludeNameIsNotNullOrEmptyString()
        {
            var column = new IndexIncludeDefinition { Name = "Bacon" };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldNotContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsNotSetThenSchemaShouldBeNull()
        {
            var expr = new CreateIndexExpression
            {
                Index =
                {
                    Name = "Test"
                }
            };

            var processed = expr.Apply(ConventionSets.NoSchemaName);

            Assert.That(processed.Index.SchemaName, Is.Null);
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsSetThenSchemaShouldNotBeChanged()
        {
            var expr = new CreateIndexExpression()
            {
                Index =
                {
                    Name = "Test",
                    SchemaName = "testschema"
                }
            };

            var processed = expr.Apply(ConventionSets.WithSchemaName);

            Assert.That(processed.Index.SchemaName, Is.EqualTo("testschema"));
        }

        [Test]
        public void WhenDefaultSchemaConventionIsChangedAndSchemaIsNotSetThenSetSchema()
        {
            var expr = new CreateIndexExpression()
            {
                Index =
                {
                    Name = "Test"
                }
            };

            var processed = expr.Apply(ConventionSets.WithSchemaName);

            Assert.That(processed.Index.SchemaName, Is.EqualTo("testdefault"));
        }
    }
}
