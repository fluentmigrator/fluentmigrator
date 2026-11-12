#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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

using FluentMigrator.MySql;

namespace FluentMigrator.Tests.Integration.CreationScript.Example
{
    /// <summary>
    /// FluentMigrator.Extensions.MySql. Firebird and Db2 have no extension package.
    /// </summary>
    [Migration(23, "MySQL extensions")]
    public class M0023_MySql : ForwardOnlyMigration
    {
        public override void Up()
        {
            var mySql = IfDatabase(Db.MySql);

            mySql.Create.Table("Session")
                .WithColumn("Token").AsFixedLengthAnsiString(32).NotNullable().PrimaryKey("PK_Session")
                .WithColumn("UserName").AsString(100).NotNullable();

            // InnoDB keeps a B-tree and warns; the statement is still valid.
            mySql.Create.Index("IX_Session_User").OnTable("Session")
                .OnColumn("UserName").Ascending()
                .WithOptions().UsingHash();
        }
    }
}
