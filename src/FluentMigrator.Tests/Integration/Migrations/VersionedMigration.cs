namespace FluentMigrator.Tests.Integration.Migrations
{
	[Migration(2)]
	public class VersionedMigration : Migration
	{
		public override void Up()
		{
			Create.Table("VersionedMigration")
				.WithColumn("Id").AsInt32().Identity().NotNullable().PrimaryKey()
				.WithColumn("PooBah").AsString(32).Nullable();
		}

		public override void Down()
		{
			Delete.Table("VersionedMigration");
		}
	}
}