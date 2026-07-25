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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.VersionTableInfo;
using FluentMigrator.Tests.Unit.Initialization.Migrations.ConnectionFactory;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Initialization
{
    /// <summary>
    /// Tests for the semantics of <see cref="IMigrationConnectionFactory"/>: connection ownership,
    /// per-connection (as opposed to per-command) creation, scoping, argument validation and
    /// error propagation.
    /// </summary>
    [TestFixture]
    [Category("ConnectionFactory")]
    public sealed class ConnectionFactorySemanticsTests
    {
        [Test]
        public void WithConnectionFactoryThrowsWhenBuilderIsNull()
        {
            Should.Throw<ArgumentNullException>(
                () => MigrationRunnerBuilderExtensions.WithConnectionFactory(null, _ => new SqliteConnection()));
        }

        [Test]
        public void WithConnectionFactoryThrowsWhenConnectionFactoryIsNull()
        {
            var builder = new Mock<IMigrationRunnerBuilder>();
            builder.SetupGet(b => b.Services).Returns(new ServiceCollection());

            Should.Throw<ArgumentNullException>(
                () => builder.Object.WithConnectionFactory(null));
        }

        [Test]
        public void ConnectionFactoryIsUsedOncePerProcessorAndNotOncePerCommand()
        {
            var connectionFactory = new RecordingMigrationConnectionFactory();

            using var processor = new TestGenericProcessor(connectionFactory);

            processor.CreateTestCommand("SELECT 1").ShouldNotBeNull();
            processor.CreateTestCommand("SELECT 2").ShouldNotBeNull();
            processor.CreateTestCommand("SELECT 3").ShouldNotBeNull();

            connectionFactory.CreateConnectionCount.ShouldBe(1);
            connectionFactory.Connections[0].OpenCount.ShouldBe(1);
        }

        [Test]
        public void ConnectionFactoryIsInvokedOncePerServiceScope()
        {
            var createdConnectionStrings = new List<string>();
            var tokenCounter = 0;

            using var serviceProvider = CreateServiceProvider(
                builder => builder.WithConnectionFactory(
                    _ =>
                    {
                        // Simulates a rotating credential: every new connection gets a fresh token.
                        var connection = new SqliteConnection(BuildConnectionString($"token-{++tokenCounter}"));
                        createdConnectionStrings.Add(connection.ConnectionString);
                        return connection;
                    }));

            for (var i = 0; i < 3; i++)
            {
                using var scope = serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IMigrationProcessor>();
                processor.Execute("CREATE TABLE IF NOT EXISTS ScopeTest (Id INTEGER)");
                processor.Execute("DROP TABLE ScopeTest");
            }

            createdConnectionStrings.Count.ShouldBe(3);
            createdConnectionStrings.ShouldBeUnique();
        }

        [Test]
        public void ConcurrentConnectionAccessCreatesASingleConnection()
        {
            var connectionFactory = new RecordingMigrationConnectionFactory();

            using var processor = new TestGenericProcessor(connectionFactory);

            var connections = new ConcurrentBag<IDbConnection>();

            Parallel.For(0, 32, _ => connections.Add(processor.GetConnection()));

            connectionFactory.CreateConnectionCount.ShouldBe(1);
            connections.ShouldAllBe(connection => connection == connectionFactory.Connections[0]);
        }

        [Test]
        public void ExceptionsThrownByTheConnectionFactoryArePropagated()
        {
            var connectionFactory = new ThrowingMigrationConnectionFactory();

            using var processor = new TestGenericProcessor(connectionFactory);

            Should.Throw<TimeoutException>(() => processor.GetConnection());

            // Lazy<T> caches the exception, so subsequent access must fail the same way instead of
            // silently returning a null connection.
            Should.Throw<TimeoutException>(() => processor.GetConnection());
            connectionFactory.CreateConnectionCount.ShouldBe(1);
        }

        [Test]
        public void ProcessorDisposesConnectionsItOwns()
        {
            var connectionFactory = new RecordingMigrationConnectionFactory { OwnsConnection = true };
            var processor = new TestGenericProcessor(connectionFactory);

            processor.GetConnection();
            processor.Dispose();

            connectionFactory.Connections[0].CloseCount.ShouldBe(1);
            connectionFactory.Connections[0].DisposeCount.ShouldBe(1);
        }

        [Test]
        public void ProcessorLeavesExternallyOwnedConnectionsUntouched()
        {
            var connectionFactory = new RecordingMigrationConnectionFactory { OwnsConnection = false };
            var processor = new TestGenericProcessor(connectionFactory);

            processor.GetConnection();
            processor.Dispose();

            connectionFactory.Connections[0].CloseCount.ShouldBe(0);
            connectionFactory.Connections[0].DisposeCount.ShouldBe(0);
            connectionFactory.Connections[0].State.ShouldBe(ConnectionState.Open);
        }

        [Test]
        public void ProcessorLeavesExternallyOwnedConnectionsUntouchedWhenConfiguredByBuilder()
        {
            using var externalConnection = new SqliteConnection(BuildConnectionString("external"));
            externalConnection.Open();

            using (var serviceProvider = CreateServiceProvider(
                       builder => builder.WithConnectionFactory(_ => externalConnection, ownsConnection: false)))
            using (var scope = serviceProvider.CreateScope())
            {
                var processor = scope.ServiceProvider.GetRequiredService<IMigrationProcessor>();
                processor.Execute("CREATE TABLE IF NOT EXISTS ExternalOwnership (Id INTEGER)");
            }

            externalConnection.State.ShouldBe(ConnectionState.Open);
        }

        [Test]
        public void EnsureConnectionIsClosedLeavesExternallyOwnedConnectionsOpen()
        {
            // Processors such as FirebirdProcessor close the connection outside of Dispose,
            // which must not affect connections owned by the caller.
            var connectionFactory = new RecordingMigrationConnectionFactory { OwnsConnection = false };

            using var processor = new TestGenericProcessor(connectionFactory);

            processor.GetConnection();
            processor.CloseConnection();

            processor.OwnsItsConnection.ShouldBeFalse();
            connectionFactory.Connections[0].CloseCount.ShouldBe(0);
            connectionFactory.Connections[0].State.ShouldBe(ConnectionState.Open);
        }

        [Test]
        public void EnsureConnectionIsClosedClosesOwnedConnections()
        {
            var connectionFactory = new RecordingMigrationConnectionFactory { OwnsConnection = true };

            using var processor = new TestGenericProcessor(connectionFactory);

            processor.GetConnection();
            processor.CloseConnection();

            processor.OwnsItsConnection.ShouldBeTrue();
            connectionFactory.Connections[0].CloseCount.ShouldBe(1);
            connectionFactory.Connections[0].State.ShouldBe(ConnectionState.Closed);
        }

        [Test]
        public void PreOpenedConnectionsAreNotReopened()
        {
            var connectionFactory = new RecordingMigrationConnectionFactory { OpenConnectionBeforeReturning = true };

            using var processor = new TestGenericProcessor(connectionFactory);

            processor.GetConnection();

            connectionFactory.Connections[0].OpenCount.ShouldBe(1);
        }

        [Test]
        public void CommandIsCreatedFromTheConnectionWhenTheProviderFactoryRejectsIt()
        {
            var connectionFactory = new RecordingMigrationConnectionFactory();

            using var processor = new TestGenericProcessor(
                connectionFactory,
                MismatchedDbProviderFactory.Instance);

            var command = processor.CreateTestCommand("SELECT 1");

            command.ShouldBeOfType<RecordingDbCommand>();
            command.CommandText.ShouldBe("SELECT 1");
            MismatchedDbProviderFactory.Instance.CreatedCommands[0].IsDisposed.ShouldBeTrue();
        }

        [Test]
        public void ObsoleteConnectionStringConstructorBehavesLikeTheConnectionFactoryConstructor()
        {
            const string connectionString = "Data Source=:memory:";

            using var connectionStringProcessor = new TestGenericProcessor(
                new PassThroughConnectionStringAccessor(connectionString));
            using var connectionFactoryProcessor = new TestGenericProcessor(
                new ConnectionStringMigrationConnectionFactory(
                    new PassThroughConnectionStringAccessor(connectionString)),
                RecordingDbProviderFactory.Instance);

            connectionStringProcessor.GetConnection().ConnectionString
                .ShouldBe(connectionFactoryProcessor.GetConnection().ConnectionString);
            connectionStringProcessor.GetConnection().State.ShouldBe(ConnectionState.Open);
        }

        [Test]
        public void TheLastConfiguredConnectionFactoryWins()
        {
            var firstFactoryCallCount = 0;
            var secondFactoryCallCount = 0;

            using var serviceProvider = CreateServiceProvider(
                builder =>
                {
                    builder.WithConnectionFactory(
                        _ =>
                        {
                            firstFactoryCallCount++;
                            return new SqliteConnection(BuildConnectionString("first"));
                        });

                    builder.WithConnectionFactory(
                        _ =>
                        {
                            secondFactoryCallCount++;
                            return new SqliteConnection(BuildConnectionString("second"));
                        });
                });

            using var scope = serviceProvider.CreateScope();
            scope.ServiceProvider.GetRequiredService<IMigrationProcessor>().Execute("SELECT 1");

            firstFactoryCallCount.ShouldBe(0);
            secondFactoryCallCount.ShouldBe(1);
        }

        [Test]
        public void ConnectionFactoryWinsRegardlessOfConfigurationOrder()
        {
            var factoryCallCount = 0;

            using var serviceProvider = CreateServiceProvider(
                builder =>
                {
                    builder.WithConnectionFactory(
                        _ =>
                        {
                            factoryCallCount++;
                            return new SqliteConnection(BuildConnectionString("factory"));
                        });

                    builder.WithGlobalConnectionString(BuildConnectionString("global"));
                });

            using var scope = serviceProvider.CreateScope();
            scope.ServiceProvider.GetRequiredService<IMigrationProcessor>().Execute("SELECT 1");

            factoryCallCount.ShouldBe(1);
        }

        [Test]
        public void FactoryWithoutConnectionFallsBackToTheConnectionlessVersionLoader()
        {
            using var serviceProvider = CreateServiceProvider(
                builder => builder
                    .ScanIn(typeof(CreateFactoryConnectionTable).Assembly).For.Migrations(),
                services => services
                    .AddScoped<IMigrationConnectionFactory>(_ => new NoConnectionMigrationConnectionFactory())
                    .Configure<TypeFilterOptions>(
                        options =>
                        {
                            options.Namespace = "FluentMigrator.Tests.Unit.Initialization.Migrations.ConnectionFactory";
                            options.NestedNamespaces = true;
                        }));

            using var scope = serviceProvider.CreateScope();

            scope.ServiceProvider.GetRequiredService<IVersionLoader>()
                .ShouldBeOfType<ConnectionlessVersionLoader>();
        }

        [Test]
        public void EmptyConnectionStringFailsWhenTheConnectionIsCreated()
        {
            // Regression test: an empty connection string must fail loudly when a connection is
            // requested, not reach the ADO.NET provider with an empty connection string.
            using var processor = new TestGenericProcessor(
                new ConnectionStringMigrationConnectionFactory(
                    new PassThroughConnectionStringAccessor(string.Empty)));

            var ex = Should.Throw<InvalidOperationException>(() => processor.GetConnection());
            ex.Message.ShouldContain("connection string");
        }

        [Test]
        public void WhitespaceOnlyConnectionStringIsTreatedAsNoConnection()
        {
            // Breaking change in 9.0.0: whitespace-only connection strings are treated like empty
            // ones (connectionless mode) instead of being handed to the provider's Open().
            var connectionFactory = new ConnectionStringMigrationConnectionFactory(
                new PassThroughConnectionStringAccessor("   "));

            connectionFactory.HasConnection.ShouldBeFalse();

            using var processor = new TestGenericProcessor(connectionFactory);
            Should.Throw<InvalidOperationException>(() => processor.GetConnection());
        }

        [Test]
        public void NullConnectionFromFactoryFailsWithDescriptiveException()
        {
            using var processor = new TestGenericProcessor(new NullReturningMigrationConnectionFactory());

            var ex = Should.Throw<InvalidOperationException>(() => processor.GetConnection());
            ex.Message.ShouldContain(nameof(NullReturningMigrationConnectionFactory));
        }

        [Test]
        public void FirebirdProcessorRejectsExternallyOwnedConnections()
        {
            // The Firebird processor closes and reopens its connection when committing
            // transactions to release Firebird metadata locks, so it cannot work with a
            // connection it does not own.
            var fbOptions = new FirebirdOptions();
            var processorOptions = Mock.Of<IOptionsSnapshot<ProcessorOptions>>(
                snapshot => snapshot.Value == new ProcessorOptions());

            var ex = Should.Throw<ArgumentException>(
                () => new FirebirdProcessor(
                    new FirebirdDbFactory(serviceProvider: null),
                    new FirebirdGenerator(fbOptions),
                    new FirebirdQuoter(fbOptions.ForceQuote),
                    NullLogger<FirebirdProcessor>.Instance,
                    processorOptions,
                    new RecordingMigrationConnectionFactory { OwnsConnection = false },
                    fbOptions));

            ex.ParamName.ShouldBe("connectionFactory");
        }

        private static string BuildConnectionString(string databaseName)
        {
            return new SqliteConnectionStringBuilder
            {
                DataSource = databaseName,
                Mode = SqliteOpenMode.Memory,
                Cache = SqliteCacheMode.Shared,
            }.ToString();
        }

        private static ServiceProvider CreateServiceProvider(
            Action<IMigrationRunnerBuilder> configureRunner,
            Action<IServiceCollection> configureServices = null)
        {
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(
                    builder =>
                    {
                        builder.AddSQLite();
                        configureRunner(builder);
                    });

            configureServices?.Invoke(services);

            return services.BuildServiceProvider();
        }

        private sealed class TestGenericProcessor : GenericProcessorBase
        {
            public TestGenericProcessor(IMigrationConnectionFactory connectionFactory)
                : this(connectionFactory, RecordingDbProviderFactory.Instance)
            {
            }

            public TestGenericProcessor(
                IMigrationConnectionFactory connectionFactory,
                DbProviderFactory providerFactory)
                : base(
                    () => providerFactory,
                    Mock.Of<IMigrationGenerator>(),
                    NullLogger.Instance,
                    new ProcessorOptions(),
                    connectionFactory)
            {
            }

#pragma warning disable CS0618 // Tests the obsolete constructor on purpose
            public TestGenericProcessor(IConnectionStringAccessor connectionStringAccessor)
                : base(
                    () => RecordingDbProviderFactory.Instance,
                    Mock.Of<IMigrationGenerator>(),
                    NullLogger.Instance,
                    new ProcessorOptions(),
                    connectionStringAccessor)
            {
            }
#pragma warning restore CS0618

            public override string DatabaseType => "test";

            public override IList<string> DatabaseTypeAliases { get; } = new List<string>();

            public IDbConnection GetConnection() => Connection;

            public void CloseConnection() => EnsureConnectionIsClosed();

            public bool OwnsItsConnection => OwnsConnection;

            public IDbCommand CreateTestCommand(string commandText) => CreateCommand(commandText);

            public override void Process(PerformDBOperationExpression expression)
            {
            }

            protected override void Process(string sql)
            {
            }

            public override DataSet ReadTableData(string schemaName, string tableName) => new DataSet();

            public override DataSet Read(string template, params object[] args) => new DataSet();

            public override bool Exists(string template, params object[] args) => false;

            public override void Execute(string template, params object[] args)
            {
            }

            public override bool SchemaExists(string schemaName) => false;

            public override bool TableExists(string schemaName, string tableName) => false;

            public override bool ColumnExists(string schemaName, string tableName, string columnName) => false;

            public override bool ConstraintExists(string schemaName, string tableName, string constraintName) => false;

            public override bool IndexExists(string schemaName, string tableName, string indexName) => false;

            public override bool SequenceExists(string schemaName, string sequenceName) => false;

            public override bool DefaultValueExists(
                string schemaName,
                string tableName,
                string columnName,
                object defaultValue) => false;
        }

        private sealed class PassThroughConnectionStringAccessor : IConnectionStringAccessor
        {
            public PassThroughConnectionStringAccessor(string connectionString)
            {
                ConnectionString = connectionString;
            }

            public string ConnectionString { get; }
        }

        private sealed class RecordingMigrationConnectionFactory : IMigrationConnectionFactory
        {
            public List<RecordingDbConnection> Connections { get; } = new List<RecordingDbConnection>();

            public int CreateConnectionCount { get; private set; }

            public bool HasConnection => true;

            public bool OwnsConnection { get; set; } = true;

            public bool OpenConnectionBeforeReturning { get; set; }

            public IDbConnection CreateConnection(DbProviderFactory providerFactory)
            {
                CreateConnectionCount++;

                var connection = new RecordingDbConnection();

                if (OpenConnectionBeforeReturning)
                {
                    connection.Open();
                }

                Connections.Add(connection);
                return connection;
            }
        }

        private sealed class NullReturningMigrationConnectionFactory : IMigrationConnectionFactory
        {
            public bool HasConnection => true;

            public bool OwnsConnection => true;

            public IDbConnection CreateConnection(DbProviderFactory providerFactory) => null;
        }

        private sealed class NoConnectionMigrationConnectionFactory : IMigrationConnectionFactory
        {
            public bool HasConnection => false;

            public bool OwnsConnection => true;

            public IDbConnection CreateConnection(DbProviderFactory providerFactory)
            {
                throw new InvalidOperationException("This factory cannot create connections.");
            }
        }

        private sealed class ThrowingMigrationConnectionFactory : IMigrationConnectionFactory
        {
            public int CreateConnectionCount { get; private set; }

            public bool HasConnection => true;

            public bool OwnsConnection => true;

            public IDbConnection CreateConnection(DbProviderFactory providerFactory)
            {
                CreateConnectionCount++;
                throw new TimeoutException("The access token could not be acquired.");
            }
        }

        private sealed class RecordingDbProviderFactory : DbProviderFactory
        {
            public static readonly RecordingDbProviderFactory Instance = new RecordingDbProviderFactory();

            private RecordingDbProviderFactory()
            {
            }

            public override DbConnection CreateConnection() => new RecordingDbConnection();

            public override DbCommand CreateCommand() => new RecordingDbCommand();
        }

        /// <summary>
        /// A provider factory whose commands reject connections created by another provider,
        /// which is what happens when a custom connection factory returns a connection for a
        /// different ADO.NET provider than the configured processor.
        /// </summary>
        private sealed class MismatchedDbProviderFactory : DbProviderFactory
        {
            public static readonly MismatchedDbProviderFactory Instance = new MismatchedDbProviderFactory();

            private MismatchedDbProviderFactory()
            {
            }

            public List<StrictDbCommand> CreatedCommands { get; } = new List<StrictDbCommand>();

            public override DbCommand CreateCommand()
            {
                var command = new StrictDbCommand();
                CreatedCommands.Add(command);
                return command;
            }
        }

        private sealed class StrictDbCommand : DbCommand
        {
            private DbConnection _connection;

            public bool IsDisposed { get; private set; }

            public override string CommandText { get; set; }

            public override int CommandTimeout { get; set; }

            public override CommandType CommandType { get; set; }

            public override bool DesignTimeVisible { get; set; }

            public override UpdateRowSource UpdatedRowSource { get; set; }

            protected override DbConnection DbConnection
            {
                get => _connection;
                set => _connection = value as StrictDbConnection
                    ?? throw new InvalidCastException(
                        $"A '{value?.GetType().Name}' cannot be used with this provider.");
            }

            protected override DbParameterCollection DbParameterCollection => null;

            protected override DbTransaction DbTransaction { get; set; }

            public override void Cancel()
            {
            }

            public override int ExecuteNonQuery() => 0;

            public override object ExecuteScalar() => null;

            public override void Prepare()
            {
            }

            protected override DbParameter CreateDbParameter() => null;

            protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => null;

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    IsDisposed = true;
                }

                base.Dispose(disposing);
            }
        }

        private abstract class StrictDbConnection : DbConnection
        {
        }

        private sealed class RecordingDbCommand : DbCommand
        {
            public override string CommandText { get; set; }

            public override int CommandTimeout { get; set; }

            public override CommandType CommandType { get; set; }

            public override bool DesignTimeVisible { get; set; }

            public override UpdateRowSource UpdatedRowSource { get; set; }

            protected override DbConnection DbConnection { get; set; }

            protected override DbParameterCollection DbParameterCollection => null;

            protected override DbTransaction DbTransaction { get; set; }

            public override void Cancel()
            {
            }

            public override int ExecuteNonQuery() => 0;

            public override object ExecuteScalar() => null;

            public override void Prepare()
            {
            }

            protected override DbParameter CreateDbParameter() => null;

            protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => null;
        }

        private sealed class RecordingDbConnection : DbConnection
        {
            private ConnectionState _state = ConnectionState.Closed;

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

            protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => null;

            protected override DbCommand CreateDbCommand() => new RecordingDbCommand { Connection = this };

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    DisposeCount++;
                }

                base.Dispose(disposing);
            }
        }
    }
}
