using System.Collections.Generic;
using FluentMigrator.Builders.Delete;
using FluentMigrator.Builders.Delete.Column;
using FluentMigrator.Builders.Delete.ForeignKey;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Builders.Delete
{
	public class DeleteExpressionRootTests
	{
		[Test]
		public void CallingTableAddsDeleteTableExpressionToContextWithSpecifiedName()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Setup(x => x.Add(It.Is<DeleteTableExpression>(e => e.TableName.Equals("Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new DeleteExpressionRoot(contextMock.Object);
			root.Table("Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingColumnAddsDeleteColumnExpressionToContextWithSpecifiedName()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Setup(x => x.Add(It.Is<DeleteColumnExpression>(e => e.ColumnName.Equals("Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new DeleteExpressionRoot(contextMock.Object);
			root.Column("Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingColumnReturnsDeleteColumnExpressionBuilder()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new DeleteExpressionRoot(contextMock.Object);
			var builder = root.Column("Bacon");

			builder.ShouldBeOfType<DeleteColumnExpressionBuilder>();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingForeignKeyAddsDeleteForeignKeyExpressionToContext()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Setup(x => x.Add(It.IsAny<DeleteForeignKeyExpression>())).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new DeleteExpressionRoot(contextMock.Object);
			root.ForeignKey();

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingForeignKeyReturnsDeleteForeignKeyExpressionBuilder()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new DeleteExpressionRoot(contextMock.Object);
			var builder = root.ForeignKey();

			builder.ShouldBeOfType<DeleteForeignKeyExpressionBuilder>();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingForeignKeyWithNameAddsDeleteForeignKeyExpressionToContextWithSpecifiedName()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Setup(x => x.Add(It.Is<DeleteForeignKeyExpression>(e => e.ForeignKey.Name.Equals("FK_Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new DeleteExpressionRoot(contextMock.Object);
			root.ForeignKey("FK_Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}
	}
}