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
using System.IO;
using System.Linq;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Logging;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Tests.Integration.CreationScript.Example;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.CreationScript
{
    /// <summary>
    /// Scripts the example migrations in <see cref="Example"/> the way
    /// <c>dotnet fm migrate --no-connection --preview --preview-from-nothing --output</c> does, runs that script on a
    /// new, empty database in each integration-test container the way the database's own tool would, and checks that
    /// the result is the database FluentMigrator would have built.
    /// </summary>
    /// <remarks>
    /// The example uses every part of the fluent syntax that can be scripted, so this is also a test of each
    /// generator against a real database. What can't be scripted is listed in <see cref="ExampleDatabase"/>.
    /// </remarks>
    [TestFixture]
    [Category("Integration")]
    [Category("CreationScript")]
    public class CreationScriptTests
    {
        private static IEnumerable<TestCaseData> Targets() =>
            CreationScriptTarget.All.Select(t => new TestCaseData(t).SetArgDisplayNames(t.Name).SetCategory(t.Name));

        [TestCaseSource(nameof(Targets))]
        public void ScriptBuildsTheExampleDatabase(CreationScriptTarget target)
        {
            var serverOptions = target.ServerOptions;
            serverOptions.IgnoreIfNotEnabled();

            var script = GenerateScript(target);

            try
            {
                using var database = target.CreateEmptyDatabase(serverOptions.ConnectionString);

                target.RunScript(database.ConnectionString, script);

                VerifyDatabase(target, database.ConnectionString);
            }
            catch
            {
                TestContext.Out.WriteLine($"The {target.Name} creation script:");
                TestContext.Out.WriteLine(script);
                throw;
            }
        }

        /// <summary>
        /// What <c>dotnet fm migrate --no-connection --preview --preview-from-nothing --output=FILE</c> writes.
        /// </summary>
        private static string GenerateScript(CreationScriptTarget target)
        {
            var outputFile = Path.Combine(Path.GetTempPath(), $"fm-creation-{target.Name}-{Guid.NewGuid():N}.sql");

            try
            {
                var services = CreateServices(target, connectionString: null)
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

        private static void VerifyDatabase(CreationScriptTarget target, string connectionString)
        {
            using var serviceProvider = CreateServices(target, connectionString).BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var processor = scope.ServiceProvider.GetRequiredService<IProcessorAccessor>().Processor;

            // The version table as the script left it. A runner's version loader would add whatever is missing, so
            // this is checked first.
            Assert.Multiple(
                () =>
                {
                    Assert.That(processor.TableExists(null, "VersionInfo"), Is.True, "VersionInfo");
                    Assert.That(processor.ColumnExists(null, "VersionInfo", "Version"), Is.True, "VersionInfo.Version");
                    Assert.That(processor.ColumnExists(null, "VersionInfo", "AppliedOn"), Is.True, "VersionInfo.AppliedOn");
                    Assert.That(processor.ColumnExists(null, "VersionInfo", "Description"), Is.True, "VersionInfo.Description");
                    Assert.That(processor.IndexExists(null, "VersionInfo", "UC_Version"), Is.True, "UC_Version");
                });

            // A live runner reads the rows the script inserted and finds nothing left to do.
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
            var versionLoader = scope.ServiceProvider.GetRequiredService<IVersionLoader>();
            Assert.That(versionLoader.VersionInfo.AppliedMigrations(), Is.EquivalentTo(ExampleDatabase.Versions));
            Assert.That(runner.HasMigrationsToApplyUp(), Is.False);

            ExampleDatabase.Verify(processor);
        }

        /// <summary>
        /// The same registration for scripting and for the live check: the example's namespace, its profile and its
        /// tags, and the target's processor. Without a connection string the processor is connectionless.
        /// </summary>
        private static IServiceCollection CreateServices(CreationScriptTarget target, string connectionString)
        {
            return new ServiceCollection()
                .AddFluentMigratorCore()
                // As dotnet-fm's -a: the assembly feeds migrations, maintenance, profiles and embedded scripts.
                .AddSingleton<IAssemblySourceItem>(new AssemblySourceItem(typeof(ExampleDatabase).Assembly))
                .Configure<TypeFilterOptions>(
                    options =>
                    {
                        options.Namespace = typeof(ExampleDatabase).Namespace;
                        options.NestedNamespaces = true;
                    })
                .Configure<RunnerOptions>(
                    options =>
                    {
                        options.Profile = ExampleDatabase.Profile;
                        options.Tags = ExampleDatabase.Tags;
                    })
                .ConfigureRunner(
                    builder =>
                    {
                        target.AddProcessor(builder);

                        if (connectionString != null)
                        {
                            builder.WithGlobalConnectionString(connectionString);
                        }
                    });
        }
    }
}
