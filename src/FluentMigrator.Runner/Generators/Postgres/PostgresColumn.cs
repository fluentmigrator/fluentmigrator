using System;
using System.Data;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.Postgres
{
	internal class PostgresColumn : ColumnBase
	{
		public PostgresColumn() : base(new PostgresTypeMap(), new PostgresQuoter()) { }

		protected override string FormatIdentity(ColumnDefinition column)
		{
			return string.Empty;
		}

		public override string AddPrimaryKeyConstraint(string tableName, System.Collections.Generic.IEnumerable<ColumnDefinition> primaryKeyColumns)
		{
			string pkName = GetPrimaryKeyConstraintName(primaryKeyColumns, tableName);

			string cols = string.Empty;
			bool first = true;
			foreach (var col in primaryKeyColumns)
			{
				if (first)
					first = false;
				else
					cols += ",";
				cols += Quoter.QuoteColumnName(col.Name);
			}

			if (string.IsNullOrEmpty(pkName))
				return string.Format(", PRIMARY KEY ({0})", cols);

			return string.Format(", {0}PRIMARY KEY ({1})", pkName, cols);
		}

		protected override string FormatSystemMethods(SystemMethods systemMethod)
		{
			switch (systemMethod)
			{
				case SystemMethods.NewGuid:
					//need to run the script share/contrib/uuid-ossp.sql to install the uuid_generate4 function
					return "uuid_generate_v4()";
				case SystemMethods.CurrentDateTime:
					return "now()";
			}

			throw new NotImplementedException();
		}

		protected override string FormatType(ColumnDefinition column)
		{
			if (column.IsIdentity)
			{
				if (column.Type == DbType.Int64)
					return "bigserial";
				return "serial";
			}

			return base.FormatType(column);
		}

		public string GetColumnType(ColumnDefinition column)
		{
			return FormatType(column);
		}
	}
}