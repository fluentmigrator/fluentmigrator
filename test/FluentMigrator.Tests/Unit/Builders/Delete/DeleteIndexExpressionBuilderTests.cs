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

using System.Collections.Generic;
using FluentMigrator.Builders.Delete.Index;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Builders.Delete
{
    [TestFixture]
    [Category("Builder")]
    [Category("DeleteIndex")]
    public class DeleteIndexExpressionBuilderTests
    {
        [Test]
        public void CallingOnTableSetsTableNameToSpecifiedValue()
        {
            var indexMock = new Mock<IndexDefinition>();

            var expressionMock = new Mock<DeleteIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            var builder = new DeleteIndexExpressionBuilder(expressionMock.Object);
            builder.OnTable("Bacon");

            indexMock.VerifySet(x => x.TableName = "Bacon");
            expressionMock.VerifyGet(e => e.Index);
        }

        [Test]
        public void CallingOnColumnAddsNewColumnToExpression()
        {
            var collectionMock = new Mock<IList<IndexColumnDefinition>>();

            var indexMock = new Mock<IndexDefinition>();
            indexMock.SetupGet(x => x.Columns).Returns(collectionMock.Object);

            var expressionMock = new Mock<DeleteIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            var builder = new DeleteIndexExpressionBuilder(expressionMock.Object);
            builder.OnColumn("BaconId");

            collectionMock.Verify(x => x.Add(It.Is<IndexColumnDefinition>(c => c.Name.Equals("BaconId"))));
            indexMock.VerifyGet(x => x.Columns);
            expressionMock.VerifyGet(e => e.Index);
        }

        [Test]
        public void CallingOnColumnsAddsMultipleNewColumnsToExpression()
        {
            var collectionMock = new Mock<IList<IndexColumnDefinition>>();

            var indexMock = new Mock<IndexDefinition>();
            indexMock.SetupGet(x => x.Columns).Returns(collectionMock.Object);

            var expressionMock = new Mock<DeleteIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);

            var builder = new DeleteIndexExpressionBuilder(expressionMock.Object);
            builder.OnColumns("BaconId", "EggsId");

            collectionMock.Verify(x => x.Add(It.Is<IndexColumnDefinition>(c => c.Name.Equals("BaconId"))));
            collectionMock.Verify(x => x.Add(It.Is<IndexColumnDefinition>(c => c.Name.Equals("EggsId"))));
            indexMock.VerifyGet(x => x.Columns);
            expressionMock.VerifyGet(e => e.Index);
        }
    }
}