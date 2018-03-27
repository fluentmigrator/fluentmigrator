using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.SQLite
{
    internal class SQLiteColumn : ColumnBase
    {
        public SQLiteColumn()
            : base(new SQLiteTypeMap(), new SQLiteQuoter())
        {
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            //SQLite only supports the concept of Identity in combination with a single primary key
            //see: http://www.sqlite.org/syntaxdiagrams.html#column-constraint syntax details
            if (column.IsIdentity && !column.IsPrimaryKey && column.Type != DbType.Int32)
            {
                throw new ArgumentException("SQLite only supports identity on single integer, primary key coulmns");
            }
            return string.Empty;
        }

        public override bool ShouldPrimaryKeysBeAddedSeparately(IEnumerable<ColumnDefinition> primaryKeyColumns)
        {
            //If there are no identity column then we can add as a separate constrint
            if (!primaryKeyColumns.Any(x => x.IsIdentity) && primaryKeyColumns.Any(x => x.IsPrimaryKey)) return true;
            return false;
        }

        protected override string FormatPrimaryKey(ColumnDefinition column)
        {
            if (!column.IsPrimaryKey) return string.Empty;

            return column.IsIdentity ? "PRIMARY KEY AUTOINCREMENT" : string.Empty;
        }

        protected override string FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.CurrentUTCDateTime:
                    return "CURRENT_TIMESTAMP";
                case SystemMethods.CurrentDateTime:
                    return "(datetime('now','localtime'))";
            }

            return null;
        }
    }
}
