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

using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Snowflake;
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
        private const string RootNamespace = "FluentMigrator.Tests.Integration.Migrations";
        private readonly bool _quotingEnabled;
        public const string TestSchema = "TestSchema";
        public const string TestTable = "TestTable";

        public SnowflakeMigrationRunnerTests(bool quotingEnabled)
        {
            _quotingEnabled = quotingEnabled;
        }


        [OneTimeSetUp]
        public void ClassSetUp()
        {
            if (!IntegrationTestOptions.Snowflake.IsEnabled)
            {
                Assert.Ignore("Snowflake integration tests are disabled.");
            }
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
            Create.Schema(SnowflakeMigrationRunnerTests.TestSchema);
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
}
