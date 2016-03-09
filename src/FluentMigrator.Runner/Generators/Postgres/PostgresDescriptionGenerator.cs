using System;
using System.Collections.Generic;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Postgres
{
    /// <summary>
    /// almost copied from OracleDescriptionGenerator,
    /// modified for escaping table description
    /// </summary>
    public class PostgresDescriptionGenerator : GenericDescriptionGenerator
    {
        private readonly IQuoter _quoter;

        public PostgresDescriptionGenerator()
        {
            _quoter = new PostgresQuoter();
        }

        protected IQuoter Quoter
        {
            get { return _quoter; }
        }

        #region Constants

        private const string TableDescriptionTemplate = "COMMENT ON TABLE {0} IS '{1}';";
        private const string ColumnDescriptionTemplate = "COMMENT ON COLUMN {0}.{1} IS '{2}';";

        #endregion

        private string GetFullTableName(string schemaName, string tableName)
        {
            return string.IsNullOrEmpty(schemaName)
               ? Quoter.QuoteTableName(tableName)
               : string.Format("{0}.{1}", Quoter.QuoteSchemaName(schemaName), Quoter.QuoteTableName(tableName));
        }

        protected override string GenerateTableDescription(
            string schemaName, string tableName, string tableDescription)
        {
            if (string.IsNullOrEmpty(tableDescription))
                return string.Empty;

            return string.Format(TableDescriptionTemplate, GetFullTableName(schemaName, tableName), tableDescription.Replace("'", "''"));
        }

        protected override string GenerateColumnDescription(
            string schemaName, string tableName, string columnName, string columnDescription)
        {
            if (string.IsNullOrEmpty(columnDescription))
                return string.Empty;

            return string.Format(
                ColumnDescriptionTemplate,
                GetFullTableName(schemaName, tableName),
                Quoter.QuoteColumnName(columnName),
                columnDescription.Replace("'", "''"));
        }
    }
}
