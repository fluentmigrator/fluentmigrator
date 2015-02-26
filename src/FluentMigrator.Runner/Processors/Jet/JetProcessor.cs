using System;
using System.Data;
using System.Data.OleDb;
using FluentMigrator.Builders.Execute;

namespace FluentMigrator.Runner.Processors.Jet
{
    public class JetProcessor : ProcessorBase
    {
        private readonly string connectionString;
        public OleDbConnection Connection { get; protected set; }
        public OleDbTransaction Transaction { get; protected set; }
        public override string ConnectionString { get { return connectionString; } }

        public override string DatabaseType
        {
            get { return "Jet"; }
        }

        public JetProcessor(OleDbConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options)
            : base(generator, announcer, options)
        {
            Connection = connection;

            // Prefetch connectionstring as after opening the security info could no longer be present
            // for instance on sql server
            connectionString = connection.ConnectionString;
        }

        protected void EnsureConnectionIsOpen()
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();
        }

        protected void EnsureConnectionIsClosed()
        {
            if (Connection.State != ConnectionState.Closed)
                Connection.Close();
        }

        public override void Process(PerformDBOperationExpression expression)
        {
            Announcer.Say("Performing DB Operation");

            if (Options.PreviewOnly)
                return;

            EnsureConnectionIsOpen();

            if (expression.Operation != null)
                expression.Operation(Connection, Transaction);
        }

        protected override void Process(string sql)
        {
            Announcer.Sql(sql);

            if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
                return;

            EnsureConnectionIsOpen();

            using (var command = new OleDbCommand(sql, Connection, Transaction))
            {
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (OleDbException ex)
                {
                    throw new Exception(string.Format("Exception while processing \"{0}\"", sql), ex);
                }
            }
        }

        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            return Read("SELECT * FROM [{0}]", tableName);
        }

        public override DataSet Read(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            var ds = new DataSet();
            using (var command = new OleDbCommand(String.Format(template, args), Connection, Transaction))
            using (var adapter = new OleDbDataAdapter(command))
            {
                adapter.Fill(ds);
                return ds;
            }
        }

        public override bool Exists(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            using (var command = new OleDbCommand(String.Format(template, args), Connection, Transaction))
            using (var reader = command.ExecuteReader())
            {
                return reader.Read();
            }
        }

        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            return false;
        }

        public override void Execute(string template, params object[] args)
        {
            Process(String.Format(template, args));
        }

        public override bool SchemaExists(string tableName)
        {
            return true;
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            EnsureConnectionIsOpen();

            var restrict = new object[] { null, null, tableName, "TABLE" };
            using (var tables = Connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, restrict))
            {
                for (int i = 0; i < tables.Rows.Count; i++)
                {
                    var name = tables.Rows[i].ItemArray[2].ToString();
                    if (name == tableName)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            EnsureConnectionIsOpen();

            var restrict = new[] { null, null, tableName, null };
            using (var columns = Connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, restrict))
            {
                for (int i = 0; i < columns.Rows.Count; i++)
                {
                    var name = columns.Rows[i].ItemArray[3].ToString();
                    if (name == columnName)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            EnsureConnectionIsOpen();

            var restrict = new[] { null, null, constraintName, null, null, tableName };
            using (var constraints = Connection.GetOleDbSchemaTable(OleDbSchemaGuid.Table_Constraints, restrict))
            {
                return constraints.Rows.Count > 0;
            }
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            EnsureConnectionIsOpen();

            var restrict = new[] { null, null, indexName, null, tableName };
            using (var indexes = Connection.GetOleDbSchemaTable(OleDbSchemaGuid.Indexes, restrict))
            {
                return indexes.Rows.Count > 0;
            }
        }

        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            return false;
        }

        public override void BeginTransaction()
        {
            if (Transaction != null) return;

            EnsureConnectionIsOpen();

            Announcer.Say("Beginning Transaction");
            Transaction = Connection.BeginTransaction();
        }

        public override void RollbackTransaction()
        {
            if (Transaction == null) return;

            Announcer.Say("Rolling back transaction");
            Transaction.Rollback();
            WasCommitted = true;
            Transaction = null;
        }

        public override void CommitTransaction()
        {
            if (Transaction == null) return;

            Announcer.Say("Committing Transaction");
            Transaction.Commit();
            WasCommitted = true;
            Transaction = null;
        }

        protected override void Dispose(bool isDisposing)
        {
            RollbackTransaction();
            EnsureConnectionIsClosed();
        }
    }
}
