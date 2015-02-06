
namespace FluentMigrator.Tests.Integration.Migrations.Firebird.FirstVersion
{
    [Migration(1)]
    public class VersionOneSimpleTableMigration : Migration
    {
        public override void Up()
        {
            Create.Table("SIMPLE")
                .WithColumn("ID").AsInt32().PrimaryKey()
                .WithColumn("COL_STR").AsString(10);
        }

        public override void Down()
        {
            Delete.Table("SIMPLE");
        }
    }
}
