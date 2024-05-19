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
using System.Data;
using System.Linq;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.VersionTableInfo;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.IssueTests.GH0911
{
    [TestFixture]
    [Category("Issue")]
    [Category("GH-0911")]
    [Category("SqlServer2016")]
    public class Fixture
    {
        private static readonly IntegrationTestOptions.DatabaseServerOptions _dbOptions =
            IntegrationTestOptions.SqlServer2016;

        private string _dbTableName;

        private ServiceProvider _serviceProvider;

        [SetUp]
        public void SetUp()
        {
            if (!_dbOptions.IsEnabled)
            {
                Assert.Ignore();
            }

            _dbTableName = $"Test_GH0911_{Guid.NewGuid():N}";
            _serviceProvider = new ServiceCollection()
                .Configure<TestMigrationOptions>(opt => opt.TableName = _dbTableName)
                .AddFluentMigratorCore()
                .ConfigureRunner(
                    rb => rb
                        .AddSqlServer2016()
                        .WithVersionTable(new VersionTableMetaData(_dbTableName))
                        .WithGlobalConnectionString(_dbOptions.ConnectionString)
                        .ScanIn(typeof(Fixture).Assembly).For.Migrations())
                .Configure<TypeFilterOptions>(
                    opt =>
                    {
                        opt.Namespace = GetType().Namespace;
                        opt.NestedNamespaces = true;
                    })
                .BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _serviceProvider?.Dispose();
        }

        [Test]
        public void EnsureThatVarCharColumnsHaveLengthOf40()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                    runner.MigrateUp();
                }

                using (var conn = new SqlConnection(_dbOptions.ConnectionString))
                {
                    conn.Open();

                    var foundColumns = 0;
                    var restriction = new string[4];
                    restriction[2] = _dbTableName;
                    var columns = conn.GetSchema("Columns", restriction);
                    foreach (var column in columns.Rows.Cast<DataRow>())
                    {
                        var dataType = (string) column["DATA_TYPE"];
                        if (dataType == "varchar")
                        {
                            var maxLength = column["CHARACTER_MAXIMUM_LENGTH"];
                            Assert.That(maxLength, Is.EqualTo(40));
                            ++foundColumns;
                        }
                    }

                    Assert.That(foundColumns, Is.EqualTo(3));
                }
            }
            finally
            {
                // Revert all migrations
                using (var scope = _serviceProvider.CreateScope())
                {
                    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                    runner.MigrateDown(version: 0);
                }
            }
        }

        private class VersionTableMetaData : IVersionTableMetaData
        {
            public VersionTableMetaData(string tableName)
            {
                TableName = $"{tableName}_Version";
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
