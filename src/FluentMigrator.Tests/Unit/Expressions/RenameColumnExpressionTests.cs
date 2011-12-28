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
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    public class RenameColumnExpressionTests
    {
        [Test]
        public void ErrorIsReturnedWhenOldNameIsNull()
        {
            var expression = new RenameColumnExpression { OldName = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.OldColumnNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenOldNameIsEmptyString()
        {
            var expression = new RenameColumnExpression { OldName = String.Empty };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.OldColumnNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsNotReturnedWhenOldNameIsNotNullEmptyString()
        {
            var expression = new RenameColumnExpression { OldName = "Bacon" };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldNotContain(ErrorMessages.OldColumnNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenNewNameIsNull()
        {
            var expression = new RenameColumnExpression { NewName = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.NewColumnNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsReturnedWhenNewNameIsEmptyString()
        {
            var expression = new RenameColumnExpression { NewName = String.Empty };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.NewColumnNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ErrorIsNotReturnedWhenNewNameIsNotNullOrEmptyString()
        {
            var expression = new RenameColumnExpression { NewName = "Bacon" };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldNotContain(ErrorMessages.NewColumnNameCannotBeNullOrEmpty);
        }

        [Test]
        public void ReverseReturnsRenameColumnExpression()
        {
            var expression = new RenameColumnExpression { TableName = "Bacon", OldName = "BaconId", NewName = "ChunkyBaconId" };
            var reverse = expression.Reverse();
            reverse.ShouldBeOfType<RenameColumnExpression>();
        }

        [Test]
        public void ReverseSetsTableNameOldNameAndNewNameOnGeneratedExpression()
        {
            var expression = new RenameColumnExpression { TableName = "Bacon", OldName = "BaconId", NewName = "ChunkyBaconId" };
            var reverse = expression.Reverse() as RenameColumnExpression;

            reverse.TableName.ShouldBe("Bacon");
            reverse.OldName.ShouldBe("ChunkyBaconId");
            reverse.NewName.ShouldBe("BaconId");
        }

        [Test]
        public void ToStringIsDescriptive()
        {
            var expression = new RenameColumnExpression { TableName = "Bacon", OldName = "BaconId", NewName = "ChunkyBaconId" };
            expression.ToString().ShouldBe("RenameColumn Bacon BaconId to ChunkyBaconId");
        }
    }
}