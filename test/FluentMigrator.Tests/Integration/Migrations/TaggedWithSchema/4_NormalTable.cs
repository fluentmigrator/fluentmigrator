namespace FluentMigrator.Tests.Integration.Migrations.TaggedWithSchema
{
    [Migration(4)]
    public class NormalTable : Migration
    {
        private const string TableName = "NormalTable";

        public override void Up()
        {
            Create.Table(TableName).InSchema("TestSchema")
                .WithColumn("Id").AsInt32();
        }

        public override void Down()
        {
            Delete.Table(TableName).InSchema("TestSchema");
        }
    }
}
