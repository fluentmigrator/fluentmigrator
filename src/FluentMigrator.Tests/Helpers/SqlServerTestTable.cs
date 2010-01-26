using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using FluentMigrator.Runner.Processors.SqlServer;

namespace FluentMigrator.Tests.Helpers
{
	public class SqlServerTestTable : IDisposable
	{
		public SqlConnection Connection { get; set; }
		public string Name { get; set; }
		protected SqlTransaction Transaction { get; set; }

		public SqlServerTestTable(SqlServerProcessor processor, params string[] columnDefinitions)
		{
			Connection = processor.Connection;
			Transaction = processor.Transaction;

			Name = "Table" + Guid.NewGuid().ToString("N");
			Create(columnDefinitions);
		}

		public void Dispose()
		{
			Drop();
		}

		public void Create(IEnumerable<string> columnDefinitions)
		{
			var sb = new StringBuilder();

			sb.Append("CREATE TABLE ");
			sb.Append(Name);

			foreach (string definition in columnDefinitions)
			{
				sb.Append("(");
				sb.Append(definition);
				sb.Append("), ");
			}

			sb.Remove(sb.Length - 2, 2);

			using (var command = new SqlCommand(sb.ToString(), Connection, Transaction))
				command.ExecuteNonQuery();
		}

		public void Drop()
		{
			using (var command = new SqlCommand("DROP TABLE " + Name, Connection, Transaction))
				command.ExecuteNonQuery();
		}
	}
}