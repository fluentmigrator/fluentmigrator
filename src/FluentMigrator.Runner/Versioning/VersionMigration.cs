using System;

namespace FluentMigrator.Runner.Versioning
{
    public class VersionMigration : Migration
    {
        public override void Up()
        {
            Create.Table(VersionInfo.TABLE_NAME)
                .WithColumn("CurrentVersion").AsInt64().NotNullable()
                .WithColumn("PreviousVersion").AsInt64().NotNullable()
                .WithColumn("LastUpdated").AsDateTime().NotNullable();

            Insert.IntoTable(VersionInfo.TABLE_NAME)
                .Row(new { CurrentVersion = 0, PreviousVersion = 0, LastUpdated = DateTime.UtcNow.ToISO8601() });
        }

        public override void Down()
        {
            Delete.Table(VersionInfo.TABLE_NAME);
        }
    }

    public static class DateTimeExtensions
    {
        public static string ToISO8601(this DateTime dateTime)
        {
            return dateTime.ToString("u").Replace("Z", ""); // to support sqlite
        }
    }
}
