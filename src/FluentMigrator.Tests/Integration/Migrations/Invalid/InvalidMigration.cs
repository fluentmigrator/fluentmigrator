namespace FluentMigrator.Tests.Integration.Migrations.Invalid
{
	[Migration(1)]
	public class InvalidMigration : Migration
	{
		public override void Up()
		{
			Create.Table("Users")
				.WithColumn("Id").AsInt32().Identity();
		}

		public override void Down()
		{
			Delete.Table("Users");
		}
	}
}
