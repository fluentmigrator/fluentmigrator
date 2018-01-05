namespace FluentMigrator.Tests.Integration.Migrations
{
    [Migration(6, BreakingChange = true)]
    public class TestBreakingMigration : Migration
    {
        public override void Up()
        {
        }

        public override void Down()
        {
        }
    }
}