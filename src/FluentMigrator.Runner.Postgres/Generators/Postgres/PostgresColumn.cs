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
using System.Collections.Generic;
using System.Data;
using System.Linq;

using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Generators.Postgres
{
    internal class PostgresColumn : ColumnBase<IPostgresTypeMap>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresColumn"/> class.
        /// </summary>
        /// <param name="quoter">The Postgres quoter.</param>
        /// <param name="typeMap">The Postgres type map.</param>
        public PostgresColumn([NotNull] PostgresQuoter quoter, IPostgresTypeMap typeMap)
            : base(typeMap, quoter)
        {
            AlterClauseOrder = new List<Func<ColumnDefinition, string>> { FormatAlterType, FormatAlterNullable };
        }

        public string FormatAlterDefaultValue(string column, object defaultValue)
        {
            string formatDefaultValue = FormatDefaultValue(new ColumnDefinition { Name = column, DefaultValue = defaultValue });

            return string.Format("SET {0}", formatDefaultValue);
        }

        private string FormatAlterNullable(ColumnDefinition column)
        {
            if (!column.IsNullable.HasValue)
                return "";

            if (column.IsNullable.Value)
                return "DROP NOT NULL";

            return "SET NOT NULL";
        }

        private string FormatAlterType(ColumnDefinition column)
        {
            var collation = FormatCollation(column);
            return $"TYPE {GetColumnType(column)}{(string.IsNullOrWhiteSpace(collation) ? string.Empty : " " + collation)}";
        }

        protected IList<Func<ColumnDefinition, string>> AlterClauseOrder { get; set; }

        public string GenerateAlterClauses(ColumnDefinition column)
        {
            var clauses = new List<string>();
            foreach (var action in AlterClauseOrder)
            {
                string columnClause = action(column);
                if (!string.IsNullOrEmpty(columnClause))
                    clauses.Add(string.Format("ALTER {0} {1}", Quoter.QuoteColumnName(column.Name), columnClause));
            }

            return string.Join(", ", clauses.ToArray());
        }

        /// <inheritdoc />
        protected override string FormatNullable(ColumnDefinition column)
        {
            if (column.IsNullable == true && column.Type == null && !string.IsNullOrEmpty(column.CustomType))
            {
                return "NULL";
            }

            return base.FormatNullable(column);
        }

        /// <inheritdoc />
        protected override string FormatIdentity(ColumnDefinition column)
        {
            return string.Empty;
        }

        /// <inheritdoc />
        public override string AddPrimaryKeyConstraint(string tableName, IEnumerable<ColumnDefinition> primaryKeyColumns)
        {
            var columnDefinitions = primaryKeyColumns.ToList();
            string pkName = GetPrimaryKeyConstraintName(columnDefinitions, tableName);

            string cols = string.Empty;
            bool first = true;
            foreach (var col in columnDefinitions)
            {
                if (first)
                    first = false;
                else
                    cols += ",";
                cols += Quoter.QuoteColumnName(col.Name);
            }

            if (string.IsNullOrEmpty(pkName))
                return string.Format(", PRIMARY KEY ({0})", cols);

            return string.Format(", {0}PRIMARY KEY ({1})", pkName, cols);
        }

        protected void FormatTypeValidator(ColumnDefinition column)
        {
            if (column.Type == DbType.DateTimeOffset && (column.Precision < 0 || column.Precision > 6))
            {
                throw new ArgumentOutOfRangeException($"Postgres {nameof(DbType.DateTimeOffset)} data type 'timestamp[(p)]' with time zone' supports allowed range from 0 to 6. " +
                    $"See: https://www.postgresql.org/docs/12/datatype-datetime.html");
            }
        }

        /// <inheritdoc />
        protected override string FormatType(ColumnDefinition column)
        {
            FormatTypeValidator(column);

            if (column.IsIdentity)
            {
                if (column.Type == DbType.Int64)
                    return "bigserial";
                return "serial";
            }
            

            return ColumnBaseFormatType(column);
        }

        protected string ColumnBaseFormatType(ColumnDefinition column)
        {
            return base.FormatType(column);
        }

        public string GetColumnType(ColumnDefinition column)
        {
            return FormatType(column);
        }

        /// <inheritdoc />
        protected override string FormatExpression(ColumnDefinition column)
        {
            return column.Expression == null ? null : $"GENERATED ALWAYS AS ({column.Expression}) STORED";
        }
    }
}
