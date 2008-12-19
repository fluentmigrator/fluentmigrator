using System;

namespace FluentMigrator.Tests
{
	[Migration(1)]
	public class TestMigration : Migration
	{
		public override void Up()
		{
			Create.Table("Users")
				.WithColumn("UserId").AsInt32().Identity().PrimaryKey()
				.WithColumn("UserName").AsString(32).NotNullable()
				.WithColumn("Password").AsString(32).NotNullable();

			Create.Column("Foo").OnTable("Users").AsInt16().Indexed();

			Create.ForeignKey("Foo").FromTable("Users").ForeignColumn("GroupId").ToTable("Groups").PrimaryColumn("GroupId");

			Create.Index().OnTable("Users")
				.OnColumn("UserName").Ascending()
				.OnColumn("Password").Descending();

			Rename.Table("Foo").To("Bar");
			Rename.Column("Fizz").To("Buzz");
		}

		public override void Down()
		{
			Delete.ForeignKey("FK_Foo");

			Delete.ForeignKey().FromTable("Users").ForeignColumn("GroupId").ToTable("Groups").PrimaryColumn("GroupId");

			Delete.Column("Foo").FromTable("Users");
			Delete.Table("Users");
		}
	}
}