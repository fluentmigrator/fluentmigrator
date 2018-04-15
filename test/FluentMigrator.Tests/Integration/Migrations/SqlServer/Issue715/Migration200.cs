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
    [Migration(200)]
    public class Migration200 : Migration
    {
        public override void Up()
        {
            Delete.DefaultConstraint().OnTable("LicenseKeys").InSchema("licensing").OnColumn("IsTr]ial");
        }

        public override void Down()
        {
        }
    }
}
