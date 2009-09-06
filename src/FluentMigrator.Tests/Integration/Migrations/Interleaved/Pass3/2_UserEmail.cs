namespace FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass3
{
    [Migration(200909060935)]
    public class UserEmail : Migration
    {
        public override void Up()
        {
            Create.Column("Email").OnTable("User").AsString();
        }

        public override void Down()
        {
            Delete.Column("Email").FromTable("User");
        }
    }
}