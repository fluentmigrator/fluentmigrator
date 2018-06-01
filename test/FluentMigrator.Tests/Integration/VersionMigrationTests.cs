#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Processors.MySql;
using FluentMigrator.Runner.Processors.SqlAnywhere;
using FluentMigrator.Runner.Processors.SQLite;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.Runner.VersionTableInfo;
using FluentMigrator.Tests.Unit;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration
{
    [TestFixture]
    [Category("Integration")]
    [Category("Firebird")]
    [Category("MySql")]
    [Category("SQLite")]
    [Category("Postgres")]
    [Category("SqlServer2005")]
    [Category("SqlServer2008")]
    [Category("SqlServer2012")]
    [Category("SqlServer2014")]
    public class VersionMigrationTests : IntegrationTestBase
    {
        [Test]
        public void CanUseVersionInfo()
        {
            ExecuteWithSupportedProcessors(
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
                });
        }

        [Test]
        public void CanUseCustomVersionInfo()
        {
            ExecuteWithSupportedProcessors(
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
                true,
                typeof(SQLiteProcessor),
                typeof(MySqlProcessor),
                typeof(FirebirdProcessor),
                typeof(SqlAnywhereProcessor));
        }

        [Test]
        public void CanUseCustomVersionInfoDefaultSchema()
        {
            ExecuteWithSupportedProcessors(
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
                });
        }
    }
}
