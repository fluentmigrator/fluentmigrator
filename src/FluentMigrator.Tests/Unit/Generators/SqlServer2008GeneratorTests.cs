using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators
{
	[TestFixture]
	public class SqlServer2008GeneratorTests
	{
		private SqlServer2008Generator generator;

		[SetUp]
		public void SetUp()
		{
			generator = new SqlServer2008Generator();
		}

		[Test]
		public void CanRenameTable()
		{
			var expression = new RenameTableExpression();
			expression.SchemaName = "dbo";
			expression.OldName = "Table1";
			expression.NewName = "Table2";

			var sql = generator.Generate( expression );
			sql.ShouldBe( "sp_rename '[dbo].[Table1]', '[Table2]'" );
		}
	}
}