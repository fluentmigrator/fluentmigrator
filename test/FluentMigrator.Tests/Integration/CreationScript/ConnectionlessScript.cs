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
using System.IO;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Logging;
using FluentMigrator.Runner.Processors;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentMigrator.Tests.Integration.CreationScript
{
    /// <summary>
    /// Writes a script the way <c>dotnet fm migrate --no-connection --preview --preview-from-nothing --output=FILE</c> does.
    /// </summary>
    public static class ConnectionlessScript
    {
        /// <param name="target">The database the script is for.</param>
        /// <param name="services">
        /// The migrations and the target's processor, with no connection string, so the processor is connectionless.
        /// </param>
        public static string Generate(CreationScriptTarget target, IServiceCollection services)
        {
            var outputFile = Path.Combine(Path.GetTempPath(), $"fm-creation-{target.Name}-{Guid.NewGuid():N}.sql");

            try
            {
                services
                    .AddSingleton<ILoggerProvider, LogFileFluentMigratorLoggerProvider>()
                    .Configure<LogFileFluentMigratorLoggerOptions>(
                        options =>
                        {
                            options.OutputFileName = outputFile;
                            options.OutputGoBetweenStatements = target.GoBetweenStatements;
                            options.ShowSql = true;
                        })
                    .Configure<RunnerOptions>(
                        options =>
                        {
                            options.NoConnection = true;
                            options.PreviewFromNothing = true;
                        })
                    .Configure<ProcessorOptions>(options => options.PreviewOnly = true);

                // Disposing the provider closes the output file.
                using (var serviceProvider = services.BuildServiceProvider())
                using (var scope = serviceProvider.CreateScope())
                {
                    scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();
                }

                return File.ReadAllText(outputFile);
            }
            finally
            {
                File.Delete(outputFile);
            }
        }
    }
}
