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

using System.Collections.ObjectModel;
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
    [Category("DeleteForeignKey")]
    public class DeleteForeignKeyExpressionTests
    {
        [Test]
        public void ToStringIsDescriptive()
        {
            new DeleteForeignKeyExpression
            {
                ForeignKey = new ForeignKeyDefinition
                {
                    ForeignColumns = new Collection<string> { "User_id" },
                    ForeignTable = "UserRoles",
                    PrimaryColumns = new Collection<string> { "Id" },
                    PrimaryTable = "User",
                    Name = "FK"
                }
            }.ToString().ShouldBe("DeleteForeignKey FK UserRoles (User_id) User (Id)");
        }

        [Test]
        public void CollectValidationErrorsShouldReturnErrorIfForeignTableNameIsEmpty()
        {
            var expression = new DeleteForeignKeyExpression { ForeignKey = new ForeignKeyDefinition { ForeignTable = string.Empty } };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.ForeignTableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void CollectValidationErrorsShouldReturnErrorIfForeignTableNameIsNull()
        {
            var expression = new DeleteForeignKeyExpression { ForeignKey = new ForeignKeyDefinition { ForeignTable = null } };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.ForeignTableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void CollectValidationErrorsShouldReturnNoErrorsIfForeignTableNameAndForeignKeyNameAreSet()
        {
            var expression = new DeleteForeignKeyExpression { ForeignKey = new ForeignKeyDefinition { ForeignTable = "ForeignTable", Name = "FK"} };
            var errors = ValidationHelper.CollectErrors(expression);

            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void CollectValidationErrorsShouldReturnErrorsIfForeignColumnsAreSetButNotPrimaryTable()
        {
            var expression = new DeleteForeignKeyExpression
            {
                ForeignKey = new ForeignKeyDefinition
                {
                    ForeignColumns = new Collection<string> { "User_id" },
                    ForeignTable = "UserRoles",
                    Name = "FK"
                }
            };
            var errors = ValidationHelper.CollectErrors(expression);

            errors.ShouldContain(ErrorMessages.PrimaryTableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void CollectValidationErrorsShouldReturnErrorsIfForeignColumnsAreSetButNotPrimaryColumns()
        {
            var expression = new DeleteForeignKeyExpression
            {
                ForeignKey = new ForeignKeyDefinition
                {
                    ForeignColumns = new Collection<string> { "User_id" },
                    ForeignTable = "UserRoles",
                    PrimaryTable = "User",
                    Name = "FK"
                }
            };
            var errors = ValidationHelper.CollectErrors(expression);

            errors.ShouldContain(ErrorMessages.ForeignKeyMustHaveOneOrMorePrimaryColumns);
        }

        [Test]
        public void CollectValidationErrorsShouldReturnNoErrorsIfAllPropertiesAreSet()
        {
            var expression = new DeleteForeignKeyExpression
            {
                ForeignKey = new ForeignKeyDefinition
                {
                    ForeignColumns = new Collection<string> { "User_id" },
                    ForeignTable = "UserRoles",
                    PrimaryColumns = new Collection<string> { "Id" },
                    PrimaryTable = "User",
                    Name = "FK"
                }
            };
            var errors = ValidationHelper.CollectErrors(expression);

            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void ReverseReturnsDeleteForeignKeyExpression()
        {
            var expression = new DeleteForeignKeyExpression
            {
                ForeignKey = new ForeignKeyDefinition
                {
                    ForeignColumns = new Collection<string> { "User_id" },
                    ForeignTable = "UserRoles",
                    PrimaryColumns = new Collection<string> { "Id" },
                    PrimaryTable = "User",
                    Name = "FK"
                }
            };
            var reverse = expression.Reverse();
            reverse.ShouldBeOfType<CreateForeignKeyExpression>();
        }

        [Test]
        public void ReverseReturnsDeleteForeignKeyExpressionAfterApplyingConventions()
        {
            var expression = new DeleteForeignKeyExpression
            {
                ForeignKey = new ForeignKeyDefinition
                {
                    ForeignColumns = new Collection<string> { "User_id" },
                    ForeignTable = "UserRoles",
                    PrimaryColumns = new Collection<string> { "Id" },
                    PrimaryTable = "User",
                }
            };

            var processed = expression.Apply(ConventionSets.WithSchemaName);

            var reverse = processed.Reverse();
            reverse.ShouldBeOfType<CreateForeignKeyExpression>();
        }

        [Test]
        public void ReverseSetsForeignTableAndForeignColumnsAndPrimaryTableAndPrimaryColumnsAOnGeneratedExpression()
        {
            var expression = new DeleteForeignKeyExpression
            {
                ForeignKey = new ForeignKeyDefinition
                {
                    ForeignColumns = new Collection<string> { "ForeignId" },
                    ForeignTable = "UserRoles",
                    PrimaryColumns = new Collection<string> { "PrimaryId" },
                    PrimaryTable = "User",
                }
            };

            var reverse = (CreateForeignKeyExpression)expression.Reverse();
            reverse.ForeignKey.ForeignTable.ShouldBe("User");
            reverse.ForeignKey.PrimaryTable.ShouldBe("UserRoles");
            reverse.ForeignKey.ForeignColumns.First().ShouldBe("PrimaryId");
            reverse.ForeignKey.PrimaryColumns.First().ShouldBe("ForeignId");
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsNotSetThenSchemaShouldBeNull()
        {
            var expression = new DeleteForeignKeyExpression();

            var processed = expression.Apply(ConventionSets.NoSchemaName);

            Assert.Multiple(() =>
            {
                Assert.That(processed.ForeignKey.ForeignTableSchema, Is.Null);
                Assert.That(processed.ForeignKey.PrimaryTableSchema, Is.Null);
            });
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsSetThenSchemaShouldNotBeChanged()
        {
            var expression = new DeleteForeignKeyExpression()
            {
                ForeignKey =
                {
                    ForeignTableSchema = "testschema",
                    PrimaryTableSchema = "testschema",
                },
            };

            var processed = expression.Apply(ConventionSets.WithSchemaName);

            Assert.Multiple(() =>
            {
                Assert.That(processed.ForeignKey.ForeignTableSchema, Is.EqualTo("testschema"));
                Assert.That(processed.ForeignKey.PrimaryTableSchema, Is.EqualTo("testschema"));
            });
        }

        [Test]
        public void WhenDefaultSchemaConventionIsChangedAndSchemaIsNotSetThenSetSchema()
        {
            var expression = new DeleteForeignKeyExpression();

            var processed = expression.Apply(ConventionSets.WithSchemaName);

            Assert.Multiple(() =>
            {
                Assert.That(processed.ForeignKey.ForeignTableSchema, Is.EqualTo("testdefault"));
                Assert.That(processed.ForeignKey.PrimaryTableSchema, Is.EqualTo("testdefault"));
            });
        }
    }
}
