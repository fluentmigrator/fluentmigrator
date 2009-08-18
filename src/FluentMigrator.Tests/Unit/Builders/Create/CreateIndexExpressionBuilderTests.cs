using System;
using System.Collections.Generic;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using Moq;
using Xunit;

namespace FluentMigrator.Tests.Unit.Builders.Create
{
	public class CreateIndexExpressionBuilderTests
	{
		[Fact]
		public void CallingOnTableSetsTableNameToSpecifiedValue()
		{
			var indexMock = new Mock<IndexDefinition>();
			indexMock.ExpectSet(x => x.TableName, "Bacon").AtMostOnce();

			var expressionMock = new Mock<CreateIndexExpression>();
			expressionMock.ExpectGet(e => e.Index).Returns(indexMock.Object).AtMostOnce();

			var builder = new CreateIndexExpressionBuilder(expressionMock.Object);
			builder.OnTable("Bacon");

			indexMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Fact]
		public void CallingOnColumnAddsNewColumnToExpression()
		{
			var collectionMock = new Mock<IList<IndexColumnDefinition>>();
			collectionMock.Expect(x => x.Add(It.Is<IndexColumnDefinition>(c => c.Name.Equals("BaconId")))).AtMostOnce();

			var indexMock = new Mock<IndexDefinition>();
			indexMock.ExpectGet(x => x.Columns).Returns(collectionMock.Object).AtMostOnce();

			var expressionMock = new Mock<CreateIndexExpression>();
			expressionMock.ExpectGet(e => e.Index).Returns(indexMock.Object).AtMostOnce();

			var builder = new CreateIndexExpressionBuilder(expressionMock.Object);
			builder.OnColumn("BaconId");

			collectionMock.VerifyAll();
			indexMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Fact]
		public void CallingAscendingSetsDirectionToAscending()
		{
			var columnMock = new Mock<IndexColumnDefinition>();
			columnMock.ExpectSet(c => c.Direction, Direction.Ascending).AtMostOnce();
			var expressionMock = new Mock<CreateIndexExpression>();

			var builder = new CreateIndexExpressionBuilder(expressionMock.Object);
			builder.CurrentColumn = columnMock.Object;

			builder.Ascending();

			columnMock.VerifyAll();
		}

		[Fact]
		public void CallingDescendingSetsDirectionToDescending()
		{
			var columnMock = new Mock<IndexColumnDefinition>();
			columnMock.ExpectSet(c => c.Direction, Direction.Descending).AtMostOnce();
			var expressionMock = new Mock<CreateIndexExpression>();

			var builder = new CreateIndexExpressionBuilder(expressionMock.Object);
			builder.CurrentColumn = columnMock.Object;

			builder.Descending();

			columnMock.VerifyAll();
		}
	}
}