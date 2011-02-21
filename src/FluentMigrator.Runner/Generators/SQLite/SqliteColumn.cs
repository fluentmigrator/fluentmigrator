

namespace FluentMigrator.Runner.Generators.SQLite
{
    using System;
    using System.Data;
    using FluentMigrator.Model;
    using FluentMigrator.Runner.Generators.Base;
    using FluentMigrator.Runner.Generators.Generic;

	class SqliteColumn : ColumnBase
	{
        protected override bool ShouldSeperatePrimaryKeyAndIdentity { get { return false; } }

		public SqliteColumn() : base(new SqliteTypeMap(), new SqliteQuoter())
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

        protected override string FormatPrimaryKey(ColumnDefinition column)
        {
            if (!column.IsPrimaryKey)
                return string.Empty;
            //Assume that if its IDENTITY and PRIMARY KEY, the it should be an AUTOINCREMENT column
            return column.IsIdentity ? "PRIMARY KEY AUTOINCREMENT" : "PRIMARY KEY";
        }

        protected override string FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.CurrentDateTime:
                    return "CURRENT_TIMESTAMP";
            }

            return null;
        }
	}
}
