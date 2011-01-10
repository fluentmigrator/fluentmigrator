using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
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

		[Test]
		public void CanCreateTableWithDateTimeOffsetColumn()
		{
			var expression = GetCreateTableExpression(tableName);
			expression.Columns[0].Type = DbType.DateTimeOffset;
			var sql = generator.Generate(expression);
			sql.ShouldBe(
				"CREATE TABLE [dbo].[NewTable] (ColumnName1 DATETIMEOFFSET NOT NULL)");
		}


		private CreateTableExpression GetCreateTableExpression(string tableName)
		{
			var columnName1 = "ColumnName1";

			var column1 = new ColumnDefinition { Name = columnName1, Type = DbType.String };

			var expression = new CreateTableExpression { TableName = tableName };
			expression.Columns.Add(column1);
			return expression;
		}

		private string tableName = "NewTable";
	}
}