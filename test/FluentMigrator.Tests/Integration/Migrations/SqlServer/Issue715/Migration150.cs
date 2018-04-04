#region License
// Copyright (c) 2018, FluentMigrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

namespace FluentMigrator.Tests.Integration.Migrations.SqlServer.Issue715
{
    [Migration(150)]
    public class Migration150 : Migration
    {
        public override void Up()
        {
            Create.Table("LicenseKeys").InSchema("licensing")
                .WithColumn("LicenseId").AsGuid().PrimaryKey()
                .WithColumn("IsTr]ial").AsBoolean().WithDefaultValue(false);
        }

        public override void Down()
        {
            Delete.Table("LicenseKeys").InSchema("licensing");
        }
    }
}
