using System;

namespace FluentMigrator.Tests.Integration.Migrations.Tagged
{

    [Tags("TenantB")]
    [Migration(2)]
    public class TenantBTable : Migration
    {
        private const string TableName = "TenantBTable";

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