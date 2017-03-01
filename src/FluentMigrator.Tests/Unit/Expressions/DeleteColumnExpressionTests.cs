#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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

using System;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Tests.Helpers;
using Xunit;

namespace FluentMigrator.Tests.Unit.Expressions
{
    public class DeleteColumnExpressionTests
    {
        [Fact]
        public void ErrorIsReturnedWhenTableNameIsNull()
        {
            var expression = new DeleteColumnExpression { TableName = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsReturnedWhenTableNameIsEmptyString()
        {
            var expression = new DeleteColumnExpression { TableName = String.Empty };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsNotReturnedWhenTableNameIsNotNullEmptyString()
        {
            var expression = new DeleteColumnExpression { TableName = "Bacon" };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldNotContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsReturnedWhenColumnNameIsNull()
        {
            var expression = new DeleteColumnExpression { ColumnNames = {null} };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsReturnedWhenColumnNameIsEmptyString()
        {
            var expression = new DeleteColumnExpression { ColumnNames = {String.Empty} };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsReturnedWhenColumnIsSpecifiedMultipleTimes() 
        {
            var expression = new DeleteColumnExpression { ColumnNames = { "Bacon", "Bacon" } };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.ColumnNamesMustBeUnique);
        }

        [Fact]
        public void ErrorIsNotReturnedWhenColumnNameIsNotNullEmptyString()
        {
            var expression = new DeleteColumnExpression { ColumnNames = {"Bacon"} };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldNotContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }

        [Fact]
        [ExpectedException(typeof(NotSupportedException))]
        public void ReverseThrowsException()
        {
            new DeleteColumnExpression().Reverse();
        }

        [Fact]
        public void ToStringIsDescriptive()
        {
            var expression = new DeleteColumnExpression { TableName = "Test", ColumnNames = {"Bacon"} };
            expression.ToString().ShouldBe("DeleteColumn Test Bacon");
        }
    }
}