

namespace FluentMigrator.Tests.Unit.Generators
{
    using System.Data;
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;
    using FluentMigrator.Runner.Generators.SqlServer;
    using NUnit.Framework;
    using NUnit.Should;

    public class SqlServer2008GeneratorTests : GeneratorTestBase
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
			sql.ShouldBe( "sp_rename '[dbo].[Table1]', 'Table2'" );
		}

		[Test]
		public void CanCreateTableWithDateTimeOffsetColumn()
		{
			var expression = GetCreateTableExpression();
			expression.Columns[0].Type = DbType.DateTimeOffset;
			var sql = generator.Generate(expression);
			sql.ShouldBe(
				"CREATE TABLE [dbo].[NewTable] (ColumnName1 DATETIMEOFFSET NOT NULL)");
		}
	}
}