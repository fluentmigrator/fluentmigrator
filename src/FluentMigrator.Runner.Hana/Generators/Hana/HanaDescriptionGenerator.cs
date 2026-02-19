using System;

using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Hana
{
    /// <summary>
    /// A description generator for SAP HANA database.
    /// </summary>
    /// <remarks>
    /// This class generates SQL statements to set descriptions for tables and columns
    /// in a SAP HANA database. It extends the functionality of the <see cref="FluentMigrator.Runner.Generators.Generic.GenericDescriptionGenerator"/> 
    /// by providing HANA-specific SQL syntax for table and column comments.
    /// </remarks>
    [Obsolete("Hana support will go away unless someone in the community steps up to provide support.")]
    public class HanaDescriptionGenerator : GenericDescriptionGenerator
    {
        #region Constants

        private const string TableDescriptionTemplate = "COMMENT ON TABLE {0} IS '{1}'";
        private const string ColumnDescriptionTemplate = "COMMENT ON COLUMN {0}.{1} IS '{2}'";

        #endregion

        private string GetFullTableName(string schemaName, string tableName)
        {
            return string.IsNullOrEmpty(schemaName)
               ? tableName
               : $"{schemaName}.{tableName}";
        }

        /// <inheritdoc />
        protected override string GenerateTableDescription(
            string schemaName, string tableName, string tableDescription)
        {
            if (string.IsNullOrEmpty(tableDescription))
                return string.Empty;

            return string.Format(TableDescriptionTemplate, GetFullTableName(schemaName, tableName), tableDescription);
        }

        /// <inheritdoc />
        protected override string GenerateColumnDescription(
            string descriptionName, string schemaName, string tableName, string columnName, string columnDescription)
        {
            if (string.IsNullOrEmpty(columnDescription))
                return string.Empty;

            return string.Format(
                ColumnDescriptionTemplate,
                GetFullTableName(schemaName, tableName),
                columnName,
                columnDescription);
        }
    }
}
