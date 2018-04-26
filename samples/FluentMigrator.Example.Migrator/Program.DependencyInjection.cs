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

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Example.Migrator
{
    internal static partial class Program
    {
        private static void RunWithServices(string connectionString)
        {
            // Initialize the services
            var serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(
                    builder => builder
                        .AddSQLite()
                        .WithGlobalConnectionString(connectionString)
                        .WithAnnouncer(new ConsoleAnnouncer() { ShowSql = true })
                        .WithMigrationsIn(typeof(AddGTDTables).Assembly))
                .BuildServiceProvider();

            // Instantiate the runner
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

            // Run the migrations
            runner.MigrateUp();
        }
    }
}
