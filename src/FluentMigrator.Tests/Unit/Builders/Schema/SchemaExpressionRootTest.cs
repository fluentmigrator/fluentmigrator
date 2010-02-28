using System;
using System.Collections;
using FluentMigrator.Builders.Schema;
using FluentMigrator.Infrastructure;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Builders.Schema
{
	[TestFixture]
	public class SchemaExpressionRootTest
	{

		[Test]
		public void TestTableExists()
		{
			var migrationContextMock = new Mock<IMigrationContext>();
			var querySchemaMock = new Mock<IQuerySchema>();
			const string testTable = "testTable";
			querySchemaMock.Setup(x => x.TableExists(testTable)).Returns(true).AtMostOnce();
			migrationContextMock.Setup(x => x.QuerySchema).Returns(querySchemaMock.Object).AtMostOnce();

			var builder = new SchemaExpressionRoot(migrationContextMock.Object);
			Assert.IsTrue(builder.Table(testTable).Exists());
			migrationContextMock.VerifyAll();
		}

		[Test]
		public void TestColumnExists()
		{
			var migrationContextMock = new Mock<IMigrationContext>();
			var querySchemaMock = new Mock<IQuerySchema>();
			const string testTable = "testTable";
			const string testColumn = "testColumn";
			querySchemaMock.Setup(x => x.ColumnExists(testTable,testColumn)).Returns(true).AtMostOnce();
			migrationContextMock.Setup(x => x.QuerySchema).Returns(querySchemaMock.Object).AtMostOnce();

			var builder = new SchemaExpressionRoot(migrationContextMock.Object);
			Assert.IsTrue(builder.Table(testTable).Column(testColumn).Exists());
			migrationContextMock.VerifyAll();
		}


	}
}
