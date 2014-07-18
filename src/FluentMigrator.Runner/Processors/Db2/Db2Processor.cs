namespace FluentMigrator.Runner.Processors.DB2
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;

    using FluentMigrator.Runner.Helpers;
    using FluentMigrator.Runner.Generators;
    using FluentMigrator.Runner.Generators.DB2;

    public class Db2Processor : GenericProcessorBase
    {
        #region Constructors

        public Db2Processor(IDbConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options, IDbFactory factory)
            : base(connection, factory, generator, announcer, options)
        {
            Quoter = new Db2Quoter();
        }

        #endregion Constructors

        #region Properties

        public IQuoter Quoter
        {
            get;
            set;
        }

        public override string DatabaseType
        {
            get { return "IBM DB2"; }
        }

        public override bool SupportsTransactions
        {
            get
            {
                return true;
            }
        }

        #endregion Properties

        #region Methods

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            var schema = string.IsNullOrEmpty(schemaName) ? string.Empty : "TABLE_SCHEMA = '" + FormatToSafeName(schemaName) + "' AND ";

            var doesExist = Exists("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE {0} TABLE_NAME = '{1}' AND COLUMN_NAME='{2}'", schema, FormatToSafeName(tableName), FormatToSafeName(columnName));
            return doesExist;
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            var schema = string.IsNullOrEmpty(schemaName) ? string.Empty : "TABLE_SCHEMA = '" + FormatToSafeName(schemaName) + "' AND ";
            
            return Exists("SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE {0} TABLE_NAME = '{1}' AND CONSTRAINT_NAME='{2}'", schema, FormatToSafeName(tableName), FormatToSafeName(constraintName));
        }

        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            var schema = string.IsNullOrEmpty(schemaName) ? string.Empty : "TABLE_SCHEMA = '" + FormatToSafeName(schemaName) + "' AND ";
            string defaultValueAsString = string.Format("%{0}%", FormatHelper.FormatSqlEscape(defaultValue.ToString()));

            return Exists("SELECT COLUMN_DEFAULT FROM INFORMATION_SCHEMA.COLUMNS WHERE {0} TABLE_NAME = '{1}' AND COLUMN_NAME = '{2}' AND COLUMN_DEFAULT LIKE '{3}'", schema, FormatToSafeName(tableName), columnName.ToUpper(), defaultValueAsString);
        }

        public override void Execute(string template, params object[] args)
        {
            Process(string.Format(template, args));
        }

        public override bool Exists(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            using (var command = Factory.CreateCommand(String.Format(template, args), Connection, Transaction))
            using (var reader = command.ExecuteReader())
            {
                return reader.Read();
            }
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            var schema = !string.IsNullOrEmpty(schemaName) ? Quoter.QuoteSchemaName(schemaName) + "." : string.Empty;
            var doesExist = Exists("SELECT NAME FROM INFORMATION_SCHEMA.SYSINDEXES WHERE TABLE_NAME = '{1}' AND NAME = '{2}'",
                schema, FormatToSafeName(tableName), FormatToSafeName(indexName));

            return doesExist;
        }

        public override void Process(Builders.Execute.PerformDBOperationExpression expression)
        {
            Announcer.Say("Performing DB Operation");

            if (Options.PreviewOnly)
                return;

            EnsureConnectionIsOpen();

            if (expression.Operation != null)
                expression.Operation(Connection, Transaction);
        }

        public override System.Data.DataSet Read(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            var ds = new DataSet();
            using (var command = Factory.CreateCommand(String.Format(template, args), Connection, Transaction))
            {
                var adapter = Factory.CreateDataAdapter(command);
                adapter.Fill(ds);
                return ds;
            }
        }

        public override System.Data.DataSet ReadTableData(string schemaName, string tableName)
        {
            var schemaAndTable = !string.IsNullOrEmpty(schemaName) ? Quoter.QuoteSchemaName(schemaName) + "." + Quoter.QuoteTableName(tableName) : Quoter.QuoteTableName(tableName);
            return Read("SELECT * FROM {0}", schemaAndTable);
        }

        public override bool SchemaExists(string schemaName)
        {
            return Exists("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{0}'", FormatToSafeName(schemaName));
        }

        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            return false;
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            var schema = string.IsNullOrEmpty(schemaName) ? string.Empty : "TABLE_SCHEMA = '" + FormatToSafeName(schemaName) + "' AND ";

            return Exists("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE {0} TABLE_NAME = '{1}'", schema, FormatToSafeName(tableName));
        }

        protected override void Process(string sql)
        {
            Announcer.Sql(sql);

            if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
                return;

            EnsureConnectionIsOpen();

            using (var command = Factory.CreateCommand(sql, Connection))
            {
                command.CommandTimeout = Options.Timeout;
                command.ExecuteNonQuery();
            }
        }

        private string FormatToSafeName(string sqlName)
        {
            var rawName = Quoter.UnQuote(sqlName);

            if (!rawName.Contains('\''))
            {
                return rawName.ToUpper();
            }

            return FormatHelper.FormatSqlEscape(rawName);
        }

        #endregion Methods
    }
}