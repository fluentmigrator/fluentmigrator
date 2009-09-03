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
			indexMock.SetupSet(x => x.TableName = "Bacon").AtMostOnce();

			var expressionMock = new Mock<CreateIndexExpression>();
			expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object).AtMostOnce();

			var builder = new CreateIndexExpressionBuilder(expressionMock.Object);
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

			var expressionMock = new Mock<CreateIndexExpression>();
			expressionMock.SetupGet(e => e.Index).Returns(indexMock.Object).AtMostOnce();

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
			columnMock.SetupSet(c => c.Direction = Direction.Ascending).AtMostOnce();
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
			columnMock.SetupSet(c => c.Direction = Direction.Descending).AtMostOnce();
			var expressionMock = new Mock<CreateIndexExpression>();

			var builder = new CreateIndexExpressionBuilder(expressionMock.Object);
			builder.CurrentColumn = columnMock.Object;

			builder.Descending();

			columnMock.VerifyAll();
		}
	}
}