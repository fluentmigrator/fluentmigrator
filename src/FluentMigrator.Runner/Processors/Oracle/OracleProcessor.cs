using System;
using System.Data;
using FluentMigrator.Builders.Execute;

namespace FluentMigrator.Runner.Processors.Oracle
{
    public class OracleProcessor : ProcessorBase
    {
        private IDbConnection Connection { get; set; }

        public override string DatabaseType
        {
            get { return "Oracle"; }
        }

        public OracleProcessor(IDbConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options)
            : base(generator, announcer, options)
        {
            Connection = connection;

            //oracle does not support ddl transactions
            //this.Transaction = this.Connection.BeginTransaction();
        }

        public override bool SchemaExists(string schemaName)
        {
            if (schemaName == null)
                throw new ArgumentNullException("schemaName");

            if (schemaName.Length == 0)
                return false;

            return Exists("SELECT 1 FROM ALL_USERS WHERE USERNAME = '{0}'", schemaName.ToUpper());
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            if (schemaName == null)
                throw new ArgumentNullException("schemaName");
            if (tableName == null)
                throw new ArgumentNullException("tableName");

            if (tableName.Length == 0)
                return false;

            if (schemaName.Length == 0)
                return Exists("SELECT 1 FROM USER_TABLES WHERE TABLE_NAME = '{0}'", tableName.ToUpper());

            return Exists("SELECT 1 FROM ALL_TABLES WHERE OWNER = '{0}' AND TABLE_NAME = '{1}'", schemaName.ToUpper(), tableName.ToUpper());
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            if (schemaName == null)
                throw new ArgumentNullException("schemaName");
            if (tableName == null)
                throw new ArgumentNullException("tableName");
            if (columnName == null)
                throw new ArgumentNullException("columnName");

            if (columnName.Length == 0 || tableName.Length == 0)
                return false;

            if (string.IsNullOrEmpty(schemaName))
                return Exists("SELECT 1 FROM USER_TAB_COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'", tableName.ToUpper(), columnName.ToUpper());

            return Exists("SELECT 1 FROM ALL_TAB_COLUMNS WHERE OWNER = '{0}' AND TABLE_NAME = '{1}' AND COLUMN_NAME = '{2}'", schemaName.ToUpper(), tableName.ToUpper(), columnName.ToUpper());
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            if (schemaName == null)
                throw new ArgumentNullException("schemaName");
            if (tableName == null)
                throw new ArgumentNullException("tableName");
            if (constraintName == null)
                throw new ArgumentNullException("constraintName");

            //In Oracle DB constraint name is unique within the schema, so the table name is not used in the query

            if (constraintName.Length == 0)
                return false;

            if (schemaName.Length == 0)
                return Exists("SELECT 1 FROM USER_CONSTRAINTS WHERE CONSTRAINT_NAME = '{0}'", constraintName.ToUpper());

            return Exists("SELECT 1 FROM ALL_CONSTRAINTS WHERE OWNER = '{0}' AND CONSTRAINT_NAME = '{1}'", schemaName.ToUpper(), constraintName.ToUpper());
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            if (schemaName == null)
                throw new ArgumentNullException("schemaName");
            if (tableName == null)
                throw new ArgumentNullException("tableName");
            if (indexName == null)
                throw new ArgumentNullException("indexName");

            //In Oracle DB index name is unique within the schema, so the table name is not used in the query

            if (indexName.Length == 0)
                return false;

            if (schemaName.Length == 0)
                return Exists("SELECT 1 FROM USER_INDEXES WHERE INDEX_NAME = '{0}'", indexName.ToUpper());

            return Exists("SELECT 1 FROM ALL_INDEXES WHERE OWNER = '{0}' AND INDEX_NAME = '{1}'", schemaName.ToUpper(), indexName.ToUpper());
        }

        public override void Execute(string template, params object[] args)
        {
            if (template == null)
                throw new ArgumentNullException("template");

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            using (var command = OracleFactory.GetCommand(Connection, String.Format(template, args)))
            {
                command.ExecuteNonQuery();
            }
        }

        public override bool Exists(string template, params object[] args)
        {
            if (template == null)
                throw new ArgumentNullException("template");

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            using (var command = OracleFactory.GetCommand(Connection, String.Format(template, args)))
            using (var reader = command.ExecuteReader())
            {
                return reader.Read();
            }
        }

        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            if (schemaName == null)
                throw new ArgumentNullException("schemaName");
            if (tableName == null)
                throw new ArgumentNullException("tableName");

            if (schemaName.Length == 0)
                return Read("SELECT * FROM {0}", tableName.ToUpper());

            return Read("SELECT * FROM {0}.{1}", schemaName.ToUpper(), tableName.ToUpper());
        }

        public override DataSet Read(string template, params object[] args)
        {
            if (template == null)
                throw new ArgumentNullException("template");

            if (Connection.State != ConnectionState.Open) Connection.Open();

            var resultDataSet = new DataSet();
            using (var command = OracleFactory.GetCommand(Connection, String.Format(template, args)))
            using (var adapter = OracleFactory.GetDataAdapter(command))
            {
                adapter.Fill(resultDataSet);
                return resultDataSet;
            }
        }

        public override void Process(PerformDBOperationExpression expression)
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

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

            using (var command = OracleFactory.GetCommand(Connection, sql))
                command.ExecuteNonQuery();
        }
    }
}