#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
// Copyright (c) 2010, Nathan Brown
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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Logging;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Processors.MySql;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Runner.Processors.Snowflake;
using FluentMigrator.Runner.Processors.SQLite;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Runner.VersionTableInfo;
using FluentMigrator.Tests.Integration.Migrations.Computed;
using FluentMigrator.Tests.Integration.Migrations.Issues;
using FluentMigrator.Tests.Integration.Migrations.Tagged;
using FluentMigrator.Tests.Integration.TestCases;
using FluentMigrator.Tests.Unit;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration
{
    [TestFixture]
    [Category("Integration")]
    public class MigrationRunnerTests : IntegrationTestBase
    {
        private const string RootNamespace = "FluentMigrator.Tests.Integration.Migrations";

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void CanRunMigration(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateAndDropTableMigration());

                    processor.TableExists(null, "TestTable").ShouldBeTrue();

                    // This is a hack until MigrationVersionRunner and MigrationRunner are refactored and merged together
                    //processor.CommitTransaction();

                    runner.Down(new TestCreateAndDropTableMigration());
                    processor.TableExists(null, "TestTable").ShouldBeFalse();
                },
                serverOptions);
        }

        [Test]
        public void CanSilentlyFail()
        {
            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Process(It.IsAny<CreateForeignKeyExpression>())).Throws(new Exception("Error"));
            processor.Setup(x => x.Process(It.IsAny<DeleteForeignKeyExpression>())).Throws(new Exception("Error"));

            var serviceProvider = ServiceCollectionExtensions.CreateServices()
                .Configure<ProcessorOptions>(opt => opt.PreviewOnly = false)
                .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader("No connection"))
                .WithProcessor(processor)
                .WithMigrationsIn(RootNamespace)
                .BuildServiceProvider();

            var runner = (MigrationRunner) serviceProvider.GetRequiredService<IMigrationRunner>();
            runner.SilentlyFail = true;

            runner.Up(new TestForeignKeySilentFailure());

            runner.CaughtExceptions.Count.ShouldBeGreaterThan(0);

            runner.Down(new TestForeignKeySilentFailure());
            runner.CaughtExceptions.Count.ShouldBeGreaterThan(0);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            SQLiteProcessor
        >))]
        public void CanApplyForeignKeyConvention(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestForeignKeyNamingConvention());
                    processor.ConstraintExists(null, "Users", "FK_Users_GroupId_Groups_GroupId").ShouldBeTrue();

                    runner.Down(new TestForeignKeyNamingConvention());
                    processor.ConstraintExists(null, "Users", "FK_Users_GroupId_Groups_GroupId").ShouldBeFalse();
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            SQLiteProcessor,
            FirebirdProcessor
        >))]
        public void CanApplyForeignKeyConventionWithSchema(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestForeignKeyNamingConventionWithSchema());

                    processor.ConstraintExists("TestSchema", "Users", "FK_Users_GroupId_Groups_GroupId").ShouldBeTrue();
                    runner.Down(new TestForeignKeyNamingConventionWithSchema());
                },
                serverOptions);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            SnowflakeProcessor /* Snowflake does not have default schema. */
        >))]
        public void CanApplyIndexConvention(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestIndexNamingConvention());
                    processor.IndexExists(null, "Users", "IX_Users_GroupId").ShouldBeTrue();
                    processor.TableExists(null, "Users").ShouldBeTrue();

                    runner.Down(new TestIndexNamingConvention());
                    processor.TableExists(null, "Users").ShouldBeFalse();
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            SQLiteProcessor
        >))]
        public void CanApplyUniqueConvention(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestUniqueConstraintNamingConvention());
                    processor.ConstraintExists(null, "Users", "UC_Users_GroupId").ShouldBeTrue();
                    processor.ConstraintExists(null, "Users", "UC_Users_AccountId").ShouldBeTrue();
                    processor.TableExists(null, "Users").ShouldBeTrue();

                    runner.Down(new TestUniqueConstraintNamingConvention());
                    processor.ConstraintExists(null, "Users", "UC_Users_GroupId").ShouldBeFalse();
                    processor.ConstraintExists(null, "Users", "UC_Users_AccountId").ShouldBeFalse();
                    processor.TableExists(null, "Users").ShouldBeFalse();
                },
                serverOptions);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            SnowflakeProcessor /* Snowflake does not support indices. */
        >))]
        public void CanApplyIndexConventionWithSchema(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestIndexNamingConventionWithSchema());
                    processor.IndexExists("TestSchema", "Users", "IX_Users_GroupId").ShouldBeTrue();
                    processor.TableExists("TestSchema", "Users").ShouldBeTrue();

                    runner.Down(new TestIndexNamingConventionWithSchema());
                    processor.IndexExists("TestSchema", "Users", "IX_Users_GroupId").ShouldBeFalse();
                    processor.TableExists("TestSchema", "Users").ShouldBeFalse();
                },
                serverOptions);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            SnowflakeProcessor /* Snowflake does not support indices. */
        >))]
        public void CanCreateAndDropIndex(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateAndDropTableMigration());
                    processor.IndexExists(null, "TestTable", "IX_TestTable_Name").ShouldBeFalse();

                    runner.Up(new TestCreateAndDropIndexMigration());
                    processor.IndexExists(null, "TestTable", "IX_TestTable_Name").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropIndexMigration());
                    processor.IndexExists(null, "TestTable", "IX_TestTable_Name").ShouldBeFalse();

                    runner.Down(new TestCreateAndDropTableMigration());
                    processor.IndexExists(null, "TestTable", "IX_TestTable_Name").ShouldBeFalse();

                    //processor.CommitTransaction();
                },
                serverOptions);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            FirebirdProcessor,
            SnowflakeProcessor /* Snowflake does not support indices. */
        >))]
        public void CanCreateAndDropIndexWithSchema(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.IndexExists("TestSchema", "TestTable", "IX_TestTable_Name").ShouldBeFalse();

                    runner.Up(new TestCreateAndDropIndexMigrationWithSchema());
                    processor.IndexExists("TestSchema", "TestTable", "IX_TestTable_Name").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropIndexMigrationWithSchema());
                    processor.IndexExists("TestSchema", "TestTable", "IX_TestTable_Name").ShouldBeFalse();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());
                    processor.IndexExists("TestSchema", "TestTable", "IX_TestTable_Name").ShouldBeFalse();

                    runner.Down(new TestCreateSchema());
                    //processor.CommitTransaction();
                },
                serverOptions);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            SnowflakeProcessor /* This test does not work with snowflake, see test CanRenameTableWithSchema in class SnowflakeMigrationRunnerTests. */
        >))]
        public void CanRenameTable(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateAndDropTableMigration());
                    processor.TableExists(null, "TestTable2").ShouldBeTrue();

                    runner.Up(new TestRenameTableMigration());
                    processor.TableExists(null, "TestTable2").ShouldBeFalse();
                    processor.TableExists(null, "TestTable'3").ShouldBeTrue();

                    runner.Down(new TestRenameTableMigration());
                    processor.TableExists(null, "TestTable'3").ShouldBeFalse();
                    processor.TableExists(null, "TestTable2").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropTableMigration());
                    processor.TableExists(null, "TestTable2").ShouldBeFalse();

                    //processor.CommitTransaction();
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            SnowflakeProcessor /* This test does not work with snowflake, see test CanRenameTableWithSchema in class SnowflakeMigrationRunnerTests. */
        >))]
        public void CanRenameTableWithSchema(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.TableExists("TestSchema", "TestTable2").ShouldBeTrue();

                    runner.Up(new TestRenameTableMigrationWithSchema());
                    processor.TableExists("TestSchema", "TestTable2").ShouldBeFalse();
                    processor.TableExists("TestSchema", "TestTable'3").ShouldBeTrue();

                    runner.Down(new TestRenameTableMigrationWithSchema());
                    processor.TableExists("TestSchema", "TestTable'3").ShouldBeFalse();
                    processor.TableExists("TestSchema", "TestTable2").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());
                    processor.TableExists("TestSchema", "TestTable2").ShouldBeFalse();

                    runner.Down(new TestCreateSchema());

                    //processor.CommitTransaction();
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            SQLiteProcessor,
            SnowflakeProcessor /* This test does not work with snowflake, see test CanRenameColumnWithSchema in class SnowflakeMigrationRunnerTests. */
        >))]
        public void CanRenameColumn(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateAndDropTableMigration());
                    processor.ColumnExists(null, "TestTable2", "Name").ShouldBeTrue();

                    runner.Up(new TestRenameColumnMigration());
                    processor.ColumnExists(null, "TestTable2", "Name").ShouldBeFalse();
                    processor.ColumnExists(null, "TestTable2", "Name'3").ShouldBeTrue();

                    runner.Down(new TestRenameColumnMigration());
                    processor.ColumnExists(null, "TestTable2", "Name'3").ShouldBeFalse();
                    processor.ColumnExists(null, "TestTable2", "Name").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropTableMigration());
                    processor.ColumnExists(null, "TestTable2", "Name").ShouldBeFalse();
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            SQLiteProcessor,
            FirebirdProcessor,
            SnowflakeProcessor /* This test does not work with snowflake, see test CanRenameColumnWithSchema in class SnowflakeMigrationRunnerTests. */
        >))]
        public void CanRenameColumnWithSchema(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "Name").ShouldBeTrue();

                    runner.Up(new TestRenameColumnMigrationWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "Name").ShouldBeFalse();
                    processor.ColumnExists("TestSchema", "TestTable2", "Name'3").ShouldBeTrue();

                    runner.Down(new TestRenameColumnMigrationWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "Name'3").ShouldBeFalse();
                    processor.ColumnExists("TestSchema", "TestTable2", "Name").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "Name").ShouldBeFalse();

                    runner.Down(new TestCreateSchema());
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void CanLoadMigrations(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, _) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    //runner.Processor.CommitTransaction();

                    runner.MigrationLoader.LoadMigrations().ShouldNotBeNull();
                },
                serverOptions);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void CanLoadVersion(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, _) =>
                {
                    var runner = (MigrationRunner) serviceProvider.GetRequiredService<IMigrationRunner>();

                    //runner.Processor.CommitTransaction();
                    runner.VersionLoader.VersionInfo.ShouldNotBeNull();
                },
                serverOptions,
                true
            );
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<
            SnowflakeProcessor,
            SqlServer2016Processor
        >))]
        public void CanLoadVersionCustomTable(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.ConfigureRunner(rb => rb.WithVersionTable(new TestVersionTableMetaData())).WithMigrationsIn(RootNamespace),
                (serviceProvider, _) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    //runner.Processor.CommitTransaction();
                    runner.VersionLoader.VersionInfo.ShouldNotBeNull();
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void CanRunMigrations(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, _) =>
                {
                    var runner = (MigrationRunner) serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.MigrateUp(false);

                    runner.VersionLoader.VersionInfo.HasAppliedMigration(1).ShouldBeTrue();
                    runner.VersionLoader.VersionInfo.HasAppliedMigration(2).ShouldBeTrue();
                    runner.VersionLoader.VersionInfo.HasAppliedMigration(3).ShouldBeTrue();
                    runner.VersionLoader.VersionInfo.HasAppliedMigration(4).ShouldBeTrue();
                    runner.VersionLoader.VersionInfo.HasAppliedMigration(5).ShouldBeTrue();
                    runner.VersionLoader.VersionInfo.HasAppliedMigration(6).ShouldBeTrue();
                    runner.VersionLoader.VersionInfo.HasAppliedMigration(7).ShouldBeTrue();
                    runner.VersionLoader.VersionInfo.HasAppliedMigration(8).ShouldBeTrue();
                    runner.VersionLoader.VersionInfo.Latest().ShouldBe(8);

                    runner.RollbackToVersion(0, false);
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void CanMigrateASpecificVersion(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner) serviceProvider.GetRequiredService<IMigrationRunner>();
                    try
                    {
                        RemoveMigration1(processor);

                        runner.MigrateUp(1, false);

                        runner.VersionLoader.VersionInfo.HasAppliedMigration(1).ShouldBeTrue();
                        processor.TableExists(null, "Users").ShouldBeTrue();
                    }
                    catch (Exception ex) when (LogException(ex))
                    {
                    }
                    finally
                    {
                        runner.RollbackToVersion(0, false);
                    }
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            SQLiteProcessor
        >))]
        public void CanMigrateASpecificVersionDown(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            try
            {
                ExecuteWithProcessor(
                    processorType,
                    services => services.WithMigrationsIn(RootNamespace),
                    (serviceProvider, processor) =>
                    {
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var runner = (MigrationRunner) scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                            runner.MigrateUp(1, false);
                            runner.VersionLoader.VersionInfo.HasAppliedMigration(1).ShouldBeTrue();
                        }

                        processor.TableExists(null, "Users").ShouldBeTrue();

                        using (var scope = serviceProvider.CreateScope())
                        {
                            var runner = (MigrationRunner) scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                            runner.MigrateDown(0, false);
                            runner.VersionLoader.VersionInfo.HasAppliedMigration(1).ShouldBeFalse();
                        }

                        processor.TableExists(null, "Users").ShouldBeFalse();
                    },
                    serverOptions);
            }
            finally
            {
                ExecuteWithProcessor(
                    processorType,
                    services => services.WithMigrationsIn(RootNamespace),
                    (serviceProvider, _) =>
                    {
                        var runner = (MigrationRunner) serviceProvider.GetRequiredService<IMigrationRunner>();
                        runner.RollbackToVersion(0, false);
                    },
                    serverOptions
                );
            }
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void RollbackAllShouldRemoveVersionInfoTable(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner) serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.MigrateUp(2);

                    processor.TableExists(
                        runner.VersionLoader.VersionTableMetaData.SchemaName,
                        runner.VersionLoader.VersionTableMetaData.TableName).ShouldBeTrue();

                    runner.RollbackToVersion(0);

                    processor.TableExists(
                        runner.VersionLoader.VersionTableMetaData.SchemaName,
                        runner.VersionLoader.VersionTableMetaData.TableName).ShouldBeFalse();
                },
                serverOptions,
                true
            );
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<
            SqlServer2008Processor
        >))]
        public void MigrateUpWithSqlServerProcessorShouldCommitItsTransaction(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                init => init.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
                    runner.MigrateUp();

                    try
                    {
                        processor.WasCommitted.ShouldBeTrue();
                    }
                    finally
                    {
                        CleanupTestDatabase(serviceProvider, processor);
                    }
                },
                serverOptions);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<
            SqlServer2008Processor
        >))]
        public void MigrateUpSpecificVersionWithSqlServerProcessorShouldCommitItsTransaction(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                init => init.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
                    runner.MigrateUp(1);

                    try
                    {
                        processor.WasCommitted.ShouldBeTrue();
                    }
                    finally
                    {
                        CleanupTestDatabase(serviceProvider, processor);
                    }
                },
                serverOptions);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void MigrateUpWithTaggedMigrationsShouldOnlyApplyMatchedMigrations(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(typeof(TenantATable).Namespace)
                    .Configure<RunnerOptions>(opt => opt.Tags = new[] { "TenantA" }),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner) serviceProvider.GetRequiredService<IMigrationRunner>();

                    try
                    {
                        runner.MigrateUp(false);

                        processor.TableExists(null, "TenantATable").ShouldBeTrue();
                        processor.TableExists(null, "NormalTable").ShouldBeTrue();
                        processor.TableExists(null, "TenantBTable").ShouldBeFalse();
                        processor.TableExists(null, "TenantAandBTable").ShouldBeTrue();
                    }
                    finally
                    {
                        runner.RollbackToVersion(0);
                    }
                },
                serverOptions,
                true
            );
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void MigrateUpWithTaggedMigrationsAndUsingMultipleTagsShouldOnlyApplyMatchedMigrations(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(typeof(TenantATable).Namespace)
                    .Configure<RunnerOptions>(opt => opt.Tags = new[] { "TenantA", "TenantB" }),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner) serviceProvider.GetRequiredService<IMigrationRunner>();

                    try
                    {
                        runner.MigrateUp(false);

                        processor.TableExists(null, "TenantATable").ShouldBeFalse();
                        processor.TableExists(null, "NormalTable").ShouldBeTrue();
                        processor.TableExists(null, "TenantBTable").ShouldBeFalse();
                        processor.TableExists(null, "TenantAandBTable").ShouldBeTrue();
                    }
                    finally
                    {
                        runner.RollbackToVersion(0);
                    }
                },
                serverOptions,
                true
            );
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void MigrateUpWithDifferentTaggedShouldIgnoreConcreteOfTagged(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(typeof(TenantATable).Namespace)
                    .Configure<RunnerOptions>(opt => opt.Tags = new[] { "TenantB" }),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner) serviceProvider.GetRequiredService<IMigrationRunner>();

                    try
                    {
                        runner.MigrateUp(false);

                        processor.TableExists(null, "TenantATable").ShouldBeFalse();
                        processor.TableExists(null, "NormalTable").ShouldBeTrue();
                        processor.TableExists(null, "TenantBTable").ShouldBeTrue();
                    }
                    finally
                    {
                        runner.RollbackToVersion(0);
                    }
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void MigrateDownWithDifferentTagsToMigrateUpShouldApplyMatchedMigrations(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            var migrationsNamespace = typeof(TenantATable).Namespace;

            try
            {
                ExecuteWithProcessor(
                    processorType,
                    services => services.WithMigrationsIn(migrationsNamespace).Configure<RunnerOptions>(opt => opt.Tags = new[] { "TenantA" }),
                    (serviceProvider, processor) =>
                    {
                        var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();
                        runner.MigrateUp(false);

                        processor.TableExists(null, "TenantATable").ShouldBeTrue();
                        processor.TableExists(null, "NormalTable").ShouldBeTrue();
                        processor.TableExists(null, "TenantBTable").ShouldBeFalse();
                        processor.TableExists(null, "TenantAandBTable").ShouldBeTrue();
                    },
                    serverOptions);

                ExecuteWithProcessor(
                    processorType,
                    services => services.WithMigrationsIn(migrationsNamespace).Configure<RunnerOptions>(opt => opt.Tags = new[] { "TenantB" }),
                    (serviceProvider, processor) =>
                    {
                        var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();
                        runner.MigrateDown(0, false);

                        processor.TableExists(null, "TenantATable").ShouldBeTrue();
                        processor.TableExists(null, "NormalTable").ShouldBeFalse();
                        processor.TableExists(null, "TenantBTable").ShouldBeFalse();
                        processor.TableExists(null, "TenantAandBTable").ShouldBeFalse();
                    },
                    serverOptions);
            }
            finally
            {
                ExecuteWithProcessor(
                    processorType,
                    services => services.WithMigrationsIn(migrationsNamespace).Configure<RunnerOptions>(opt => opt.Tags = new[] { "TenantA" }),
                    (serviceProvider, _) =>
                    {
                        var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();
                        runner.RollbackToVersion(0, false);
                    },
                    serverOptions);
            }
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<
            SqlServer2008Processor
        >))]
        public void VersionInfoCreationScriptsOnlyGeneratedOnceInPreviewMode(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            var outputSql = new StringWriter();
            var provider = new SqlScriptFluentMigratorLoggerProvider(
                outputSql,
                new SqlScriptFluentMigratorLoggerOptions()
                {
                    ShowSql = true,
                });

            ExecuteWithProcessor(
                processorType,
                services => services
                    .ConfigureRunner(rb => rb.WithVersionTable(new TestVersionTableMetaData()))
                    .WithMigrationsIn(RootNamespace)
                    .Configure<ProcessorOptions>(opt => opt.PreviewOnly = true)
                    .AddSingleton<ILoggerProvider>(provider)
                    .AddSingleton<IVersionTableMetaData, TestVersionTableMetaData>(),
                (serviceProvider, processor) =>
                {
                    try
                    {
                        var versionTableMetaData = new TestVersionTableMetaData();

                        var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();
                        runner.MigrateUp(1, false);

                        processor.CommitTransaction();

                        var schemaName = versionTableMetaData.SchemaName;
                        var schemaAndTableName = $"[{schemaName}].[{TestVersionTableMetaData.TABLE_NAME}]";

                        var outputSqlString = outputSql.ToString();

                        var createSchemaMatches = new Regex(Regex.Escape($"CREATE SCHEMA [{schemaName}]"))
                            .Matches(outputSqlString).Count;
                        var createTableMatches = new Regex(Regex.Escape("CREATE TABLE " + schemaAndTableName))
                            .Matches(outputSqlString).Count;
                        var createIndexMatches = new Regex(Regex.Escape("CREATE UNIQUE CLUSTERED INDEX [" + TestVersionTableMetaData.UNIQUE_INDEX_NAME + "] ON " + schemaAndTableName))
                            .Matches(outputSqlString).Count;
                        var alterTableMatches = new Regex(Regex.Escape("ALTER TABLE " + schemaAndTableName))
                            .Matches(outputSqlString).Count;

                        System.Console.WriteLine(outputSqlString);

                        createSchemaMatches.ShouldBe(1);
                        createTableMatches.ShouldBe(1);
                        alterTableMatches.ShouldBe(2);
                        createIndexMatches.ShouldBe(1);
                    }
                    finally
                    {
                        CleanupTestDatabase(serviceProvider, processor);
                    }
                },
                serverOptions);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void MigrateUpWithTaggedMigrationsShouldNotApplyAnyMigrationsIfNoTagsParameterIsPassedIntoTheRunner(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(typeof(TenantATable).Namespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    try
                    {
                        runner.MigrateUp(false);

                        processor.TableExists(null, "TenantATable").ShouldBeFalse();
                        processor.TableExists(null, "NormalTable").ShouldBeTrue();
                        processor.TableExists(null, "TenantBTable").ShouldBeFalse();
                        processor.TableExists(null, "TenantAandBTable").ShouldBeFalse();
                    }
                    finally
                    {
                        runner.RollbackToVersion(0);
                    }
                },
                serverOptions,
                true
            );
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            MySqlProcessor,
            PostgresProcessor
        >))]
        public void ValidateVersionOrderShouldDoNothingIfUnappliedMigrationVersionIsGreaterThanLatestAppliedMigration(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            var namespacePass2 = typeof(Migrations.Interleaved.Pass2.User).Namespace;
            var namespacePass3 = typeof(Migrations.Interleaved.Pass3.User).Namespace;

            try
            {
                ExecuteWithProcessor(
                    processorType,
                    services => services.WithMigrationsIn(namespacePass2),
                    (serviceProvider, _) =>
                    {
                        var runner = serviceProvider
                            .GetRequiredService<IMigrationRunner>();

                        runner.MigrateUp(3);
                    },
                    serverOptions);

                ExecuteWithProcessor(
                    processorType,
                    services => services.WithMigrationsIn(namespacePass3),
                    (serviceProvider, _) =>
                    {
                        var runner = serviceProvider
                            .GetRequiredService<IMigrationRunner>();

                        Assert.DoesNotThrow(runner.ValidateVersionOrder);
                    },
                    serverOptions);
            }
            finally
            {
                ExecuteWithProcessor(
                    processorType,
                    services => services.WithMigrationsIn(namespacePass3),
                    (serviceProvider, _) =>
                    {
                        var runner = serviceProvider
                            .GetRequiredService<IMigrationRunner>();
                        runner.RollbackToVersion(0);
                    },
                    serverOptions,
                    true);
            }
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            MySqlProcessor
        >))]
        public void ValidateVersionOrderShouldThrowExceptionIfUnappliedMigrationVersionIsLessThanLatestAppliedMigration(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            var namespacePass2 = typeof(Migrations.Interleaved.Pass2.User).Namespace;
            var namespacePass3 = typeof(Migrations.Interleaved.Pass3.User).Namespace;

            VersionOrderInvalidException caughtException = null;

            try
            {
                ExecuteWithProcessor(
                    processorType,
                    services => services.WithMigrationsIn(namespacePass2),
                    (serviceProvider, _) =>
                    {
                        var migrationRunner = serviceProvider
                            .GetRequiredService<IMigrationRunner>();

                        migrationRunner.MigrateUp();
                    },
                    serverOptions);

                ExecuteWithProcessor(
                    processorType,
                    services => services.WithMigrationsIn(namespacePass3),
                    (serviceProvider, _) =>
                    {
                        var migrationRunner = serviceProvider
                            .GetRequiredService<IMigrationRunner>();

                        migrationRunner.ValidateVersionOrder();
                    },
                    serverOptions);
            }
            catch (VersionOrderInvalidException ex)
            {
                caughtException = ex;
            }
            finally
            {
                ExecuteWithProcessor(
                    processorType,
                    services => services.WithMigrationsIn(namespacePass3),
                    (serviceProvider, _) =>
                    {
                        var migrationRunner = serviceProvider
                            .GetRequiredService<IMigrationRunner>();

                        migrationRunner.RollbackToVersion(0);
                    },
                    serverOptions,
                    true);
            }

            caughtException.ShouldNotBeNull();

            caughtException.InvalidMigrations.Count().ShouldBe(1);
            var keyValuePair = caughtException.InvalidMigrations.First();
            keyValuePair.Key.ShouldBe(200909060935);
            keyValuePair.Value.Migration.ShouldBeOfType<Migrations.Interleaved.Pass3.UserEmail>();
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<
            SnowflakeProcessor,
            SqlServer2012Processor
        >))]
        public void CanCreateSequence(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateSequence());
                    processor.SequenceExists(null, "TestSequence");

                    runner.Down(new TestCreateSequence());
                    processor.SequenceExists(null, "TestSequence").ShouldBeFalse();
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<
            PostgresProcessor,
            SnowflakeProcessor,
            SqlServer2012Processor
        >))]
        public void CanCreateSequenceWithSchema(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateSchema());
                    runner.Up(new TestCreateSequenceWithSchema());
                    processor.SequenceExists("TestSchema", "TestSequence").ShouldBeTrue();

                    runner.Down(new TestCreateSequenceWithSchema());
                    runner.Down(new TestCreateSchema());
                    processor.SequenceExists("TestSchema", "TestSequence").ShouldBeFalse();
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            SQLiteProcessor,
            FirebirdProcessor,
            SnowflakeProcessor /* Snowflake does not support decreasing varchar column length! */
        >))]
        public void CanAlterColumnWithSchema(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "Name2").ShouldBeTrue();
                    processor.DefaultValueExists("TestSchema", "TestTable", "Name", "Anonymous").ShouldBeTrue();

                    runner.Up(new TestAlterColumnWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "Name2").ShouldBeTrue();

                    runner.Down(new TestAlterColumnWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "Name2").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Down(new TestCreateSchema());
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            SQLiteProcessor,
            FirebirdProcessor
        >))]
        public void CanAlterTableWithSchema(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "NewColumn").ShouldBeFalse();

                    runner.Up(new TestAlterTableWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "NewColumn").ShouldBeTrue();

                    runner.Down(new TestAlterTableWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "NewColumn").ShouldBeFalse();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Down(new TestCreateSchema());
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            SQLiteProcessor,
            FirebirdProcessor
        >))]
        public void CanAlterTablesSchema(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.TableExists("TestSchema", "TestTable").ShouldBeTrue();

                    runner.Up(new TestAlterSchema());
                    processor.TableExists("NewSchema", "TestTable").ShouldBeTrue();

                    runner.Down(new TestAlterSchema());
                    processor.TableExists("TestSchema", "TestTable").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Down(new TestCreateSchema());
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void CanCreateUniqueConstraint(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateAndDropTableMigration());
                    processor.ConstraintExists(null, "TestTable2", "TestUnique").ShouldBeFalse();

                    runner.Up(new TestCreateUniqueConstraint());
                    processor.ConstraintExists(null, "TestTable2", "TestUnique").ShouldBeTrue();

                    runner.Down(new TestCreateUniqueConstraint());
                    processor.ConstraintExists(null, "TestTable2", "TestUnique").ShouldBeFalse();

                    runner.Down(new TestCreateAndDropTableMigration());
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            FirebirdProcessor
        >))]
        public void CanCreateUniqueConstraintWithSchema(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.ConstraintExists("TestSchema", "TestTable2", "TestUnique").ShouldBeFalse();

                    runner.Up(new TestCreateUniqueConstraintWithSchema());
                    processor.ConstraintExists("TestSchema", "TestTable2", "TestUnique").ShouldBeTrue();

                    runner.Down(new TestCreateUniqueConstraintWithSchema());
                    processor.ConstraintExists("TestSchema", "TestTable2", "TestUnique").ShouldBeFalse();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Down(new TestCreateSchema());
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void CanInsertData(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateAndDropTableMigration());
                    DataSet ds = processor.ReadTableData(null, "TestTable");
                    ds.Tables[0].Rows.Count.ShouldBe(1);
                    ds.Tables[0].Rows[0][1].ShouldBe("Test");

                    runner.Down(new TestCreateAndDropTableMigration());
                },
                serverOptions);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            FirebirdProcessor
        >))]
        public void CanInsertDataWithSchema(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    DataSet ds = processor.ReadTableData("TestSchema", "TestTable");
                    ds.Tables[0].Rows.Count.ShouldBe(1);
                    ds.Tables[0].Rows[0][1].ShouldBe("Test");

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Down(new TestCreateSchema());
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            FirebirdProcessor
        >))]
        public void CanUpdateData(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Up(new TestUpdateData());
                    DataSet upDs = processor.ReadTableData("TestSchema", "TestTable");
                    upDs.Tables[0].Rows.Count.ShouldBe(1);
                    upDs.Tables[0].Rows[0][1].ShouldBe("Updated");

                    runner.Down(new TestUpdateData());
                    DataSet downDs = processor.ReadTableData("TestSchema", "TestTable");
                    downDs.Tables[0].Rows.Count.ShouldBe(1);
                    downDs.Tables[0].Rows[0][1].ShouldBe("Test");

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Down(new TestCreateSchema());
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            FirebirdProcessor
        >))]
        public void CanDeleteData(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateAndDropTableMigration());

                    runner.Up(new TestDeleteData());
                    DataSet upDs = processor.ReadTableData(null, "TestTable");
                    upDs.Tables[0].Rows.Count.ShouldBe(0);

                    runner.Down(new TestDeleteData());
                    DataSet downDs = processor.ReadTableData(null, "TestTable");
                    downDs.Tables[0].Rows.Count.ShouldBe(1);
                    downDs.Tables[0].Rows[0][1].ShouldBe("Test");

                    runner.Down(new TestCreateAndDropTableMigration());
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            FirebirdProcessor
        >))]
        public void CanDeleteDataWithSchema(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Up(new TestDeleteDataWithSchema());
                    DataSet upDs = processor.ReadTableData("TestSchema", "TestTable");
                    upDs.Tables[0].Rows.Count.ShouldBe(0);

                    runner.Down(new TestDeleteDataWithSchema());
                    DataSet downDs = processor.ReadTableData("TestSchema", "TestTable");
                    downDs.Tables[0].Rows.Count.ShouldBe(1);
                    downDs.Tables[0].Rows[0][1].ShouldBe("Test");

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Down(new TestCreateSchema());
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            SnowflakeProcessor /* Snowflake does not support indices. */
        >))]
        public void CanReverseCreateIndex(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Up(new TestCreateIndexWithReversing());
                    processor.IndexExists("TestSchema", "TestTable2", "IX_TestTable2_Name2").ShouldBeTrue();

                    runner.Down(new TestCreateIndexWithReversing());
                    processor.IndexExists("TestSchema", "TestTable2", "IX_TestTable2_Name2").ShouldBeFalse();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Down(new TestCreateSchema());
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void CanReverseCreateUniqueConstraint(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateAndDropTableMigration());

                    runner.Up(new TestCreateUniqueConstraintWithReversing());
                    processor.ConstraintExists(null, "TestTable2", "TestUnique").ShouldBeTrue();

                    runner.Down(new TestCreateUniqueConstraintWithReversing());
                    processor.ConstraintExists(null, "TestTable2", "TestUnique").ShouldBeFalse();

                    runner.Down(new TestCreateAndDropTableMigration());
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void CanReverseCreateUniqueConstraintWithSchema(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Up(new TestCreateUniqueConstraintWithSchemaWithReversing());
                    processor.ConstraintExists("TestSchema", "TestTable2", "TestUnique").ShouldBeTrue();

                    runner.Down(new TestCreateUniqueConstraintWithSchemaWithReversing());
                    processor.ConstraintExists("TestSchema", "TestTable2", "TestUnique").ShouldBeFalse();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Down(new TestCreateSchema());
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            FirebirdProcessor
        >))]
        public void CanExecuteSql(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, _) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestExecuteSql());
                    runner.Down(new TestExecuteSql());
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void CanSaveSqlStatementWithDescription(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            var outputSql = new StringBuilder();

            var provider = new SqlScriptFluentMigratorLoggerProvider(
                new StringWriter(outputSql),
                new SqlScriptFluentMigratorLoggerOptions()
                {
                    ShowSql = true,
                });

            ExecuteWithProcessor(
                processorType,
                services =>
                {
                    // Clear sql output between each processor execution
                    outputSql.Clear();

                    services
                        .ConfigureRunner(rb => rb.WithVersionTable(new TestVersionTableMetaData()))
                        .WithMigrationsIn(RootNamespace)
                        .Configure<ProcessorOptions>(opt => opt.PreviewOnly = true)
                        .AddSingleton<ILoggerProvider>(provider);
                },
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestExecuteSqlDescription());
                    runner.Down(new TestExecuteSqlDescription());
                    processor.CommitTransaction();
                    var outputSqlString = outputSql.ToString();
                    var selectUpMatches = new Regex("SELECT 1 FROM FOO")
                            .Matches(outputSqlString).Count;

                    var selectDownMatches = new Regex("SELECT 2 FROM BAR")
                            .Matches(outputSqlString).Count;

                    selectUpMatches.ShouldBe(1);
                    selectDownMatches.ShouldBe(1);
                },
                serverOptions);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void CanInjectParametersInExecuteSql(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            var outputSql = new StringBuilder();

            var provider = new SqlScriptFluentMigratorLoggerProvider(
                new StringWriter(outputSql),
                new SqlScriptFluentMigratorLoggerOptions()
                {
                    ShowSql = true,
                });

            ExecuteWithProcessor(
                processorType,
                services =>
                {
                    // Clear sql output between each processor execution
                    outputSql.Clear();

                    services
                        .ConfigureRunner(rb => rb.WithVersionTable(new TestVersionTableMetaData()))
                        .WithMigrationsIn(RootNamespace)
                        .Configure<ProcessorOptions>(opt => opt.PreviewOnly = true)
                        .AddSingleton<ILoggerProvider>(provider);
                },
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestExecuteSqlParameters());

                    processor.CommitTransaction();
                    var outputSqlString = outputSql.ToString();
                    var selectUpMatches = new Regex("SELECT 1 FROM FOO WHERE BAR = 'test'")
                        .Matches(outputSqlString).Count;

                    selectUpMatches.ShouldBe(1);
                },
                serverOptions);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSource))]
        public void CanUseRawSqlInUpdateAndDelete(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services =>
                {
                    services.WithMigrationsIn(RootNamespace);
                },
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateSchema());

                    runner.Up(new RawSqlCreateTableMigration());
                    DataSet upDs = processor.ReadTableData("TestSchema", "Foo");

                    var rows = upDs.Tables[0].Rows;
                    rows.Count.ShouldBe(3);

                    rows[0]["Baz"].ShouldBe(1);
                    rows[1]["Baz"].ShouldBe(2);
                    rows[2]["Baz"].ShouldBe(3);

                    runner.Up(new RawSqlUpdateMigration());
                    upDs = processor.ReadTableData("TestSchema", "Foo");
                    rows = upDs.Tables[0].Rows;

                    rows[0]["Baz"].ShouldBe(101);
                    rows[1]["Baz"].ShouldBe(102);
                    rows[2]["Baz"].ShouldBe(103);

                    runner.Up(new RawSqlDeleteMigration());
                    upDs = processor.ReadTableData("TestSchema", "Foo");
                    rows = upDs.Tables[0].Rows;

                    rows.Count.ShouldBe(0);

                    runner.Down(new RawSqlCreateTableMigration());
                    runner.Down(new TestCreateSchema());
                },
                serverOptions);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceExcept<
            FirebirdProcessor
        >))]
        public void CanInsertLargeText(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestLargeTextInsertMigration_Issue1196());

                    DataSet upDs = processor.ReadTableData(null, "SimpleTable");

                    var rows = upDs.Tables[0].Rows;
                    rows.Count.ShouldBe(1);
                    rows[0]["LargeUnicodeString"].ShouldBe(TestLargeTextInsertMigration_Issue1196.LargeString);

                    runner.Down(new TestLargeTextInsertMigration_Issue1196());
                },
                serverOptions,
                true);
        }

        private void RemoveMigration1(ProcessorBase processor)
        {
            foreach (var tableName in new[] { "Users", "Groups" })
            {
                if (processor.TableExists(null, tableName))
                {
                    var dropTableSql = processor.Generator.Generate(
                        new DeleteTableExpression() { TableName = tableName });
                    processor.Execute(dropTableSql);
                }
            }
        }

        private void CleanupTestDatabase<TProcessor>(IServiceProvider serviceProvider, TProcessor origProcessor)
            where TProcessor : ProcessorBase
        {
            if (origProcessor.WasCommitted)
            {
                if (origProcessor is GenericProcessorBase gpb)
                {
                    gpb.Connection.Close();
                }

                using (var scope = serviceProvider.CreateScope())
                {
                    var cleanupRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                    cleanupRunner.RollbackToVersion(0);
                }
            }
            else
            {
                origProcessor.RollbackTransaction();
            }
        }
    }

    internal class TestForeignKeyNamingConvention : Migration
    {
        public override void Up()
        {
            Create.Table("Users")
                .WithColumn("UserId").AsInt32().Identity().PrimaryKey()
                .WithColumn("GroupId").AsInt32().NotNullable()
                .WithColumn("UserName").AsString(32).NotNullable()
                .WithColumn("Password").AsString(32).NotNullable();

            Create.Table("Groups")
                .WithColumn("GroupId").AsInt32().Identity().PrimaryKey()
                .WithColumn("Name").AsString(32).NotNullable();

            Create.ForeignKey().FromTable("Users").ForeignColumn("GroupId").ToTable("Groups").PrimaryColumn("GroupId");
        }

        public override void Down()
        {
            Delete.Table("Users");
            Delete.Table("Groups");
        }
    }

    internal class TestUniqueConstraintNamingConvention : Migration
    {
        public override void Up()
        {
            Create.Table("Users")
                .WithColumn("UserId").AsInt32().Identity().PrimaryKey()
                .WithColumn("GroupId").AsInt32().NotNullable()
                .WithColumn("AccountId").AsInt32().NotNullable()
                .WithColumn("UserName").AsString(32).NotNullable()
                .WithColumn("Password").AsString(32).NotNullable();

            Create.UniqueConstraint().OnTable("Users").Column("GroupId");
            Create.UniqueConstraint().OnTable("Users").Column("AccountId");
        }

        public override void Down()
        {
            Delete.UniqueConstraint("UC_Users_GroupId").FromTable("Users");
            Delete.UniqueConstraint().FromTable("Users").Column("AccountId");
            Delete.Table("Users");
        }
    }

    internal class TestIndexNamingConvention : Migration
    {
        public override void Up()
        {
            Create.Table("Users")
                .WithColumn("UserId").AsInt32().Identity().PrimaryKey()
                .WithColumn("GroupId").AsInt32().NotNullable()
                .WithColumn("UserName").AsString(32).NotNullable()
                .WithColumn("Password").AsString(32).NotNullable();

            Create.Index().OnTable("Users").OnColumn("GroupId").Ascending();
        }

        public override void Down()
        {
            Delete.Index("IX_Users_GroupId").OnTable("Users").OnColumn("GroupId");
            Delete.Table("Users");
        }
    }

    internal class TestForeignKeySilentFailure : Migration
    {
        public override void Up()
        {
            Create.Table("Users")
                .WithColumn("UserId").AsInt32().Identity().PrimaryKey()
                .WithColumn("GroupId").AsInt32().NotNullable()
                .WithColumn("UserName").AsString(32).NotNullable()
                .WithColumn("Password").AsString(32).NotNullable();

            Create.Table("Groups")
                .WithColumn("GroupId").AsInt32().Identity().PrimaryKey()
                .WithColumn("Name").AsString(32).NotNullable();

            Create.ForeignKey("FK_Foo").FromTable("Users").ForeignColumn("GroupId").ToTable("Groups").PrimaryColumn("GroupId");
        }

        public override void Down()
        {
            Delete.ForeignKey("FK_Foo").OnTable("Users");
            Delete.Table("Users");
            Delete.Table("Groups");
        }
    }

    internal class TestCreateAndDropTableMigration : Migration
    {
        public override void Up()
        {
            // SQLite only supports FK's defined in the create statement so
            // we ensure this is the only approach used so that SQLite can
            // successfully tested. At time of implementing, the FK constraint
            // wasn't explicitly used by any tests and so should affect anything.

            Create.Table("TestTable")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Name").AsString(255).NotNullable().WithDefaultValue("Anonymous");

            var testTable2 = Create.Table("TestTable2")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Name").AsString(255).Nullable()
                .WithColumn("TestTableId").AsInt32().NotNullable();

            IfDatabase(ProcessorIdConstants.SQLite)
                .Delegate(() => testTable2.ForeignKey("fk_TestTable2_TestTableId_TestTable_Id", "TestTable", "Id"));
            IfDatabase(t => t != ProcessorIdConstants.SQLite)
                .Create.ForeignKey("fk_TestTable2_TestTableId_TestTable_Id")
                    .FromTable("TestTable2").ForeignColumn("TestTableId")
                    .ToTable("TestTable").PrimaryColumn("Id");

            Create.Index("ix_Name").OnTable("TestTable2").OnColumn("Name").Ascending()
                .WithOptions().NonClustered();

            Create.Column("Name2").OnTable("TestTable2").AsBoolean().Nullable();

            Insert.IntoTable("TestTable").Row(new { Name = "Test" });
        }

        public override void Down()
        {
            Delete.Table("TestTable2");
            Delete.Table("TestTable");
        }
    }

    internal class TestRenameTableMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Rename.Table("TestTable2").To("TestTable'3");
        }
    }

    internal class TestRenameColumnMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Rename.Column("Name").OnTable("TestTable2").To("Name'3");
        }
    }

    internal class TestCreateAndDropIndexMigration : Migration
    {
        public override void Up()
        {
            Create.Index("IX_TestTable_Name").OnTable("TestTable").OnColumn("Name");
        }

        public override void Down()
        {
            Delete.Index("IX_TestTable_Name").OnTable("TestTable");
        }
    }

    internal class TestForeignKeyNamingConventionWithSchema : Migration
    {
        public override void Up()
        {
            _ = Create.Schema("TestSchema");

            Create.Table("Users")
                .InSchema("TestSchema")
                .WithColumn("UserId").AsInt32().Identity().PrimaryKey()
                .WithColumn("GroupId").AsInt32().NotNullable()
                .WithColumn("UserName").AsString(32).NotNullable()
                .WithColumn("Password").AsString(32).NotNullable();

            Create.Table("Groups")
                .InSchema("TestSchema")
                .WithColumn("GroupId").AsInt32().Identity().PrimaryKey()
                .WithColumn("Name").AsString(32).NotNullable();

            Create.ForeignKey().FromTable("Users").InSchema("TestSchema").ForeignColumn("GroupId").ToTable("Groups").InSchema("TestSchema").PrimaryColumn("GroupId");
        }

        public override void Down()
        {
            Delete.Table("Users").InSchema("TestSchema");
            Delete.Table("Groups").InSchema("TestSchema");
            Delete.Schema("TestSchema");
        }
    }

    internal class TestIndexNamingConventionWithSchema : Migration
    {
        public override void Up()
        {
            // SQLite doesn't support creating schemas so for non SQLite DB's we'll create
            // the schema, but for SQLite we'll attach a temp DB with the schema alias
            _ = IfDatabase(t => t != ProcessorIdConstants.SQLite).Create.Schema("TestSchema");

            IfDatabase(ProcessorIdConstants.SQLite).Execute.Sql("ATTACH DATABASE '' AS \"TestSchema\"");

            Create.Table("Users")
                .InSchema("TestSchema")
                .WithColumn("UserId").AsInt32().Identity().PrimaryKey()
                .WithColumn("GroupId").AsInt32().NotNullable()
                .WithColumn("UserName").AsString(32).NotNullable()
                .WithColumn("Password").AsString(32).NotNullable();

            Create.Index().OnTable("Users").InSchema("TestSchema").OnColumn("GroupId").Ascending();
        }

        public override void Down()
        {
            Delete.Index("IX_Users_GroupId").OnTable("Users").InSchema("TestSchema").OnColumn("GroupId");
            Delete.Table("Users").InSchema("TestSchema");
            IfDatabase(t => t != ProcessorIdConstants.SQLite).Delete.Schema("TestSchema");

            // Can't actually detatch SQLite DB here as migrations run in a transaction
            // and you can't detach a database whilst in a transaction
            // IfDatabase(ProcessorId.SQLite).Execute.Sql("DETACH DATABASE \"TestSchema\"");
        }
    }

    internal class TestCreateAndDropTableMigrationWithSchema : Migration
    {
        public override void Up()
        {
            // SQLite only supports FK's defined in the create statement so
            // we ensure this is the only approach used so that SQLite can
            // successfully tested. At time of implementing, the FK constraint
            // wasn't explicitly used by any tests and so should affect anything.

            Create.Table("TestTable")
                .InSchema("TestSchema")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Name").AsString(255).NotNullable().WithDefaultValue("Anonymous");

            Create.Table("TestTable2")
                .InSchema("TestSchema")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Name").AsString(255).Nullable()
                .WithColumn("TestTableId").AsInt32().NotNullable().ForeignKey("fk_TestTable2_TestTableId_TestTable_Id", "TestSchema", "TestTable", "Id");

            Create.Index("ix_Name").OnTable("TestTable2").InSchema("TestSchema").OnColumn("Name").Ascending()
                .WithOptions().NonClustered();

            Create.Column("Name2").OnTable("TestTable2").InSchema("TestSchema").AsString(10).Nullable();

            Insert.IntoTable("TestTable").InSchema("TestSchema").Row(new { Name = "Test" });
        }

        public override void Down()
        {
            Delete.Table("TestTable2").InSchema("TestSchema");
            Delete.Table("TestTable").InSchema("TestSchema");
        }
    }

    internal class TestRenameTableMigrationWithSchema : AutoReversingMigration
    {
        public override void Up()
        {
            Rename.Table("TestTable2").InSchema("TestSchema").To("TestTable'3");
        }
    }

    internal class TestRenameColumnMigrationWithSchema : AutoReversingMigration
    {
        public override void Up()
        {
            Rename.Column("Name").OnTable("TestTable2").InSchema("TestSchema").To("Name'3");
        }
    }

    internal class TestCreateAndDropIndexMigrationWithSchema : Migration
    {
        public override void Up()
        {
            Create.Index("IX_TestTable_Name").OnTable("TestTable").InSchema("TestSchema").OnColumn("Name");
        }

        public override void Down()
        {
            Delete.Index("IX_TestTable_Name").OnTable("TestTable").InSchema("TestSchema");
        }
    }

    internal class TestCreateSchema : Migration
    {
        public override void Up()
        {
            // SQLite doesn't support creating schemas so for non SQLite DB's we'll create
            // the schema, but for SQLite we'll attach a temp DB with the schema alias
            _ = IfDatabase(t => t != ProcessorIdConstants.SQLite).Create.Schema("TestSchema");

            IfDatabase(ProcessorIdConstants.SQLite).Execute.Sql("ATTACH DATABASE '' AS \"TestSchema\"");
        }

        public override void Down()
        {
            IfDatabase(t => t != ProcessorIdConstants.SQLite).Delete.Schema("TestSchema");

            // Can't actually detatch SQLite DB here as migrations run in a transaction
            // and you can't detach a database whilst in a transaction
            // IfDatabase(ProcessorId.SQLite).Execute.Sql("DETACH DATABASE \"TestSchema\"");
        }
    }

    internal class TestCreateSequence : Migration
    {
        public override void Up()
        {
            Create.Sequence("TestSequence").StartWith(1).IncrementBy(1).MinValue(0).MaxValue(1000).Cycle().Cache(10);
        }

        public override void Down()
        {
            Delete.Sequence("TestSequence");
        }
    }

    internal class TestCreateSequenceWithSchema : Migration
    {
        public override void Up()
        {
            Create.Sequence("TestSequence").InSchema("TestSchema").StartWith(1).IncrementBy(1).MinValue(0).MaxValue(1000).Cycle().Cache(10);
        }

        public override void Down()
        {
            Delete.Sequence("TestSequence").InSchema("TestSchema");
        }
    }

    internal class TestAlterColumnWithSchema: Migration
    {
        public override void Up()
        {
            Alter.Column("Name2").OnTable("TestTable2").InSchema("TestSchema").AsAnsiString(100).Nullable();
        }

        public override void Down()
        {
            Alter.Column("Name2").OnTable("TestTable2").InSchema("TestSchema").AsString(10).Nullable();
        }
    }

    internal class TestAlterTableWithSchema : Migration
    {
        public override void Up()
        {
            Alter.Table("TestTable2").InSchema("TestSchema").AddColumn("NewColumn").AsInt32().WithDefaultValue(1);
        }

        public override void Down()
        {
            Delete.Column("NewColumn").FromTable("TestTable2").InSchema("TestSchema");
        }
    }

    internal class TestAlterSchema : Migration
    {
        public override void Up()
        {
            _ = Create.Schema("NewSchema");

            Alter.Table("TestTable").InSchema("TestSchema").ToSchema("NewSchema");
        }

        public override void Down()
        {
            Alter.Table("TestTable").InSchema("NewSchema").ToSchema("TestSchema");
            Delete.Schema("NewSchema");
        }
    }

    internal class TestCreateUniqueConstraint : Migration
    {
        public override void Up()
        {
            Create.UniqueConstraint("TestUnique").OnTable("TestTable2").Column("Name");
        }

        public override void Down()
        {
            Delete.UniqueConstraint("TestUnique").FromTable("TestTable2");
        }
    }

    internal class TestCreateUniqueConstraintWithSchema : Migration
    {
        public override void Up()
        {
            Create.UniqueConstraint("TestUnique").OnTable("TestTable2").WithSchema("TestSchema").Column("Name");
        }

        public override void Down()
        {
            Delete.UniqueConstraint("TestUnique").FromTable("TestTable2").InSchema("TestSchema");
        }
    }

    internal class TestUpdateData : Migration
    {
        public override void Up()
        {
            Update.Table("TestTable").InSchema("TestSchema").Set(new { Name = "Updated" }).AllRows();
        }

        public override void Down()
        {
            Update.Table("TestTable").InSchema("TestSchema").Set(new { Name = "Test" }).AllRows();
        }
    }

    public class RawSqlCreateTableMigration : Migration
    {
        public override void Up()
        {
            Create.Table("Foo")
                .InSchema("TestSchema")
                .WithColumn("baz").AsInt32().NotNullable();

            Insert.IntoTable("Foo")
                .InSchema("TestSchema")
                .Row(new
                {
                    baz = 1,
                })
                .Row(new
                {
                    baz = 2,
                })
                .Row(new
                {
                    baz = 3,
                })
                ;
        }

        public override void Down()
        {
            Delete.Table("Foo").InSchema("TestSchema");
        }
    }

    public class RawSqlUpdateMigration : ForwardOnlyMigration
    {
        public override void Up()
        {
            // UPDATE : Raw SQL with string
            Update.Table("Foo").InSchema("TestSchema")
                .Set("baz = CASE WHEN baz = 1 THEN 101 ELSE 0 END")
                .Where("baz = 1");

            // UPDATE : Raw SQL with RawSql object
            Update.Table("Foo").InSchema("TestSchema")
                .Set(RawSql.Insert("baz = CASE WHEN baz = 2 THEN 102 ELSE 0 END"))
                .Where(RawSql.Insert("baz = 2"));

            // UPDATE : Raw SQL with RawSql object inside an anonymous object
            Update.Table("Foo").InSchema("TestSchema")
                .Set(new
                {
                    baz = RawSql.Insert("CASE WHEN baz = 3 THEN 103 ELSE 0 END")
                })
                .Where(new
                {
                    baz = RawSql.Insert("= 3")
                });
        }
    }

    public class RawSqlDeleteMigration : ForwardOnlyMigration
    {
        public override void Up()
        {
            Delete.FromTable("Foo").InSchema("TestSchema")

                // DELETE : Raw SQL with string
                .Row("baz = 101")

                // DELETE : Raw SQL with RawSql object
                .Row(RawSql.Insert("baz = 102"))

                // DELETE : Raw SQL with RawSql object inside an anonymous object
                .Row(new
                {
                    baz = RawSql.Insert("= 103")
                });
        }
    }

    internal class TestDeleteData : Migration
    {
        public override void Up()
        {
            Delete.FromTable("TestTable").Row(new { Name = "Test" });
        }

        public override void Down()
        {
            Insert.IntoTable("TestTable").Row(new { Name = "Test" });
        }
    }

    internal class TestDeleteDataWithSchema :Migration
    {
        public override void Up()
        {
            Delete.FromTable("TestTable").InSchema("TestSchema").Row(new { Name = "Test"});
        }

        public override void Down()
        {
            Insert.IntoTable("TestTable").InSchema("TestSchema").Row(new { Name = "Test" });
        }
    }

    internal class TestCreateIndexWithReversing : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Index().OnTable("TestTable2").InSchema("TestSchema").OnColumn("Name2").Ascending();
        }
    }

    internal class TestCreateUniqueConstraintWithReversing : AutoReversingMigration
    {
        public override void Up()
        {
            Create.UniqueConstraint("TestUnique").OnTable("TestTable2").Column("Name");
        }
    }

    internal class TestCreateUniqueConstraintWithSchemaWithReversing : AutoReversingMigration
    {
        public override void Up()
        {
            Create.UniqueConstraint("TestUnique").OnTable("TestTable2").WithSchema("TestSchema").Column("Name");
        }
    }

    internal class TestExecuteSqlDescription : Migration
    {
        public override void Up()
        {
            Execute.Sql("SELECT 1 FROM FOO", "Description Up");
        }

        public override void Down()
        {
            Execute.Sql("SELECT 2 FROM BAR", "Description Down");
        }
    }

    internal class TestExecuteSqlParameters : Migration
    {
        public override void Up()
        {
            Execute.Sql("SELECT 1 FROM FOO WHERE BAR = $(BAZ)", new Dictionary<string, string>()
            {
                ["BAZ"] = "'test'"
            });
        }

        public override void Down()
        {
        }
    }

    internal class TestExecuteSql : Migration
    {
        public override void Up()
        {
            Execute.Sql("select 1");
        }

        public override void Down()
        {
            Execute.Sql("select 2");
        }
    }
}
