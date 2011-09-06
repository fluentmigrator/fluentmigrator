using System;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.Oracle
{
    internal class OracleColumn : ColumnBase
    {
        public OracleColumn() : base(new OracleTypeMap(), new OracleQuoter())
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

        protected override string FormatDefaultValue(ColumnDefinition column)
        {
            if (column.DefaultValue is FunctionValue)
            {
                return "DEFAULT " + column.DefaultValue;
            }

            return base.FormatDefaultValue(column);
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            if (column.IsIdentity)
            {
                throw new DatabaseOperationNotSupportedException("Oracle does not support identity columns. Please use a SEQUENCE instead");
            }
            return string.Empty;
        }

        protected override object FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.NewGuid:
                    return (FunctionValue) "sys_guid()";
                case SystemMethods.CurrentDateTime:
                    return (FunctionValue) "sysdate";
            }

            throw new NotImplementedException();
        }
    }
}