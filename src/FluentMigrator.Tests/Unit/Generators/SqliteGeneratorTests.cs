using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators;
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators
{
	public class SqliteGeneratorTests
	{
		SqliteGenerator generator;
		string table = "Table";
		private string oldTable = "OldTable";
		string newTable = "NewTable";

		string column = "Column";
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
		public void CanRenameTable()
		{
			RenameTableExpression expression = new RenameTableExpression { OldName = oldTable, NewName = newTable };
			string sql = generator.Generate(expression);
			Assert.Equal(string.Format("ALTER TABLE {0} RENAME TO {1}", oldTable, newTable), sql);
		}

		[Fact]
		public void CanDeleteTable()
		{
			DeleteTableExpression expression = new DeleteTableExpression { TableName = table };
			string sql = generator.Generate(expression);
			Assert.Equal(string.Format("DROP TABLE {0}", table), sql);
		}

		[Fact]
		public void CanCreateColumn()
		{
			CreateColumnExpression expression = GetCreateColumnExpression();
			string sql = generator.Generate(expression);
			Assert.Equal(string.Format("ALTER TABLE {0} ADD COLUMN {1}", table, newColumn), sql);
		}

		[Fact]
		public void CanRenameColumn()
		{
			RenameColumnExpression expression = GetRenameColumnExpression();
			string sql = generator.Generate(expression);
			Assert.Equal(string.Format("UPDATE {0} SET {1}={2}", table, oldColumn, newColumn), sql);
		}

		[Fact]
		public void CanDeleteColumn()
		{
			DeleteColumnExpression expression = new DeleteColumnExpression { TableName = table, ColumnName = column };
			string sql = generator.Generate(expression);
			Assert.Equal(string.Format("ALTER TABLE {0} DROP COLUMN {1}", table, column), sql);
		}

		// CreateForeignKey -- Not supported in Sqlite
		// DeleteForeignKey -- Not supported in Sqlite

		// CreateIndex
		[Fact]
		public void CanCreateIndex()
		{
//			ColumnDefinition 
//			CreateIndexExpression expression = new CreateIndexExpression { new IndexDefinition {}};
		}

		// DeleteIndex

		private RenameColumnExpression GetRenameColumnExpression()
		{
			return new RenameColumnExpression { OldName = oldColumn, NewName = newColumn, TableName = table };
		}

		private CreateColumnExpression GetCreateColumnExpression()
		{
			ColumnDefinition column = new ColumnDefinition { Name = newColumn };
			return new CreateColumnExpression { TableName = table, Column = column };
		}

		private CreateTableExpression GetCreateTableExpression()
		{
			CreateTableExpression expression = new CreateTableExpression() { TableName = table, };
			expression.Columns.Add(new ColumnDefinition { Name = "NewColumn", Type = DbType.String });
			return expression;
		}
	}
}