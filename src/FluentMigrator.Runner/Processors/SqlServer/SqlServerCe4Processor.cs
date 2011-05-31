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
            :this(connection, new SqlServerCe4Generator(), new NullAnnouncer(), new ProcessorOptions())
        {
            
        }

        public SqlServerCe4Processor(SqlCeConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options) 
            : base(generator, announcer, options)
        {
            Connection = connection;
        }

        public override void Process(PerformDBOperationExpression expression)
        {
            throw new System.NotImplementedException();
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
            throw new System.NotImplementedException();
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            throw new System.NotImplementedException();
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            throw new System.NotImplementedException();
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            throw new System.NotImplementedException();
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            throw new System.NotImplementedException();
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
            throw new System.NotImplementedException();
        }
    }
}