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

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentMigrator.Example.Migrator
{
    internal static class Program
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

            // Initialize the services
            var serviceProvider = new ServiceCollection()
                .AddLogging(lb => lb.AddDebug().AddFluentMigratorConsole())
                .AddFluentMigratorCore()
                .ConfigureRunner(
                    builder => builder
                        .AddSQLite()
                        .WithGlobalConnectionString(csb.ConnectionString)
                        .WithMigrationsIn(typeof(AddGTDTables).Assembly))
                .BuildServiceProvider();

            // Instantiate the runner
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

            // Run the migrations
            runner.MigrateUp();
        }
    }
}
