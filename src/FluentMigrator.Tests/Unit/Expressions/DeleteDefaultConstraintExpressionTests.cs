using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Tests.Helpers;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    public class DeleteDefaultConstraintExpressionTests
    {
        [Test]
        public void CollectValidationErrorsShouldReturnErrorIfColumnNameIsEmpty()
        {
            var expression = new DeleteDefaultConstraintExpression {ColumnName = string.Empty};
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }

        [Test]
        public void CollectValidationErrorsShouldReturnErrorIfColumnNameIsNull()
        {
            var expression = new DeleteDefaultConstraintExpression {ColumnName = null};
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }

        [Test]
        public void CollectValidationErrorsShouldReturnErrorIfTableNameIsEmpty()
        {
            var expression = new DeleteDefaultConstraintExpression {TableName = string.Empty};
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void CollectValidationErrorsShouldReturnErrorIfTableNameIsNull()
        {
            var expression = new DeleteDefaultConstraintExpression {TableName = null};
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ExecuteWithShouldDelegateProcessOnMigrationProcessor()
        {
            var expression = new DeleteDefaultConstraintExpression();
            var processorMock = new Mock<IMigrationProcessor>(MockBehavior.Strict);
            processorMock.Setup(p => p.Process(expression)).Verifiable();

            expression.ExecuteWith(processorMock.Object);

            processorMock.VerifyAll();
        }

        [Test]
        public void ToStringIsDescriptive()
        {
            var expression = new DeleteDefaultConstraintExpression {SchemaName = "ThaSchema", TableName = "ThaTable", ColumnName = "ThaColumn"};

            expression.ToString().ShouldBe("DeleteDefaultConstraint ThaSchema.ThaTable ThaColumn");
        }
    }
}