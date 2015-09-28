using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Oracle
{
    public class OracleDescriptionGenerator : GenericDescriptionGenerator
    {
        #region Constants

        private const string TableDescriptionTemplate = "COMMENT ON TABLE {0} IS '{1}'";
        private const string ColumnDescriptionTemplate = "COMMENT ON COLUMN {0}.{1} IS '{2}'";

        private const string TableDescriptionAltQuotingTemplate = "COMMENT ON TABLE {0} IS q'${1}$'";
        private const string ColumnDescriptionAltQuotingTemplate = "COMMENT ON COLUMN {0}.{1} IS q'${2}$'";

        #endregion

        private string GetFullTableName(string schemaName, string tableName)
        {
            return string.IsNullOrEmpty(schemaName)
               ? tableName
               : string.Format("{0}.{1}", schemaName, tableName);
        }

        protected override string GenerateTableDescription(
            string schemaName, string tableName, string tableDescription)
        {
            if (string.IsNullOrEmpty(tableDescription))
                return string.Empty;

            string template = (tableDescription.Contains("'")
                ? TableDescriptionAltQuotingTemplate
                : TableDescriptionTemplate);

            return string.Format(template, GetFullTableName(schemaName, tableName), tableDescription);
        }

        protected override string GenerateColumnDescription(
            string schemaName, string tableName, string columnName, string columnDescription)
        {
            if (string.IsNullOrEmpty(columnDescription))
                return string.Empty;

            string template = (columnDescription.Contains("'")
                ? ColumnDescriptionAltQuotingTemplate
                : ColumnDescriptionTemplate);

            return string.Format(
                template,
                GetFullTableName(schemaName, tableName),
                columnName,
                columnDescription);
        }
    }
}
