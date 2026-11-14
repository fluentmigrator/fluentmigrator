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

using System.Collections.Generic;

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Tests.Integration.CreationScript.Example
{
    // The kinds of migration, and the attributes that decide which run. Each leaves a row in note, so the test can
    // tell that it ran.

    /// <summary>
    /// Down is worked out from Up.
    /// </summary>
    [Migration(11, "Tags")]
    public class M0011_AutoReversing : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("Tag")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_Tag")
                .WithColumn("Label").AsString(50).NotNullable();

            Create.Index("IX_Tag_Label").OnTable("Tag").OnColumn("Label").Unique();
        }
    }

    /// <summary>
    /// No Down at all.
    /// </summary>
    [Migration(12, "Seed tags")]
    public class M0012_ForwardOnly : ForwardOnlyMigration
    {
        public override void Up()
        {
            Insert.IntoTable("Tag").Row(new { Id = 1, Label = "new" }).Row(new { Id = 2, Label = "sale" });
            Insert.IntoTable("note").Row(new { id = 9, body = "ForwardOnlyMigration" });
        }
    }

    /// <summary>
    /// Runs outside a transaction in a live run. A script has none anyway.
    /// </summary>
    [Migration(13, TransactionBehavior.None, "Outside a transaction")]
    public class M0013_NoTransaction : ForwardOnlyMigration
    {
        public override void Up()
        {
            Insert.IntoTable("note").Row(new { id = 6, body = "TransactionBehavior.None" });
        }
    }

    /// <summary>
    /// Runs the embedded script named after it and the database:
    /// <c>Scripts.Up.14_M0014_AutoScript_&lt;database&gt;.sql</c>.
    /// </summary>
    [Migration(14, "Auto script")]
    public class M0014_AutoScript : AutoScriptMigration
    {
        public M0014_AutoScript(IEnumerable<IEmbeddedResourceProvider> embeddedResourceProviders)
            : base(embeddedResourceProviders)
        {
        }
    }

    /// <summary>
    /// Selected: the run's tags include Example.
    /// </summary>
    [Migration(15, "Tagged")]
    [Tags("Example")]
    public class M0015_Tagged : ForwardOnlyMigration
    {
        public override void Up()
        {
            Insert.IntoTable("note").Row(new { id = 7, body = "Tags" });
        }
    }

    /// <summary>
    /// Not selected: the run's tags don't include Archive, so neither the script nor the version table has it.
    /// </summary>
    [Migration(16, "Not selected by its tag")]
    [Tags("Archive")]
    public class M0016_NotSelectedByTag : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("Excluded").WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_Excluded");
        }
    }

    /// <summary>
    /// The version is the date and time: 202611120930.
    /// </summary>
    [TimestampedMigration(2026, 11, 12, 9, 30, "Timestamped")]
    public class M202611120930_Timestamped : ForwardOnlyMigration
    {
        public override void Up()
        {
            Insert.IntoTable("note").Row(new { id = 8, body = "TimestampedMigration" });
        }
    }

    /// <summary>
    /// Runs after the migrations whenever the run selects its profile. Not recorded in the version table.
    /// </summary>
    [Profile(ExampleDatabase.Profile)]
    public class ExampleProfile : ForwardOnlyMigration
    {
        public override void Up()
        {
            Insert.IntoTable("note").Row(new { id = 11, body = "Profile" });
        }
    }

    /// <summary>
    /// Before anything else, and before the version table: a check that the connection works.
    /// </summary>
    [Maintenance(MigrationStage.BeforeAll)]
    public class BeforeAll : ForwardOnlyMigration
    {
        public override void Up()
        {
            ConnectionCheck.Run(this);
        }
    }

    /// <summary>
    /// Around each migration. These run before the first table exists, so they don't write to one.
    /// </summary>
    [Maintenance(MigrationStage.BeforeEach)]
    public class BeforeEach : ForwardOnlyMigration
    {
        public override void Up()
        {
            ConnectionCheck.Run(this);
        }
    }

    [Maintenance(MigrationStage.AfterEach)]
    public class AfterEach : ForwardOnlyMigration
    {
        public override void Up()
        {
            ConnectionCheck.Run(this);
        }
    }

    [Maintenance(MigrationStage.BeforeProfiles)]
    public class BeforeProfiles : ForwardOnlyMigration
    {
        public override void Up()
        {
            Insert.IntoTable("note").Row(new { id = 10, body = "BeforeProfiles" });
        }
    }

    [Maintenance(MigrationStage.AfterAll)]
    public class AfterAll : ForwardOnlyMigration
    {
        public override void Up()
        {
            Insert.IntoTable("note").Row(new { id = 12, body = "AfterAll" });
        }
    }

    /// <summary>
    /// The cheapest query each database accepts, with IfDatabase choosing the dialect.
    /// </summary>
    internal static class ConnectionCheck
    {
        public static void Run(MigrationBase migration)
        {
            migration.IfDatabase(Db.SqlServer, Db.Postgres, Db.MySql).Execute.Sql("SELECT 1;");
            migration.IfDatabase(Db.Oracle).Execute.Sql("SELECT 1 FROM DUAL;");
            migration.IfDatabase(Db.Firebird).Execute.Sql("SELECT 1 FROM RDB$DATABASE;");
            migration.IfDatabase(Db.Db2).Execute.Sql("SELECT 1 FROM SYSIBM.SYSDUMMY1;");
        }
    }
}
