#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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
using System.Data;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Snowflake;
using FluentMigrator.SqlAnywhere;
using FluentMigrator.Tests.Integration.Migrations.TaggedWithSchema;
using FluentMigrator.Tests.Unit;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Snowflake
{
    [TestFixture(true)]
    [TestFixture(false)]
    [Category("Integration")]
    [Category("Snowflake")]
    public class SnowflakeMigrationRunnerTests
    {
        private const string RootNamespace = "FluentMigrator.Tests.Integration.Migrations.WithSchema";
        private readonly bool _quotingEnabled;
        public const string TestSchema = "TestSchema";
        public const string TestTable = "TestTable";

        public SnowflakeMigrationRunnerTests(bool quotingEnabled)
        {
            _quotingEnabled = quotingEnabled;
        }

        private void ExecuteWithProcessor(
            Action<IServiceCollection> initAction,
            Action<IServiceProvider, ProcessorBase> testAction,
            bool tryRollback)
        {
            var services = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(r => r.AddSnowflake())
                .AddScoped<IProcessorAccessor>(
                    sp =>
                    {
                        var proc = sp.GetRequiredService<SnowflakeProcessor>();
                        var opt = sp.GetRequiredService<IOptionsSnapshot<SelectingProcessorAccessorOptions>>();
                        var opt2 = sp.GetRequiredService<IOptionsSnapshot<SelectingGeneratorAccessorOptions>>();
                        return new SelectingProcessorAccessor(new[] {proc}, opt, opt2, sp);
                    })
                .AddScoped<IGeneratorAccessor>(
                    sp =>
                    {
                        var proc = sp.GetRequiredService<SnowflakeProcessor>();
                        var opt = sp.GetRequiredService<IOptionsSnapshot<SelectingGeneratorAccessorOptions>>();
                        var opt2 = sp.GetRequiredService<IOptionsSnapshot<SelectingProcessorAccessorOptions>>();
                        return new SelectingGeneratorAccessor(new[] {proc.Generator}, opt, opt2);
                    })
                .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader(IntegrationTestOptions.Snowflake.ConnectionString))
                .AddScoped(_ => _quotingEnabled ? SnowflakeOptions.QuotingEnabled() : SnowflakeOptions.QuotingDisabled());

            initAction?.Invoke(services);

            services
                .AddScoped(sp => sp.GetRequiredService<IProcessorAccessor>().Processor)
                .AddScoped(sp => sp.GetRequiredService<IGeneratorAccessor>().Generator);

            using (var serviceProvider = services.BuildServiceProvider(true))
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var sp = scope.ServiceProvider;
                    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger(GetType());

                    logger.LogHeader($"Testing Migration against {nameof(SnowflakeProcessor)}");

                    var processor = sp.GetRequiredService<SnowflakeProcessor>();

                    // Ensure that other tests didn't leave anything lying around.
                    var versionTableSchemaName = new TestVersionTableMetaData().SchemaName;
                    processor.Execute($"drop schema if exists {processor.Quoter.QuoteSchemaName(TestSchema)}");
                    processor.Execute($"drop schema if exists {processor.Quoter.QuoteSchemaName(versionTableSchemaName)}");

                    try
                    {
                        testAction(sp, processor);
                    }
                    finally
                    {
                        if (tryRollback && !processor.WasCommitted)
                        {
                            processor.RollbackTransaction();
                        }
                    }
                }
            }
        }

        [Test]
        public void CanRunMigration()
        {
            ExecuteWithProcessor(
                init => init.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateAndDropTableMigration());

                    processor.TableExists(TestSchema, TestTable).ShouldBeTrue();

                    runner.Down(new TestCreateAndDropTableMigration());
                    processor.TableExists(TestSchema, TestTable).ShouldBeFalse();
                },
                false);
        }

        [Test]
        public void CanApplyForeignKeyConvention()
        {
            ExecuteWithProcessor(
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestForeignKeyNamingConvention());
                    processor.ConstraintExists(TestSchema, "Users", "FK_Users_GroupId_Groups_GroupId").ShouldBeTrue();

                    runner.Down(new TestForeignKeyNamingConvention());
                    processor.ConstraintExists(TestSchema, "Users", "FK_Users_GroupId_Groups_GroupId").ShouldBeFalse();
                },
                true);
        }

        [Test]
        public void CanApplyUniqueConvention()
        {
            ExecuteWithProcessor(
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestUniqueConstraintNamingConvention());
                    processor.ConstraintExists(TestSchema, "Users", "UC_Users_GroupId").ShouldBeTrue();
                    processor.ConstraintExists(TestSchema, "Users", "UC_Users_AccountId").ShouldBeTrue();
                    processor.TableExists(TestSchema, "Users").ShouldBeTrue();

                    runner.Down(new TestUniqueConstraintNamingConvention());
                    processor.ConstraintExists(TestSchema, "Users", "UC_Users_GroupId").ShouldBeFalse();
                    processor.ConstraintExists(TestSchema, "Users", "UC_Users_AccountId").ShouldBeFalse();
                    processor.TableExists(TestSchema, "Users").ShouldBeFalse();
                },
                false);
        }

        [Test]
        public void CanRenameTableWithSchema()
        {
            if (!_quotingEnabled)
            {
                Assert.Ignore("This test uses single quotes in identifier names but quoting is disabled.");
            }

            ExecuteWithProcessor(
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.TableExists(TestSchema, "TestTable2").ShouldBeTrue();

                    runner.Up(new TestRenameTableMigrationWithSchema());
                    processor.TableExists(TestSchema, "TestTable2").ShouldBeFalse();
                    processor.TableExists(TestSchema, "TestTable'3").ShouldBeTrue();

                    runner.Down(new TestRenameTableMigrationWithSchema());
                    processor.TableExists(TestSchema, "TestTable'3").ShouldBeFalse();
                    processor.TableExists(TestSchema, "TestTable2").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());
                    processor.TableExists(TestSchema, "TestTable2").ShouldBeFalse();

                    runner.Down(new TestCreateSchema());

                    //processor.CommitTransaction();
                },
                true);
        }

        [Test]
        public void CanRenameColumnWithSchema()
        {
            if (!_quotingEnabled)
            {
                Assert.Ignore("This test uses single quotes in identifier names but quoting is disabled.");
            }

            ExecuteWithProcessor(
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.ColumnExists(TestSchema, "TestTable2", "Name").ShouldBeTrue();

                    runner.Up(new TestRenameColumnMigrationWithSchema());
                    processor.ColumnExists(TestSchema, "TestTable2", "Name").ShouldBeFalse();
                    processor.ColumnExists(TestSchema, "TestTable2", "Name'3").ShouldBeTrue();

                    runner.Down(new TestRenameColumnMigrationWithSchema());
                    processor.ColumnExists(TestSchema, "TestTable2", "Name'3").ShouldBeFalse();
                    processor.ColumnExists(TestSchema, "TestTable2", "Name").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());
                    processor.ColumnExists(TestSchema, "TestTable2", "Name").ShouldBeFalse();

                    runner.Down(new TestCreateSchema());
                },
                true);
        }

        [Test]
        public void CanRunMigrations()
        {
            ExecuteWithProcessor(
                services => services.ConfigureRunner(rb => rb.WithVersionTable(new TestVersionTableMetaData())).WithMigrationsIn(RootNamespace),
                (serviceProvider, _) =>
                {
                    var runner = (MigrationRunner) serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.MigrateUp(false);

                    runner.VersionLoader.VersionInfo.HasAppliedMigration(1001).ShouldBeTrue();
                    runner.VersionLoader.VersionInfo.HasAppliedMigration(1002).ShouldBeTrue();
                    runner.VersionLoader.VersionInfo.HasAppliedMigration(1003).ShouldBeTrue();
                    runner.VersionLoader.VersionInfo.Latest().ShouldBe(1003);

                    runner.RollbackToVersion(0, false);
                },
                true);
        }

        [Test]
        public void CanMigrateASpecificVersion()
        {
            ExecuteWithProcessor(
                services => services.ConfigureRunner(rb => rb.WithVersionTable(new TestVersionTableMetaData())).WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner) serviceProvider.GetRequiredService<IMigrationRunner>();
                    try
                    {
                        runner.MigrateUp(1001, false);

                        runner.VersionLoader.VersionInfo.HasAppliedMigration(1001).ShouldBeTrue();
                        processor.TableExists(TestSchema, "User").ShouldBeTrue();
                    }
                    catch (Exception ex) when (LogException(ex))
                    {
                    }
                    finally
                    {
                        runner.RollbackToVersion(0, false);
                    }
                },
                true);
        }

        [Test]
        public void CanMigrateASpecificVersionDown()
        {
            try
            {
                ExecuteWithProcessor(
                    services => services.ConfigureRunner(rb => rb.WithVersionTable(new TestVersionTableMetaData())).WithMigrationsIn(RootNamespace),
                    (serviceProvider, processor) =>
                    {
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var runner = (MigrationRunner) scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                            runner.MigrateUp(1001, false);
                            runner.VersionLoader.VersionInfo.HasAppliedMigration(1001).ShouldBeTrue();
                        }

                        processor.TableExists(TestSchema, "User").ShouldBeTrue();

                        using (var scope = serviceProvider.CreateScope())
                        {
                            var runner = (MigrationRunner) scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                            runner.MigrateDown(0, false);
                            runner.VersionLoader.VersionInfo.HasAppliedMigration(1001).ShouldBeFalse();
                        }

                        processor.TableExists(TestSchema, "User").ShouldBeFalse();
                    },
                    false);
            }
            finally
            {
                ExecuteWithProcessor(
                    services => services.ConfigureRunner(rb => rb.WithVersionTable(new TestVersionTableMetaData())).WithMigrationsIn(RootNamespace),
                    (serviceProvider, _) =>
                    {
                        var runner = (MigrationRunner) serviceProvider.GetRequiredService<IMigrationRunner>();
                        runner.RollbackToVersion(0, false);
                    },
                    false);
            }
        }

        [Test]
        public void RollbackAllShouldRemoveVersionInfoTable()
        {
            ExecuteWithProcessor(
                services => services.ConfigureRunner(rb => rb.WithVersionTable(new TestVersionTableMetaData())).WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner) serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.MigrateUp(1002);

                    processor.TableExists(
                        runner.VersionLoader.VersionTableMetaData.SchemaName,
                        runner.VersionLoader.VersionTableMetaData.TableName).ShouldBeTrue();

                    runner.RollbackToVersion(0);

                    processor.TableExists(
                        runner.VersionLoader.VersionTableMetaData.SchemaName,
                        runner.VersionLoader.VersionTableMetaData.TableName).ShouldBeFalse();
                },
                true);
        }

        [Test]
        public void MigrateUpWithTaggedMigrationsShouldOnlyApplyMatchedMigrations()
        {
            ExecuteWithProcessor(
                services => services.ConfigureRunner(rb => rb.WithVersionTable(new TestVersionTableMetaData())).WithMigrationsIn(typeof(CustomerATable).Namespace)
                    .Configure<RunnerOptions>(opt => opt.Tags = new[] { "CustomerA" }),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner) serviceProvider.GetRequiredService<IMigrationRunner>();

                    try
                    {
                        runner.MigrateUp(false);

                        processor.TableExists(TestSchema, "CustomerATable").ShouldBeTrue();
                        processor.TableExists(TestSchema, "NormalTable").ShouldBeTrue();
                        processor.TableExists(TestSchema, "CustomerBTable").ShouldBeFalse();
                        processor.TableExists(TestSchema, "CustomerAandBTable").ShouldBeTrue();
                    }
                    finally
                    {
                        runner.RollbackToVersion(0);
                    }
                },
                true);
        }

        [Test]
        public void MigrateUpWithTaggedMigrationsAndUsingMultipleTagsShouldOnlyApplyMatchedMigrations()
        {
            ExecuteWithProcessor(
                services => services.ConfigureRunner(rb => rb.WithVersionTable(new TestVersionTableMetaData())).WithMigrationsIn(typeof(CustomerATable).Namespace)
                    .Configure<RunnerOptions>(opt => opt.Tags = new[] { "CustomerA", "CustomerB" }),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner) serviceProvider.GetRequiredService<IMigrationRunner>();

                    try
                    {
                        runner.MigrateUp(false);

                        processor.TableExists(TestSchema, "CustomerATable").ShouldBeFalse();
                        processor.TableExists(TestSchema, "NormalTable").ShouldBeTrue();
                        processor.TableExists(TestSchema, "CustomerBTable").ShouldBeFalse();
                        processor.TableExists(TestSchema, "CustomerAandBTable").ShouldBeTrue();
                    }
                    finally
                    {
                        runner.RollbackToVersion(0);
                    }
                },
                true);
        }

        [Test]
        public void MigrateUpWithDifferentTaggedShouldIgnoreConcreteOfTagged()
        {
            ExecuteWithProcessor(
                services => services.ConfigureRunner(rb => rb.WithVersionTable(new TestVersionTableMetaData())).WithMigrationsIn(typeof(CustomerBTable).Namespace)
                    .Configure<RunnerOptions>(opt => opt.Tags = new[] { "CustomerB" }),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner) serviceProvider.GetRequiredService<IMigrationRunner>();

                    try
                    {
                        runner.MigrateUp(false);

                        processor.TableExists(TestSchema, "CustomerATable").ShouldBeFalse();
                        processor.TableExists(TestSchema, "NormalTable").ShouldBeTrue();
                        processor.TableExists(TestSchema, "CustomerBTable").ShouldBeTrue();
                    }
                    finally
                    {
                        runner.RollbackToVersion(0);
                    }
                },
                true);
        }

        [Test]
        public void MigrateDownWithDifferentTagsToMigrateUpShouldApplyMatchedMigrations()
        {
            var migrationsNamespace = typeof(CustomerATable).Namespace;
            var versionTableMetaData = new TestVersionTableMetaData();
            try
            {
                ExecuteWithProcessor(
                    services => services.ConfigureRunner(rb => rb.WithVersionTable(versionTableMetaData)).WithMigrationsIn(migrationsNamespace).Configure<RunnerOptions>(opt => opt.Tags = new[] { "CustomerA" }),
                    (serviceProvider, processor) =>
                    {
                        var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();
                        runner.MigrateUp(false);

                        processor.TableExists(TestSchema, "CustomerATable").ShouldBeTrue();
                        processor.TableExists(TestSchema, "NormalTable").ShouldBeTrue();
                        processor.TableExists(TestSchema, "CustomerBTable").ShouldBeFalse();
                        processor.TableExists(TestSchema, "CustomerAandBTable").ShouldBeTrue();
                    },
                    false);

                ExecuteWithProcessor(
                    services => services.ConfigureRunner(rb => rb.WithVersionTable(versionTableMetaData)).WithMigrationsIn(migrationsNamespace).Configure<RunnerOptions>(opt => opt.Tags = new[] { "CustomerB" }),
                    (serviceProvider, processor) =>
                    {
                        var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();
                        runner.MigrateDown(0, false);

                        // MigrateDown actually drops also customer A stuff because the schema is dropped!!!
                        processor.TableExists(TestSchema, "CustomerATable").ShouldBeFalse();
                        processor.TableExists(TestSchema, "NormalTable").ShouldBeFalse();
                        processor.TableExists(TestSchema, "CustomerBTable").ShouldBeFalse();
                        processor.TableExists(TestSchema, "CustomerAandBTable").ShouldBeFalse();
                    },
                    false);
            }
            finally
            {
                ExecuteWithProcessor(
                    services => services.ConfigureRunner(rb => rb.WithVersionTable(versionTableMetaData)).WithMigrationsIn(migrationsNamespace),
                    (serviceProvider, processor) =>
                    {
                        // MigrateDown above actually drops whole schema with Customer A stuff as well. Fix the situation by manually dropping also version table schema.
                        processor.Execute($"drop schema if exists {TestSchema}");
                        processor.Execute($"drop schema if exists {versionTableMetaData.SchemaName}");
                    },
                    false);
            }
        }

        [Test]
        public void MigrateUpWithTaggedMigrationsShouldNotApplyAnyMigrationsIfNoTagsParameterIsPassedIntoTheRunner()
        {
            ExecuteWithProcessor(
                services => services.ConfigureRunner(rb => rb.WithVersionTable(new TestVersionTableMetaData())).WithMigrationsIn(typeof(CustomerATable).Namespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    try
                    {
                        runner.MigrateUp(false);

                        processor.TableExists(TestSchema, "CustomerATable").ShouldBeFalse();
                        processor.TableExists(TestSchema, "NormalTable").ShouldBeTrue();
                        processor.TableExists(TestSchema, "CustomerBTable").ShouldBeFalse();
                        processor.TableExists(TestSchema, "CustomerAandBTable").ShouldBeFalse();
                    }
                    finally
                    {
                        runner.RollbackToVersion(0);
                    }
                },
                true);
        }

        [Test]
        public void ValidateVersionOrderShouldDoNothingIfUnappliedMigrationVersionIsGreaterThanLatestAppliedMigration()
        {
            var namespacePass2 = typeof(Migrations.Interleaved.Pass2.User).Namespace;
            var namespacePass3 = typeof(Migrations.Interleaved.Pass3.User).Namespace;

            try
            {
                ExecuteWithProcessor(
                    services => services.ConfigureRunner(rb => rb.WithVersionTable(new TestVersionTableMetaData())).WithMigrationsIn(namespacePass2),
                    (serviceProvider, _) =>
                    {
                        var runner = serviceProvider
                            .GetRequiredService<IMigrationRunner>();

                        runner.MigrateUp(3);
                    },
                    tryRollback: false);

                ExecuteWithProcessor(
                    services => services.ConfigureRunner(rb => rb.WithVersionTable(new TestVersionTableMetaData())).WithMigrationsIn(namespacePass3),
                    (serviceProvider, _) =>
                    {
                        var runner = serviceProvider
                            .GetRequiredService<IMigrationRunner>();

                        Assert.DoesNotThrow(runner.ValidateVersionOrder);
                    },
                    tryRollback: false);
            }
            finally
            {
                ExecuteWithProcessor(
                    services => services.ConfigureRunner(rb => rb.WithVersionTable(new TestVersionTableMetaData())).WithMigrationsIn(namespacePass3),
                    (serviceProvider, _) =>
                    {
                        var runner = serviceProvider
                            .GetRequiredService<IMigrationRunner>();
                        runner.RollbackToVersion(0);
                    },
                    tryRollback: true);
            }
        }

        [Test]
        public void CanAlterColumnWithSchema()
        {
            ExecuteWithProcessor(
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.ColumnExists(TestSchema, "TestTable2", "Name2").ShouldBeTrue();
                    processor.DefaultValueExists(TestSchema, TestTable, "Name", "Anonymous").ShouldBeTrue();

                    runner.Up(new TestAlterColumnWithSchema());
                    processor.ColumnExists(TestSchema, "TestTable2", "Name2").ShouldBeTrue();

                    runner.Down(new TestAlterColumnWithSchema());
                    processor.ColumnExists(TestSchema, "TestTable2", "Name2").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Down(new TestCreateSchema());
                },
                true);
        }

        [Test]
        public void CanCreateUniqueConstraint()
        {
            ExecuteWithProcessor(
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateAndDropTableMigration());
                    processor.ConstraintExists(TestSchema, "TestTable2", "TestUnique").ShouldBeFalse();

                    runner.Up(new TestCreateUniqueConstraint());
                    processor.ConstraintExists(TestSchema, "TestTable2", "TestUnique").ShouldBeTrue();

                    runner.Down(new TestCreateUniqueConstraint());
                    processor.ConstraintExists(TestSchema, "TestTable2", "TestUnique").ShouldBeFalse();

                    runner.Down(new TestCreateAndDropTableMigration());
                },
                true);
        }

        [Test]
        public void CanDeleteData()
        {
            ExecuteWithProcessor(
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateAndDropTableMigration());

                    runner.Up(new TestDeleteData());
                    DataSet upDs = processor.ReadTableData(TestSchema, TestTable);
                    upDs.Tables[0].Rows.Count.ShouldBe(0);

                    runner.Down(new TestDeleteData());
                    DataSet downDs = processor.ReadTableData(TestSchema, TestTable);
                    downDs.Tables[0].Rows.Count.ShouldBe(1);
                    downDs.Tables[0].Rows[0][1].ShouldBe("Test");

                    runner.Down(new TestCreateAndDropTableMigration());
                },
                true);
        }

        [Test]
        public void CanReverseCreateUniqueConstraint()
        {
            ExecuteWithProcessor(
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestCreateAndDropTableMigration());

                    runner.Up(new TestCreateUniqueConstraintWithReversing());
                    processor.ConstraintExists(TestSchema, "TestTable2", "TestUnique").ShouldBeTrue();

                    runner.Down(new TestCreateUniqueConstraintWithReversing());
                    processor.ConstraintExists(TestSchema, "TestTable2", "TestUnique").ShouldBeFalse();

                    runner.Down(new TestCreateAndDropTableMigration());
                },
                true);
        }

        protected static bool LogException(Exception exception)
        {
            TestContext.WriteLine(exception);
            return false;
        }
    }

    internal class TestForeignKeyNamingConvention : Migration
    {
        public override void Up()
        {
            Create.Schema(SnowflakeMigrationRunnerTests.TestSchema);

            Create.Table("Users").InSchema(SnowflakeMigrationRunnerTests.TestSchema)
                .WithColumn("UserId").AsInt32().Identity().PrimaryKey()
                .WithColumn("GroupId").AsInt32().NotNullable()
                .WithColumn("UserName").AsString(32).NotNullable()
                .WithColumn("Password").AsString(32).NotNullable();

            Create.Table("Groups").InSchema(SnowflakeMigrationRunnerTests.TestSchema)
                .WithColumn("GroupId").AsInt32().Identity().PrimaryKey()
                .WithColumn("Name").AsString(32).NotNullable();

            Create.ForeignKey().FromTable("Users").InSchema(SnowflakeMigrationRunnerTests.TestSchema).ForeignColumn("GroupId").ToTable("Groups").InSchema(SnowflakeMigrationRunnerTests.TestSchema).PrimaryColumn("GroupId");
        }

        public override void Down()
        {
            Delete.Table("Users").InSchema(SnowflakeMigrationRunnerTests.TestSchema);
            Delete.Table("Groups").InSchema(SnowflakeMigrationRunnerTests.TestSchema);
            Delete.Schema(SnowflakeMigrationRunnerTests.TestSchema);
        }
    }

    internal class TestUniqueConstraintNamingConvention : Migration
    {
        public override void Up()
        {
            Create.Schema(SnowflakeMigrationRunnerTests.TestSchema);
            Create.Table("Users").InSchema(SnowflakeMigrationRunnerTests.TestSchema)
                .WithColumn("UserId").AsInt32().Identity().PrimaryKey()
                .WithColumn("GroupId").AsInt32().NotNullable()
                .WithColumn("AccountId").AsInt32().NotNullable()
                .WithColumn("UserName").AsString(32).NotNullable()
                .WithColumn("Password").AsString(32).NotNullable();

            Create.UniqueConstraint().OnTable("Users").WithSchema(SnowflakeMigrationRunnerTests.TestSchema).Column("GroupId");
            Create.UniqueConstraint().OnTable("Users").WithSchema(SnowflakeMigrationRunnerTests.TestSchema).Column("AccountId");
        }

        public override void Down()
        {
            Delete.UniqueConstraint("UC_Users_GroupId").FromTable("Users").InSchema(SnowflakeMigrationRunnerTests.TestSchema);
            Delete.UniqueConstraint().FromTable("Users").InSchema(SnowflakeMigrationRunnerTests.TestSchema).Column("AccountId");
            Delete.Table("Users").InSchema(SnowflakeMigrationRunnerTests.TestSchema);
            Delete.Schema(SnowflakeMigrationRunnerTests.TestSchema);
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

    internal class TestCreateAndDropTableMigration : Migration
    {
        public override void Up()
        {
            Create.Schema(SnowflakeMigrationRunnerTests.TestSchema);

            Create.Table(SnowflakeMigrationRunnerTests.TestTable).InSchema(SnowflakeMigrationRunnerTests.TestSchema)
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Name").AsString(255).NotNullable().WithDefaultValue("Anonymous");

            Create.Table("TestTable2").InSchema(SnowflakeMigrationRunnerTests.TestSchema)
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Name").AsString(255).Nullable()
                .WithColumn("TestTableId").AsInt32().NotNullable();

            Create.Column("Name2").OnTable("TestTable2").InSchema(SnowflakeMigrationRunnerTests.TestSchema).AsBoolean().Nullable();

            Create.ForeignKey("fk_TestTable2_TestTableId_TestTable_Id")
                .FromTable("TestTable2").InSchema(SnowflakeMigrationRunnerTests.TestSchema).ForeignColumn("TestTableId")
                .ToTable(SnowflakeMigrationRunnerTests.TestTable).InSchema(SnowflakeMigrationRunnerTests.TestSchema).PrimaryColumn("Id");

            Insert.IntoTable(SnowflakeMigrationRunnerTests.TestTable).InSchema(SnowflakeMigrationRunnerTests.TestSchema).Row(new { Name = "Test" });
        }

        public override void Down()
        {
            Delete.Table("TestTable2").InSchema(SnowflakeMigrationRunnerTests.TestSchema);
            Delete.Table(SnowflakeMigrationRunnerTests.TestTable).InSchema(SnowflakeMigrationRunnerTests.TestSchema);
            Delete.Schema(SnowflakeMigrationRunnerTests.TestSchema);
        }
    }

    internal class TestRenameTableMigrationWithSchema : AutoReversingMigration
    {
        public override void Up()
        {
            Rename.Table("TestTable2").InSchema(SnowflakeMigrationRunnerTests.TestSchema).To("TestTable'3");
        }
    }

    internal class TestRenameColumnMigrationWithSchema : AutoReversingMigration
    {
        public override void Up()
        {
            Rename.Column("Name").OnTable("TestTable2").InSchema(SnowflakeMigrationRunnerTests.TestSchema).To("Name'3");
        }
    }

    internal class TestCreateSchema : Migration
    {
        public override void Up()
        {
            var createSchemaExpr = Create.Schema(SnowflakeMigrationRunnerTests.TestSchema);
            IfDatabase(t => t.StartsWith("SqlAnywhere"))
                .Delegate(() => createSchemaExpr.Password("TestSchemaPassword"));
        }

        public override void Down()
        {
            Delete.Schema(SnowflakeMigrationRunnerTests.TestSchema);
        }
    }

    internal class TestAlterColumnWithSchema: Migration
    {
        public override void Up()
        {
            // Decreasing length is not allowed in Snowflake, so just changing to synonymous type & changing nullability.
            Alter.Column("Name2").OnTable("TestTable2").InSchema(SnowflakeMigrationRunnerTests.TestSchema).AsCustom("nvarchar(10)").NotNullable();
        }

        public override void Down()
        {
            Alter.Column("Name2").OnTable("TestTable2").InSchema(SnowflakeMigrationRunnerTests.TestSchema).AsString(10).Nullable();
        }
    }

    internal class TestCreateUniqueConstraint : Migration
    {
        public override void Up()
        {
            Create.UniqueConstraint("TestUnique").OnTable("TestTable2").WithSchema(SnowflakeMigrationRunnerTests.TestSchema).Column("Name");
        }

        public override void Down()
        {
            Delete.UniqueConstraint("TestUnique").FromTable("TestTable2").InSchema(SnowflakeMigrationRunnerTests.TestSchema);
        }
    }

    internal class TestDeleteData : Migration
    {
        public override void Up()
        {
            Delete.FromTable(SnowflakeMigrationRunnerTests.TestTable).InSchema(SnowflakeMigrationRunnerTests.TestSchema).Row(new { Name = "Test" });
        }

        public override void Down()
        {
            Insert.IntoTable(SnowflakeMigrationRunnerTests.TestTable).InSchema(SnowflakeMigrationRunnerTests.TestSchema).Row(new { Name = "Test" });
        }
    }

    internal class TestCreateUniqueConstraintWithReversing : AutoReversingMigration
    {
        public override void Up()
        {
            Create.UniqueConstraint("TestUnique").OnTable("TestTable2").WithSchema(SnowflakeMigrationRunnerTests.TestSchema).Column("Name");
        }
    }
}
