namespace FluentMigrator.Tests.Integration.Migrations.Tagged
{
    [Migration(3)]
    public class NormalTable : Migration
    {
        private const string TableName = "NormalTable";

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