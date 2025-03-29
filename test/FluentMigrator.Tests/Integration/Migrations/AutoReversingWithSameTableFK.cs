namespace FluentMigrator.Tests.Integration.Migrations
{
    [Migration(7)]
    public class AutoReversingWithSameTableFK : AutoReversingMigration
    {
        public override void Up()
        {
            IfDatabase(t => t != ProcessorIdConstants.SQLite)
                .Create.Table("SameTableFK")
                    .WithColumn("Id").AsInt32().PrimaryKey()
                    .WithColumn("ParentId").AsInt32().Nullable()
                        .ForeignKey("SameTableFK", "Id");

            // Foreign keys are not supported in SQLite
            IfDatabase(ProcessorIdConstants.SQLite)
                .Create.Table("SameTableFK")
                    .WithColumn("Id").AsInt32().PrimaryKey()
                    .WithColumn("ParentId").AsInt32().Nullable();

            Insert.IntoTable("SameTableFK")
                .Row(new { Id = 1 })
                .Row(new { Id = 2 })
                .Row(new { Id = 10, ParentId = 1 })
                .Row(new { Id = 20, ParentId = 2 });
        }
    }
}
