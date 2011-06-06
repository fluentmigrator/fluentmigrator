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
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Builders.Create
{
	[TestFixture]
	public class CreateIndexExpressionBuilderTests
	{
		[Test]
		public void CallingOnTableSetsTableNameToSpecifiedValue()
		{
			var indexMock = new Mock<IndexDefinition>();
			indexMock.VerifySet(x => x.TableName = "Bacon", Times.AtMostOnce());

			var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);
            expressionMock.VerifyGet(e => e.Index, Times.AtMostOnce());

			var builder = new CreateIndexExpressionBuilder(expressionMock.Object);
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
            indexMock.Setup(x => x.Columns).Returns(collectionMock.Object);
			indexMock.VerifyGet(x => x.Columns, Times.AtMostOnce());

			var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object);
            expressionMock.VerifyGet(e => e.Index, Times.AtMostOnce());

			var builder = new CreateIndexExpressionBuilder(expressionMock.Object);
			builder.OnColumn("BaconId");

			collectionMock.VerifyAll();
			indexMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Test]
		public void CallingAscendingSetsDirectionToAscending()
		{
			var columnMock = new Mock<IndexColumnDefinition>();
            columnMock.VerifySet(c => c.Direction = Direction.Ascending, Times.AtMostOnce());
			var expressionMock = new Mock<CreateIndexExpression>();

			var builder = new CreateIndexExpressionBuilder(expressionMock.Object);
			builder.CurrentColumn = columnMock.Object;

			builder.Ascending();

			columnMock.VerifyAll();
		}

		[Test]
		public void CallingDescendingSetsDirectionToDescending()
		{
			var columnMock = new Mock<IndexColumnDefinition>();
            columnMock.VerifySet(c => c.Direction = Direction.Descending, Times.AtMostOnce());
			var expressionMock = new Mock<CreateIndexExpression>();

			var builder = new CreateIndexExpressionBuilder(expressionMock.Object);
			builder.CurrentColumn = columnMock.Object;

			builder.Descending();

			columnMock.VerifyAll();
		}
	}
}