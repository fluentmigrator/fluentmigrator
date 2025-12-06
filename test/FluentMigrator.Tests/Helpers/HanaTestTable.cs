using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using FluentMigrator.Generation;
using FluentMigrator.Runner.Generators.Hana;
using FluentMigrator.Runner.Processors.Hana;
using Sap.Data.Hana;

namespace FluentMigrator.Tests.Helpers
{
    public class HanaTestTable : IDisposable
    {
        private readonly IQuoter _quoter = new HanaQuoter();
        private readonly string _schemaName;
        public HanaConnection Connection { get; private set; }
        public string Name { get; set; }
        public string NameWithSchema { get; set; }
        public HanaTransaction Transaction { get; private set; }

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

            Connection = (HanaConnection)processor.Connection;
            Transaction = (HanaTransaction)processor.Transaction;

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
            using (var command = new HanaCommand(s, Connection, Transaction))
                command.ExecuteNonQuery();
        }

        public void Drop()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("DROP TABLE {0}", NameWithSchema);
            var quotedSchema = _quoter.QuoteSchemaName(_schemaName);
            if (!string.IsNullOrEmpty(quotedSchema))
                sb.AppendFormat(";DROP SCHEMA {0}", quotedSchema);

            using (var command = new HanaCommand(sb.ToString(), Connection, Transaction))
                command.ExecuteNonQuery();
        }

        public void WithDefaultValueOn(string column)
        {
            const int defaultValue = 1;
            using (var command = new HanaCommand(string.Format(" ALTER TABLE {0} ALTER {1} SET DEFAULT {2}", _quoter.QuoteTableName(Name, _schemaName), _quoter.QuoteColumnName(column), defaultValue), Connection, Transaction))
                command.ExecuteNonQuery();
        }

        public string WithIndexOn(string column)
        {
            var indexName = string.Format("idx_{0}", column);

            var quotedObjectName = _quoter.QuoteTableName(Name);

            var quotedIndexName = _quoter.QuoteIndexName(indexName);

            using (var command = new HanaCommand(string.Format("CREATE INDEX {0} ON {1} ({2})", quotedIndexName, quotedObjectName, _quoter.QuoteColumnName(column)), Connection, Transaction))
                command.ExecuteNonQuery();

            return indexName;
        }
        public void WithUniqueConstraintOn(string column)
        {
            WithUniqueConstraintOn(column, "UC_" + column);
        }

        public void WithUniqueConstraintOn(string column, string name)
        {
            var sb = new StringBuilder();
            sb.Append(string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} UNIQUE ({2})", _quoter.QuoteTableName(Name), _quoter.QuoteConstraintName(name), _quoter.QuoteColumnName(column)));
            using (var command = new HanaCommand(sb.ToString(), Connection, Transaction))
                command.ExecuteNonQuery();
        }
    }
}
