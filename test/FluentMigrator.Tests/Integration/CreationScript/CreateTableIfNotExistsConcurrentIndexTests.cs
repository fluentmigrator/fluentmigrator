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
using System.Linq;
using System.Text.RegularExpressions;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Tests.Integration.CreationScript.ConcurrentIndex;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.CreationScript
{
    /// <summary>
    /// <c>Create.Table(...).IfNotExists()</c> with an index on a column, and an index on the same table built with
    /// <c>AsConcurrently()</c>, on a real PostgreSQL database (issue #2444).
    /// </summary>
    /// <remarks>
    /// PostgreSQL creates such a table and its own indexes in a <c>DO</c> block, and <c>CREATE INDEX CONCURRENTLY</c>
    /// can't run in one. The fluent API can't make a concurrent index part of the table: <c>AsConcurrently()</c> is
    /// only on <c>Create.Index(...).WithOptions()</c>, a separate expression that runs after the block. These tests run
    /// the migration, as a script and live, on an empty database and on one that already has the table.
    /// </remarks>
    [TestFixture]
    [Category("Integration")]
    [Category("CreationScript")]
    [Category("Postgres15_0")]
    public class CreateTableIfNotExistsConcurrentIndexTests
    {
        private static readonly CreationScriptTarget Postgres = CreationScriptTarget.All.Single(t => t.Name == "Postgres15_0");

        /// <summary>
        /// The table as someone else made it, with the index the migration declares on a column, and an extra column the
        /// migration would not create. It doesn't have the concurrent index.
        /// </summary>
        private static readonly string ExistingTable = string.Join(
            ";" + Environment.NewLine,
            "CREATE TABLE \"public\".\"FmGadget\" (\"Id\" integer NOT NULL PRIMARY KEY, \"Name\" varchar(100) NOT NULL, \"Code\" varchar(20) NULL, \"Marker\" varchar(20) NULL)",
            "CREATE INDEX \"IX_FmGadget_Name\" ON \"public\".\"FmGadget\" (\"Name\")") + ";";

        [Test]
        public void TheConcurrentIndexRunsAfterTheDoBlock()
        {
            // Needs no database: the script is generated without a connection.
            var script = GenerateScript();

            var doBlock = Regex.Match(script, @"DO (\$\w*\$)\s.*?\1;", RegexOptions.Singleline);

            Assert.Multiple(
                () =>
                {
                    Assert.That(doBlock.Success, Is.True, "the script has no DO block");
                    Assert.That(doBlock.Value, Does.Contain($"CREATE INDEX \"{ConcurrentIndexMigrations.TableIndex}\""), "the table's index in the DO block");
                    Assert.That(doBlock.Value, Does.Not.Contain("CONCURRENTLY"), "CONCURRENTLY in the DO block");
                    Assert.That(
                        script.Substring(doBlock.Index + doBlock.Length),
                        Does.Contain($"CREATE INDEX CONCURRENTLY \"{ConcurrentIndexMigrations.ConcurrentIndex}\""),
                        "the concurrent index after the DO block");
                });
        }

        [TestCase(false, TestName = "{m}(empty database)")]
        [TestCase(true, TestName = "{m}(existing table)")]
        public void ScriptCreatesTheConcurrentIndex(bool tableExisted)
        {
            Postgres.ServerOptions.IgnoreIfNotEnabled();

            var script = GenerateScript();

            try
            {
                using var database = Postgres.CreateEmptyDatabase(Postgres.ServerOptions.ConnectionString);
                if (tableExisted)
                {
                    Postgres.RunScript(database.ConnectionString, ExistingTable);
                }

                Postgres.RunScript(database.ConnectionString, script);

                Verify(database.ConnectionString, tableExisted);
            }
            catch
            {
                TestContext.Out.WriteLine("The script:");
                TestContext.Out.WriteLine(script);
                throw;
            }
        }

        [TestCase(false, TestName = "{m}(empty database)")]
        [TestCase(true, TestName = "{m}(existing table)")]
        public void LiveRunCreatesTheConcurrentIndex(bool tableExisted)
        {
            Postgres.ServerOptions.IgnoreIfNotEnabled();

            using var database = Postgres.CreateEmptyDatabase(Postgres.ServerOptions.ConnectionString);
            if (tableExisted)
            {
                Postgres.RunScript(database.ConnectionString, ExistingTable);
            }

            using (var serviceProvider = CreateServices(database.ConnectionString).BuildServiceProvider())
            using (var scope = serviceProvider.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();
            }

            Verify(database.ConnectionString, tableExisted);
        }

        private static string GenerateScript() =>
            ConnectionlessScript.Generate(Postgres, CreateServices(connectionString: null));

        private static void Verify(string connectionString, bool tableExisted)
        {
            using var serviceProvider = CreateServices(connectionString).BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IProcessorAccessor>().Processor;
            var versionLoader = scope.ServiceProvider.GetRequiredService<IVersionLoader>();
            var table = ConcurrentIndexMigrations.Table;

            Assert.Multiple(
                () =>
                {
                    Assert.That(processor.TableExists(null, table), Is.True, "table");
                    Assert.That(processor.IndexExists(null, table, ConcurrentIndexMigrations.TableIndex), Is.True, "the table's index");
                    Assert.That(processor.IndexExists(null, table, ConcurrentIndexMigrations.ConcurrentIndex), Is.True, "the concurrent index");

                    // The extra column shows that the existing table was left as it was.
                    Assert.That(processor.ColumnExists(null, table, "Marker"), Is.EqualTo(tableExisted), "Marker");
                    Assert.That(
                        versionLoader.VersionInfo.AppliedMigrations(),
                        Is.EquivalentTo(ConcurrentIndexMigrations.Versions),
                        "applied migrations");
                });
        }

        private static IServiceCollection CreateServices(string connectionString)
        {
            return new ServiceCollection()
                .AddFluentMigratorCore()
                .AddSingleton<IAssemblySourceItem>(new AssemblySourceItem(typeof(ConcurrentIndexMigrations).Assembly))
                .Configure<TypeFilterOptions>(
                    options =>
                    {
                        options.Namespace = typeof(ConcurrentIndexMigrations).Namespace;
                        options.NestedNamespaces = false;
                    })
                .Configure<RunnerOptions>(options => options.Tags = [ConcurrentIndexMigrations.Tag])
                .ConfigureRunner(
                    builder =>
                    {
                        Postgres.AddProcessor(builder);

                        if (connectionString != null)
                        {
                            builder.WithGlobalConnectionString(connectionString);
                        }
                    });
        }
    }
}
