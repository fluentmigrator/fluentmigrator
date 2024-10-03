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

using FluentMigrator.Model;
using FluentMigrator.SqlServer;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    internal class SqlServer2005Column : SqlServer2000Column
    {
        public SqlServer2005Column(ISqlServerTypeMap typeMap, IQuoter quoter)
            : base(typeMap, quoter)
        {
            ClauseOrder.Add(FormatRowGuid);
        }

        /// <inheritdoc />
        protected override string FormatNullable(ColumnDefinition column)
        {
            if (column.IsNullable == true && column.Type == null && !string.IsNullOrEmpty(column.CustomType) && column.Expression == null)
            {
                return "NULL";
            }

            return base.FormatNullable(column);
        }

        /// <summary>
        /// Add <c>ROWGUIDCOL</c> when <see cref="SqlServerExtensions.RowGuidColumn"/> is set.
        /// </summary>
        /// <param name="column">The column to create the definition part for</param>
        /// <returns>The generated SQL string part</returns>
        protected virtual string FormatRowGuid(ColumnDefinition column)
        {
            if (column.AdditionalFeatures.ContainsKey(SqlServerExtensions.RowGuidColumn))
            {
                return "ROWGUIDCOL";
            }

            return string.Empty;
        }
    }
}
