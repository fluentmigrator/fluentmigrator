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
    public class SnowflakeDescriptionGenerator : GenericDescriptionGenerator
    {
        public SnowflakeDescriptionGenerator(SnowflakeQuoter quoter)
        {
            Quoter = quoter;
        }

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
