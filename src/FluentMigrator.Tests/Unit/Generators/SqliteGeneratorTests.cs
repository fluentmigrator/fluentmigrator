using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators;
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators
{
	public class SqliteGeneratorTests
	{
		private SqliteGenerator generator;
		string table = "Table";
		string oldColumn = "OldColumn";
		string newColumn = "NewColumn";

		public SqliteGeneratorTests()
		{
			generator = new SqliteGenerator();
		}

		[Fact]
		public void CanCreateTable()
		{
			CreateTableExpression expression = GetCreateTableExpression();
			string sql = generator.Generate(expression);
			Assert.Equal(string.Format("CREATE TABLE {0} (NewColumn NVARCHAR(255))", table), sql);
		}

		[Fact]
		public void CanRenameColumn()
		{
			RenameColumnExpression expression = GetRenameColumnExpression();
			string sql = generator.Generate(expression);
			Assert.Equal(string.Format("UPDATE {0} SET {1}={2}", table, oldColumn, newColumn), sql);
		}

		private RenameColumnExpression GetRenameColumnExpression()
		{
			return new RenameColumnExpression { OldName = oldColumn, NewName = newColumn, TableName = table };
		}

		private CreateTableExpression GetCreateTableExpression()
		{
			CreateTableExpression expression = new CreateTableExpression() { TableName = table, };
			expression.Columns.Add(new ColumnDefinition { Name = "NewColumn", Type = DbType.String });
			return expression;
		}
	}
}