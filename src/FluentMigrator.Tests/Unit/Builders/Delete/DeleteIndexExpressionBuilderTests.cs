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

using System.Collections.Generic;
using FluentMigrator.Builders.Delete.Index;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Builders.Delete
{
	[TestFixture]
	public class DeleteIndexExpressionBuilderTests
	{
		[Test]
		public void CallingOnTableSetsTableNameToSpecifiedValue()
		{
			var indexMock = new Mock<IndexDefinition>();
			indexMock.VerifySet(x => x.TableName = "Bacon", Times.AtMostOnce());

			var expressionMock = new Mock<DeleteIndexExpression>();
			expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object).AtMostOnce();

			var builder = new DeleteIndexExpressionBuilder(expressionMock.Object);
			builder.OnTable("Bacon");

			indexMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Test]
		public void CallingOnColumnAddsNewColumnToExpression()
		{
			var collectionMock = new Mock<IList<IndexColumnDefinition>>();
            collectionMock.Verify(x => x.Add(It.Is<IndexColumnDefinition>(c => c.Name.Equals("BaconId"))), Times.AtMostOnce());

			var indexMock = new Mock<IndexDefinition>();
			indexMock.SetupGet(x => x.Columns).Returns(collectionMock.Object).AtMostOnce();

			var expressionMock = new Mock<DeleteIndexExpression>();
			expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object).AtMostOnce();

			var builder = new DeleteIndexExpressionBuilder(expressionMock.Object);
			builder.OnColumn("BaconId");

			collectionMock.VerifyAll();
			indexMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Test]
		public void CallingOnColumnsAddsMultipleNewColumnsToExpression()
		{
			var collectionMock = new Mock<IList<IndexColumnDefinition>>();
            collectionMock.Verify(x => x.Add(It.Is<IndexColumnDefinition>(c => c.Name.Equals("BaconId"))), Times.AtMostOnce());
            collectionMock.Verify(x => x.Add(It.Is<IndexColumnDefinition>(c => c.Name.Equals("EggsId"))), Times.AtMostOnce());

			var indexMock = new Mock<IndexDefinition>();
			indexMock.SetupGet(x => x.Columns).Returns(collectionMock.Object).AtMost(2);

			var expressionMock = new Mock<DeleteIndexExpression>();
			expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object).AtMost(2);

			var builder = new DeleteIndexExpressionBuilder(expressionMock.Object);
			builder.OnColumns("BaconId", "EggsId");

			collectionMock.VerifyAll();
			indexMock.VerifyAll();
			expressionMock.VerifyAll();
		}
	}
}