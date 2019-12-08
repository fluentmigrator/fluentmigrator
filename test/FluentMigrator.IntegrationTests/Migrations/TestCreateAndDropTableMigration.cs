namespace FluentMigrator.IntegrationTests.Migrations
{
    // ReSharper disable once UnusedMember.Global
    [Migration(0, "TestCreateAndDropTableMigration")]
    public class TestCreateAndDropTableMigration : Migration
    {
        public override void Up()
        {
            Create.Table("TestTable")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Name").AsString(255).NotNullable().WithDefaultValue("Anonymous");

            Create.Table("TestTable2")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Name").AsString(255).Nullable()
                .WithColumn("TestTableId").AsInt32().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("TestTable");
            Delete.Table("TestTable2");
        }
    }
}
