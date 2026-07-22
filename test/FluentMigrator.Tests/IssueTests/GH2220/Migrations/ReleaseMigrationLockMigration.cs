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

using System;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Tests.IssueTests.GH2220.Migrations
{
    /// <summary>
    /// Releases the SQL Server application lock acquired by <see cref="AcquireMigrationLockMigration"/>
    /// after all migrations have run.
    /// </summary>
    [Maintenance(MigrationStage.AfterAll, TransactionBehavior.None)]
    public class ReleaseMigrationLockMigration : Migration
    {
        private readonly TestMigrationOptions _options;

        public ReleaseMigrationLockMigration(IOptions<TestMigrationOptions> options)
        {
            _options = options.Value;
        }

        /// <inheritdoc />
        public override void Up()
        {
            Execute.Sql($"EXEC sp_releaseapplock '{_options.LockName}', 'Session'");
        }

        /// <inheritdoc />
        public override void Down()
        {
            throw new NotImplementedException("Down migrations not supported for sp_releaseapplock");
        }
    }
}
