namespace FluentMigrator.Tests.Integration.Migrations.Invalid
{
	[Migration(2)]
	public class InvalidMigration2 : Migration
	{
		public override void Up()
		{
			Create.ForeignKey()
				.FromTable("Users").ForeignColumn("NonexistentId")
				.ToTable("NonexistentTable").PrimaryColumn("Id");
		}

		public override void Down()
		{
			Delete.ForeignKey("Users_NonexistentId_NonexistentTable_Id");
		}
	}
}
