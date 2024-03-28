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

using FluentMigrator.Example.Migrations;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentMigrator.Example.Migrator
{
    internal static partial class Program
    {
        private static void RunWithServices(DatabaseConfiguration dbConfig)
        {
            // Initialize the services
            var serviceProvider = new ServiceCollection()
                .AddLogging(lb => lb.AddDebug().AddFluentMigratorConsole())
                .AddFluentMigratorCore()
                .ConfigureRunner(
                    builder => builder
#if NETFRAMEWORK
                        .AddJet()
#endif
                        .AddSqlServer()
                        .AddSQLite()
                        .WithGlobalConnectionString(dbConfig.ConnectionString)
                        .ScanIn(typeof(AddGTDTables).Assembly).For.Migrations())
                .Configure<SelectingProcessorAccessorOptions>(
                    opt => opt.ProcessorId = dbConfig.ProcessorId)
                .BuildServiceProvider();

            // Instantiate the runner
            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

            // Run the migrations
            runner.MigrateUp();
        }
    }
}
