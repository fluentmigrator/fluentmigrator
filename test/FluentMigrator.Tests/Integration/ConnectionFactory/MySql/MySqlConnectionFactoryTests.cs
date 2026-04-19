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

using FluentMigrator.Runner;
using FluentMigrator.Tests.Integration.ConnectionFactory.Migrations;

using Microsoft.Extensions.DependencyInjection;

using MySql.Data.MySqlClient;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.ConnectionFactory.MySql
{
    [TestFixture]
    [Category("ConnectionFactory")]
    [Category("MySql")]
    public sealed class MySqlConnectionFactoryTests : ConnectionFactoryProviderTestBase
    {
        [Test]
        public void MySql_WithConnectionFactory_RunsMigration()
        {
            var connectionString = GetRequiredEnvironmentVariableOrIgnore("FM_TEST_MYSQL");
            var createConnectionCount = 0;

            Cleanup(connectionString);

            try
            {
                using (var serviceProvider = CreateServiceProvider(runnerBuilder => runnerBuilder.AddMySql5(), _ =>
                {
                    createConnectionCount++;
                    return new MySqlConnection(connectionString);
                }))
                using (var scope = serviceProvider.CreateScope())
                {
                    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

                    runner.MigrateUp(CreateFactoryConnectionTable.Version);
                }

                Assert.That(createConnectionCount, Is.GreaterThan(0));

                Assert.That(
                    TableExists(connectionString, CreateFactoryConnectionTable.TableName),
                    Is.True);
            }
            finally
            {
                Cleanup(connectionString);
            }
        }

        private static bool TableExists(string connectionString, string tableName)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
SELECT COUNT(*)
FROM information_schema.tables
WHERE table_schema = DATABASE()
  AND table_name = @tableName;";

                    command.Parameters.AddWithValue("@tableName", tableName);

                    return Convert.ToInt32(command.ExecuteScalar()) == 1;
                }
            }
        }

        private static void Cleanup(string connectionString)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
DROP TABLE IF EXISTS `FactoryConnectionTest`;
DROP TABLE IF EXISTS `VersionInfo`;";

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
