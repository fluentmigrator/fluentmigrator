using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Exceptions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.Oracle
{
    internal class OracleColumn : ColumnBase
    {
        private const int OracleObjectNameMaxLength = 30;

        public OracleColumn(IQuoter quoter)
            : base(new OracleTypeMap(), quoter)
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

		protected override string FormatNullable(ColumnDefinition column)
		{
			//Creates always return Not Null unless is nullable is true
			if (column.ModificationType == ColumnModificationType.Create) {
				if (column.IsNullable.HasValue && column.IsNullable.Value) {
					return string.Empty;
				}
				else {
					return "NOT NULL";
				}
			}

			//alter only returns "Not Null" if IsNullable is explicitly set 
			if (column.IsNullable.HasValue) {
				return column.IsNullable.Value ? "NULL" : "NOT NULL";
			}
			else {
				return String.Empty;
			}

		}

        protected override string FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.NewGuid:
                    return "sys_guid()";
                case SystemMethods.CurrentDateTime:
                    return "CURRENT_TIMESTAMP";
                case SystemMethods.CurrentUser:
                    return "USER";
            }

            throw new NotImplementedException();
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

            var result = string.Format("CONSTRAINT {0} ", Quoter.QuoteConstraintName(primaryKeyName));
            return result;
        }
    }
}