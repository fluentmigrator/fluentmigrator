#region License
// Copyright (c) 2019, Fluent Migrator Project
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

using FluentMigrator.Runner.Constraints;

namespace FluentMigrator.Example.Migrations
{
    /// <summary>
    /// Update notes.
    /// </summary>
    /// <remarks>
    /// This is only run when the migration 20090906205440 was already applied to the database.
    /// </remarks>
    [Migration(20190416112000)]
    [CurrentVersionMigrationConstraint(20090906205440)]
    public class UpdateNotesByScript : Migration
    {
        /// <inheritdoc />
        public override void Up()
        {
            IfDatabase(processorId => processorId == ProcessorIdConstants.SQLite)
                .Execute.Sql(@"/* this is a test script */
update Notes set body=body || ' (modified)';
");
            IfDatabase(processorId=> processorId != ProcessorIdConstants.SQLite)
                .Execute.Sql(@"/* this is a test script */
update Notes set body=body + ' (modified)';
");
        }

        /// <inheritdoc />
        public override void Down()
        {
            // Nothing to do here
        }
    }
}
