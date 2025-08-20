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
using FluentMigrator.Model;
using FluentMigrator.SqlServer;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    internal class SqlServer2008Column : SqlServer2005Column
    {
        public SqlServer2008Column(ISqlServerTypeMap typeMap, IQuoter quoter)
           : base(typeMap, quoter)
        {
            ClauseOrder.Add(FormatSparse);
        }

        /// <summary>
        /// Add <c>SPARSE</c> when <see cref="SqlServerExtensions.SparseColumn"/> is set.
        /// </summary>
        /// <param name="column">The column to create the definition part for</param>
        /// <returns>The generated SQL string part</returns>
        protected virtual string FormatSparse(ColumnDefinition column)
        {
            if (column.AdditionalFeatures.ContainsKey(SqlServerExtensions.SparseColumn)
                && (column.IsNullable ?? false))
            {
                return "SPARSE";
            }

            return string.Empty;
        }

    }
}
