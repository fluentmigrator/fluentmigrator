using System;
using System.Data;
using System.Data.SQLite;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Processors
{
	public class SqliteProcessor : IMigrationProcessor
	{
		public SQLiteConnection Connection { get; set; }

		public SqliteProcessor(SQLiteConnection connection)
		{
			Connection = connection;
		}

		private string JoinColumns(CreateTableExpression expression)
		{
			string columns = "";
			int total = expression.Columns.Count - 1;
			for (int i = 0; i < expression.Columns.Count; i++)
			{
				columns += expression.Columns[i];

				if (i != total)
					columns += ", ";
			}

			return columns;
		}

		public void Process(CreateTableExpression expression)
		{
			SQLiteCommand cmd = Connection.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = string.Format("CREATE TABLE {0} ({1})", expression.TableName, JoinColumns(expression));
			cmd.ExecuteNonQuery();
		}

		public void Process(CreateColumnExpression expression)
		{
			SQLiteCommand cmd = Connection.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = string.Format("ALTER TABLE {0} ADD COLUMN {1}", expression.TableName, expression.Column.Name);
			cmd.ExecuteNonQuery();
		}

		public void Process(RenameColumnExpression expression)
		{
			throw new NotImplementedException();
		}

		public void Process(DeleteTableExpression expression)
		{
			throw new NotImplementedException();
		}

		public void Process(DeleteColumnExpression expression)
		{
			throw new NotImplementedException();
		}

		public void Process(CreateForeignKeyExpression expression)
		{
			throw new NotImplementedException();
		}

		public void Process(DeleteForeignKeyExpression expression)
		{
			throw new NotImplementedException();
		}

		public void Process(DeleteIndexExpression expression)
		{
			throw new NotImplementedException();
		}

		public void Process(RenameTableExpression expression)
		{
			throw new NotImplementedException();
		}

		public void Process(CreateIndexExpression expression)
		{
			throw new NotImplementedException();
		}
	}
}