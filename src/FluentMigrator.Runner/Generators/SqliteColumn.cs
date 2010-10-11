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

		public override string Generate(ColumnDefinition column)
		{
			var sb = new StringBuilder();

			sb.Append(column.Name);
			sb.Append(" ");

			if (!column.IsIdentity)
			{
				if (column.Type.HasValue)
				{
					sb.Append(GetTypeMap(column.Type.Value, column.Size, column.Precision));
				}
				else
				{
					sb.Append(column.CustomType);
				}
			}
			else
			{
				sb.Append(GetTypeMap(DbType.Int32, column.Size, column.Precision));
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
				sb.Append(" IDENTITY");
			}

			if (column.IsPrimaryKey)
			{
				sb.Append(" PRIMARY KEY");
			}

			//Assume that if its IDENTITY and PRIMARY KEY, the it should be an AUTOINCREMENT column
			sb.Replace(" IDENTITY PRIMARY KEY", " PRIMARY KEY AUTOINCREMENT");

			return sb.ToString();
		}
	}
}
