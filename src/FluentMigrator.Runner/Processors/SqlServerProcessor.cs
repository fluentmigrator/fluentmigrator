using System;
using System.Data.SqlClient;
using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Processors
{
	public class SqlServerProcessor: IMigrationProcessor
	{
		private readonly IMigrationGenerator generator;
		public virtual SqlConnection Connection { get; set; }

		public SqlServerProcessor(SqlConnection connection, IMigrationGenerator generator)
		{
			this.generator = generator;
			Connection = connection;
		}

		public bool TableExists(string tableName)
		{
			return Exists("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", tableName);
		}

		public bool ColumnExists(string tableName, string columnName)
		{
			return Exists("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'", tableName, columnName);
		}

		public bool ConstraintExists(string tableName, string constraintName)
		{
			return Exists("SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_CATALOG = DB_NAME() AND TABLE_NAME = '{0}' AND CONSTRAINT_NAME = '{1}'", tableName, constraintName);
		}
        
		public bool Exists(string template, params object[] args)
		{
			using (var command = new SqlCommand(String.Format(template, args), Connection))
			using (var reader = command.ExecuteReader())
			{
				return reader.Read();
			}
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
			using (var command = new SqlCommand(sql, Connection))
				command.ExecuteNonQuery();
		}
	}
}