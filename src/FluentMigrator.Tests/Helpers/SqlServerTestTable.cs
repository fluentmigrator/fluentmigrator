using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace FluentMigrator.Tests.Helpers
{
	public class SqlServerTestTable : IDisposable
	{
		public SqlConnection Connection { get; set; }
		public string Name { get; set; }

		public SqlServerTestTable(SqlConnection connection, params string[] columnDefinitions)
		{
			Connection = connection;
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

			using (var command = new SqlCommand(sb.ToString(), Connection))
				command.ExecuteNonQuery();
		}

		public void Drop()
		{
			using (var command = new SqlCommand("DROP TABLE " + Name, Connection))
				command.ExecuteNonQuery();
		}
	}
}