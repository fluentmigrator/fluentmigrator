using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTest
{
    [Migration(000001)]
    public class Base
        : Migration
    {
        public override void Up()
        {
            Create.Table("User")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("FirstName").AsString().Nullable()
                .WithColumn("LastName").AsString().Nullable()
                .WithColumn("EmailAddress").AsString().Nullable()
                .WithColumn("CreatedDate").AsDateTimeOffset().Nullable()
                .WithColumn("UpdatedDate").AsDateTimeOffset().Nullable();
        }
        public override void Down()
        {
            Delete.Table("User");
        }
    }
}
