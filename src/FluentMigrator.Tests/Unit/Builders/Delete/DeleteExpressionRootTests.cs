using System;
using System.Collections.Generic;
using FluentMigrator.Builders.Delete;
using FluentMigrator.Builders.Delete.Column;
using FluentMigrator.Builders.Delete.ForeignKey;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using Moq;
using Xunit;

namespace FluentMigrator.Tests.Unit.Builders.Delete
{
	public class DeleteExpressionRootTests
	{
		[Fact]
		public void CallingTableAddsDeleteTableExpressionToContextWithSpecifiedName()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Expect(x => x.Add(It.Is<DeleteTableExpression>(e => e.TableName.Equals("Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new DeleteExpressionRoot(contextMock.Object);
			root.Table("Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Fact]
		public void CallingColumnAddsDeleteColumnExpressionToContextWithSpecifiedName()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Expect(x => x.Add(It.Is<DeleteColumnExpression>(e => e.ColumnName.Equals("Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new DeleteExpressionRoot(contextMock.Object);
			root.Column("Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Fact]
		public void CallingColumnReturnsDeleteColumnExpressionBuilder()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new DeleteExpressionRoot(contextMock.Object);
			var builder = root.Column("Bacon");

			Assert.IsType<DeleteColumnExpressionBuilder>(builder);
			contextMock.VerifyAll();
		}

		[Fact]
		public void CallingForeignKeyAddsDeleteForeignKeyExpressionToContext()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Expect(x => x.Add(It.IsAny<DeleteForeignKeyExpression>())).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new DeleteExpressionRoot(contextMock.Object);
			root.ForeignKey();

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Fact]
		public void CallingForeignKeyReturnsDeleteForeignKeyExpressionBuilder()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new DeleteExpressionRoot(contextMock.Object);
			var builder = root.ForeignKey();

			Assert.IsType<DeleteForeignKeyExpressionBuilder>(builder);
			contextMock.VerifyAll();
		}

		[Fact]
		public void CallingForeignKeyWithNameAddsDeleteForeignKeyExpressionToContextWithSpecifiedName()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Expect(x => x.Add(It.Is<DeleteForeignKeyExpression>(e => e.ForeignKey.Name.Equals("FK_Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new DeleteExpressionRoot(contextMock.Object);
			root.ForeignKey("FK_Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}
	}
}