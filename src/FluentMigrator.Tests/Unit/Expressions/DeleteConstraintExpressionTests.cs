﻿using System;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Tests.Helpers;
using Xunit;

namespace FluentMigrator.Tests.Unit.Expressions
{
    public class DeleteConstraintExpressionTests
    {
        [Fact]
        public void ErrorIsReturnedWhenTableNameIsNull()
        {
            var expression = new DeleteConstraintExpression(ConstraintType.Unique) { Constraint = { TableName = null } };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsReturnedWhenTableNameIsEmptyString()
        {
            var expression = new DeleteConstraintExpression(ConstraintType.Unique) { Constraint = { TableName = string.Empty } };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsNotReturnedWhenTableNameIsNotNullEmptyString()
        {
            var expression = new DeleteConstraintExpression(ConstraintType.Unique) { Constraint = { TableName = "aTable" } };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldNotContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }
    }
}