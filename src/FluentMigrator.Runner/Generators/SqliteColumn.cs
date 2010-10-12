using System;
using System.Data;
using System.Text;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
	class SqliteColumn : ColumnBase
	{
		public SqliteColumn() : base(new SqliteTypeMap(), new ConstantFormatter())
		{
		}

		protected override string FormatIdentity(ColumnDefinition column)
		{
			if (!column.IsIdentity)
				return string.Empty;

			//Assume that if its IDENTITY and PRIMARY KEY, the it should be an AUTOINCREMENT column
			return !column.IsPrimaryKey ? "IDENTITY" : string.Empty;
		}

		protected override string FormatPrimaryKey(ColumnDefinition column)
		{
			if (!column.IsPrimaryKey)
				return string.Empty;

			//Assume that if its IDENTITY and PRIMARY KEY, the it should be an AUTOINCREMENT column
			return !column.IsIdentity ? "PRIMARY KEY" : "PRIMARY KEY AUTOINCREMENT";
		}

		protected override string FormatType(ColumnDefinition column)
		{
			if (column.IsIdentity)
				return GetTypeMap(DbType.Int32, column.Size, column.Precision);

            return base.FormatType(column);
		}
	}
}
