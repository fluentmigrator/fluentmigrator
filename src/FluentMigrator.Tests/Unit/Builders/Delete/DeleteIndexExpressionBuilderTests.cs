using System.Collections.Generic;
using FluentMigrator.Builders.Delete.Index;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Builders.Delete
{
	public class DeleteIndexExpressionBuilderTests
	{
		[Test]
		public void CallingOnTableSetsTableNameToSpecifiedValue()
		{
			var indexMock = new Mock<IndexDefinition>();
			indexMock.SetupSet(x => x.TableName = "Bacon").AtMostOnce();

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
			collectionMock.Setup(x => x.Add(It.Is<IndexColumnDefinition>(c => c.Name.Equals("BaconId")))).AtMostOnce();

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
			collectionMock.Setup(x => x.Add(It.Is<IndexColumnDefinition>(c => c.Name.Equals("BaconId")))).AtMostOnce();
			collectionMock.Setup(x => x.Add(It.Is<IndexColumnDefinition>(c => c.Name.Equals("EggsId")))).AtMostOnce();

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