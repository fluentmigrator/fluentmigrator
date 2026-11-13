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

using FluentMigrator.SqlServer;

namespace FluentMigrator.Tests.Integration.CreationScript.Example
{
    /// <summary>
    /// FluentMigrator.Extensions.SqlServer. Each provider's extensions are in a file of their own, because several
    /// share names (Include, Filter) and would be ambiguous together.
    /// </summary>
    [Migration(20, "SQL Server extensions")]
    public class M0020_SqlServer : ForwardOnlyMigration
    {
        public override void Up()
        {
            var sqlServer = IfDatabase(Db.SqlServer);

            sqlServer.Create.Schema("audit").Authorization("dbo");

            sqlServer.Create.Table("Document").InSchema("audit")
                .WithColumn("Id").AsInt64().NotNullable().Identity(1000, 1)
                .WithColumn("RowId").AsGuid().NotNullable().RowGuid().WithDefault(SystemMethods.NewSequentialId)
                .WithColumn("Title").AsString(200).NotNullable()
                .WithColumn("Extra").AsString(100).Nullable().Sparse();

            sqlServer.Create.PrimaryKey("PK_Document").OnTable("Document").WithSchema("audit").Column("Id").NonClustered();

            sqlServer.Create.Index("CIX_Document_Title").OnTable("Document").InSchema("audit")
                .OnColumn("Title").Ascending()
                .WithOptions().Clustered();

            sqlServer.Create.Index("IX_Document_Extra").OnTable("Document").InSchema("audit")
                .OnColumn("Extra").Ascending()
                .WithOptions().Filter("[Extra] IS NOT NULL")
                .Include("Title");

            sqlServer.Insert.IntoTable("Document").InSchema("audit").WithIdentityInsert()
                .Row(new { Id = 1, Title = "Charter" });
            sqlServer.Insert.IntoTable("Document").InSchema("audit")
                .Row(new { Title = "Minutes" });
        }
    }
}
