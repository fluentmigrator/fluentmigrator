using System;
using System.Collections.Generic;
using System.Text;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors.Postgres;
using Npgsql;

namespace FluentMigrator.Tests.Helpers
{
    public class PostgresTestTable : IDisposable
    {
        private readonly PostgresQuoter _quoter = new PostgresQuoter(new PostgresOptions());
        private readonly string _schemaName;
        public NpgsqlConnection Connection { get; private set; }
        public string Name { get; set; }
        public string NameWithSchema { get; set; }
        public NpgsqlTransaction Transaction { get; private set; }

        public PostgresTestTable(PostgresProcessor processor, string schemaName, params string[] columnDefinitions)
        {
            _schemaName = schemaName;
            Name = "Table" + Guid.NewGuid().ToString("N");
            Init(processor, columnDefinitions);

        }

        public PostgresTestTable(string tableName, PostgresProcessor processor, string schemaName, params string[] columnDefinitions)
        {
            _schemaName = schemaName;

            Name = _quoter.UnQuote(tableName);
            Init(processor, columnDefinitions);
        }

        private void Init(PostgresProcessor processor, IEnumerable<string> columnDefinitions)
        {
            Connection = (NpgsqlConnection)processor.Connection;
            Transaction = (NpgsqlTransaction)processor.Transaction;

            NameWithSchema = _quoter.QuoteTableName(Name, _schemaName);
            Create(columnDefinitions);
        }

        public void Dispose()
        {
            Drop();
        }

        public void Create(IEnumerable<string> columnDefinitions)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(_schemaName))
                sb.AppendFormat("CREATE SCHEMA \"{0}\";", _schemaName);

            sb.Append("CREATE TABLE ");

            sb.Append(NameWithSchema);
            sb.Append(" (");

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
            var sb = new StringBuilder();
            sb.AppendFormat("DROP TABLE {0}", NameWithSchema);
            if (!string.IsNullOrEmpty(_schemaName))
                sb.AppendFormat(";DROP SCHEMA \"{0}\"", _schemaName);

            using (var command = new NpgsqlCommand(sb.ToString(), Connection, Transaction))
                command.ExecuteNonQuery();
        }

        public void WithDefaultValueOn(string column)
        {
            const int defaultValue = 1;
            using (var command = new NpgsqlCommand(string.Format(" ALTER TABLE {0} ALTER {1} SET DEFAULT {2}", _quoter.QuoteTableName(Name, _schemaName), _quoter.QuoteColumnName(column), defaultValue), Connection, Transaction))
                command.ExecuteNonQuery();
        }
    }
}
