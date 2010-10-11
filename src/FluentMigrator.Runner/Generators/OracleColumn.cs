using System;
using System.Collections.Generic;
using System.Text;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
	internal class OracleColumn : ColumnBase
	{
		public OracleColumn() : base(new OracleTypeMap(), new ConstantFormatter())
		{
			int a = ClauseOrder.IndexOf(FormatDefaultValue);
			int b = ClauseOrder.IndexOf(FormatNullable);

			// Oracle requires DefaultValue before nullable
			if (a > b) {
				ClauseOrder[b] = FormatDefaultValue;
				ClauseOrder[a] = FormatNullable;
			}
		}

		protected override string FormatIdentity(ColumnDefinition column)
		{
			if (column.IsIdentity) {
				//todo: would like to throw a warning here
			}
			return string.Empty;
		}

		protected override string FormatPrimaryKey(ColumnDefinition column)
		{
			return column.IsPrimaryKey ? "PRIMARY KEY" : string.Empty;
		}

		/// <summary>
		/// Returns empty string as the constraint for Primary Key. 
		/// Oracle will generate a coinstraint name if none is specified ie. SYS_C008004
		/// Oracle is limited to 30 chars and the constraints must be unique for the enire schema
		/// so there is no way to get an intelligent name using table and column names
		/// </summary>
		/// <param name="primaryKeyColumns"></param>
		/// <param name="tableName"></param>
		/// <returns></returns>
		protected override string GetPrimaryKeyConstraintName(IList<ColumnDefinition> primaryKeyColumns, string tableName)
		{
			return string.Empty;
		}
	}
}
