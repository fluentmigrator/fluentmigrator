using System;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.MySql
{
    internal class MySqlColumn : ColumnBase
    {
        public MySqlColumn()
            : base(new MySqlTypeMap(), new MySqlQuoter())
        {
            ClauseOrder.Add(FormatDescription);
        }

        internal string FormatDefaultValue(object defaultValue)
        {
            string formatDefaultValue = base.FormatDefaultValue(new ColumnDefinition { DefaultValue = defaultValue });
            return formatDefaultValue;
        }

        protected string FormatDescription(ColumnDefinition column)
        {
            return string.IsNullOrEmpty(column.ColumnDescription)
                ? string.Empty
                : string.Format("COMMENT {0}", Quoter.QuoteValue(column.ColumnDescription));
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? "AUTO_INCREMENT" : string.Empty;
        }

        protected override string FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.CurrentDateTime:
                    return "CURRENT_TIMESTAMP";
            }

            throw new NotImplementedException();
        }
    }
}
