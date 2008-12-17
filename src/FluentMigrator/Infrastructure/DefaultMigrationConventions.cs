using System;
using System.Text;
using FluentMigrator.Expressions;

namespace FluentMigrator.Infrastructure
{
	public static class DefaultMigrationConventions
	{
		public static string GetPrimaryKeyName(CreateTableExpression expression)
		{
			return "PK_" + expression.TableName;
		}

		public static string GetForeignKeyName(CreateForeignKeyExpression expression)
		{
			var sb = new StringBuilder();

			sb.Append("FK_");
			sb.Append(expression.ForeignTable);

			foreach (string foreignColumn in expression.ForeignColumns)
			{
				sb.Append(foreignColumn);
				sb.Append("_");
			}

			sb.Append(expression.PrimaryTable);

			foreach (string primaryColumn in expression.PrimaryColumns)
			{
				sb.Append(primaryColumn);
				sb.Append("_");
			}

			sb.Remove(sb.Length - 1, 1);

			return sb.ToString();
		}

		public static bool TypeIsMigration(Type type)
		{
			return typeof(IMigration).IsAssignableFrom(type) && type.HasAttribute<MigrationAttribute>();
		}

		public static long GetMigrationVersion(Type type)
		{
			var attribute = type.GetOneAttribute<MigrationAttribute>();
			return attribute.Version;
		}
	}
}