using System;
using System.Data.SqlClient;

namespace FluentMigrator.Runner.Processors
{
	public class SqlServerProcessor: ProcessorBase
	{
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

		protected override void Process(string sql)
		{
			using (var command = new SqlCommand(sql, Connection))
				command.ExecuteNonQuery();
		}
	}
}