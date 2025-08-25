#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Processors.MySql;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Runner.Processors.SQLite;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.Runner.VersionTableInfo;
using FluentMigrator.Tests.Integration.TestCases;
using FluentMigrator.Tests.Unit;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration
{
    [TestFixture]
    public class VersionMigrationTests : IntegrationTestBase
    {
        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void CanUseVersionInfo(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn("FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass3"),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    IVersionTableMetaData tableMetaData = new DefaultVersionTableMetaData(
                        ConventionSets.NoSchemaName,
                        serviceProvider.GetRequiredService<IOptions<RunnerOptions>>());

                    //ensure table doesn't exist
                    if (processor.TableExists(tableMetaData.SchemaName, tableMetaData.TableName))
                        runner.Down(new VersionMigration(tableMetaData));

                    runner.Up(new VersionMigration(tableMetaData));
                    processor.TableExists(tableMetaData.SchemaName, tableMetaData.TableName).ShouldBeTrue();

                    runner.Down(new VersionMigration(tableMetaData));
                    processor.TableExists(tableMetaData.SchemaName, tableMetaData.TableName).ShouldBeFalse();
                },
                serverOptions);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            SQLiteProcessor,
            MySqlProcessor,
            FirebirdProcessor,
            OracleProcessorBase
        >))]
        public void CanUseCustomVersionInfo(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services
                    .WithMigrationsIn("FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass3")
                    .AddScoped<IVersionTableMetaDataAccessor>(_ => new PassThroughVersionTableMetaDataAccessor(new TestVersionTableMetaData())),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    var tableMetaData = new TestVersionTableMetaData();

                    //ensure table doesn't exist
                    if (processor.TableExists(tableMetaData.SchemaName, tableMetaData.TableName))
                        runner.Down(new VersionMigration(tableMetaData));

                    //ensure schema doesn't exist
                    if (processor.SchemaExists(tableMetaData.SchemaName))
                        runner.Down(new VersionSchemaMigration(tableMetaData));

                    runner.MigrateUp(200909060930);

                    processor.SchemaExists(tableMetaData.SchemaName).ShouldBeTrue();
                    processor.TableExists(tableMetaData.SchemaName, tableMetaData.TableName).ShouldBeTrue();

                    runner.RollbackToVersion(0);

                    processor.TableExists(tableMetaData.SchemaName, tableMetaData.TableName).ShouldBeFalse();
                    processor.SchemaExists(tableMetaData.SchemaName).ShouldBeFalse();
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void CanUseCustomVersionInfoDefaultSchema(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services
                    .WithMigrationsIn("FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass3")
                    .AddScoped<IVersionTableMetaDataAccessor>(
                        _ => new PassThroughVersionTableMetaDataAccessor(
                            new TestVersionTableMetaData()
                            {
                                SchemaName = null
                            })),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    IVersionTableMetaData tableMetaData = new TestVersionTableMetaData { SchemaName = null };

                    //ensure table doesn't exist
                    if (processor.TableExists(tableMetaData.SchemaName, tableMetaData.TableName))
                        runner.Down(new VersionMigration(tableMetaData));

                    runner.MigrateUp(200909060930);

                    processor.TableExists(null, tableMetaData.TableName).ShouldBeTrue();

                    runner.RollbackToVersion(0);

                    processor.TableExists(null, tableMetaData.TableName).ShouldBeFalse();
                },
                serverOptions);
        }
    }
}
