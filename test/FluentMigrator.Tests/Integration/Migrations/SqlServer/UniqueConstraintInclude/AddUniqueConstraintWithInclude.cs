#region License
// Copyright (c) 2018, Fluent Migrator Project
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

using FluentMigrator.SqlServer;

namespace FluentMigrator.Tests.Integration.Migrations.SqlServer.UniqueConstraintInclude
{
    /// <summary>
    /// Creates a unique constraint with an included (non-key) column via the
    /// <c>Create.UniqueConstraint(...).Column(...).Include(...)</c> extension. Because SQL Server has
    /// no INCLUDE syntax on a constraint, FluentMigrator emits a covering <c>CREATE UNIQUE INDEX</c>.
    /// Before the fix the include column was silently dropped (a plain UNIQUE constraint was created);
    /// an end-to-end run proves the include column reaches the database as a non-key index column.
    /// </summary>
    [Migration(1045_31_07_2026)]
    public class AddUniqueConstraintWithInclude : Migration
    {
        public const string TableName = "FmUniqueConstraintInclude";
        public const string ConstraintName = "UC_FmUniqueConstraintInclude";
        public const string KeyColumn = "A";
        public const string IncludedColumn = "B";

        public override void Up()
        {
            Create.Table(TableName)
                .WithColumn(KeyColumn).AsInt32().NotNullable()
                .WithColumn(IncludedColumn).AsInt32().Nullable();

            Create.UniqueConstraint(ConstraintName).OnTable(TableName)
                .Column(KeyColumn)
                .Include(IncludedColumn);
        }

        public override void Down()
        {
            Delete.Table(TableName);
        }
    }
}
