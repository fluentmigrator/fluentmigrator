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

using System;

using FluentMigrator.Runner.Generators.Generic;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Generators.Postgres
{
    /// <summary>
    /// A generator for creating SQL statements to set descriptions for PostgreSQL tables and columns.
    /// </summary>
    /// <remarks>
    /// This class is adapted from <c>OracleDescriptionGenerator</c>, with modifications to handle
    /// escaping of table descriptions specific to PostgreSQL.
    /// </remarks>
    public class PostgresDescriptionGenerator : GenericDescriptionGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresDescriptionGenerator"/> class.
        /// </summary>
        /// <param name="quoter">
        /// The <see cref="PostgresQuoter"/> instance used for quoting and escaping SQL identifiers and values
        /// specific to PostgreSQL.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="quoter"/> parameter is <c>null</c>.
        /// </exception>
        public PostgresDescriptionGenerator([NotNull] PostgresQuoter quoter)
        {
            Quoter = quoter;
        }

        /// <summary>
        /// Gets the <see cref="IQuoter"/> instance used for quoting and escaping SQL identifiers and values.
        /// </summary>
        /// <remarks>
        /// This property provides access to the quoting mechanism specific to PostgreSQL, enabling
        /// proper handling of table and column names, as well as other SQL identifiers.
        /// </remarks>
        protected IQuoter Quoter { get; }

        #region Constants

        private const string TableDescriptionTemplate = "COMMENT ON TABLE {0} IS '{1}';";
        private const string ColumnDescriptionTemplate = "COMMENT ON COLUMN {0}.{1} IS '{2}';";

        #endregion

        private string GetFullTableName(string schemaName, string tableName)
        {
            return Quoter.QuoteTableName(tableName, schemaName);
        }

        /// <summary>
        /// Generates a SQL statement to set a description for a PostgreSQL table.
        /// </summary>
        /// <param name="schemaName">The name of the schema containing the table.</param>
        /// <param name="tableName">The name of the table for which the description is being generated.</param>
        /// <param name="tableDescription">The description to be set for the table.</param>
        /// <returns>
        /// A SQL statement to set the table description, or an empty string if the <paramref name="tableDescription"/> is null or empty.
        /// </returns>
        /// <remarks>
        /// The generated SQL statement uses the PostgreSQL-specific syntax for setting table comments.
        /// Any single quotes in the <paramref name="tableDescription"/> are escaped to ensure valid SQL syntax.
        /// </remarks>
        protected override string GenerateTableDescription(
            string schemaName, string tableName, string tableDescription)
        {
            if (string.IsNullOrEmpty(tableDescription))
                return string.Empty;

            return string.Format(TableDescriptionTemplate, GetFullTableName(schemaName, tableName), tableDescription.Replace("'", "''"));
        }

        /// <summary>
        /// Generates a SQL statement to set a description for a specific column in a PostgreSQL table.
        /// </summary>
        /// <param name="descriptionName">The name of the description (not used in this implementation).</param>
        /// <param name="schemaName">The schema name of the table containing the column.</param>
        /// <param name="tableName">The name of the table containing the column.</param>
        /// <param name="columnName">The name of the column to describe.</param>
        /// <param name="columnDescription">The description to be applied to the column.</param>
        /// <returns>
        /// A SQL statement to set the description for the specified column, or an empty string
        /// if <paramref name="columnDescription"/> is <c>null</c> or empty.
        /// </returns>
        /// <remarks>
        /// The generated SQL statement uses the PostgreSQL-specific syntax for adding comments
        /// to columns. Single quotes in the column description are escaped to ensure valid SQL.
        /// </remarks>
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
