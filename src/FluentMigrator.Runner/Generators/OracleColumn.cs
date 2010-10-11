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
	}
}
