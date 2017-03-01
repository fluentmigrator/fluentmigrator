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
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Tests.Helpers;
using Xunit;

namespace FluentMigrator.Tests.Unit.Definitions
{
    public class ForeignKeyDefinitionTests
    {
        [Fact]
        public void ErrorIsReturnedWhenNameIsNull()
        {
            var column = new ForeignKeyDefinition { Name = null };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.ForeignKeyNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsNotReturnedWhenNameIsNotNullOrEmptyString()
        {
            var column = new ForeignKeyDefinition { Name = "Bacon" };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldNotContain(ErrorMessages.ForeignKeyNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsReturnedWhenForeignTableNameIsNull()
        {
            var column = new ForeignKeyDefinition { ForeignTable = null };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.ForeignTableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsNotReturnedWhenTableNameIsNotNullOrEmptyString()
        {
            var column = new ForeignKeyDefinition { ForeignTable = "Bacon" };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldNotContain(ErrorMessages.ForeignTableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsReturnedWhenForeignTableNameIsEmptyString()
        {
            var column = new ForeignKeyDefinition { ForeignTable = String.Empty };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.ForeignTableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsReturnedWhenPrimaryTableNameIsNull()
        {
            var column = new ForeignKeyDefinition { PrimaryTable = null };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.PrimaryTableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsReturnedWhenPrimaryTableNameIsEmptyString()
        {
            var column = new ForeignKeyDefinition { PrimaryTable = String.Empty };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.PrimaryTableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsNotReturnedWhenPrimaryTableNameIsNotNullOrEmptyString()
        {
            var column = new ForeignKeyDefinition { PrimaryTable = "Bacon" };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldNotContain(ErrorMessages.PrimaryTableNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsReturnedWhenForeignColumnsIsEmpty()
        {
            var column = new ForeignKeyDefinition { ForeignColumns = new string[0] };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.ForeignKeyMustHaveOneOrMoreForeignColumns);
        }

        [Fact]
        public void ErrorIsNotReturnedWhenForeignColumnsIsNotEmpty()
        {
            var column = new ForeignKeyDefinition { ForeignColumns = new[] { "Bacon" } };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldNotContain(ErrorMessages.ForeignKeyMustHaveOneOrMoreForeignColumns);
        }

        [Fact]
        public void ErrorIsReturnedWhenPrimaryColumnsIsEmpty()
        {
            var column = new ForeignKeyDefinition { PrimaryColumns = new string[0] };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.ForeignKeyMustHaveOneOrMorePrimaryColumns);
        }

        [Fact]
        public void ErrorIsNotReturnedWhenPrimaryColumnsIsNotEmpty()
        {
            var column = new ForeignKeyDefinition { PrimaryColumns = new[] { "Bacon" } };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldNotContain(ErrorMessages.ForeignKeyMustHaveOneOrMorePrimaryColumns);
        }
    }
}