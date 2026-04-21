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
using System.Data;
using System.Data.Common;
using System.IO;

using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Tests.Unit.Initialization.Migrations.ConnectionFactory;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Initialization
{
    [TestFixture]
    [Category("ConnectionFactory")]
    public sealed class ConnectionFactoryTests
    {
        private const string MigrationNamespace = "FluentMigrator.Tests.Unit.Initialization.Migrations.ConnectionFactory";
        private string _tempDataDirectory;

        [SetUp]
        public void SetUpDataDirectory()
        {
            _tempDataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDataDirectory);
            AppDomain.CurrentDomain.SetData("DataDirectory", _tempDataDirectory);
        }

        [TearDown]
        public void TearDownDataDirectory()
        {
            if (!string.IsNullOrEmpty(_tempDataDirectory) && Directory.Exists(_tempDataDirectory))
            {
                Directory.Delete(_tempDataDirectory, true);
            }
        }

        [Test]
        public void WithGlobalConnectionStringStillRunsMigration()
        {
            using (var serviceProvider = CreateProviderWithGlobalConnectionString())
            using (var scope = serviceProvider.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

                runner.MigrateUp(CreateGlobalConnectionStringTable.Version);

                Assert.That(
                    TableExists(CreateGlobalConnectionStringTable.TableName),
                    Is.True);

                runner.MigrateDown(0);
            }
        }

        [Test]
        public void WithConnectionFactoryUsesFactoryWhenConnectionStringIsEmpty()
        {
            var createdConnectionCount = 0;

            using var connection = CreateConnection();

            using (var serviceProvider = CreateProviderWithConnectionFactory(_ =>
            {
                createdConnectionCount++;
                return connection;
            }))
            using (var scope = serviceProvider.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

                runner.MigrateUp(CreateFactoryConnectionTable.Version);

                Assert.That(
                    createdConnectionCount,
                    Is.GreaterThan(0),
                    "The configured connection factory should have been used.");

                Assert.That(
                    TableExists(CreateFactoryConnectionTable.TableName),
                    Is.True,
                    "The migration should execute against the factory-created connection.");

                runner.MigrateDown(0);
            }
        }

        [Test]
        public void GenericProcessorBaseOpensFactoryCreatedConnection()
        {
            var connectionFactory = new RecordingMigrationConnectionFactory();

            using (var processor = new TestGenericProcessor(connectionFactory))
            {
                Assert.That(connectionFactory.Connection.State, Is.EqualTo(ConnectionState.Closed));

                processor.TouchConnection();

                Assert.That(connectionFactory.CreateConnectionCount, Is.EqualTo(1));
                Assert.That(connectionFactory.Connection.OpenCount, Is.EqualTo(1));
                Assert.That(connectionFactory.Connection.State, Is.EqualTo(ConnectionState.Open));
            }
        }

        [Test]
        public void GenericProcessorBaseDisposesFactoryCreatedConnection()
        {
            var connectionFactory = new RecordingMigrationConnectionFactory();
            var processor = new TestGenericProcessor(connectionFactory);

            processor.TouchConnection();

            processor.Dispose();

            Assert.That(connectionFactory.CreateConnectionCount, Is.EqualTo(1));
            Assert.That(connectionFactory.Connection.CloseCount, Is.EqualTo(1));
            Assert.That(connectionFactory.Connection.DisposeCount, Is.EqualTo(1));
            Assert.That(connectionFactory.Connection.State, Is.EqualTo(ConnectionState.Closed));

            processor.Dispose();

            Assert.That(
                connectionFactory.Connection.DisposeCount,
                Is.EqualTo(1),
                "Disposing the processor more than once should not dispose the connection more than once.");
        }

        [Test]
        public void PreviewOnlyWithConnectionFactoryCanResolveVersionLoader()
        {
            var createdConnectionCount = 0;

            using (var serviceProvider = CreateProviderWithConnectionFactory(_ =>
            {
                createdConnectionCount++;
                return CreateConnection();
            },
            services => services.Configure<ProcessorOptions>(options => options.PreviewOnly = true)))
            using (var scope = serviceProvider.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

                Assert.DoesNotThrow(
                    () => runner.MigrateUp(CreatePreviewOnlyTable.Version));

                Assert.That(
                    createdConnectionCount,
                    Is.GreaterThan(0),
                    "Preview-only mode still opens a connection to read version information.");

                runner.MigrateDown(0);
            }
        }

        private static ServiceProvider CreateProviderServiceProvider(
            Action<IMigrationRunnerBuilder> configureRunner,
            Action<IServiceCollection> configureServices = null)
        {
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(runnerBuilder =>
                {
                    runnerBuilder
                        .AddSQLite()
                        .ScanIn(typeof(CreateFactoryConnectionTable).Assembly)
                        .For.Migrations();

                    configureRunner(runnerBuilder);
                })
                .Configure<TypeFilterOptions>(
                    options =>
                    {
                        options.Namespace = MigrationNamespace;
                        options.NestedNamespaces = true;
                    });

            configureServices?.Invoke(services);

            return services.BuildServiceProvider();
        }
        private static ServiceProvider CreateProviderWithConnectionFactory(
            Func<IServiceProvider, IDbConnection> connectionFactory,
            Action<IServiceCollection> configureServices = null)
        {
            return CreateProviderServiceProvider(x => x.WithConnectionFactory(connectionFactory), configureServices);
        }
        private static ServiceProvider CreateProviderWithGlobalConnectionString()
        {
            return CreateProviderServiceProvider(x => x.WithGlobalConnectionString(IntegrationTestOptions.SQLite.ConnectionString));
        }

        private SqliteConnection CreateConnection()
        {
            return new SqliteConnection(IntegrationTestOptions.SQLite.ConnectionString);
        }

        private static bool TableExists(string tableName)
        {
            using var connection = new SqliteConnection(IntegrationTestOptions.SQLite.ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $tableName;";
            command.Parameters.AddWithValue("$tableName", tableName);

            var result = command.ExecuteScalar();

            return Convert.ToInt64(result) == 1;
        }

        private sealed class TestGenericProcessor : GenericProcessorBase
        {
            public TestGenericProcessor(IMigrationConnectionFactory connectionFactory)
                : base(
                    () => RecordingDbProviderFactory.Instance,
                    Mock.Of<IMigrationGenerator>(),
                    NullLogger.Instance,
                    new ProcessorOptions(),
                    connectionFactory)
            {
            }

            public override string DatabaseType => "test";

            public override IList<string> DatabaseTypeAliases { get; } = new List<string>();

            public void TouchConnection()
            {
                _ = Connection;
            }

            public override void Process(PerformDBOperationExpression expression)
            {
            }

            protected override void Process(string sql)
            {
            }

            public override DataSet ReadTableData(string schemaName, string tableName)
            {
                return new DataSet();
            }

            public override DataSet Read(string template, params object[] args)
            {
                return new DataSet();
            }

            public override bool Exists(string template, params object[] args)
            {
                return false;
            }

            public override void Execute(string template, params object[] args)
            {
            }

            public override bool SchemaExists(string schemaName)
            {
                return false;
            }

            public override bool TableExists(string schemaName, string tableName)
            {
                return false;
            }

            public override bool ColumnExists(string schemaName, string tableName, string columnName)
            {
                return false;
            }

            public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
            {
                return false;
            }

            public override bool IndexExists(string schemaName, string tableName, string indexName)
            {
                return false;
            }

            public override bool SequenceExists(string schemaName, string sequenceName)
            {
                return false;
            }

            public override bool DefaultValueExists(
                string schemaName,
                string tableName,
                string columnName,
                object defaultValue)
            {
                return false;
            }
        }

        private sealed class RecordingMigrationConnectionFactory : IMigrationConnectionFactory
        {
            public RecordingDbConnection Connection { get; } = new RecordingDbConnection();

            public int CreateConnectionCount { get; private set; }

            public bool HasConnection => true;

            public IDbConnection CreateConnection(DbProviderFactory providerFactory)
            {
                CreateConnectionCount++;
                return Connection;
            }
        }

        private sealed class RecordingDbProviderFactory : DbProviderFactory
        {
            public static readonly RecordingDbProviderFactory Instance =
                new RecordingDbProviderFactory();

            private RecordingDbProviderFactory()
            {
            }
        }

        private sealed class RecordingDbConnection : DbConnection
        {
            private ConnectionState _state = ConnectionState.Closed;

            public bool ThrowOnOpen { get; set; }

            public int OpenCount { get; private set; }

            public int CloseCount { get; private set; }

            public int DisposeCount { get; private set; }

            public override string ConnectionString { get; set; }

            public override string Database => "RecordingDatabase";

            public override string DataSource => "RecordingDataSource";

            public override string ServerVersion => "1.0";

            public override ConnectionState State => _state;

            public override void ChangeDatabase(string databaseName)
            {
            }

            public override void Open()
            {
                OpenCount++;

                if (ThrowOnOpen)
                {
                    throw new InvalidOperationException("The test connection should not be opened.");
                }

                _state = ConnectionState.Open;
            }

            public override void Close()
            {
                if (_state != ConnectionState.Closed)
                {
                    CloseCount++;
                }

                _state = ConnectionState.Closed;
            }

            protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            {
                return new RecordingDbTransaction(this, isolationLevel);
            }

            protected override DbCommand CreateDbCommand()
            {
                throw new NotSupportedException();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    DisposeCount++;
                }

                base.Dispose(disposing);
            }
        }

        private sealed class RecordingDbTransaction : DbTransaction
        {
            private readonly DbConnection _connection;

            public RecordingDbTransaction(
                DbConnection connection,
                IsolationLevel isolationLevel)
            {
                _connection = connection;
                IsolationLevel = isolationLevel;
            }

            public override IsolationLevel IsolationLevel { get; }

            protected override DbConnection DbConnection => _connection;

            public override void Commit()
            {
            }

            public override void Rollback()
            {
            }
        }
    }
}
