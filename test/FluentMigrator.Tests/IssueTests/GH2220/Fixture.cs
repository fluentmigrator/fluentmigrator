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

using System;
using System.Threading;
using System.Threading.Tasks;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.VersionTableInfo;
using FluentMigrator.Tests.Containers;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.IssueTests.GH2220
{
    /// <summary>
    /// Regression test for https://github.com/fluentmigrator/fluentmigrator/issues/2220
    ///
    /// Simulates two independent application servers/processes (two independently built
    /// <see cref="ServiceProvider"/>s, each with its own <see cref="IMigrationRunner"/>) racing to
    /// run migrations concurrently against the same SQL Server database, coordinated through a
    /// <c>sp_getapplock</c> acquired/released via <c>BeforeAll</c>/<c>AfterAll</c> maintenance
    /// migrations (the pattern documented in the multi-server deployments FAQ). Only one of the two
    /// runners should actually execute the pending migration; the other must observe it as already
    /// applied once it acquires the lock, instead of trying (and failing) to re-run it.
    /// </summary>
    [TestFixture]
    [Category("Issue")]
    [Category("GH-2220")]
    [Category("SqlServer2016")]
    public class Fixture
    {
        private static readonly IntegrationTestOptions.DatabaseServerOptions _dbOptions =
            IntegrationTestOptions.SqlServer2016;

        private string _dbTableName;
        private string _versionTableName;
        private string _lockName;

        private ServiceProvider _serviceProviderA;
        private ServiceProvider _serviceProviderB;

        [OneTimeSetUp]
        public async Task ClassSetUp()
        {
            // Ensure the SQL Server test container is running even when this fixture is executed
            // in isolation (e.g. via a test filter), instead of relying on the assembly-wide
            // Integration test container startup.
            await new SqlServerContainer().Start();
        }

        [SetUp]
        public void SetUp()
        {
            _dbOptions.IgnoreIfNotEnabled();

            var uniqueSuffix = Guid.NewGuid().ToString("N");
            _dbTableName = $"Test_GH2220_{uniqueSuffix}";
            _versionTableName = $"Test_GH2220_Version_{uniqueSuffix}";
            _lockName = $"Test_GH2220_Lock_{uniqueSuffix}";

            _serviceProviderA = BuildServiceProvider();
            _serviceProviderB = BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _serviceProviderA?.Dispose();
            _serviceProviderB?.Dispose();
        }

        private ServiceProvider BuildServiceProvider()
        {
            return new ServiceCollection()
                .Configure<TestMigrationOptions>(
                    opt =>
                    {
                        opt.TableName = _dbTableName;
                        opt.LockName = _lockName;
                    })
                .AddFluentMigratorCore()
                .ConfigureRunner(
                    rb => rb
                        .AddSqlServer2016()
                        .WithVersionTable(new VersionTableMetaData(_versionTableName))
                        .WithGlobalConnectionString(_dbOptions.ConnectionString)
                        .ScanIn(typeof(Fixture).Assembly).For.All())
                .Configure<TypeFilterOptions>(
                    opt =>
                    {
                        opt.Namespace = GetType().Namespace;
                        opt.NestedNamespaces = true;
                    })
                .BuildServiceProvider();
        }

        [Test]
        public void TwoConcurrentRunnersSafelyCoordinateThroughApplicationLock()
        {
            // Start both runners at the same instant so that they race for the sp_getapplock
            // acquired by the BeforeAll maintenance migration, just like two application servers
            // starting up concurrently would.
            using var barrier = new Barrier(2);

            Task RunMigrations(ServiceProvider serviceProvider)
            {
                return Task.Run(
                    () =>
                    {
                        using var scope = serviceProvider.CreateScope();
                        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

                        barrier.SignalAndWait();
                        runner.MigrateUp();
                    });
            }

            var taskA = RunMigrations(_serviceProviderA);
            var taskB = RunMigrations(_serviceProviderB);

            // Neither runner should throw: the second one to acquire the lock must recognize that
            // the migration was already applied by the first one, rather than trying to re-run it
            // (which would fail because the table already exists).
            Assert.DoesNotThrowAsync(() => Task.WhenAll(taskA, taskB));

            using var conn = new SqlConnection(_dbOptions.ConnectionString);
            conn.Open();

            using (var tableCountCommand = conn.CreateCommand())
            {
                tableCountCommand.CommandText = "SELECT COUNT(*) FROM sys.tables WHERE name = @tableName";
                tableCountCommand.Parameters.AddWithValue("@tableName", _dbTableName);
                var tableCount = (int) tableCountCommand.ExecuteScalar();
                Assert.That(tableCount, Is.EqualTo(1), "The migrated table should have been created exactly once.");
            }

            using (var versionCountCommand = conn.CreateCommand())
            {
                versionCountCommand.CommandText = $"SELECT COUNT(*) FROM [{_versionTableName}] WHERE [Version] = 1";
                var versionCount = (int) versionCountCommand.ExecuteScalar();
                Assert.That(versionCount, Is.EqualTo(1), "The migration should be recorded as applied exactly once.");
            }
        }

        private class VersionTableMetaData : IVersionTableMetaData
        {
            public VersionTableMetaData(string tableName)
            {
                TableName = tableName;
            }

            /// <inheritdoc />
            public bool OwnsSchema { get; } = true;

            /// <inheritdoc />
            public string SchemaName { get; } = null;

            /// <inheritdoc />
            public string TableName { get; }

            /// <inheritdoc />
            public string ColumnName { get; } = "Version";

            /// <inheritdoc />
            public string DescriptionColumnName { get; } = "Description";

            /// <inheritdoc />
            public string UniqueIndexName { get; } = "UC_Version";

            /// <inheritdoc />
            public string AppliedOnColumnName { get; } = "AppliedOn";

            /// <inheritdoc />
            public bool CreateWithPrimaryKey { get; } = false;
        }
    }
}
