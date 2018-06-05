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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FluentMigrator.Example.Migrations;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;

using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentMigrator.Example.Migrator
{
    internal static class Program
    {
        private static IReadOnlyDictionary<string, DatabaseConfiguration> DefaultConfigurations =
            typeof(DefaultDatabaseConfigurations).GetRuntimeFields()
                .Where(x => x.FieldType == typeof(DatabaseConfiguration))
                .ToDictionary(
                    x => x.Name,
                    x => (DatabaseConfiguration) x.GetValue(obj: null),
                    StringComparer.OrdinalIgnoreCase);

        static int Main(string[] args)
        {
            var app = new CommandLineApplication();

            app.HelpOption();
            var processor = app.Option(
                "-d|--dialect <DIALECT>",
                $"The database dialect ({string.Join(",", DefaultConfigurations.Keys)})",
                CommandOptionType.SingleValue);
            var connectionString = app.Option(
                "-c|--connection <CONNECTION-STRING>",
                $"The connection string to connect to the database",
                CommandOptionType.SingleValue);

            app.OnExecute(
                () =>
                {
                    var dbConfig = CreateDatabaseConfiguration(processor, connectionString);
                    return ExecuteMigration(dbConfig);
                });

            return app.Execute(args);
        }

        private static int ExecuteMigration(DatabaseConfiguration dbConfig)
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

            return 0;
        }

        private static DatabaseConfiguration CreateDatabaseConfiguration(
            CommandOption processor,
            CommandOption connectionString)
        {
            if (processor.HasValue())
            {
                var processorId = processor.Value();
                if (connectionString.HasValue())
                {
                    return new DatabaseConfiguration
                    {
                        ProcessorId = processorId,
                        ConnectionString = connectionString.Value(),
                    };
                }

                if (!DefaultConfigurations.TryGetValue(processorId, out var result))
                {
                    throw new InvalidOperationException(
                        $"No default configuration for dialect {processorId} available");
                }

                return result;
            }

            if (connectionString.HasValue())
            {
                return new DatabaseConfiguration()
                {
                    ProcessorId = "sqlite",
                    ConnectionString = connectionString.Value(),
                };
            }

            return DefaultDatabaseConfigurations.Sqlite;
        }
    }
}
