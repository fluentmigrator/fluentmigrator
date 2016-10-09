using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Tests.Helpers;
using Xunit;

namespace FluentMigrator.Tests.Unit.Expressions
{
    public class AlterDefaultConstraintExpressionTests
    {
        [Test]
        public void ErrorIsReturnedWhenTableNameIsNull()
        {
            var expression = new AlterDefaultConstraintExpression { TableName = null, ColumnName = "Column1", DefaultValue = SystemMethods.CurrentDateTime };

            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenColumnNameIsNull()
        {
            var expression = new AlterDefaultConstraintExpression { TableName = "test", ColumnName = "", DefaultValue = SystemMethods.CurrentDateTime };

            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenDefaultValueIsNull()
        {
            var expression = new AlterDefaultConstraintExpression { TableName = "test", ColumnName = "Column1", DefaultValue = null};

            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.DefaultValueCannotBeNull);
        }

        [Test]
        public void ErrorIsNotReturnedWhenTableNameAndColumnNameAndDefaultValueAreSet()
        {
            var expression = new AlterDefaultConstraintExpression { TableName = "test", ColumnName = "Column1", DefaultValue = SystemMethods.CurrentDateTime};

            var errors = ValidationHelper.CollectErrors(expression);
            Assert.That(errors.Count, Is.EqualTo(0));
        }
    }
}