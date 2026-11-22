#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using FluentMigrator.Builders.Merge;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Builders.Merge
{
    [TestFixture]
    [Category("Builder")]
    [Category("MergeData")]
    public class MergeExpressionRootTests
    {
        [Test]
        public void IntoTableCreatesMergeDataExpression()
        {
            var contextMock = new Mock<IMigrationContext>();

            var root = new MergeExpressionRoot(contextMock.Object);
            var result = root.IntoTable("TestTable");

            result.ShouldNotBeNull();
            result.ShouldBeOfType<MergeDataExpressionBuilder>();

            contextMock.Verify(x => x.Expressions.Add(It.IsAny<MergeDataExpression>()), Times.Once);
        }

        [Test]
        public void IntoTableSetsCorrectTableName()
        {
            var contextMock = new Mock<IMigrationContext>();
            MergeDataExpression expression = null;

            contextMock.Setup(x => x.Expressions.Add(It.IsAny<MergeDataExpression>()))
                      .Callback<IMigrationExpression>(expr => expression = expr as MergeDataExpression);

            var root = new MergeExpressionRoot(contextMock.Object);
            root.IntoTable("TestTable");

            expression.ShouldNotBeNull();
            expression.TableName.ShouldBe("TestTable");
        }
    }
}