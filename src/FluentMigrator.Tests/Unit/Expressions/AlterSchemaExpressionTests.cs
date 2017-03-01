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
    public class AlterSchemaExpressionTests
    {
        [Fact]
        public void ErrorIsReturnedWhenTableNameIsNull()
        {
            var expression = new AlterSchemaExpression { TableName = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsReturnedWhenTableNameIsEmptyString()
        {
            var expression = new AlterSchemaExpression { TableName = String.Empty };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsNotReturnedWhenTableNameIsNotNullEmptyString()
        {
            var expression = new AlterSchemaExpression { TableName = "Bacon" };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldNotContain(ErrorMessages.TableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsReturnedWhenDestinationSchemaNameIsNull()
        {
            var expression = new AlterSchemaExpression { DestinationSchemaName = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.DestinationSchemaCannotBeNull);
        }

        [Fact]
        public void ErrorIsReturnedWhenDestinationSchemaNameIsEmptyString()
        {
            var expression = new AlterSchemaExpression { DestinationSchemaName = String.Empty };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.DestinationSchemaCannotBeNull);
        }

        [Fact]
        public void ErrorIsNotReturnedWhenDestinationSchemaNameIsNotNullEmptyString()
        {
            var expression = new AlterSchemaExpression { DestinationSchemaName = "Bacon" };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldNotContain(ErrorMessages.DestinationSchemaCannotBeNull);
        }

        [Fact]
        [ExpectedException(typeof(NotSupportedException))]
        public void ReverseThrowsException()
        {
            new AlterSchemaExpression().Reverse();
        }

        [Fact]
        public void ToStringIsDescriptive()
        {
            var expression = new AlterSchemaExpression { TableName = "Test", DestinationSchemaName = "Bacon" };
            expression.ToString().ShouldBe("AlterSchema Bacon Table Test");
        }
    }
}