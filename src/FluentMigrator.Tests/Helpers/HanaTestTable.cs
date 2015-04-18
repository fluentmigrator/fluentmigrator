using System;
using System.Collections.Generic;
using System.Text;
using FluentMigrator.Runner.Generators.Hana;
using FluentMigrator.Runner.Processors.Hana;
using Sap.Data.Hana;

namespace FluentMigrator.Tests.Helpers
{
    public class HanaTestTable : IDisposable
    {
        private readonly HanaQuoter quoter = new HanaQuoter();
        private readonly string _schemaName;
        public HanaConnection Connection { get; private set; }
        public string Name { get; set; }
        public string NameWithSchema { get; set; }
        public HanaTransaction Transaction { get; private set; }

        private List<string> constraints = new List<string>();
        private List<string> indexies = new List<string>();

        public HanaTestTable(HanaProcessor processor, string schemaName, params string[] columnDefinitions)
        {
            _schemaName = schemaName;
            Name = "\"Table" + Guid.NewGuid().ToString("N") + "\"";
            Init(processor, columnDefinitions);

        }

        public HanaTestTable(string tableName, HanaProcessor processor, string schemaName, params string[] columnDefinitions)
        {
            _schemaName = schemaName;

            Name = quoter.QuoteTableName(tableName);
            Init(processor, columnDefinitions);
        }

        private void Init(HanaProcessor processor, IEnumerable<string> columnDefinitions)
        {
            Connection = (HanaConnection)processor.Connection;
            Transaction = (HanaTransaction)processor.Transaction;

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
            using (var command = new HanaCommand(s, Connection, Transaction))
                command.ExecuteNonQuery();
        }

        public void Drop()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("DROP TABLE {0}", NameWithSchema);
            if (!string.IsNullOrEmpty(_schemaName))
                sb.AppendFormat(";DROP SCHEMA \"{0}\"", _schemaName);

            using (var command = new HanaCommand(sb.ToString(), Connection, Transaction))
                command.ExecuteNonQuery();
        }

        public void WithDefaultValueOn(string column)
        {
            const int defaultValue = 1;
            using (var command = new HanaCommand(string.Format(" ALTER TABLE {0}.{1} ALTER {2} SET DEFAULT {3}", quoter.QuoteSchemaName(_schemaName), quoter.QuoteTableName(Name), quoter.QuoteColumnName(column), defaultValue), Connection, Transaction))
                command.ExecuteNonQuery();
        }

        public string WithIndexOn(string column)
        {
            var indexName = string.Format("idx_{0}", column);

            var quotedObjectName = quoter.QuoteTableName(Name);

            var quotedIndexName = quoter.QuoteIndexName(indexName);

            indexies.Add(quotedIndexName);

            using (var command = new HanaCommand(string.Format("CREATE INDEX {0} ON {1} ({2})", quotedIndexName, quotedObjectName, quoter.QuoteColumnName(column)), Connection, Transaction))
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
            sb.Append(string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} UNIQUE ({2})", quoter.QuoteTableName(Name), quoter.QuoteConstraintName(name), quoter.QuoteColumnName(column)));
            using (var command = new HanaCommand(sb.ToString(), Connection))
                command.ExecuteNonQuery();
            
            constraints.Add(name);
        }
    }
}