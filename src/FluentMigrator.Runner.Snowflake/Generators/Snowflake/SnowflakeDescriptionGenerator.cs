#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using FluentMigrator.Generation;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Snowflake
{
    /// <summary>
    /// A generator for creating SQL statements to set descriptions for tables and columns
    /// specific to the Snowflake database.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="FluentMigrator.Runner.Generators.Generic.GenericDescriptionGenerator"/> 
    /// to provide Snowflake-specific implementations for generating table and column descriptions.
    /// </remarks>
    public class SnowflakeDescriptionGenerator : GenericDescriptionGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.Generators.Snowflake.SnowflakeDescriptionGenerator"/> class.
        /// </summary>
        /// <param name="quoter">
        /// An instance of <see cref="FluentMigrator.Runner.Generators.Snowflake.SnowflakeQuoter"/> 
        /// used to handle SQL quoting specific to the Snowflake database.
        /// </param>
        /// <remarks>
        /// This constructor sets up the generator to create Snowflake-specific SQL statements 
        /// for describing tables and columns.
        /// </remarks>
        public SnowflakeDescriptionGenerator(SnowflakeQuoter quoter)
        {
            Quoter = quoter;
        }

        /// <summary>
        /// Gets the <see cref="FluentMigrator.Runner.Generators.IQuoter"/> instance used for quoting SQL identifiers
        /// and values specific to the Snowflake database.
        /// </summary>
        /// <value>
        /// An instance of <see cref="FluentMigrator.Runner.Generators.Snowflake.SnowflakeQuoter"/> 
        /// that provides Snowflake-specific quoting functionality.
        /// </value>
        /// <remarks>
        /// This property is used internally by the generator to ensure that SQL statements are properly quoted 
        /// according to Snowflake's requirements.
        /// </remarks>
        protected IQuoter Quoter { get; }

        #region Constants

        private const string TableDescriptionTemplate = "COMMENT ON TABLE {0} IS '{1}';";
        private const string ColumnDescriptionTemplate = "COMMENT ON COLUMN {0}.{1} IS '{2}';";

        #endregion

        /// <inheritdoc />
        protected override string GenerateTableDescription(string schemaName, string tableName, string tableDescription)
        {
            if (string.IsNullOrEmpty(tableDescription))
            {
                return string.Empty;
            }

            return string.Format(TableDescriptionTemplate, Quoter.QuoteTableName(tableName, schemaName), tableDescription.Replace("'", "''"));
        }

        /// <inheritdoc />
        protected override string GenerateColumnDescription(
            string descriptionName,
            string schemaName,
            string tableName,
            string columnName,
            string columnDescription)
        {
            if (string.IsNullOrEmpty(columnDescription))
                return string.Empty;

            return string.Format(
                ColumnDescriptionTemplate,
                Quoter.QuoteTableName(tableName, schemaName),
                Quoter.QuoteColumnName(columnName),
                columnDescription.Replace("'", "''"));
        }
    }
}
