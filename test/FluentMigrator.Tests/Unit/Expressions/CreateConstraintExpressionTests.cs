using System;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    public class CreateConstraintExpressionTests
    {
        [Test]
        public void ErrorIsReturnedWhenTableNameIsEmptyString()
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique)
                                 {
                                     Constraint =
                                         {
                                             TableName =
                                                 String.Empty
                                         }
                                 };

            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenHasNoColumns()
        {
            var expression = new CreateConstraintExpression(ConstraintType.PrimaryKey)
            {
                Constraint =
                {
                    TableName = "table1"
                }
            };

            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.ConstraintMustHaveAtLeastOneColumn);
        }

        [Test]
        public void ErrorIsNotReturnedWhenTableNameIsSetAndHasAtLeastOneColumn()
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique)
            {
                Constraint =
                {
                    TableName = "table1"
                }
            };
            expression.Constraint.Columns.Add("column1");

            var errors = ValidationHelper.CollectErrors(expression);
            Assert.That(errors.Count, Is.EqualTo(0));
        }
  
        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsNotSetThenSchemaShouldBeNull()
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique);

            expression.ApplyConventions(new MigrationConventions());

            Assert.That(expression.Constraint.SchemaName, Is.Null);
        }

        [Test]
        public void WhenDefaultSchemaConventionIsAppliedAndSchemaIsSetThenSchemaShouldNotBeChanged()
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique) { Constraint = { SchemaName = "testschema" } };

            expression.ApplyConventions(new MigrationConventions());

            Assert.That(expression.Constraint.SchemaName, Is.EqualTo("testschema"));
        }

        [Test]
        public void WhenDefaultSchemaConventionIsChangedAndSchemaIsNotSetThenSetSchema()
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique);
            var migrationConventions = new MigrationConventions { GetDefaultSchema = () => "testdefault" };

            expression.ApplyConventions(migrationConventions);

            Assert.That(expression.Constraint.SchemaName, Is.EqualTo("testdefault"));
        }
    }
}
