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
    }
}