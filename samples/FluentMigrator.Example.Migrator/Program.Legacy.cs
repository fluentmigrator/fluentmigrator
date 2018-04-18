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

using FluentMigrator.Example.Migrations;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SQLite;

namespace FluentMigrator.Example.Migrator
{
    internal static partial class Program
    {
        private static void RunInLegacyMode(string connectionString)
        {
            // Create the announcer to output the migration messages
            var announcer = new ConsoleAnnouncer()
            {
                ShowSql = true,
            };

            // Processor specific options (usually none are needed)
            var options = new ProcessorOptions();

            // Initialize the DB-specific processor
#pragma warning disable 612
            var processorFactory = new SQLiteProcessorFactory();
#pragma warning restore 612
            var processor = processorFactory.Create(connectionString, announcer, options);

            // Configure the runner
            var context = new RunnerContext(announcer)
            {
                AllowBreakingChange = true,
            };

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
