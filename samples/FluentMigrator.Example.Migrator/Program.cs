#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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

using System;
using System.IO;

using FluentMigrator.Example.Migrations;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SQLite;

using Microsoft.Data.Sqlite;

namespace FluentMigrator.Example.Migrator
{
    static class Program
    {
        static void Main()
        {
            // Configure the DB connection
            var dbFileName = Path.Combine(AppContext.BaseDirectory, "test.db");
            var csb = new SqliteConnectionStringBuilder
            {
                DataSource = dbFileName,
                Mode = SqliteOpenMode.ReadWriteCreate
            };

            // Create the announcer to output the migration messages
            var announcer = new ConsoleAnnouncer()
            {
                ShowSql = true,
            };

            // Processor specific options (usually none are needed)
            var options = new ProcessorOptions();

            // Initialize the DB-specific processor
            var processorFactory = new SQLiteProcessorFactory();
            var processor = processorFactory.Create(csb.ConnectionString, announcer, options);

            // Configure the runner
            var context = new RunnerContext(announcer)
            {
                AllowBreakingChange = true,
            };

            // Create the migration runner
            var runner = new MigrationRunner(
                typeof(AddGTDTables).Assembly,
                context,
                processor);

            // Run the migrations
            runner.MigrateUp();
        }
    }
}
