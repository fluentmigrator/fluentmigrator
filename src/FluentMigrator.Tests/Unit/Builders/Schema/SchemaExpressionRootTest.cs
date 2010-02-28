using FluentMigrator.Builders.Schema;
using FluentMigrator.Infrastructure;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Builders.Schema
{
	[TestFixture]
	public class SchemaExpressionRootTest
	{
		private Mock<IQuerySchema> _querySchemaMock;
		private Mock<IMigrationContext> _migrationContextMock;
		private string _testColumn;
		private string _testTable;
		private SchemaExpressionRoot _builder;

		[SetUp]
		public void SetUp()
		{
			_migrationContextMock = new Mock<IMigrationContext>();
			_querySchemaMock = new Mock<IQuerySchema>();
			_testTable = "testTable";
			_testColumn = "testColumn";

			_migrationContextMock.Setup(x => x.QuerySchema).Returns(_querySchemaMock.Object).AtMostOnce();
			_builder = new SchemaExpressionRoot(_migrationContextMock.Object);
		}

		[Test]
		public void TestTableExists()
		{
			_querySchemaMock.Setup(x => x.TableExists(_testTable)).Returns(true).AtMostOnce();

			_builder.Table(_testTable).Exists().ShouldBeTrue();
			_migrationContextMock.VerifyAll();
		}

		[Test]
		public void TestColumnExists()
		{
			_querySchemaMock.Setup(x => x.ColumnExists(_testTable, _testColumn)).Returns(true).AtMostOnce();

			_builder.Table(_testTable).Column(_testColumn).Exists().ShouldBeTrue();
			_migrationContextMock.VerifyAll();
		}
	}
}
