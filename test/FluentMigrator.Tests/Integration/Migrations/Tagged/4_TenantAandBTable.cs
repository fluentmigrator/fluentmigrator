using System;

namespace FluentMigrator.Tests.Integration.Migrations.Tagged
{

    [Tags("TenantB", "TenantA")]
    [Migration(4)]
    public class TenantAandBTable : Migration
    {
        private const string TableName = "TenantAandBTable";

        public override void Up()
        {
            Create.Table(TableName)
                .WithColumn("Id").AsInt32();
        }

        public override void Down()
        {
            Delete.Table(TableName);
        }
    }
}