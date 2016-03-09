using System;

namespace FluentMigrator.Tests.Integration.Migrations.Tagged
{
    [Migration(1)]
    public class TenantATable : TenantABaseMigration
    {
        private const string TableName = "TenantATable";

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