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
    public class IndexColumnDefinitionTests
    {
        [Fact]
        public void DirectionIsAscendingIfNotSpecified()
        {
            var column = new IndexColumnDefinition();
            column.Direction.ShouldBe(Direction.Ascending);
        }

        [Fact]
        public void ErrorIsReturnedWhenColumnNameIsNull()
        {
            var column = new IndexColumnDefinition { Name = null };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsReturnedWhenColumnNameIsEmptyString()
        {
            var column = new IndexColumnDefinition { Name = String.Empty };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsNotReturnedWhenColumnNameIsNotNullOrEmptyString()
        {
            var column = new IndexColumnDefinition { Name = "Bacon" };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldNotContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsReturnedWhenIncludeNameIsNull()
        {
            var column = new IndexIncludeDefinition { Name = null };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsReturnedWhenIncludeNameIsEmptyString()
        {
            var column = new IndexIncludeDefinition { Name = String.Empty };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }

        [Fact]
        public void ErrorIsNotReturnedWhenIncludeNameIsNotNullOrEmptyString()
        {
            var column = new IndexIncludeDefinition { Name = "Bacon" };
            var errors = ValidationHelper.CollectErrors(column);
            errors.ShouldNotContain(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }
    }
}