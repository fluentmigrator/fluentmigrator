using System;

namespace FluentMigrator.Runner.Versioning
{
    public class VersionMigration : Migration
    {
        public override void Up()
        {
            Create.Table(VersionInfo.TABLE_NAME)
                .WithColumn(VersionInfo.COLUMN_NAME).AsInt64().NotNullable();
        }

        public override void Down()
        {
            Delete.Table(VersionInfo.TABLE_NAME);
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
