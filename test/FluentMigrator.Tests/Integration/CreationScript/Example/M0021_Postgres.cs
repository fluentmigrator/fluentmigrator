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

using FluentMigrator.Postgres;

namespace FluentMigrator.Tests.Integration.CreationScript.Example
{
    /// <summary>
    /// FluentMigrator.Extensions.Postgres. CREATE INDEX CONCURRENTLY can't run in a transaction, so neither can this
    /// migration in a live run.
    /// </summary>
    [Migration(21, TransactionBehavior.None, "PostgreSQL extensions")]
    public class M0021_Postgres : ForwardOnlyMigration
    {
        public override void Up()
        {
            var postgres = IfDatabase(Db.Postgres);

            postgres.Create.Table("Event")
                .WithColumn("Id").AsInt64().NotNullable().PrimaryKey("PK_Event").Identity(PostgresGenerationType.ByDefault)
                .WithColumn("Kind").AsAnsiString(30).Nullable()
                .WithColumn("Payload").AsCustom("jsonb").NotNullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            postgres.Create.Index("IX_Event_Payload").OnTable("Event")
                .OnColumn("Payload").Ascending()
                .WithOptions().UsingGin();

            postgres.Create.Index("IX_Event_Kind").OnTable("Event")
                .OnColumn("Kind").Ascending().NullsLast()
                .WithOptions().Filter("\"Kind\" IS NOT NULL")
                .Include("CreatedAt");

            postgres.Create.Index("UX_Event_Kind").OnTable("Event")
                .OnColumn("Kind").Unique().NullsNotDistinct();

            postgres.Create.Index("IX_Event_Created").OnTable("Event")
                .OnColumn("CreatedAt").Ascending()
                .WithOptions().UsingBrin().AsConcurrently();

            postgres.Insert.IntoTable("Event").Row(new { Kind = "signup", Payload = "{}" });
            postgres.Insert.IntoTable("Event").WithOverridingSystemValue().Row(new { Id = 100, Kind = "import", Payload = "[]" });
        }
    }
}
