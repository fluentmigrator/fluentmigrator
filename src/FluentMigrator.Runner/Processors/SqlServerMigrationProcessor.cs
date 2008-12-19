using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Processors
{
	public class SqlServerMigrationProcessor : IMigrationProcessor
	{
		public virtual SqlConnection Connection { get; set; }

		public SqlServerMigrationProcessor(SqlConnection connection)
		{
			Connection = connection;
		}

		public virtual void Process(CreateTableExpression expression)
		{
			throw new NotImplementedException();
		}

		public virtual void Process(CreateColumnExpression expression)
		{
			throw new NotImplementedException();
		}

		public virtual void Process(DeleteTableExpression expression)
		{
			throw new NotImplementedException();
		}

		public virtual void Process(DeleteColumnExpression expression)
		{
			throw new NotImplementedException();
		}

		public virtual void Process(CreateForeignKeyExpression expression)
		{
			throw new NotImplementedException();
		}

		public virtual void Process(DeleteForeignKeyExpression expression)
		{
			throw new NotImplementedException();
		}

		public virtual void Process(CreateIndexExpression expression)
		{
			throw new NotImplementedException();
		}

		public virtual void Process(DeleteIndexExpression expression)
		{
			throw new NotImplementedException();
		}

		public virtual void Process(RenameTableExpression expression)
		{
			throw new NotImplementedException();
		}

		public virtual void Process(RenameColumnExpression expression)
		{
			throw new NotImplementedException();
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
			const string template =
				"SELECT * FROM "
			+ "(SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_CATALOG = DB_NAME() AND TABLE_NAME = '{0}') "
			+ "WHERE CONSTRAINT_NAME = '{1}'";

			return Exists(template, tableName, constraintName);
		}

		public void ExecuteNonQuery(string template, params object[] args)
		{
			using (var command = new SqlCommand(String.Format(template, args), Connection))
				command.ExecuteNonQuery();
		}

		public bool Exists(string template, params object[] args)
		{
			using (var command = new SqlCommand(String.Format(template, args), Connection))
			using (var reader = command.ExecuteReader())
			{
				return reader.Read();
			}
		}
	}
}