using System.Collections.Generic;
using System.Text;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
	class OracleColumn : ColumnBase
	{
		public OracleColumn() : base(new OracleTypeMap(), new ConstantFormatter())
		{
		}

		public override string Generate(ColumnDefinition column)
		{
			var sb = new StringBuilder();

			sb.Append(column.Name);
			sb.Append(" ");
			sb.Append(GetTypeMap(column.Type.Value, column.Size, column.Precision));

			//Oracle requires Default before Not null
			if (!(column.DefaultValue is ColumnDefinition.UndefinedDefaultValue))
			{
				sb.Append(" DEFAULT ");
				sb.Append(Constant.Format(column.DefaultValue));
			}

			if (!column.IsNullable)
			{
				sb.Append(" NOT NULL");
			}

			if (column.IsIdentity)
			{
				//todo: would like to throw a warning here
			}

			if (column.IsPrimaryKey)
			{
				sb.Append(" PRIMARY KEY");
			}

			return sb.ToString();
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
