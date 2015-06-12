using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Hana;
using FluentMigrator.Runner.Helpers;

namespace FluentMigrator.Runner.Processors.Hana
{
    public class HanaProcessor : GenericProcessorBase
    {
        public override string DatabaseType
        {
            get { return "Hana"; }
        }

        public override bool SupportsTransactions
        {
            get { return true; }
        }

        public HanaProcessor(IDbConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options, IDbFactory factory)
            : base(connection, factory, generator, announcer, options)
        {
        }

        public IQuoter Quoter
        {
            get { return ((HanaGenerator)Generator).Quoter; }
        }

        public override bool SchemaExists(string schemaName)
        {
            return false;
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            return Exists(
                "SELECT 1 FROM TABLES WHERE SCHEMA_NAME = CURRENT_SCHEMA AND upper(TABLE_NAME) = upper('{0}')",
                FormatHelper.FormatSqlEscape(Quoter.UnQuote(tableName).ToUpper()));
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            if (tableName == null)
                throw new ArgumentNullException("tableName");
            if (columnName == null)
                throw new ArgumentNullException("columnName");

            return Exists("SELECT 1 FROM TABLE_COLUMNS WHERE SCHEMA_NAME = CURRENT_SCHEMA AND upper(TABLE_NAME) = '{0}' AND upper(COLUMN_NAME) = '{1}'",
                FormatHelper.FormatSqlEscape(Quoter.UnQuote(tableName).ToUpper()),
                FormatHelper.FormatSqlEscape(Quoter.UnQuote(columnName).ToUpper()));
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            if (tableName == null)
                throw new ArgumentNullException("tableName");
            if (constraintName == null)
                throw new ArgumentNullException("constraintName");

            if (constraintName.Length == 0)
                return false;

            return Exists("SELECT 1 FROM CONSTRAINTS WHERE SCHEMA_NAME = CURRENT_SCHEMA and upper(CONSTRAINT_NAME) = '{0}'",
                FormatHelper.FormatSqlEscape(Quoter.UnQuote(constraintName).ToUpper()));
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            if (tableName == null)
                throw new ArgumentNullException("tableName");
            if (indexName == null)
                throw new ArgumentNullException("indexName");

            if (indexName.Length == 0)
                return false;

            return Exists("SELECT 1 FROM INDEXES WHERE SCHEMA_NAME = CURRENT_SCHEMA AND upper(INDEX_NAME) = '{0}'",
                    FormatHelper.FormatSqlEscape(Quoter.UnQuote(indexName).ToUpper()));
        }

        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException("sequenceName");

            if (string.IsNullOrEmpty(sequenceName))
                return false;

            return Exists("SELECT 1 FROM SEQUENCES WHERE SCHEMA_NAME = CURRENT_SCHEMA and upper(SEQUENCE_NAME) = '{0}'",
                FormatHelper.FormatSqlEscape(Quoter.UnQuote(sequenceName).ToUpper()));
        }

        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            return false;
        }

        public override void Execute(string template, params object[] args)
        {
            Process(string.Format(template, args));
        }

        public override bool Exists(string template, params object[] args)
        {
            if (template == null)
                throw new ArgumentNullException("template");

            EnsureConnectionIsOpen();

            var querySql = String.Format(template, args);

            Announcer.Sql(string.Format("{0};", querySql));

            using (var command = Factory.CreateCommand(String.Format(template, args), Connection))
            using (var reader = command.ExecuteReader())
            {
                return reader.Read();
            }
        }

        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException("tableName");

            if (String.IsNullOrEmpty(schemaName))
                return Read("SELECT * FROM {0}", Quoter.QuoteTableName(tableName));

            return Read("SELECT * FROM {0}.{1}", Quoter.QuoteSchemaName(schemaName), Quoter.QuoteTableName(tableName));
        }

        public override DataSet Read(string template, params object[] args)
        {
            if (template == null)
                throw new ArgumentNullException("template");

            EnsureConnectionIsOpen();

            var result = new DataSet();
            using (var command = Factory.CreateCommand(String.Format(template, args), Connection))
            {
                var adapter = Factory.CreateDataAdapter(command);
                adapter.Fill(result);
                return result;
            }
        }

        public override void Process(PerformDBOperationExpression expression)
        {
            Announcer.Say("Performing DB Operation");

            if (Options.PreviewOnly)
                return;

            EnsureConnectionIsOpen();

            if (expression.Operation != null)
                expression.Operation(Connection, null);
        }

        protected override void Process(string sql)
        {
            Announcer.Sql(sql);

            if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
                return;

            EnsureConnectionIsOpen();

            var batches = Regex.Split(sql, @"^\s*;\s*$", RegexOptions.Multiline)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(c => c.Trim());

            foreach (var batch in batches)
            {
                var batchCommand = batch.EndsWith(";") 
                    ? batch.Remove(batch.Length - 1) 
                    : batch;

                using (var command = Factory.CreateCommand(batchCommand, Connection))
                    command.ExecuteNonQuery();
            }
        }

    }
}
