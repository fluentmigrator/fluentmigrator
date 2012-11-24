using System;
using System.Data;
using System.Data.OleDb;
using FluentMigrator.Builders.Execute;

namespace FluentMigrator.Runner.Processors.Jet
{
    public sealed class JetProcessor : ProcessorBase
    {
        private OleDbConnection Connection { get; set; }
        public OleDbTransaction Transaction { get; private set; }

        public override string DatabaseType
        {
            get { return "Jet"; }
        }

        public JetProcessor(OleDbConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options)
            : base(generator, announcer, options)
        {
            Connection = connection;
            connection.Open(); 
            BeginTransaction();
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
            CloseConnection();
        }

        public override void RollbackTransaction()
        {
            Announcer.Say("Rolling back transaction");
            Transaction.Rollback();
            WasCommitted = true;
            CloseConnection();
        }

        public override void CloseConnection()
        {
            if (Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
        }
        
        public override void Process(PerformDBOperationExpression expression)
        {
            Announcer.Say("Performing DB Operation");

            if (Options.PreviewOnly)
                return;
			
            if (Connection.State != ConnectionState.Open) Connection.Open();

            if (expression.Operation != null)
                expression.Operation(Connection, null);
        }

        protected override void Process(string sql)
        {
            Announcer.Sql(sql);

            if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
                return;

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

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
            if (Connection.State != ConnectionState.Open) Connection.Open();

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
            throw new NotImplementedException();
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
            if (Connection.State != ConnectionState.Open) Connection.Open();

            var restrict = new object[] { null, null, tableName, "TABLE" };
            using (var tables = Connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, restrict))
            {
                for (int i = 0; i < tables.Rows.Count; i++) {
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
            if (Connection.State != ConnectionState.Open) Connection.Open();

            var restrict = new[] { null, null, tableName, null };
            using (var columns = Connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, restrict))
            {
                for (int i = 0; i < columns.Rows.Count; i++) {
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
            if (Connection.State != ConnectionState.Open) Connection.Open();

            var restrict = new[] { null, null, constraintName, null, null, tableName };
            using (var constraints = Connection.GetOleDbSchemaTable(OleDbSchemaGuid.Table_Constraints, restrict))
            {
                return constraints.Rows.Count > 0;
            }
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            if (Connection.State != ConnectionState.Open) Connection.Open();

            var restrict = new[] { null, null, indexName, null, tableName };
            using (var indexes = Connection.GetOleDbSchemaTable(OleDbSchemaGuid.Indexes, restrict))
            {
                return indexes.Rows.Count > 0;
            }
        }
    }
}
