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
