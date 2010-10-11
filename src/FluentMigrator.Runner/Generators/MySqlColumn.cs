using System.Text;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
	class MySqlColumn : ColumnBase
	{
		public MySqlColumn() : base(new MySqlTypeMap(), new ConstantFormatter())
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
				sb.Append(" AUTO_INCREMENT");
			}

			if (column.IsPrimaryKey)
			{
				sb.Append(string.Format(", PRIMARY KEY (`{0}`)", column.Name));
			}

			return sb.ToString();
		}


	}
}
