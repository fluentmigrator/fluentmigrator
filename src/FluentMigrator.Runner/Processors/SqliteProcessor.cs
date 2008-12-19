using System.Data.SQLite;
using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Processors
{
	public class SqliteProcessor: IMigrationProcessor
	{
		private readonly IMigrationGenerator generator;

		public SQLiteConnection Connection { get; set; }

		public SqliteProcessor(SQLiteConnection connection, IMigrationGenerator generator)
		{
			this.generator = generator;
			Connection = connection;
		}

		public void Process(CreateTableExpression expression)
		{
			Process(generator.Generate(expression));
		}

		public void Process(CreateColumnExpression expression)
		{
			Process(generator.Generate(expression));
		}

		public void Process(DeleteTableExpression expression)
		{
			Process(generator.Generate(expression));
		}

		public void Process(DeleteColumnExpression expression)
		{
			Process(generator.Generate(expression));
		}

		public void Process(CreateForeignKeyExpression expression)
		{
			Process(generator.Generate(expression));
		}

		public void Process(DeleteForeignKeyExpression expression)
		{
			Process(generator.Generate(expression));
		}

		public void Process(CreateIndexExpression expression)
		{
			Process(generator.Generate(expression));
		}

		public void Process(DeleteIndexExpression expression)
		{
			Process(generator.Generate(expression));
		}

		public void Process(RenameTableExpression expression)
		{
			Process(generator.Generate(expression));
		}

		public void Process(RenameColumnExpression expression)
		{
			Process(generator.Generate(expression));
		}

		private void Process(string sql)
		{
			using (var command = new SQLiteCommand(sql, Connection))
				command.ExecuteNonQuery();
		}
	}
}