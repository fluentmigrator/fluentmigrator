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
    public class RenameTableExpressionTests
    {
        [Fact]
        public void ErrorIsReturnedWhenOldNameIsNull()
        {
            var expression = new RenameTableExpression { OldName = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.OldTableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsReturnedWhenOldNameIsEmptyString()
        {
            var expression = new RenameTableExpression { OldName = String.Empty };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.OldTableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsNotReturnedWhenOldNameIsNotNullEmptyString()
        {
            var expression = new RenameTableExpression { OldName = "Bacon" };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldNotContain(ErrorMessages.OldTableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsReturnedWhenNewNameIsNull()
        {
            var expression = new RenameTableExpression { NewName = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.NewTableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsReturnedWhenNewNameIsEmptyString()
        {
            var expression = new RenameTableExpression { NewName = String.Empty };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.NewTableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsNotReturnedWhenNewNameIsNotNullOrEmptyString()
        {
            var expression = new RenameTableExpression { NewName = "Bacon" };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldNotContain(ErrorMessages.NewTableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ReverseReturnsRenameTableExpression()
        {
            var expression = new RenameTableExpression { OldName = "Bacon", NewName = "ChunkyBacon" };
            var reverse = expression.Reverse();
            reverse.ShouldBeOfType<RenameTableExpression>();
        }

        [Fact]
        public void ReverseSetsOldNameAndNewNameOnGeneratedExpression()
        {
            var expression = new RenameTableExpression { OldName = "Bacon", NewName = "ChunkyBacon" };
            var reverse = expression.Reverse() as RenameTableExpression;
            reverse.OldName.ShouldBe("ChunkyBacon");
            reverse.NewName.ShouldBe("Bacon");
        }

        [Fact]
        public void ToStringIsDescriptive()
        {
            var expression = new RenameTableExpression { OldName = "Bacon", NewName = "ChunkyBacon" };
            expression.ToString().ShouldBe("RenameTable Bacon ChunkyBacon");
        }
    }
}