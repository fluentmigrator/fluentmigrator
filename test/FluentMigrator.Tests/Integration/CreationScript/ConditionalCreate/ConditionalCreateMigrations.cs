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

namespace FluentMigrator.Tests.Integration.CreationScript.ConditionalCreate
{
    /// <summary>
    /// The migrations <see cref="CreateTableIfNotExistsScriptTests"/> runs: the table from issue #2444, created with
    /// <c>IfNotExists()</c>, and a later migration that uses it.
    /// </summary>
    public static class ConditionalCreateMigrations
    {
        /// <summary>
        /// Keeps these migrations out of runs that scan the whole test assembly.
        /// </summary>
        public const string Tag = "ConditionalCreate";

        public const string Table = "FmWidget";

        public const string Description = "Created by the migration's Create.Table(...).IfNotExists()";

        public static long[] Versions { get; } = [1, 2];
    }

    /// <summary>
    /// A table description and a column description, which SQL Server and Oracle write as separate statements. They
    /// must run only when the table is created. The apostrophe is escaped once for the literal and, on Oracle, once
    /// more for <c>EXECUTE IMMEDIATE</c>.
    /// </summary>
    [Tags(ConditionalCreateMigrations.Tag)]
    [Migration(1, "Widgets, unless the table is already there")]
    public class M0001_CreateWidgetIfNotExists : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table(ConditionalCreateMigrations.Table).IfNotExists()
                .WithDescription(ConditionalCreateMigrations.Description)
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_FmWidget")
                .WithColumn("Name").AsString(100).NotNullable().WithColumnDescription("The widget's name");
        }
    }

    /// <summary>
    /// The table is usable after the guarded statement, whichever way the guard went.
    /// </summary>
    [Tags(ConditionalCreateMigrations.Tag)]
    [Migration(2, "A widget")]
    public class M0002_InsertWidget : ForwardOnlyMigration
    {
        public override void Up()
        {
            Insert.IntoTable(ConditionalCreateMigrations.Table).Row(new { Id = 2, Name = "Inserted by migration 2" });
        }
    }
}
