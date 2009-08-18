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
using Xunit;

namespace FluentMigrator.Tests.Unit.Builders.Create
{
	public class CreateExpressionRootTests
	{
		[Fact]
		public void CallingTableAddsCreateTableExpressionToContextWithSpecifiedNameSet()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Expect(x => x.Add(It.Is<CreateTableExpression>(e => e.TableName.Equals("Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			root.Table("Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Fact]
		public void CallingTableReturnsCreateTableExpressionBuilder()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			var builder = root.Table("Bacon");

			Assert.IsType<CreateTableExpressionBuilder>(builder);
			contextMock.VerifyAll();
		}

		[Fact]
		public void CallingColumnAddsCreateColumnExpressionToContextWithSpecifiedNameSet()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Expect(x => x.Add(It.Is<CreateColumnExpression>(e => e.Column.Name.Equals("Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			root.Column("Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Fact]
		public void CallingColumnReturnsCreateColumnExpression()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			var builder = root.Column("Bacon");

			Assert.IsType<CreateColumnExpressionBuilder>(builder);
			contextMock.VerifyAll();
		}

		[Fact]
		public void CallingForeignKeyWithoutNameAddsCreateForeignKeyExpressionToContext()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Expect(x => x.Add(It.IsAny<CreateForeignKeyExpression>())).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			root.ForeignKey();

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Fact]
		public void CallingForeignKeyAddsCreateForeignKeyExpressionToContextWithSpecifiedNameSet()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Expect(x => x.Add(It.Is<CreateForeignKeyExpression>(e => e.ForeignKey.Name.Equals("FK_Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			root.ForeignKey("FK_Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Fact]
		public void CallingForeignKeyWithoutNameReturnsCreateForeignKeyExpression()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			var builder = root.ForeignKey();

			Assert.IsType<CreateForeignKeyExpressionBuilder>(builder);
			contextMock.VerifyAll();
		}

		[Fact]
		public void CallingForeignKeyCreatesCreateForeignKeyExpression()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			var builder = root.ForeignKey("FK_Bacon");

			Assert.IsType<CreateForeignKeyExpressionBuilder>(builder);
			contextMock.VerifyAll();
		}

		[Fact]
		public void CallingIndexWithoutNameAddsCreateIndexExpressionToContext()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Expect(x => x.Add(It.IsAny<CreateIndexExpression>())).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			root.Index();

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Fact]
		public void CallingIndexAddsCreateIndexExpressionToContextWithSpecifiedNameSet()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Expect(x => x.Add(It.Is<CreateIndexExpression>(e => e.Index.Name.Equals("IX_Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			root.Index("IX_Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Fact]
		public void CallingIndexWithoutNameReturnsCreateIndexExpression()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			var builder = root.Index();

			Assert.IsType<CreateIndexExpressionBuilder>(builder);
			contextMock.VerifyAll();
		}

		[Fact]
		public void CallingIndexCreatesCreateIndexExpression()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new CreateExpressionRoot(contextMock.Object);
			var builder = root.Index("IX_Bacon");

			Assert.IsType<CreateIndexExpressionBuilder>(builder);
			contextMock.VerifyAll();
		}
	}
}