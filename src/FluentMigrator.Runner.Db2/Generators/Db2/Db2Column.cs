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
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.DB2
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;

    internal class Db2Column : ColumnBase<Db2TypeMap>
    {
        public Db2Column(IQuoter quoter)
            : base(new Db2TypeMap(), quoter)
        {
            ClauseOrder = new List<Func<ColumnDefinition, string>> { FormatString, FormatType, FormatCCSID, FormatNullable, FormatDefaultValue, FormatIdentity };
            AlterClauseOrder = new List<Func<ColumnDefinition, string>> { FormatType, FormatCCSID, FormatNullable, FormatDefaultValue, FormatIdentity };
        }

        public List<Func<ColumnDefinition, string>> AlterClauseOrder
        {
            get; set;
        }

        public string FormatAlterDefaultValue(string column, object defaultValue)
        {
            return Quoter.QuoteValue(defaultValue);
        }

        public string GenerateAlterClause(ColumnDefinition column)
        {
            if (column.IsIdentity)
            {
                throw new NotSupportedException("Altering an identity column is not supported.");
            }

            var alterClauses = AlterClauseOrder.Aggregate(new StringBuilder(), (acc, newRow) =>
            {
                var clause = newRow(column);
                if (acc.Length == 0)
                {
                    acc.Append(newRow(column));
                }
                else if (!string.IsNullOrEmpty(clause))
                {
                    acc.Append(clause.PadLeft(clause.Length + 1));
                }

                return acc;
            });

            return string.Format(
                "ALTER COLUMN {0} SET DATA TYPE {1}",
                Quoter.QuoteColumnName(column.Name),
                alterClauses);
        }

        protected virtual string FormatCCSID(ColumnDefinition column)
        {
            if (column.Type == null)
            {
                return string.Empty;
            }

            var dbType = (DbType)column.Type;

            if (DbType.String.Equals(dbType) || DbType.StringFixedLength.Equals(dbType))
            {
                // Force UTF-16 on double-byte character types.
                return "CCSID 1200";
            }

            return string.Empty;
        }

        protected override string FormatDefaultValue(ColumnDefinition column)
        {
            var isCreate = column.GetAdditionalFeature("IsCreateColumn", false);

            if (isCreate && (column.DefaultValue is ColumnDefinition.UndefinedDefaultValue))
            {
                return "DEFAULT";
            }

            if (column.DefaultValue is ColumnDefinition.UndefinedDefaultValue)
            {
                return string.Empty;
            }

            var method = Quoter.QuoteValue(column.DefaultValue);
            if (string.IsNullOrEmpty(method))
            {
                return string.Empty;
            }

            return "DEFAULT " + method;
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? "AS IDENTITY" : string.Empty;
        }

        protected override string FormatNullable(ColumnDefinition column)
        {
            if (column.IsNullable.HasValue && column.IsNullable.Value)
            {
                return string.Empty;
            }

            return "NOT NULL";
        }
    }
}
