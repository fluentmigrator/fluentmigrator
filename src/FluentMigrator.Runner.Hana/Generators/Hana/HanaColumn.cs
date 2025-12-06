using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Exceptions;
using FluentMigrator.Generation;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.Hana
{
    internal class HanaColumn : ColumnBase<HanaTypeMap>
    {
        private const int HanaObjectNameMaxLength = 30;

        public HanaColumn(IQuoter quoter)
            : base(new HanaTypeMap(), quoter)
        {

            var a = ClauseOrder.IndexOf(FormatDefaultValue);
            var b = ClauseOrder.IndexOf(FormatNullable);

            // Hana requires DefaultValue before nullable
            if (a <= b) return;

            ClauseOrder[b] = FormatDefaultValue;
            ClauseOrder[a] = FormatNullable;
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? "GENERATED ALWAYS AS IDENTITY" : string.Empty;
        }


        protected override string FormatNullable(ColumnDefinition column)
        {
            if (!(column.DefaultValue is ColumnDefinition.UndefinedDefaultValue))
                return string.Empty;

            return column.IsNullable.HasValue
                ? (column.IsNullable.Value ? "NULL" : "NOT NULL")
                : string.Empty;
        }

        protected override string GetPrimaryKeyConstraintName(IEnumerable<ColumnDefinition> primaryKeyColumns, string tableName)
        {
            if (primaryKeyColumns == null)
                throw new ArgumentNullException(nameof(primaryKeyColumns));
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var primaryKeyName = primaryKeyColumns.First().PrimaryKeyName;

            if (string.IsNullOrEmpty(primaryKeyName))
            {
                return string.Empty;
            }

            if (primaryKeyName.Length > HanaObjectNameMaxLength)
                throw new DatabaseOperationNotSupportedException(
                    string.Format(
                        "Hana does not support length of primary key name greater than {0} characters. Reduce length of primary key name. ({1})",
                        HanaObjectNameMaxLength, primaryKeyName));

            var result = string.Format("CONSTRAINT {0} ", Quoter.QuoteConstraintName(primaryKeyName));

            return result;
        }

        public override string AddPrimaryKeyConstraint(string tableName, IEnumerable<ColumnDefinition> primaryKeyColumns)
        {
            var keyColumns = string.Join(", ", primaryKeyColumns.Select(x => Quoter.QuoteColumnName(x.Name)).ToArray());

            return string.Format(", PRIMARY KEY ({0})", keyColumns);
        }
    }
}
