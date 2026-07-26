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
    /// Acquires a SQL Server application lock (<c>sp_getapplock</c>) before any migrations run,
    /// mirroring the multi-server coordination pattern described in the FAQ. This blocks a second
    /// server from running migrations concurrently with a first one.
    /// </summary>
    [Maintenance(MigrationStage.BeforeAll, TransactionBehavior.None)]
    public class AcquireMigrationLockMigration : Migration
    {
        private readonly TestMigrationOptions _options;

        public AcquireMigrationLockMigration(IOptions<TestMigrationOptions> options)
        {
            _options = options.Value;
        }

        /// <inheritdoc />
        public override void Up()
        {
            Execute.Sql($@"
                DECLARE @result INT;
                EXEC @result = sp_getapplock '{_options.LockName}', 'Exclusive', 'Session';

                IF @result < 0
                BEGIN
                    DECLARE @msg NVARCHAR(1000) = 'Received error code ' +
                        CAST(@result AS VARCHAR(10)) + ' from sp_getapplock during migrations';
                    THROW 99999, @msg, 1;
                END
            ");
        }

        /// <inheritdoc />
        public override void Down()
        {
            throw new NotImplementedException("Down migrations not supported for sp_getapplock");
        }
    }
}
