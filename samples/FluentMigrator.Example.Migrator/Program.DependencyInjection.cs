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

using FluentMigrator.Example.Migrations;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Example.Migrator
{
    internal static partial class Program
    {
        private static void RunWithServices(string connectionString)
        {
            var serviceProvider = ConfigureServices(new ServiceCollection(), "SQLite", connectionString);
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

            // Run the migrations
            runner.MigrateUp();
        }

        private static IServiceProvider ConfigureServices(IServiceCollection services, string databaseId, string connectionString)
        {
            services
                .AddFluentMigrator(databaseId)
                .AddScoped<IConnectionStringReader>(sp => new PassThroughConnectionStringReader(connectionString))
                .ConfigureRunner(
                    builder => builder
                        .WithAnnouncer(new ConsoleAnnouncer() { ShowSql = true })
                        .WithMigrationsIn(typeof(AddGTDTables).Assembly));
            return services.BuildServiceProvider();
        }
    }
}
