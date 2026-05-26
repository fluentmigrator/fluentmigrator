using FluentMigrator;

namespace AspireFluentMigrator.MigrationService.Migrations;

[Migration(1, "Create initial schema")]
public class M001_CreateInitialSchema : Migration
{
    public override void Up()
    {
        Create.Table("entries")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);
    }

    public override void Down()
    {
        Delete.Table("entries");
    }
}
