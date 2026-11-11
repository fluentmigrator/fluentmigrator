#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.VersionTableInfo;
using FluentMigrator.Tests.Containers;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.IssueTests.GH1949
{
    /// <summary>
    /// Regression test for https://github.com/fluentmigrator/fluentmigrator/issues/1949
    ///
    /// Several independent application instances (independently built <see cref="ServiceProvider"/>s) start
    /// against an empty SQL Server database at the same moment, each with an <see cref="IApplicationLocker"/>
    /// backed by <c>sp_getapplock</c> and no maintenance migrations. Without the lock they race to create the
    /// version table. With it, exactly one creates the table and applies the migration, and the others wait,
    /// then find nothing left to do.
    /// </summary>
    [TestFixture]
    [Category("Issue")]
    [Category("GH-1949")]
    [Category("SqlServer2016")]
    public class Fixture
    {
        private const int InstanceCount = 4;

        private static readonly IntegrationTestOptions.DatabaseServerOptions _dbOptions =
            IntegrationTestOptions.SqlServer2016;

        private string _dbTableName;
        private string _versionTableName;
        private string _lockName;
        private LockHolderTracker _tracker;
        private ServiceProvider[] _serviceProviders;

        [OneTimeSetUp]
        public async Task ClassSetUp()
        {
            await new SqlServerContainer().Start();
        }

        [SetUp]
        public void SetUp()
        {
            _dbOptions.IgnoreIfNotEnabled();

            var uniqueSuffix = Guid.NewGuid().ToString("N");
            _dbTableName = $"Test_GH1949_{uniqueSuffix}";
            _versionTableName = $"Test_GH1949_Version_{uniqueSuffix}";
            _lockName = $"Test_GH1949_Lock_{uniqueSuffix}";
            _tracker = new LockHolderTracker();

            _serviceProviders = Enumerable.Range(0, InstanceCount).Select(_ => BuildServiceProvider()).ToArray();
        }

        [TearDown]
        public void TearDown()
        {
            if (_serviceProviders != null)
            {
                foreach (var serviceProvider in _serviceProviders)
                {
                    serviceProvider.Dispose();
                }
            }
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
                .AddSingleton(_tracker)
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
                .ConfigureApplicationLocker<SqlServerApplicationLocker>(
                    opt => opt.AcquireTimeout = TimeSpan.FromMinutes(2))
                .BuildServiceProvider();
        }

        [Test]
        public void ConcurrentInstancesCreateTheVersionTableAndApplyTheMigrationOnce()
        {
            using var barrier = new Barrier(InstanceCount);

            var tasks = _serviceProviders
                .Select(
                    serviceProvider => Task.Run(
                        () =>
                        {
                            using var scope = serviceProvider.CreateScope();
                            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

                            // Resolving the runner must not touch the version table yet, so every instance
                            // reaches MigrateUp, and with it the version-table check, at the same moment.
                            barrier.SignalAndWait();
                            runner.MigrateUp();
                        }))
                .ToArray();

            Assert.DoesNotThrowAsync(() => Task.WhenAll(tasks));

            Assert.Multiple(
                () =>
                {
                    Assert.That(_tracker.MaxHolders, Is.EqualTo(1), "More than one instance held the database lock at once.");
                    Assert.That(_tracker.Acquisitions, Is.GreaterThanOrEqualTo(InstanceCount), "Every instance should have taken the lock.");
                });

            using var conn = new SqlConnection(_dbOptions.ConnectionString);
            conn.Open();

            using (var tableCountCommand = conn.CreateCommand())
            {
                tableCountCommand.CommandText = "SELECT COUNT(*) FROM sys.tables WHERE name = @tableName";
                tableCountCommand.Parameters.AddWithValue("@tableName", _dbTableName);
                Assert.That((int) tableCountCommand.ExecuteScalar(), Is.EqualTo(1), "The migrated table should exist exactly once.");
            }

            using (var versionCountCommand = conn.CreateCommand())
            {
                versionCountCommand.CommandText = $"SELECT COUNT(*) FROM [{_versionTableName}] WHERE [Version] = 1";
                Assert.That((int) versionCountCommand.ExecuteScalar(), Is.EqualTo(1), "The migration should be recorded as applied exactly once.");
            }

            using (var lockTestCommand = conn.CreateCommand())
            {
                // 1 when the lock could be granted now, i.e. no instance still holds it.
                lockTestCommand.CommandText = "SELECT APPLOCK_TEST('public', @resource, 'Exclusive', 'Session')";
                lockTestCommand.Parameters.AddWithValue("@resource", _lockName);
                Assert.That((int) lockTestCommand.ExecuteScalar(), Is.EqualTo(1), "The application lock should have been released.");
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
