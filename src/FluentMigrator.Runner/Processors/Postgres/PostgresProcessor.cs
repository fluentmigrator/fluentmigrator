using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using FluentMigrator.Builders.Execute;
using Npgsql;

namespace FluentMigrator.Runner.Processors.Postgres
{
    public class PostgresProcessor : ProcessorBase
    {
        public NpgsqlConnection Connection { get; set; }
        public NpgsqlTransaction Transaction { get; private set; }
        public bool WasCommitted { get; private set; }

        public PostgresProcessor(NpgsqlConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options) : base(generator, announcer, options)
        {
            Connection = connection;
            connection.Open();
            Transaction = connection.BeginTransaction();
        }

        public override void Execute(string template, params object[] args)
        {
            Process(string.Format(template, args));
        }

        public override bool SchemaExists(string schemaName)
        {
            return Exists("select * from information_schema.schemata where schema_name = '{0}'", schemaName);
        }

        public override bool TableExists(string tableName)
        {
            return Exists("select * from information_schema.tables where table_name = '{0}'", tableName);
        }

        public override bool ColumnExists(string tableName, string columnName)
        {
            return Exists("select * from information_schema.columns where table_name = '{0}' and column_name = '{1}'", tableName, columnName);
        }

        public override bool ConstraintExists(string tableName, string constraintName)
        {
            //return Exists("select * from pg_catalog.pg_constraint con inner join pg_class cls on con.conrelid = cls.oid where cls.relname = '{0}' and con.conname = '{1}'", tableName, constraintName);
            return Exists("select * from information_schema.table_constraints where constraint_catalog = current_catalog and table_name = '{0}' and constraint_name = '{1}'", tableName, constraintName);
        }

        public override bool IndexExists(string tableName, string indexName)
        {
            return Exists("select * from pg_catalog.pg_indexes where tablename = '{0}' and indexname = '{1}'", tableName, indexName);
        }

        public override DataSet ReadTableData(string tableName)
        {
            return Read("SELECT * FROM \"{0}\"", tableName);
        }

        public override DataSet Read(string template, params object[] args)
        {
            if (Connection.State != ConnectionState.Open) Connection.Open();

            DataSet ds = new DataSet();
            using (NpgsqlCommand command = new NpgsqlCommand(String.Format(template, args), Connection, Transaction))
            using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
            {
                adapter.Fill(ds);
                return ds;
            }
        }

        public override bool Exists(string template, params object[] args)
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            using (var command = new NpgsqlCommand(String.Format(template, args), Connection, Transaction))
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

            using (var command = new NpgsqlCommand(sql, Connection, Transaction))
            {
                try
                {
                    command.CommandTimeout = Options.Timeout;
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    using (StringWriter message = new StringWriter())
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
            if (Connection.State != ConnectionState.Open) Connection.Open();

            if (expression.Operation != null)
                expression.Operation(Connection, Transaction);
        }

        
    }
}
