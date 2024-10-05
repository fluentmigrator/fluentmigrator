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
    [Category("ForeignKey")]
    public class ForeignKeyDefinitionTests
    {
        [Test]
        public void ErrorIsReturnedWhenNameIsNull()
        {
            var column = new ForeignKeyDefinition { Name = null };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.ForeignKeyNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsNotReturnedWhenNameIsNotNullOrEmptyString()
        {
            var column = new ForeignKeyDefinition { Name = "Bacon" };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldNotContain(ErrorMessages.ForeignKeyNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenForeignTableNameIsNull()
        {
            var column = new ForeignKeyDefinition { ForeignTable = null };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.ForeignTableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsNotReturnedWhenTableNameIsNotNullOrEmptyString()
        {
            var column = new ForeignKeyDefinition { ForeignTable = "Bacon" };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldNotContain(ErrorMessages.ForeignTableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenForeignTableNameIsEmptyString()
        {
            var column = new ForeignKeyDefinition { ForeignTable = string.Empty };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.ForeignTableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenPrimaryTableNameIsNull()
        {
            var column = new ForeignKeyDefinition { PrimaryTable = null };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.PrimaryTableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenPrimaryTableNameIsEmptyString()
        {
            var column = new ForeignKeyDefinition { PrimaryTable = string.Empty };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.PrimaryTableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsNotReturnedWhenPrimaryTableNameIsNotNullOrEmptyString()
        {
            var column = new ForeignKeyDefinition { PrimaryTable = "Bacon" };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldNotContain(ErrorMessages.PrimaryTableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenForeignColumnsIsEmpty()
        {
            var column = new ForeignKeyDefinition { Name = "FkName", ForeignTable = "FkTable", ForeignColumns = new string[0], PrimaryTable = "PkTable" };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.ForeignKeyMustHaveOneOrMoreForeignColumns);
        }

        [Test]
        public void ErrorIsNotReturnedWhenForeignColumnsIsNotEmpty()
        {
            var column = new ForeignKeyDefinition { ForeignColumns = new[] { "Bacon" } };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldNotContain(ErrorMessages.ForeignKeyMustHaveOneOrMoreForeignColumns);
        }

        [Test]
        public void ErrorIsReturnedWhenPrimaryColumnsIsEmpty()
        {
            var column = new ForeignKeyDefinition { Name = "FkName", ForeignTable = "FkTable", PrimaryTable = "PkTable", PrimaryColumns = new string[0] };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.ForeignKeyMustHaveOneOrMorePrimaryColumns);
        }

        [Test]
        public void ErrorIsNotReturnedWhenPrimaryColumnsIsNotEmpty()
        {
            var column = new ForeignKeyDefinition { PrimaryColumns = new[] { "Bacon" } };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldNotContain(ErrorMessages.ForeignKeyMustHaveOneOrMorePrimaryColumns);
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsNotSetThenSchemaShouldBeNull()
        {
            var expr = new CreateForeignKeyExpression()
            {
                ForeignKey =
                {
                    Name = "Test"
                }
            };

            var processed = expr.Apply(ConventionSets.NoSchemaName);

            Assert.Multiple(() =>
            {
                Assert.That(processed.ForeignKey.ForeignTableSchema, Is.Null);
                Assert.That(processed.ForeignKey.PrimaryTableSchema, Is.Null);
            });
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsSetThenSchemaShouldNotBeChanged()
        {
            var expr = new CreateForeignKeyExpression()
            {
                ForeignKey =
                {
                    Name = "Test",
                    ForeignTableSchema = "testschema",
                    PrimaryTableSchema = "testschema"
                }
            };

            var processed = expr.Apply(ConventionSets.WithSchemaName);

            Assert.Multiple(() =>
            {
                Assert.That(processed.ForeignKey.ForeignTableSchema, Is.EqualTo("testschema"));
                Assert.That(processed.ForeignKey.PrimaryTableSchema, Is.EqualTo("testschema"));
            });
        }

        [Test]
        public void WhenDefaultSchemaConventionIsChangedAndSchemaIsNotSetThenSetSchema()
        {
            var expr = new CreateForeignKeyExpression()
            {
                ForeignKey =
                {
                    Name = "Test"
                }
            };

            var processed = expr.Apply(ConventionSets.WithSchemaName);

            Assert.Multiple(() =>
            {
                Assert.That(processed.ForeignKey.ForeignTableSchema, Is.EqualTo("testdefault"));
                Assert.That(processed.ForeignKey.PrimaryTableSchema, Is.EqualTo("testdefault"));
            });
        }
    }
}
