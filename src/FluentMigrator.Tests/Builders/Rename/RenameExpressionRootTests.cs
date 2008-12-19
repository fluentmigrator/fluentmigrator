using System;
using System.Collections.Generic;
using FluentMigrator.Builders.Rename;
using FluentMigrator.Builders.Rename.Column;
using FluentMigrator.Builders.Rename.Table;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using Moq;
using Xunit;

namespace FluentMigrator.Tests.Builders.Rename
{
	public class RenameExpressionRootTests
	{
		[Fact]
		public void CallingTableAddsRenameTableExpressionToContextWithSpecifiedOldName()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Expect(x => x.Add(It.Is<RenameTableExpression>(e => e.OldName.Equals("Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new RenameExpressionRoot(contextMock.Object);
			root.Table("Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Fact]
		public void CallingTableReturnsRenameTableExpressionBuilder()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new RenameExpressionRoot(contextMock.Object);
			var builder = root.Table("Bacon");

			Assert.IsType<RenameTableExpressionBuilder>(builder);
			contextMock.VerifyAll();
		}

		[Fact]
		public void CallingColumnAddsRenameColumnExpressionToContextWithSpecifiedOldName()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Expect(x => x.Add(It.Is<RenameColumnExpression>(e => e.OldName.Equals("Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new RenameExpressionRoot(contextMock.Object);
			root.Column("Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Fact]
		public void CallingColumnReturnsRenameColumnExpressionBuilder()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.ExpectGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new RenameExpressionRoot(contextMock.Object);
			var builder = root.Column("Bacon");

			Assert.IsType<RenameColumnExpressionBuilder>(builder);
			contextMock.VerifyAll();
		}
	}
}