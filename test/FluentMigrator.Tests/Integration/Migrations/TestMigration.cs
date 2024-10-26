#region License

// 
// Copyright (c) 2007-2024, Fluent Migrator Project
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#endregion

namespace FluentMigrator.Tests.Integration.Migrations
{
    [Migration(1)]
    public class TestMigration : Migration
    {
        public override void Up()
        {
            // SQLite only supports FK's defined in the create statement so
            // we ensure this is the only approach used so that SQLite can
            // successfully tested. At time of implementing, the FK constraint
            // wasn't explicitly used by any tests and so should affect anything.

            Create.Table("Users")
                .WithColumn("UserId").AsInt32().Identity().PrimaryKey()
                .WithColumn("GroupId").AsInt32().NotNullable().ForeignKey("Groups", "GroupId")
                .WithColumn("UserName").AsString(32).NotNullable()
                .WithColumn("Password").AsString(32).NotNullable();

            Create.Table("Groups")
                .WithColumn("GroupId").AsInt32().Identity().PrimaryKey()
                .WithColumn("Name").AsString(32).NotNullable();

            Create.Column("Foo").OnTable("Users").AsInt16().Indexed().WithDefaultValue(1);

            Create.Table("Foo")
                .WithColumn("Fizz").AsString(32);

            Rename.Table("Foo").To("Bar");

            // commented out because this throws not implemented exception
            //Alter.Table( "" ).InSchema( "" ).ToSchema( "" );

            //does not work in sqlite
            //Rename.Column("Fizz").OnTable("Bar").To("Buzz");

            //Insert.IntoTable("Users").Row(new { UserName = "Data1", Password = "Data2" });
        }

        public override void Down()
        {
            //Delete.ForeignKey("FK_Foo").OnTable("Users");

            Schema.Table("Bar").Exists();

            Delete.Table("Bar");
            Delete.Table("Users");
            Delete.Table("Groups");
        }
    }
}
