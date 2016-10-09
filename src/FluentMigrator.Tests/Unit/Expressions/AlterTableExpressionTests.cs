using System;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Tests.Helpers;
using Xunit;

namespace FluentMigrator.Tests.Unit.Expressions
{
    public class AlterTableExpressionTests
    {
        [Test]
        public void ErrorIsReturnedWhenTableNameIsEmptyString()
        {
            var expression = new AlterTableExpression { TableName = String.Empty };

            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsNotReturnedWhenTableNameIsSet()
        {
            var expression = new AlterTableExpression { TableName = "table1" };

            var errors = ValidationHelper.CollectErrors(expression);
            Assert.That(errors.Count, Is.EqualTo(0));
        }
    }
}