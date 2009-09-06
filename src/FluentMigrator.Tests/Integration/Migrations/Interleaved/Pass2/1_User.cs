namespace FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass2
{
	[Migration(200909060930)]
	public class User : Migration
	{
		public override void Up()
		{
			Create.Table("User")
				.WithColumn("Id").AsInt32().PrimaryKey().Identity()
				.WithColumn("Name").AsString();
		}

		public override void Down()
		{
			Delete.Table("User");
		}
	}
}