using System.Collections.Generic;
using FluentMigrator.Builders.Rename;
using FluentMigrator.Builders.Rename.Column;
using FluentMigrator.Builders.Rename.Table;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Builders.Rename
{
	public class RenameExpressionRootTests
	{
		[Test]
		public void CallingTableAddsRenameTableExpressionToContextWithSpecifiedOldName()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Setup(x => x.Add(It.Is<RenameTableExpression>(e => e.OldName.Equals("Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new RenameExpressionRoot(contextMock.Object);
			root.Table("Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingTableReturnsRenameTableExpressionBuilder()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new RenameExpressionRoot(contextMock.Object);
			var builder = root.Table("Bacon");

			builder.ShouldBeOfType<RenameTableExpressionBuilder>();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingColumnAddsRenameColumnExpressionToContextWithSpecifiedOldName()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Setup(x => x.Add(It.Is<RenameColumnExpression>(e => e.OldName.Equals("Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new RenameExpressionRoot(contextMock.Object);
			root.Column("Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}

		[Test]
		public void CallingColumnReturnsRenameColumnExpressionBuilder()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new RenameExpressionRoot(contextMock.Object);
			var builder = root.Column("Bacon");

			builder.ShouldBeOfType<RenameColumnExpressionBuilder>();
			contextMock.VerifyAll();
		}
	}
}