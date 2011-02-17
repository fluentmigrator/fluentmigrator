using System.Data;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
	class SqliteColumn : ColumnBase
	{
        protected override bool CanSeperatePrimaryKeyAndIdentity { get { return false; } }

		public SqliteColumn() : base(new SqliteTypeMap(), new ConstantFormatter())
		{
		}

        protected override string FormatName(ColumnDefinition column)
        {
            return string.Format("[{0}]",column.Name);
        }
		protected override string FormatIdentity(ColumnDefinition column)
		{
            //SQLite only supports the concept of Identity in combination with a single primary key
            //see: http://www.sqlite.org/syntaxdiagrams.html#column-constraint syntax details
		    return string.Empty;
		}

		protected override string FormatPrimaryKey(ColumnDefinition column)
		{
			if (!column.IsPrimaryKey)
				return string.Empty;

			//Assume that if its IDENTITY and PRIMARY KEY, the it should be an AUTOINCREMENT column
			return !column.IsIdentity ? "PRIMARY KEY" : "PRIMARY KEY AUTOINCREMENT";
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

		protected override string FormatType(ColumnDefinition column)
		{
			if (column.IsIdentity)
				return GetTypeMap(DbType.Int32, column.Size, column.Precision);

			return base.FormatType(column);
		}
	}
}
