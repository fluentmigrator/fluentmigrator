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

namespace FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass1
{
    [Migration(200909060953)]
    public class UserToRole : Migration
    {
        public override void Up()
        {
            Create.Table("UserRoles")
                .WithColumn("User_id").AsInt64().NotNullable()
                .WithColumn("Role_id").AsInt64().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("UserRoles");
        }
    }
}
