using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.Oracle
{
    internal class OracleColumn : ColumnBase
    {
        private const int OracleObjectNameMaxLength = 30;

        public OracleColumn()
            : base(new OracleTypeMap(), new OracleQuoter())
        {
            int a = ClauseOrder.IndexOf(FormatDefaultValue);
            int b = ClauseOrder.IndexOf(FormatNullable);

            // Oracle requires DefaultValue before nullable
            if (a > b)
            {
                ClauseOrder[b] = FormatDefaultValue;
                ClauseOrder[a] = FormatNullable;
            }
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            if (column.IsIdentity)
            {
                throw new DatabaseOperationNotSupportedException("Oracle does not support identity columns. Please use a SEQUENCE instead");
            }
            return string.Empty;
        }

        protected override string FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.NewGuid:
                    return "sys_guid()";
            }

            return null;
        }

        protected override string GetPrimaryKeyConstraintName(IEnumerable<ColumnDefinition> primaryKeyColumns, string tableName)
        {
            if (primaryKeyColumns == null)
                throw new ArgumentNullException("primaryKeyColumns");
            if (tableName == null)
                throw new ArgumentNullException("tableName");

            var primaryKeyName = primaryKeyColumns.First().PrimaryKeyName;

            if (string.IsNullOrEmpty(primaryKeyName))
            {
                return string.Empty;
            }

            if (primaryKeyName.Length > OracleObjectNameMaxLength)
                throw new DatabaseOperationNotSupportedException(
                    string.Format(
                        "Oracle does not support length of primary key name greater than {0} characters. Reduce length of primary key name. ({1})",
                        OracleObjectNameMaxLength, primaryKeyName));

            var result = string.Format("CONSTRAINT {0} ", Quoter.QuoteIndexName(primaryKeyName));
            return result;
        }
    }
}