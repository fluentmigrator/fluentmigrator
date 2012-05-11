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

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Tests.Helpers;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    public class ExecuteSqlStatementExpressionTests
    {
        [Test]
        public void ErrorIsReturnWhenSqlStatementIsNullOrEmpty()
        {
            var expression = new ExecuteSqlStatementExpression() { SqlStatement = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.SqlStatementCannotBeNullOrEmpty);
        }

        [Test]
        public void ExecutesTheStatement()
        {
            var expression = new ExecuteSqlStatementExpression() { SqlStatement = "INSERT INTO BLAH" };

            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute(expression.SqlStatement)).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ToStringIsDescriptive()
        {
            var expression = new ExecuteSqlStatementExpression() { SqlStatement = "INSERT INTO BLAH" };
            expression.ToString().ShouldBe("ExecuteSqlStatement INSERT INTO BLAH");
        }
    }
}
