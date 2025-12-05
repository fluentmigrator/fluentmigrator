#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using FluentMigrator.Generation;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Redshift
{
    /// <summary>
    /// almost copied from OracleDescriptionGenerator,
    /// modified for escaping table description
    /// </summary>
    public class RedshiftDescriptionGenerator : GenericDescriptionGenerator
    {
        private readonly IQuoter _quoter;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.Generators.Redshift.RedshiftDescriptionGenerator"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor sets up the description generator specifically for Amazon Redshift by
        /// initializing the <see cref="FluentMigrator.Runner.Generators.Redshift.RedshiftQuoter"/>
        /// to handle SQL quoting and escaping requirements.
        /// </remarks>
        public RedshiftDescriptionGenerator()
        {
            _quoter = new RedshiftQuoter();
        }

        /// <summary>
        /// Gets the <see cref="IQuoter"/> instance used for quoting
        /// and escaping SQL identifiers and values specific to Amazon Redshift.
        /// </summary>
        /// <remarks>
        /// This property provides access to the quoting logic implemented by the
        /// <see cref="FluentMigrator.Runner.Generators.Redshift.RedshiftQuoter"/> class,
        /// which ensures that SQL identifiers and values are correctly escaped and quoted
        /// according to Redshift's requirements.
        /// </remarks>
        protected IQuoter Quoter => _quoter;

        #region Constants

        private const string TableDescriptionTemplate = "COMMENT ON TABLE {0} IS '{1}';";
        private const string ColumnDescriptionTemplate = "COMMENT ON COLUMN {0}.{1} IS '{2}';";

        #endregion

        private string GetFullTableName(string schemaName, string tableName)
        {
            return string.IsNullOrEmpty(schemaName)
               ? Quoter.QuoteTableName(tableName)
               : $"{Quoter.QuoteSchemaName(schemaName)}.{Quoter.QuoteTableName(tableName)}";
        }

        /// <inheritdoc />
        protected override string GenerateTableDescription(
            string schemaName, string tableName, string tableDescription)
        {
            if (string.IsNullOrEmpty(tableDescription))
                return string.Empty;

            return string.Format(TableDescriptionTemplate, GetFullTableName(schemaName, tableName), tableDescription.Replace("'", "''"));
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
                Quoter.QuoteColumnName(columnName),
                columnDescription.Replace("'", "''"));
        }
    }
}
