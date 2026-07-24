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
using System.Data.Common;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
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
