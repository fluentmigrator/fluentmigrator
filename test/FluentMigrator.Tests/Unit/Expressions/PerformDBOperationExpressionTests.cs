#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Tests.Helpers;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    [Category("Expression")]
    [Category("PerformDBOperation")]
    public class PerformDBOperationExpressionTests
    {
        [Test]
        public void ErrorIsReturnedWhenOperationIsNull()
        {
            var expression = new PerformDBOperationExpression() { Operation = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.OperationCannotBeNull);
        }

        [Test]
        public void DescriptionCanBeSet()
        {
            const string testDescription = "Test operation description";
            var expression = new PerformDBOperationExpression 
            { 
                Operation = (connection, transaction) => { },
                Description = testDescription
            };
            
            expression.Description.ShouldBe(testDescription);
        }

        [Test]
        public void DescriptionCanBeNull()
        {
            var expression = new PerformDBOperationExpression 
            { 
                Operation = (connection, transaction) => { },
                Description = null
            };
            
            expression.Description.ShouldBeNull();
        }
    }
}
