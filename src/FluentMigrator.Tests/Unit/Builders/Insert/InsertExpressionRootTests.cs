using System.Collections.Generic;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Builders.Insert
{
	[TestFixture]
	public class InsertExpressionRootTests
	{
		[Test]
		public void CallingIntoTableSetsTableName()
		{
			var collectionMock = new Mock<ICollection<IMigrationExpression>>();
			collectionMock.Setup(x => x.Add(It.Is<InsertDataExpression>(e => e.TableName.Equals("Bacon")))).AtMostOnce();

			var contextMock = new Mock<IMigrationContext>();
			contextMock.SetupGet(x => x.Expressions).Returns(collectionMock.Object).AtMostOnce();

			var root = new InsertExpressionRoot(contextMock.Object);
			root.IntoTable("Bacon");

			collectionMock.VerifyAll();
			contextMock.VerifyAll();
		}
	}
}