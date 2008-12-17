using System;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;

namespace FluentMigrator.Infrastructure
{
	public static class DefaultMigrationConventions
	{
		public static string GetPrimaryKeyName(CreateTableExpression expression)
		{
			return "PK_" + expression.TableName;
		}

		public static string GetForeignKeyName(ForeignKeyDefinition foreignKey)
		{
			var sb = new StringBuilder();

			sb.Append("FK_");
			sb.Append(foreignKey.ForeignTable);

			foreach (string foreignColumn in foreignKey.ForeignColumns)
			{
				sb.Append("_");
				sb.Append(foreignColumn);
			}

			sb.Append("_");
			sb.Append(foreignKey.PrimaryTable);

			foreach (string primaryColumn in foreignKey.PrimaryColumns)
			{
				sb.Append("_");
				sb.Append(primaryColumn);
			}

			return sb.ToString();
		}

		public static bool TypeIsMigration(Type type)
		{
			return typeof(IMigration).IsAssignableFrom(type) && type.HasAttribute<MigrationAttribute>();
		}

		public static MigrationMetadata GetMetadataForMigration(Type type)
		{
			var attribute = type.GetOneAttribute<MigrationAttribute>();
			return new MigrationMetadata { Type = type, Version = attribute.Version };
		}
	}
}