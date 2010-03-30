using System;
using FluentMigrator.VersionTableInfo;

namespace FluentMigrator.Runner.Versioning
{
	public class VersionMigration : Migration
	{
		private IVersionTableMetaData _versionTableMetaData;

		public VersionMigration(IVersionTableMetaData versionTableMetaData)
		{
			_versionTableMetaData = versionTableMetaData;
		}

		public override void Up()
		{
			Create.Table(_versionTableMetaData.TableName)
				.WithColumn(_versionTableMetaData.ColumnName).AsInt64().NotNullable();
		}

		public override void Down()
		{
			Delete.Table(_versionTableMetaData.TableName);
		}
	}

	internal static class DateTimeExtensions
	{
		public static string ToISO8601(this DateTime dateTime)
		{
			return dateTime.ToString("u").Replace("Z", "");
		}
	}
}
