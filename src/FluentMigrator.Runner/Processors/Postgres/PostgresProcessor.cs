using System;
using System.Data;
using System.IO;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Runner.Generators.Postgres;

namespace FluentMigrator.Runner.Processors.Postgres
{
    public class PostgresProcessor : ProcessorBase
    {
        private readonly IDbFactory factory;
        readonly PostgresQuoter quoter = new PostgresQuoter();
        public IDbConnection Connection { get; private set; }
        public IDbTransaction Transaction { get; private set; }

        public override string DatabaseType
        {
            get { return "Postgres"; }
        }

        public PostgresProcessor(IDbConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options, IDbFactory factory)
            : base(generator, announcer, options)
        {
            this.factory = factory;
            Connection = connection;
            connection.Open();

            Announcer.Say("Beginning Transaction");
            Transaction = Connection.BeginTransaction();
        }

        public override void Execute(string template, params object[] args)
        {
            Process(string.Format(template, args));
        }

        public override bool SchemaExists(string schemaName)
        {
            return Exists("select * from information_schema.schemata where schema_name = '{0}'", FormatToSafeSchemaName(schemaName));
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            return Exists("select * from information_schema.tables where table_schema = '{0}' and table_name = '{1}'", FormatToSafeSchemaName(schemaName), FormatToSafeName(tableName));
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            return Exists("select * from information_schema.columns where table_schema = '{0}' and table_name = '{1}' and column_name = '{2}'", FormatToSafeSchemaName(schemaName), FormatToSafeName(tableName), FormatToSafeName(columnName));
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            return Exists("select * from information_schema.table_constraints where constraint_catalog = current_catalog and table_schema = '{0}' and table_name = '{1}' and constraint_name = '{2}'", FormatToSafeSchemaName(schemaName), FormatToSafeName(tableName), FormatToSafeName(constraintName));
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            return Exists("select * from pg_catalog.pg_indexes where schemaname='{0}' and tablename = '{1}' and indexname = '{2}'", FormatToSafeSchemaName(schemaName), FormatToSafeName(tableName), FormatToSafeName(indexName));
        }

        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            throw new NotImplementedException();
        }

        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            return Read("SELECT * FROM {0}.{1}", quoter.QuoteSchemaName(schemaName), quoter.QuoteTableName(tableName));
        }

        public override DataSet Read(string template, params object[] args)
        {
            if (Connection.State != ConnectionState.Open) Connection.Open();

            var ds = new DataSet();
            using (var command = factory.CreateCommand(String.Format(template, args), Connection, Transaction))
            {
                var adapter = factory.CreateDataAdapter(command);
                adapter.Fill(ds);
                return ds;
            }
        }

        public override bool Exists(string template, params object[] args)
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            using (var command = factory.CreateCommand(String.Format(template, args), Connection, Transaction))
            using (var reader = command.ExecuteReader())
            {
                return reader.Read();
            }
        }

        public override void BeginTransaction()
        {
            Announcer.Say("Beginning Transaction");
            Transaction = Connection.BeginTransaction();
        }

        public override void CommitTransaction()
        {
            Announcer.Say("Committing Transaction");
            Transaction.Commit();
            WasCommitted = true;
            if (Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
        }

        public override void RollbackTransaction()
        {
            Announcer.Say("Rolling back transaction");
            Transaction.Rollback();
            WasCommitted = true;
            if (Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
        }

        protected override void Process(string sql)
        {
            Announcer.Sql(sql);

            if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
                return;

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            using (var command = factory.CreateCommand(sql, Connection, Transaction))
            {
                try
                {
                    command.CommandTimeout = Options.Timeout;
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    using (var message = new StringWriter())
                    {
                        message.WriteLine("An error occurred executing the following sql:");
                        message.WriteLine(sql);
                        message.WriteLine("The error was {0}", ex.Message);

                        throw new Exception(message.ToString(), ex);
                    }
                }
            }
        }

        public override void Process(PerformDBOperationExpression expression)
        {
            Announcer.Say("Performing DB Operation");

            if (Options.PreviewOnly)
                return;
			
            if (Connection.State != ConnectionState.Open) Connection.Open();

            if (expression.Operation != null)
                expression.Operation(Connection, Transaction);
        }

        private string FormatToSafeSchemaName(string schemaName)
        {
            return FormatSqlEscape(quoter.UnQuoteSchemaName(schemaName));
        }

        private string FormatToSafeName(string sqlName)
        {
            return FormatSqlEscape(quoter.UnQuote(sqlName));
        }

        private static string FormatSqlEscape(string sql)
        {
            return sql.Replace("'", "''");
        }
    }
}
