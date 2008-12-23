using System.Data;
using System.Data.SQLite;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
using Moq;
using Xunit;

namespace FluentMigrator.Tests.Integration.Processors
{
	public class SqliteProcessorTests
	{
		private SQLiteConnection connection;
		private SqliteProcessor processor;
		private Mock<ColumnDefinition> column;
		private SQLiteCommand command;
		private string columnName;
		private string tableName;

		public SqliteProcessorTests()
		{
			// This connection used in the tests
			connection = new SQLiteConnection { ConnectionString = "Data Source=:memory:;Version=3;New=True;" };
			connection.Open();
			command = connection.CreateCommand();

			// SUT
			processor = new SqliteProcessor(connection, new SqliteGenerator());

			column = new Mock<ColumnDefinition>();
			tableName = "NewTable";
			columnName = "ColumnName";
			column.ExpectGet(c => c.Name).Returns(columnName);
		}

		[Fact]
		public void CanCreateTableExpression()
		{
			CreateTableExpression expression = new CreateTableExpression { TableName = tableName };
			expression.Columns.Add(column.Object);

			using (connection)
			{
				processor.Process(expression);
				command.CommandText = string.Format("SELECT name FROM sqlite_master WHERE type='table' and name='{0}'", tableName);
				Assert.True(command.ExecuteReader().Read());
			}
		}

		public void InsertTable()
		{
			SQLiteCommand cmd = connection.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = string.Format("CREATE TABLE {0} ({1})", tableName, columnName);
			cmd.ExecuteNonQuery();
		}

		/*[Fact]
		public void CanCreateColumnExpression()
		{
			InsertTable();
			CreateColumnExpression expression = new CreateColumnExpression { TableName = tableName, Column = column.Object };

			using (connection)
			{
				processor.Generate(expression);
				command.CommandText = string.Format("SELECT {0} FROM {1}", columnName, expression.TableName);

				Assert.True(command.ExecuteReader().Read());
			}
		}*/
	}
}