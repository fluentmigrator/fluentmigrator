using System;
using System.Collections.Generic;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Create.Column;
using FluentMigrator.Builders.Create.ForeignKey;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Builders.Create
{
	public class CreateExpressionRootTests
	{
		[Test]
		public void CallingTableAddsCreateTableExpressionToContextWithSpecifiedNameSet()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Setup(x => x.Add(It.Is<CreateTableExpression>(e => e.TableName.Equals("Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			root.Table("Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingTableReturnsCreateTableExpressionBuilder()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			var builder = root.Table("Bacon");

			builder.ShouldBeOfType<CreateTableExpressionBuilder>();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingColumnAddsCreateColumnExpressionToContextWithSpecifiedNameSet()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Setup(x => x.Add(It.Is<CreateColumnExpression>(e => e.Column.Name.Equals("Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			root.Column("Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingColumnReturnsCreateColumnExpression()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			var builder = root.Column("Bacon");

			builder.ShouldBeOfType<CreateColumnExpressionBuilder>();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingForeignKeyWithoutNameAddsCreateForeignKeyExpressionToContext()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Setup(x => x.Add(It.IsAny<CreateForeignKeyExpression>())).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			root.ForeignKey();

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingForeignKeyAddsCreateForeignKeyExpressionToContextWithSpecifiedNameSet()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Setup(x => x.Add(It.Is<CreateForeignKeyExpression>(e => e.ForeignKey.Name.Equals("FK_Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			root.ForeignKey("FK_Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingForeignKeyWithoutNameReturnsCreateForeignKeyExpression()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			var builder = root.ForeignKey();

			builder.ShouldBeOfType<CreateForeignKeyExpressionBuilder>();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingForeignKeyCreatesCreateForeignKeyExpression()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			var builder = root.ForeignKey("FK_Bacon");

			builder.ShouldBeOfType<CreateForeignKeyExpressionBuilder>();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingIndexWithoutNameAddsCreateIndexExpressionToContext()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Setup(x => x.Add(It.IsAny<CreateIndexExpression>())).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			root.Index();

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingIndexAddsCreateIndexExpressionToContextWithSpecifiedNameSet()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Setup(x => x.Add(It.Is<CreateIndexExpression>(e => e.Index.Name.Equals("IX_Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			root.Index("IX_Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingIndexWithoutNameReturnsCreateIndexExpression()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			var builder = root.Index();

			builder.ShouldBeOfType<CreateIndexExpressionBuilder>();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingIndexCreatesCreateIndexExpression()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			var builder = root.Index("IX_Bacon");

			builder.ShouldBeOfType<CreateIndexExpressionBuilder>();
			contextMock.VerifyAll();
		}
	}
}