#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentMigrator.Tests.SyntaxRoots
{
    /// <summary>
    /// Runs migrations written with a syntax root through the real runner: the builders, the conventions, validation,
    /// the processor and the generator, as a migration in an application goes.
    /// </summary>
    public static class SyntaxRootRunner
    {
        /// <summary>
        /// The SQL that <paramref name="act"/> produces without a database, as in a connectionless script.
        /// <paramref name="arrange"/>, if any, runs first and its SQL is left out.
        /// </summary>
        public static IReadOnlyList<string> Script(Action<IMigrationRunnerBuilder> configureRunner, IMigration arrange, IMigration act)
        {
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(configureRunner)
                .Configure<RunnerOptions>(options => options.NoConnection = true)
                .Configure<ProcessorOptions>(options => options.PreviewOnly = true);

            return Run(services, arrange, act);
        }

        /// <summary>
        /// Runs <paramref name="arrange"/>, if any, and then <paramref name="act"/> on a database, and returns the SQL
        /// that <paramref name="act"/> ran.
        /// </summary>
        public static IReadOnlyList<string> Live(Action<IMigrationRunnerBuilder> configureRunner, string connectionString, IMigration arrange, IMigration act)
        {
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(builder =>
                {
                    configureRunner(builder);
                    builder.WithGlobalConnectionString(connectionString);
                });

            return Run(services, arrange, act);
        }

        private static IReadOnlyList<string> Run(IServiceCollection services, IMigration arrange, IMigration act)
        {
            var capture = new SqlCaptureLoggerProvider();
            services.AddSingleton<ILoggerProvider>(capture);

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            if (arrange != null)
            {
                runner.Up(arrange);
                capture.Clear();
            }

            runner.Up(act);
            return capture.Sql.ToList();
        }
    }
}
