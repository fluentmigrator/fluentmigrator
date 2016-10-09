using System;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Tests.Helpers;
using Xunit;

namespace FluentMigrator.Tests.Unit.Expressions
{
    public class CreateConstraintExpressionTests
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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