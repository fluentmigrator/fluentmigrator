using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using FluentMigrator.Generation;
using FluentMigrator.Runner.Generators.Hana;
using FluentMigrator.Runner.Processors.Hana;

namespace FluentMigrator.Tests.Helpers
{
    public class HanaTestTable : IDisposable
    {
        private readonly IQuoter _quoter = new HanaQuoter();
        private readonly string _schemaName;
        public IDbConnection Connection { get; private set; }
        public string Name { get; set; }
        public string NameWithSchema { get; set; }
        public IDbTransaction Transaction { get; private set; }

        public HanaTestTable(HanaProcessor processor, string schemaName, params string[] columnDefinitions)
        {
            _schemaName = schemaName;
            Name = "Table" + Guid.NewGuid().ToString("N");
            Init(processor, columnDefinitions);

        }

        public HanaTestTable(string tableName, HanaProcessor processor, string schemaName, params string[] columnDefinitions)
        {
            _schemaName = schemaName;

            Name = _quoter.UnQuote(tableName);
            Init(processor, columnDefinitions);
        }

        private void Init(HanaProcessor processor, IEnumerable<string> columnDefinitions)
        {
            NameWithSchema = _quoter.QuoteTableName(Name, _schemaName);

            Connection = processor.Connection;
            Transaction = processor.Transaction;

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            Create(columnDefinitions);
        }

        public void Dispose()
        {
            Drop();
        }

        public void Create(IEnumerable<string> columnDefinitions)
        {
            var sb = new StringBuilder();

            var quotedSchema = _quoter.QuoteSchemaName(_schemaName);
            if (!string.IsNullOrEmpty(quotedSchema))
                sb.AppendFormat("CREATE SCHEMA {0};", quotedSchema);

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
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = s;
                command.Transaction = Transaction;
                command.ExecuteNonQuery();
            }
        }

        public void Drop()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("DROP TABLE {0}", NameWithSchema);
            var quotedSchema = _quoter.QuoteSchemaName(_schemaName);
            if (!string.IsNullOrEmpty(quotedSchema))
                sb.AppendFormat(";DROP SCHEMA {0}", quotedSchema);

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = sb.ToString();
                command.Transaction = Transaction;
                command.ExecuteNonQuery();
            }
        }

        public void WithDefaultValueOn(string column)
        {
            const int defaultValue = 1;
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = string.Format(" ALTER TABLE {0} ALTER {1} SET DEFAULT {2}", _quoter.QuoteTableName(Name, _schemaName), _quoter.QuoteColumnName(column), defaultValue);
                command.Transaction = Transaction;
                command.ExecuteNonQuery();
            }
        }

        public string WithIndexOn(string column)
        {
            var indexName = string.Format("idx_{0}", column);

            var quotedObjectName = _quoter.QuoteTableName(Name, _schemaName);

            var quotedIndexName = _quoter.QuoteIndexName(indexName);

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = string.Format("CREATE INDEX {0} ON {1} ({2})", quotedIndexName, quotedObjectName, _quoter.QuoteColumnName(column));
                command.Transaction = Transaction;
                command.ExecuteNonQuery();
            }

            return indexName;
        }
        public void WithUniqueConstraintOn(string column)
        {
            WithUniqueConstraintOn(column, "UC_" + column);
        }

        public void WithUniqueConstraintOn(string column, string name)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} UNIQUE ({2})", _quoter.QuoteTableName(Name, _schemaName), _quoter.QuoteConstraintName(name), _quoter.QuoteColumnName(column));
                command.Transaction = Transaction;
                command.ExecuteNonQuery();
            }
        }
    }
}
