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
using System.IO;

using FluentMigrator.Runner;
using FluentMigrator.Tests.Integration.ConnectionFactory.Migrations;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.ConnectionFactory.SQLite
{
    [TestFixture]
    [Category("ConnectionFactory")]
    [Category("SQLite")]
    public sealed class SQLiteConnectionFactoryTests : ConnectionFactoryProviderTestBase
    {
        [Test]
        public void SQLite_WithConnectionFactory_RunsMigration()
        {
            var databasePath = GetTempDatabasePath();
            var connectionString = BuildSqliteConnectionString(databasePath);
            var createdConnectionCount = 0;

            try
            {
                using (var serviceProvider = CreateServiceProvider(runnerBuilder => runnerBuilder.AddSQLite(), _ =>
                {
                    createdConnectionCount++;
                    return new SqliteConnection(connectionString);
                }))
                using (var scope = serviceProvider.CreateScope())
                {
                    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

                    runner.MigrateUp(CreateFactoryConnectionTable.Version);
                }

                Assert.That(createdConnectionCount, Is.GreaterThan(0));

                Assert.That(
                    TableExists(connectionString, CreateFactoryConnectionTable.TableName),
                    Is.True);
            }
            finally
            {
                DeleteDatabaseFile(databasePath);
            }
        }

        private static string GetTempDatabasePath()
        {
            return Path.Combine(
                Path.GetTempPath(),
                $"fluentmigrator-connection-factory-{Guid.NewGuid():N}.sqlite");
        }

        private static string BuildSqliteConnectionString(string databasePath)
        {
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = databasePath,
                Pooling = false
            };

            return builder.ToString();
        }

        private static bool TableExists(string connectionString, string tableName)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $tableName;";
                    command.Parameters.AddWithValue("$tableName", tableName);

                    return Convert.ToInt64(command.ExecuteScalar()) == 1;
                }
            }
        }

        private static void DeleteDatabaseFile(string databasePath)
        {
            SqliteConnection.ClearAllPools();

            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }
}
