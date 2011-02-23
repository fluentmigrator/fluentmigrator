using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using FluentMigrator.Runner.Processors.Postgres;
using Npgsql;

namespace FluentMigrator.Tests.Helpers
{
    public class PostgresTestTable : IDisposable
    {
        public NpgsqlConnection Connection { get; private set; }
        public string Name { get; set; }
        public NpgsqlTransaction Transaction { get; private set; }

        public PostgresTestTable(PostgresProcessor processor, params string[] columnDefinitions)
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

            sb.Append("CREATE TABLE \"");
            sb.Append(Name);
            sb.Append("\" (");

            foreach (string definition in columnDefinitions)
            {
                sb.Append(definition);
                sb.Append(", ");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append(")");

            var s = sb.ToString();
            using (var command = new NpgsqlCommand(s, Connection, Transaction))
                command.ExecuteNonQuery();
        }

        public void Drop()
        {
            using (var command = new NpgsqlCommand(string.Format("DROP TABLE \"{0}\"", Name), Connection, Transaction))
                command.ExecuteNonQuery();
        }
    }
}