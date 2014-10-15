using System;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors.DotConnectPostgres;
using Devart.Data.PostgreSql;

namespace FluentMigrator.Tests.Helpers
{
    public class DotConnectPostgresTestSequence : IDisposable
    {
        private readonly PostgresQuoter quoter = new PostgresQuoter();
        private readonly string _schemaName;
        private PgSqlConnection Connection { get; set; }
        public string Name { get; set; }
        public string NameWithSchema { get; set; }
        private PgSqlTransaction Transaction { get; set; }

        public DotConnectPostgresTestSequence(DotConnectPostgresProcessor processor, string schemaName, string sequenceName)
        {
            _schemaName = schemaName;
            Name = quoter.QuoteSequenceName(sequenceName);

            Connection = (PgSqlConnection)processor.Connection;
            Transaction = (PgSqlTransaction)processor.Transaction;
            NameWithSchema = string.IsNullOrEmpty(_schemaName) ? Name : string.Format("\"{0}\".{1}", _schemaName, Name);
            Create();
        }

        public void Dispose()
        {
            Drop();
        }

        public void Create()
        {
            if (!string.IsNullOrEmpty(_schemaName))
            {
                using (var command = new PgSqlCommand(string.Format("CREATE SCHEMA \"{0}\";", _schemaName), Connection, Transaction))
                    command.ExecuteNonQuery();
            }

            string createCommand = string.Format("CREATE SEQUENCE {0} INCREMENT 2 MINVALUE 0 MAXVALUE 100 START WITH 2 CACHE 10 CYCLE", NameWithSchema);
            using (var command = new PgSqlCommand(createCommand, Connection, Transaction))
                command.ExecuteNonQuery();
        }

        public void Drop()
        {
            using (var command = new PgSqlCommand("DROP SEQUENCE " + NameWithSchema, Connection, Transaction))
                command.ExecuteNonQuery();

            if (!string.IsNullOrEmpty(_schemaName))
            {
                using (var command = new PgSqlCommand(string.Format("DROP SCHEMA \"{0}\"", _schemaName), Connection, Transaction))
                    command.ExecuteNonQuery();
            }
        }
    }
}
