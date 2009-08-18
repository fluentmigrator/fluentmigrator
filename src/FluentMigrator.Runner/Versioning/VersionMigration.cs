using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;

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
                .Row(new { CurrentVersion = 0, PreviousVersion = 0, LastUpdated = DateTime.UtcNow.ToString() });
        }

        public override void Down()
        {
            Delete.Table(VersionInfo.TABLE_NAME);
        }
    }
}
