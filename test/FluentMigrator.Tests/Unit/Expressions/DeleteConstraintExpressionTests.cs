using System;

using FluentMigrator.Builders.Delete.Constraint;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    public class DeleteConstraintExpressionTests
    {
        [Test]
        public void ErrorIsReturnedWhenTableNameIsNull()
        {
            var expression = new DeleteConstraintExpression(ConstraintType.Unique) { Constraint = { TableName = null } };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenTableNameIsEmptyString()
        {
            var expression = new DeleteConstraintExpression(ConstraintType.Unique) { Constraint = { TableName = string.Empty } };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsNotReturnedWhenTableNameIsNotNullEmptyString()
        {
            var expression = new DeleteConstraintExpression(ConstraintType.Unique) { Constraint = { TableName = "aTable" } };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldNotContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ApplyDefaultContraintName()
        {
            var expression = new DeleteConstraintExpression(ConstraintType.Unique);
            var builder = new DeleteConstraintExpressionBuilder(expression);
            builder.FromTable("Users").Column("AccountId");

            var processed = expression.Apply(ConventionSets.NoSchemaName);

            Assert.That(processed.Constraint.ConstraintName, Is.EqualTo("UC_Users_AccountId"));
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsNotSetThenSchemaShouldBeNull()
        {
            var expression = new DeleteConstraintExpression(ConstraintType.Unique);

            var processed = expression.Apply(ConventionSets.NoSchemaName);

            Assert.That(processed.Constraint.SchemaName, Is.Null);
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsSetThenSchemaShouldNotBeChanged()
        {
            var expression = new DeleteConstraintExpression(ConstraintType.Unique)
            {
                Constraint =
                {
                    SchemaName = "testschema",
                },
            };

            var processed = expression.Apply(ConventionSets.WithSchemaName);

            Assert.That(processed.Constraint.SchemaName, Is.EqualTo("testschema"));
        }

        [Test]
        public void WhenDefaultSchemaConventionIsChangedAndSchemaIsNotSetThenSetSchema()
        {
            var expression = new DeleteConstraintExpression(ConstraintType.Unique);

            var processed = expression.Apply(ConventionSets.WithSchemaName);

            Assert.That(processed.Constraint.SchemaName, Is.EqualTo("testdefault"));
        }
    }
}
