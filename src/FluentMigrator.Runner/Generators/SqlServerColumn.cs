using System.Text;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
	class SqlServerColumn : ColumnBase
	{
		public SqlServerColumn(ITypeMap typeMap) : base(typeMap, new ConstantFormatter())
		{
		}

		public override string Generate(ColumnDefinition column)
		{
			var sb = new StringBuilder();

			sb.Append(column.Name);
			sb.Append(" ");

			if (column.Type.HasValue)
			{
				sb.Append(GetTypeMap(column.Type.Value, column.Size, column.Precision));
			}
			else
			{
				sb.Append(column.CustomType);
			}

			if (!column.IsNullable)
			{
				sb.Append(" NOT NULL");
			}

			if (!(column.DefaultValue is ColumnDefinition.UndefinedDefaultValue))
			{
				sb.Append(" DEFAULT ");
				sb.Append(Constant.Format(column.DefaultValue));
			}

			if (column.IsIdentity)
			{
				sb.Append(" IDENTITY(1,1)");
			}

			if (column.IsPrimaryKey)
			{
				sb.Append(" PRIMARY KEY CLUSTERED");
			}

			return sb.ToString();
		}
	}
}
