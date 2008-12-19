using System;
using System.Collections.Generic;
using FluentMigrator.Builders.Delete.Index;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using Moq;
using Xunit;

namespace FluentMigrator.Tests.Builders.Delete
{
	public class DeleteIndexExpressionBuilderTests
	{
		[Fact]
		public void CallingOnTableSetsTableNameToSpecifiedValue()
		{
			var indexMock = new Mock<IndexDefinition>();
			indexMock.ExpectSet(x => x.TableName, "Bacon").AtMostOnce();

			var expressionMock = new Mock<DeleteIndexExpression>();
			expressionMock.ExpectGet(e => e.Index).Returns(indexMock.Object).AtMostOnce();

			var builder = new DeleteIndexExpressionBuilder(expressionMock.Object);
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

			var expressionMock = new Mock<DeleteIndexExpression>();
			expressionMock.ExpectGet(e => e.Index).Returns(indexMock.Object).AtMostOnce();

			var builder = new DeleteIndexExpressionBuilder(expressionMock.Object);
			builder.OnColumn("BaconId");

			collectionMock.VerifyAll();
			indexMock.VerifyAll();
			expressionMock.VerifyAll();
		}

		[Fact]
		public void CallingOnColumnsAddsMultipleNewColumnsToExpression()
		{
			var collectionMock = new Mock<IList<IndexColumnDefinition>>();
			collectionMock.Expect(x => x.Add(It.Is<IndexColumnDefinition>(c => c.Name.Equals("BaconId")))).AtMostOnce();
			collectionMock.Expect(x => x.Add(It.Is<IndexColumnDefinition>(c => c.Name.Equals("EggsId")))).AtMostOnce();

			var indexMock = new Mock<IndexDefinition>();
			indexMock.ExpectGet(x => x.Columns).Returns(collectionMock.Object).AtMost(2);

			var expressionMock = new Mock<DeleteIndexExpression>();
			expressionMock.ExpectGet(e => e.Index).Returns(indexMock.Object).AtMost(2);

			var builder = new DeleteIndexExpressionBuilder(expressionMock.Object);
			builder.OnColumns("BaconId", "EggsId");

			collectionMock.VerifyAll();
			indexMock.VerifyAll();
			expressionMock.VerifyAll();
		}
	}
}