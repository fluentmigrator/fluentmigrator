using System;
using System.Collections.Generic;
using System.Text;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors.DotConnectPostgres;
using Devart.Data.PostgreSql;

namespace FluentMigrator.Tests.Helpers
{
    public class DotConnectPostgresTestTable : IDisposable
    {
        private readonly PostgresQuoter quoter = new PostgresQuoter();
        private readonly string _schemaName;
        public PgSqlConnection Connection { get; private set; }
        public string Name { get; set; }
        public string NameWithSchema { get; set; }
        public PgSqlTransaction Transaction { get; private set; }

        public DotConnectPostgresTestTable(DotConnectPostgresProcessor processor, string schemaName, params string[] columnDefinitions)
        {
            _schemaName = schemaName;
            Name = "\"Table" + Guid.NewGuid().ToString("N") + "\"";
            Init(processor, columnDefinitions);

        }

        public DotConnectPostgresTestTable(string tableName, DotConnectPostgresProcessor processor, string schemaName, params string[] columnDefinitions)
        {
            _schemaName = schemaName;

            Name = quoter.QuoteTableName(tableName);
            Init(processor, columnDefinitions);
        }

        private void Init(DotConnectPostgresProcessor processor, IEnumerable<string> columnDefinitions)
        {
            Connection = (PgSqlConnection)processor.Connection;
            Transaction = (PgSqlTransaction)processor.Transaction;

            NameWithSchema = string.IsNullOrEmpty(_schemaName) ? Name : string.Format("\"{0}\".{1}", _schemaName, Name);
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
            using (var command = new PgSqlCommand(s, Connection, Transaction))
                command.ExecuteNonQuery();
        }

        public void Drop()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("DROP TABLE {0}", NameWithSchema);
            if (!string.IsNullOrEmpty(_schemaName))
                sb.AppendFormat(";DROP SCHEMA \"{0}\"", _schemaName);

            using (var command = new PgSqlCommand(sb.ToString(), Connection, Transaction))
                command.ExecuteNonQuery();
        }

        public void WithDefaultValueOn(string column)
        {
            const int defaultValue = 1;
            using (var command = new PgSqlCommand(string.Format(" ALTER TABLE {0}.{1} ALTER {2} SET DEFAULT {3}", quoter.QuoteSchemaName(_schemaName), quoter.QuoteTableName(Name), quoter.QuoteColumnName(column), defaultValue), Connection, Transaction))
                command.ExecuteNonQuery();
        }
    }
}
