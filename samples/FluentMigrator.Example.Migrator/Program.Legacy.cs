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

using System;
using System.Linq;

using FluentMigrator.Example.Migrations;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Example.Migrator
{
    internal static partial class Program
    {
        private static void RunInLegacyMode(DatabaseConfiguration dbConfig)
        {
            // Create the announcer to output the migration messages
#pragma warning disable 612
            var announcer = new ConsoleAnnouncer()
            {
                ShowSql = true,
            };
#pragma warning restore 612

            // Processor specific options (usually none are needed)
            var options = new ProcessorOptions()
            {
                ConnectionString = dbConfig.ConnectionString,
            };

            // Initialize the DB-specific processor
#pragma warning disable 612
            var processor = MigrationProcessorFactoryProvider
                .RegisteredFactories.Single(
                    x => string.Equals(x.Name, dbConfig.ProcessorId, StringComparison.OrdinalIgnoreCase))
                .Create(dbConfig.ConnectionString, announcer, options);
#pragma warning restore 612

            // Configure the runner
#pragma warning disable 612
            var context = new RunnerContext(announcer)
            {
                AllowBreakingChange = true,
            };
#pragma warning restore 612

            // Create the migration runner
#pragma warning disable 612
            var runner = new MigrationRunner(
                typeof(AddGTDTables).Assembly,
                context,
                processor);
#pragma warning restore 612

            // Run the migrations
            runner.MigrateUp();
        }
    }
}
