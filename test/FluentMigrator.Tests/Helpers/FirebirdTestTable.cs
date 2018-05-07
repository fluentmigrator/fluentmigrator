using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using FirebirdSql.Data.FirebirdClient;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Processors.Firebird;

namespace FluentMigrator.Tests.Helpers
{
    public class FirebirdTestTable : IDisposable
    {
        private readonly FirebirdQuoter _quoter = new FirebirdQuoter(false);
        private readonly FirebirdProcessor _processor;

        public FbConnection Connection => (FbConnection)_processor.Connection;

        public string Name { get; }

        public FbTransaction Transaction => (FbTransaction)_processor.Transaction;

        public FirebirdTestTable(FirebirdProcessor processor, params string[] columnDefinitions)
        {
            _processor = processor;
            if (_processor.Connection.State != ConnectionState.Open)
                _processor.Connection.Open();
            string guid = Guid.NewGuid().ToString("N");
            Name = "Table" + guid.Substring(0, Math.Min(guid.Length, 16));
            Init(columnDefinitions);
        }

        public FirebirdTestTable(string tableName, FirebirdProcessor processor, params string[] columnDefinitions)
        {
            _processor = processor;
            if (_processor.Connection.State != ConnectionState.Open)
                _processor.Connection.Open();
            Name = tableName;
            Init(columnDefinitions);
        }

        private void Init(IEnumerable<string> columnDefinitions)
        {
            Create(columnDefinitions);
        }

        public void Dispose()
        {
            if (Connection.State == ConnectionState.Open && !_processor.WasCommitted)
                Drop();
        }

        private void Create(IEnumerable<string> columnDefinitions)
        {
            var sb = new StringBuilder();

            sb.Append("CREATE TABLE ");

            sb.Append(_quoter.QuoteTableName(Name, string.Empty));
            sb.Append(" (");

            foreach (string definition in columnDefinitions)
            {
                sb.Append(definition);
                sb.Append(", ");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append(")");

            var s = sb.ToString();

            using (var command = new FbCommand(s, Connection, Transaction))
                command.ExecuteNonQuery();

            _processor.AutoCommit();

            _processor.LockTable(Name);

        }

        private void Drop()
        {
            _processor.CheckTable(Name);
            var sb = new StringBuilder();
            sb.AppendFormat("DROP TABLE {0}", _quoter.QuoteTableName(Name));

            using (var command = new FbCommand(sb.ToString(), Connection, Transaction))
                command.ExecuteNonQuery();

            _processor.AutoCommit();

        }
    }
}
