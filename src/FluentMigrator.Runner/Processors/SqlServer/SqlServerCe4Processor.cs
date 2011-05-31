using System;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SqlServer;

namespace FluentMigrator.Runner.Processors.SqlServer
{
    public class SqlServerCe4Processor : ProcessorBase
    {
        public SqlCeConnection Connection { get; set; }
        public SqlCeTransaction Transaction { get; set; }

        public SqlServerCe4Processor(SqlCeConnection connection)
            :this(connection, new SqlServerCe4Generator(), new NullAnnouncer())
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
            if (Connection.State != ConnectionState.Open) Connection.Open();

            if (expression.Operation != null)
                expression.Operation(Connection, Transaction);
        }

        protected override void Process(string sql)
        {
            Announcer.Sql(sql);

            if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
                return;

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            if (Transaction == null)
                BeginTransaction();

            using (var command = new SqlCeCommand(sql, Connection, Transaction))
            {
                try
                {
                    command.CommandTimeout = 0; // SQL Server CE does not support non-zero command timeout values!! :/
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
        
        public override void BeginTransaction()
        {
            Announcer.Say("Beginning Transaction");
            Transaction = Connection.BeginTransaction();
        }

        public override void CommitTransaction()
        {
            Announcer.Say("Committing Transaction");

            if (Transaction != null)
            {
                Transaction.Commit();
                Transaction.Dispose();
            }

            if (Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
        }

        public override void RollbackTransaction()
        {
            if (Transaction == null)
            {
                Announcer.Say("No transaction was available to rollback!");
                return;
            }

            Announcer.Say("Rolling back transaction");

            Transaction.Rollback();
            Transaction.Dispose();

            if (Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
        }

        public override void Execute(string template, params object[] args)
        {
            throw new System.NotImplementedException();
        }

        public override bool SchemaExists(string schemaName)
        {
            return true;
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            return Exists("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", FormatSqlEscape(tableName));
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            return Exists("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'", FormatSqlEscape(tableName), FormatSqlEscape(columnName));
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            return Exists("SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = '{0}' AND CONSTRAINT_NAME = '{1}'", FormatSqlEscape(tableName), FormatSqlEscape(constraintName));
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            throw new System.NotImplementedException();
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName, string columnName)
        {
            return Exists("SELECT * FROM INFORMATION_SCHEMA.INDEXES WHERE TABLE_NAME = '{0}' AND INDEX_NAME = '{1}' AND COLUMN_NAME = '{2}'",
                FormatSqlEscape(tableName), FormatSqlEscape(indexName), FormatSqlEscape(columnName));
        }

        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            throw new System.NotImplementedException();
        }

        public override DataSet Read(string template, params object[] args)
        {
            throw new System.NotImplementedException();
        }

        public override bool Exists(string template, params object[] args)
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            using (var command = new SqlCeCommand(String.Format(template, args), Connection, Transaction))
            using (var reader = command.ExecuteReader())
            {
                return reader.Read();
            }
        }

        protected string FormatSqlEscape(string sql)
        {
            return sql.Replace("'", "''");
        }
    }
}