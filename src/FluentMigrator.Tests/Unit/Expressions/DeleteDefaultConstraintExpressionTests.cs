﻿using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Tests.Helpers;
using Moq;
using Xunit;

namespace FluentMigrator.Tests.Unit.Expressions
{
    public class DeleteDefaultConstraintExpressionTests
    {
        [Fact]
        public void CollectValidationErrorsShouldReturnErrorIfColumnNameIsEmpty()
        {
            var expression = new DeleteDefaultConstraintExpression {ColumnName = string.Empty};
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void CollectValidationErrorsShouldReturnErrorIfColumnNameIsNull()
        {
            var expression = new DeleteDefaultConstraintExpression {ColumnName = null};
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void CollectValidationErrorsShouldReturnErrorIfTableNameIsEmpty()
        {
            var expression = new DeleteDefaultConstraintExpression {TableName = string.Empty};
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void CollectValidationErrorsShouldReturnErrorIfTableNameIsNull()
        {
            var expression = new DeleteDefaultConstraintExpression {TableName = null};
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ExecuteWithShouldDelegateProcessOnMigrationProcessor()
        {
            var expression = new DeleteDefaultConstraintExpression();
            var processorMock = new Mock<IMigrationProcessor>(MockBehavior.Strict);
            processorMock.Setup(p => p.Process(expression)).Verifiable();

            expression.ExecuteWith(processorMock.Object);

            processorMock.VerifyAll();
        }

        [Fact]
        public void ToStringIsDescriptive()
        {
            var expression = new DeleteDefaultConstraintExpression {SchemaName = "ThaSchema", TableName = "ThaTable", ColumnName = "ThaColumn"};

            expression.ToString().ShouldBe("DeleteDefaultConstraint ThaSchema.ThaTable ThaColumn");
        }
    }
}