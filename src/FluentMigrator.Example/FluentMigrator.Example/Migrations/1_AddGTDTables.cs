namespace FluentMigrator.Example.Migrations
{
	[Migration(20090906205342)]
	public class AddGTDTables : Migration
	{
		public override void Up()
		{
			Create.Table("Contexts")
				.WithIdColumn()
				.WithColumn("Name").AsString().NotNullable();

			Create.Table("Projects")
				.WithIdColumn()
				.WithColumn("Name").AsString().NotNullable()
				.WithColumn("Position").AsInt32().NotNullable()
				.WithColumn("Done").AsBoolean().NotNullable();

			Create.Table("Users")
				.WithIdColumn()
				.WithColumn("Name").AsString().NotNullable()
				.WithColumn("Login").AsString().NotNullable()
				.WithColumn("Password").AsString().NotNullable()
				.WithColumn("PasswordSalt").AsString().NotNullable()
				.WithColumn("IsAdmin").AsBoolean().NotNullable();
		}

		public override void Down()
		{
			Delete.Table("Contexts");
			Delete.Table("Projects");
			Delete.Table("Users");
		}
	}
}