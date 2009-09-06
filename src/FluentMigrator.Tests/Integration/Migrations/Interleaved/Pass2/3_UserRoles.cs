namespace FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass2
{
	[Migration(200909060953)]
	public class UserToRole : Migration
	{
		public override void Up()
		{
			Create.Table("UserRoles")
				.WithColumn("User_id").AsInt64().NotNullable()
				.WithColumn("Role_id").AsInt64().NotNullable();
		}

		public override void Down()
		{
			Delete.Table("UserRoles");
		}
	}
}