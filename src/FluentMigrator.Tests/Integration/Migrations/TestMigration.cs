namespace FluentMigrator.Tests.Integration.Migrations
{
	[Migration(1)]
	public class TestMigration : Migration
	{
		public override void Up()
		{
			Create.Table("Users")
				.WithColumn("UserId").AsInt32().Identity().PrimaryKey()
				.WithColumn("GroupId").AsInt32().NotNullable()
				.WithColumn("UserName").AsString(32).NotNullable()
				.WithColumn("Password").AsString(32).NotNullable();

			Create.Table("Groups")
				.WithColumn("GroupId").AsInt32().Identity().PrimaryKey()
				.WithColumn("Name").AsString(32).NotNullable();

			Create.Column("Foo").OnTable("Users").AsInt16().Indexed().WithDefaultValue(1);

			//commenting out the FK so it passes the sqlite tests
			//Create.ForeignKey("FK_Foo").FromTable("Users").ForeignColumn("GroupId").ToTable("Groups").PrimaryColumn("GroupId");

			Create.Table("Foo")
				.WithColumn("Fizz").AsString(32);

			Rename.Table("Foo").To("Bar");

			//does not work in sqlite
			//Rename.Column("Fizz").OnTable("Bar").To("Buzz");

			//Insert.IntoTable("Users").Row(new { UserName = "Data1", Password = "Data2" });
		}

		public override void Down()
		{
			//Delete.ForeignKey("FK_Foo").OnTable("Users");

			Delete.Table("Bar");
			Delete.Table("Users");
			Delete.Table("Groups");
		}
	}
}