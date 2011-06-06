using System;
using System.Data;
using System.Data.SqlServerCe;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SqlServer;

namespace FluentMigrator.Runner.Processors.SqlServer
{
    public class SqlServerCe4Processor : ProcessorBase
    {
        public SqlCeConnection Connection { get; set; }

        public SqlServerCe4Processor(SqlCeConnection connection)
            : this(connection, new SqlServerCe4Generator(), new NullAnnouncer())
        {
        }

        public SqlServerCe4Processor(SqlCeConnection connection, IAnnouncer announcer)
            : this(connection, new SqlServerCe4Generator(), announcer)
        {
        }

        public SqlServerCe4Processor(SqlCeConnection connection, IMigrationGenerator generator, IAnnouncer announcer)
            : this(connection, generator, announcer, new ProcessorOptions())
        {
        }

        public SqlServerCe4Processor(SqlCeConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options)
            : base(generator, announcer, options)
        {
            Connection = connection;
        }

        public override void Process(PerformDBOperationExpression expression)
        {
            if (expression.Operation == null)
            {
                return;
            }
            if (Connection.State != ConnectionState.Open) Connection.Open();
            Announcer.Say("PerformDBOperationExpression");
            using (var trans = BeginTransaction())
            {
                try
                {
                    expression.Operation(Connection, trans);
                    CommitTransaction(trans);
                }
                catch (Exception ex)
                {
                    Announcer.Error(ex.Message);
                    RollbackTransaction(trans);
                    throw;
                }
            }
        }

        protected override void Process(string sql)
        {
            Announcer.Sql(sql);

            if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
                return;

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            using (var trans = BeginTransaction())
            using (var command = new SqlCeCommand(sql, Connection, trans))
            {
                try
                {
                    command.CommandTimeout = 0; // SQL Server CE does not support non-zero command timeout values!! :/
                    command.ExecuteNonQuery();
                    CommitTransaction(trans);
                }
                catch (Exception ex)
                {
                    Announcer.Error(ex.Message);
                    RollbackTransaction(trans);
                    throw;
                }
            }
        }

        public new SqlCeTransaction BeginTransaction()
        {
            Announcer.Say("Beginning Transaction");
            return Connection.BeginTransaction();
        }

        public void CommitTransaction(SqlCeTransaction transaction)
        {
            Announcer.Say("Committing Transaction");

            if (transaction != null)
            {
                transaction.Commit();
            }

            if (Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
        }

        public void RollbackTransaction(SqlCeTransaction transaction)
        {
            if (transaction == null)
            {
                Announcer.Say("No transaction was available to rollback!");
                return;
            }

            Announcer.Say("Rolling back transaction");

            transaction.Rollback();

            if (Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
        }

        public override void Execute(string template, params object[] args)
        {
            Process(String.Format(template, args));
        }

        public override bool SchemaExists(string schemaName)
        {
            return true;
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            Announcer.Say("TableExists");
            return Exists("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", FormatSqlEscape(tableName));
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            Announcer.Say("ColumnExists");
            return Exists("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'", FormatSqlEscape(tableName), FormatSqlEscape(columnName));
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            Announcer.Say("ConstraintExists");
            return Exists("SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = '{0}' AND CONSTRAINT_NAME = '{1}'", FormatSqlEscape(tableName), FormatSqlEscape(constraintName));
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            Announcer.Say("IndexExists");
            return Exists("SELECT * FROM INFORMATION_SCHEMA.INDEXES WHERE TABLE_NAME = '{0}' AND INDEX_NAME = '{1}'",
                          FormatSqlEscape(tableName), FormatSqlEscape(indexName));
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName, string columnName)
        {
            Announcer.Say("IndexExists - columnName");
            return Exists("SELECT * FROM INFORMATION_SCHEMA.INDEXES WHERE TABLE_NAME = '{0}' AND INDEX_NAME = '{1}' AND COLUMN_NAME = '{2}'",
                          FormatSqlEscape(tableName), FormatSqlEscape(indexName), FormatSqlEscape(columnName));
        }

        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            return Read("SELECT * FROM [{0}]", tableName);
        }

        public override DataSet Read(string template, params object[] args)
        {
            if (Connection.State != ConnectionState.Open) Connection.Open();

            var ds = new DataSet();
            using (var trans = BeginTransaction())
            using (var command = new SqlCeCommand(String.Format(template, args), Connection, trans))
            using (var adapter = new SqlCeDataAdapter(command))
            {
                adapter.Fill(ds);
                CommitTransaction(trans);
                return ds;
            }
        }

        public override bool Exists(string template, params object[] args)
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            using (var trans = BeginTransaction())
            using (var command = new SqlCeCommand(String.Format(template, args), Connection, trans))
            using (var reader = command.ExecuteReader())
            {
                var exists = reader.Read();
                reader.Close();
                CommitTransaction(trans);
                return exists;
            }
        }

        protected string FormatSqlEscape(string sql)
        {
            return sql.Replace("'", "''");
        }
    }
}