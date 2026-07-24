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

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Processors.MySql;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Runner.Processors.Snowflake;
using FluentMigrator.Runner.Processors.SQLite;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Tests.Integration.TestCases;

using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using FirebirdSql.Data.FirebirdClient;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using Snowflake.Data.Client;

using NUnit.Framework;
using Shouldly;

namespace FluentMigrator.Tests.Integration
{
    [Category("ConnectionFactory")]
    public class ConnectionFactoryTests : IntegrationTestBase
    {
        private const string RootNamespace = "FluentMigrator.Tests.Integration.Migrations";

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void CanRunMigrationUsingConnectionFactory(
            Type processorType,
            Func<IntegrationTestOptions.DatabaseServerOptions> serverOptionsGetter)
        {
            var serverOptions = serverOptionsGetter();
            var createConnectionCount = 0;

            ExecuteWithProcessor(
                processorType,
                services =>
                {
                    services.WithMigrationsIn(RootNamespace);

                    services.Replace(
                        ServiceDescriptor.Scoped<IConnectionStringReader>(
                            _ => new PassThroughConnectionStringReader(string.Empty)));

                    services.ConfigureRunner(
                        runnerBuilder => runnerBuilder.WithConnectionFactory(
                            _ =>
                            {
                                createConnectionCount++;
                                return CreateConnection(processorType, serverOptions.ConnectionString);
                            }));
                },
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    try
                    {
                        runner.Up(new TestCreateAndDropTableMigration());

                        processor.TableExists(null, "TestTable").ShouldBeTrue();
                        createConnectionCount.ShouldBeGreaterThan(0);
                    }
                    finally
                    {
                        if (processor.TableExists(null, "TestTable"))
                        {
                            runner.Down(new TestCreateAndDropTableMigration());
                        }
                    }
                },
                serverOptionsGetter,
                tryRollback: true);
        }

        /// <summary>
        /// Verifies that the connection factory is asked for a connection once per connection
        /// (that is, once per processor scope) and not once per executed command. This is the
        /// behavior applications rely on when the factory acquires a rotating credential such as
        /// an Azure Entra ID access token or an AWS RDS IAM authentication token.
        /// </summary>
        /// <remarks>
        /// Enable the TestContainers-backed databases with
        /// <c>FM_TestConnectionStrings__&lt;Db&gt;__IsEnabled=true</c> and
        /// <c>FM_TestConnectionStrings__&lt;Db&gt;__ContainerEnabled=true</c>.
        /// </remarks>
        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void ConnectionFactoryIsInvokedPerConnectionAndNotPerCommand(
            Type processorType,
            Func<IntegrationTestOptions.DatabaseServerOptions> serverOptionsGetter)
        {
            var serverOptions = serverOptionsGetter();
            var createdConnections = new List<DbConnection>();

            ExecuteWithProcessor(
                processorType,
                services =>
                {
                    services.WithMigrationsIn(RootNamespace);

                    services.Replace(
                        ServiceDescriptor.Scoped<IConnectionStringReader>(
                            _ => new PassThroughConnectionStringReader(string.Empty)));

                    services.ConfigureRunner(
                        runnerBuilder => runnerBuilder.WithConnectionFactory(
                            _ =>
                            {
                                var connection = CreateConnection(processorType, serverOptions.ConnectionString);
                                createdConnections.Add(connection);
                                return connection;
                            }));
                },
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    try
                    {
                        runner.Up(new TestCreateAndDropTableMigration());

                        // Several additional round-trips through the same processor.
                        processor.TableExists(null, "TestTable").ShouldBeTrue();
                        processor.ColumnExists(null, "TestTable", "Id").ShouldBeTrue();
                        processor.TableExists(null, "TestTable").ShouldBeTrue();

                        createdConnections.Count.ShouldBe(
                            1,
                            "The connection factory must be invoked once per connection, not once per command.");
                    }
                    finally
                    {
                        if (processor.TableExists(null, "TestTable"))
                        {
                            runner.Down(new TestCreateAndDropTableMigration());
                        }
                    }
                },
                serverOptionsGetter,
                tryRollback: true);
        }

        /// <summary>
        /// Verifies that every migration scope gets a freshly created connection, so a factory
        /// backed by a rotating credential provider hands out the current token instead of an
        /// expired, cached one.
        /// </summary>
        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void EachMigrationScopeGetsAFreshlyCreatedConnection(
            Type processorType,
            Func<IntegrationTestOptions.DatabaseServerOptions> serverOptionsGetter)
        {
            var serverOptions = serverOptionsGetter();
            var createdConnections = new List<DbConnection>();
            var tokenVersions = new List<int>();
            var tokenVersion = 0;

            void RunScope(Action<IMigrationRunner, ProcessorBase> testAction)
            {
                ExecuteWithProcessor(
                    processorType,
                    services =>
                    {
                        services.WithMigrationsIn(RootNamespace);

                        services.Replace(
                            ServiceDescriptor.Scoped<IConnectionStringReader>(
                                _ => new PassThroughConnectionStringReader(string.Empty)));

                        services.ConfigureRunner(
                            runnerBuilder => runnerBuilder.WithConnectionFactory(
                                _ =>
                                {
                                    // Stands in for acquiring a rotating credential.
                                    tokenVersions.Add(++tokenVersion);

                                    var connection = CreateConnection(processorType, serverOptions.ConnectionString);
                                    createdConnections.Add(connection);
                                    return connection;
                                }));
                    },
                    (serviceProvider, processor) => testAction(
                        serviceProvider.GetRequiredService<IMigrationRunner>(),
                        processor),
                    serverOptionsGetter,
                    tryRollback: true);
            }

            try
            {
                RunScope((runner, processor) =>
                {
                    runner.Up(new TestCreateAndDropTableMigration());
                    processor.TableExists(null, "TestTable").ShouldBeTrue();
                });

                RunScope((runner, processor) =>
                {
                    processor.TableExists(null, "TestTable").ShouldBeTrue();
                    runner.Down(new TestCreateAndDropTableMigration());
                });

                tokenVersions.ShouldBe(new[] { 1, 2 });
                createdConnections.Count.ShouldBe(2);
                ReferenceEquals(createdConnections[0], createdConnections[1]).ShouldBeFalse();
                createdConnections.ShouldAllBe(connection => connection.State == ConnectionState.Closed);
            }
            finally
            {
                foreach (var connection in createdConnections)
                {
                    connection.Dispose();
                }
            }
        }

        private DbConnection CreateConnection(Type processorType, string connectionString)
        {
            if (typeof(SqlServerProcessor).IsAssignableFrom(processorType))
            {
                return new SqlConnection(connectionString);
            }

            if (typeof(SQLiteProcessor).IsAssignableFrom(processorType))
            {
                return new SqliteConnection(connectionString);
            }

            if (typeof(PostgresProcessor).IsAssignableFrom(processorType))
            {
                return new NpgsqlConnection(connectionString);
            }

            if (typeof(MySqlProcessor).IsAssignableFrom(processorType))
            {
                return new MySqlConnection(connectionString);
            }

            if (typeof(FirebirdProcessor).IsAssignableFrom(processorType))
            {
                return new FbConnection(connectionString);
            }

            if (typeof(OracleManagedProcessor).IsAssignableFrom(processorType))
            {
                return new OracleConnection(connectionString);
            }

            if (typeof(SnowflakeProcessor).IsAssignableFrom(processorType))
            {
                return new SnowflakeDbConnection
                {
                    ConnectionString = connectionString
                };
            }

            throw new NotSupportedException(
                $"No test connection factory mapping has been configured for processor '{processorType.FullName}'.");
        }
    }
}
