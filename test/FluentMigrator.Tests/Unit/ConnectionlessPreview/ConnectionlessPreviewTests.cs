#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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
using System.Text;
using System.Threading.Tasks;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Logging;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Snowflake;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

using VerifyNUnit;

namespace FluentMigrator.Tests.Unit.ConnectionlessPreview
{
    [TestFixture]
    [Category("ConnectionlessPreview")]
    public class ConnectionlessPreviewTests
    {
        private static IEnumerable<TestCaseData> ProviderCases()
        {
            yield return Case("Db2", builder => builder.AddDb2());
            yield return Case("Db2ISeries", builder => builder.AddDb2ISeries());
            yield return Case("DotConnectOracle", builder => builder.AddDotConnectOracle());
            yield return Case("DotConnectOracle12C", builder => builder.AddDotConnectOracle12C());
            yield return Case("Firebird", builder => builder.AddFirebird());
            yield return Case("Hana8", builder => builder.AddHana8());
            yield return Case("Hana10", builder => builder.AddHana10());
            yield return Case("MySql", builder => builder.AddMySql());
            yield return Case("MySql4", builder => builder.AddMySql4());
            yield return Case("MySql5", builder => builder.AddMySql5());
            yield return Case("MySql8", builder => builder.AddMySql8());
            yield return Case("Oracle", builder => builder.AddOracle());
            yield return Case("Oracle12C", builder => builder.AddOracle12C());
            yield return Case("OracleManaged", builder => builder.AddOracleManaged());
            yield return Case("Oracle12CManaged", builder => builder.AddOracle12CManaged());
            yield return Case("Postgres", builder => builder.AddPostgres());
            yield return Case("Postgres92", builder => builder.AddPostgres92());
            yield return Case("Postgres10_0", builder => builder.AddPostgres10_0());
            yield return Case("Postgres11_0", builder => builder.AddPostgres11_0());
            yield return Case("Postgres15_0", builder => builder.AddPostgres15_0());
            yield return Case("Redshift", builder => builder.AddRedshift());
            yield return Case("Snowflake", builder => builder.AddSnowflake());
            yield return Case("SQLite", builder => builder.AddSQLite());
            yield return Case("SqlServer", builder => builder.AddSqlServer());
            yield return Case("SqlServer2000", builder => builder.AddSqlServer2000());
            yield return Case("SqlServer2005", builder => builder.AddSqlServer2005());
            yield return Case("SqlServer2008", builder => builder.AddSqlServer2008());
            yield return Case("SqlServer2012", builder => builder.AddSqlServer2012());
            yield return Case("SqlServer2014", builder => builder.AddSqlServer2014());
            yield return Case("SqlServer2016", builder => builder.AddSqlServer2016());
        }

        [TestCaseSource(nameof(ProviderCases))]
        public Task GeneratesCompleteScriptFromEmptyDatabase(ProviderCase provider)
        {
            var script = GenerateScript(
                provider.Configure,
                options =>
                {
                    options.NoConnection = true;
                    options.PreviewFromNothing = true;
                },
                previewOnly: true);

            return Verifier.Verify(script, "sql").UseParameters(provider.Name);
        }

        /// <summary>
        /// Without a connection string the runner is connectionless even when <see cref="RunnerOptions.NoConnection"/>
        /// is not set (for example the MSBuild task or an in-process runner), and preview mode isn't needed either,
        /// because the connectionless processor never executes anything.
        /// </summary>
        [Test]
        public void EmitsTheVersionTableWithoutAConnectionStringOrPreviewMode()
        {
            var script = GenerateScript(
                builder => builder.AddSqlServer2016(),
                options => options.PreviewFromNothing = true,
                previewOnly: false);

            Assert.That(script, Does.Contain("CREATE TABLE [dbo].[VersionInfo]"));
            Assert.That(script, Does.Contain("CREATE TABLE [dbo].[Users]"));
        }

        /// <summary>
        /// Nothing runs, so there is nothing to protect from a breaking change, as in preview mode.
        /// </summary>
        [Test]
        public void ScriptsABreakingChangeWithoutPreviewMode()
        {
            var script = GenerateScript(
                builder => builder.AddSqlServer2016(),
                options => options.PreviewFromNothing = true,
                previewOnly: false,
                typeFilterNamespace: typeof(WithBreakingChange.DropLegacyTable).Namespace);

            Assert.That(script, Does.Contain("CREATE TABLE [dbo].[VersionInfo]"));
            Assert.That(script, Does.Contain("DROP TABLE [dbo].[Legacy]"));
        }

        /// <summary>
        /// SQL Server compares schema names by the database collation, which a script can't know: <c>DBO</c> is
        /// <c>dbo</c> in a case-insensitive database and a new schema in a case-sensitive one. So the script asks the
        /// database.
        /// </summary>
        [Test]
        public void LeavesDboInAnotherCaseToTheDatabaseCollation()
        {
            var script = GenerateScript(
                builder => builder.AddSqlServer2016(),
                PreviewFromNothingWithoutConnection,
                previewOnly: true,
                configureServices: services => services.AddSingleton<IConventionSet>(new DefaultConventionSet("DBO", null)));

            const string createSchema = "IF SCHEMA_ID(N'DBO') IS NULL EXEC(N'CREATE SCHEMA [DBO]');";
            var createSchemaIndex = script.IndexOf(createSchema, StringComparison.Ordinal);
            Assert.That(createSchemaIndex, Is.GreaterThanOrEqualTo(0), script);
            Assert.That(script.IndexOf("CREATE TABLE [DBO].[VersionInfo]", StringComparison.Ordinal), Is.GreaterThan(createSchemaIndex), script);
            Assert.That(script.Replace(createSchema, ""), Does.Not.Contain("CREATE SCHEMA"), script);
        }

        private static IEnumerable<TestCaseData> BuiltInSchemaCases()
        {
            yield return SchemaCase("SqlServer2016 dbo", builder => builder.AddSqlServer2016(), null, "dbo", "CREATE TABLE [dbo].[VersionInfo]");
            yield return SchemaCase("Postgres15_0 public", builder => builder.AddPostgres15_0(), null, "public", "CREATE TABLE \"public\".\"VersionInfo\"");
            yield return SchemaCase("Postgres15_0 unquoted PUBLIC", builder => builder.AddPostgres15_0(), PostgresQuotingOff, "PUBLIC", "CREATE TABLE PUBLIC.VersionInfo");
            yield return SchemaCase("Snowflake unquoted public", builder => builder.AddSnowflake(), null, "public", "CREATE TABLE public.VersionInfo");
        }

        private static IEnumerable<TestCaseData> NewSchemaCases()
        {
            yield return SchemaCase("SqlServer2016 app", builder => builder.AddSqlServer2016(), null, "app", "CREATE TABLE [app].[VersionInfo]", "CREATE SCHEMA [app]");
            yield return SchemaCase("Postgres15_0 quoted PUBLIC", builder => builder.AddPostgres15_0(), null, "PUBLIC", "CREATE TABLE \"PUBLIC\".\"VersionInfo\"", "CREATE SCHEMA \"PUBLIC\"");
            yield return SchemaCase("Snowflake quoted public", builder => builder.AddSnowflake(), SnowflakeQuotingOn, "public", "CREATE TABLE \"public\".\"VersionInfo\"", "CREATE SCHEMA \"public\"");
        }

        /// <summary>
        /// A new database already has its built-in default schema, so <c>CREATE SCHEMA</c> for it would fail. The
        /// default version table metadata claims to own whatever the default schema convention gives it. An unquoted
        /// name is folded the way the database folds it.
        /// </summary>
        [TestCaseSource(nameof(BuiltInSchemaCases))]
        public void DoesNotCreateTheBuiltInDefaultSchema(
            Action<IMigrationRunnerBuilder> configureProvider,
            Action<IServiceCollection> configureServices,
            string schema,
            string createVersionTable,
            string createSchema)
        {
            var script = GenerateScript(
                configureProvider,
                PreviewFromNothingWithoutConnection,
                previewOnly: true,
                configureServices: services =>
                {
                    services.AddSingleton<IConventionSet>(new DefaultConventionSet(schema, null));
                    configureServices?.Invoke(services);
                });

            Assert.That(script, Does.Not.Contain("CREATE SCHEMA"));
            Assert.That(script, Does.Contain(createVersionTable));
        }

        /// <summary>
        /// Any other owned schema is created before the version table, including a quoted name that differs from
        /// the built-in one only in case.
        /// </summary>
        [TestCaseSource(nameof(NewSchemaCases))]
        public void CreatesAnOwnedDefaultSchemaThatIsNotBuiltIn(
            Action<IMigrationRunnerBuilder> configureProvider,
            Action<IServiceCollection> configureServices,
            string schema,
            string createVersionTable,
            string createSchema)
        {
            var script = GenerateScript(
                configureProvider,
                PreviewFromNothingWithoutConnection,
                previewOnly: true,
                configureServices: services =>
                {
                    services.AddSingleton<IConventionSet>(new DefaultConventionSet(schema, null));
                    configureServices?.Invoke(services);
                });

            var createSchemaIndex = script.IndexOf(createSchema, StringComparison.Ordinal);
            Assert.That(createSchemaIndex, Is.GreaterThanOrEqualTo(0), script);
            Assert.That(script.IndexOf(createVersionTable, StringComparison.Ordinal), Is.GreaterThan(createSchemaIndex), script);
        }

        /// <summary>
        /// The primary key carries <c>UniqueIndexName</c>, so a unique index with the same name would fail on SQL
        /// Server and PostgreSQL (#2051).
        /// </summary>
        [TestCase("SqlServer2016")]
        [TestCase("Postgres15_0")]
        public void DoesNotAddAUniqueIndexToAPrimaryKeyVersionTable(string provider)
        {
            var script = GenerateScript(
                builder =>
                {
                    if (provider == "SqlServer2016")
                    {
                        builder.AddSqlServer2016();
                    }
                    else
                    {
                        builder.AddPostgres15_0();
                    }

                    builder.WithVersionTable(new PrimaryKeyVersionTableMetaData());
                },
                PreviewFromNothingWithoutConnection,
                previewOnly: true);

            // Only the version table's primary key has this name.
            Assert.That(script, Does.Contain("UC_Version\" PRIMARY KEY").Or.Contain("UC_Version] PRIMARY KEY"), script);
            Assert.That(script, Does.Not.Contain("INDEX"), script);
            Assert.That(script, Does.Contain("AppliedOn"), script);
        }

        /// <summary>
        /// An empty database has nothing to revert. With a connection string and without preview mode, the revert
        /// would otherwise run against the database.
        /// </summary>
        [TestCase("rollback")]
        [TestCase("rollback:toversion")]
        [TestCase("migrate:down")]
        public void RejectsRevertingMigrationsWithAConnection(string task)
        {
            Action<IMigrationRunner> run = task switch
            {
                "rollback" => runner => runner.Rollback(1),
                "rollback:toversion" => runner => runner.RollbackToVersion(0),
                _ => runner => runner.MigrateDown(0),
            };

            using var serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .Configure<RunnerOptions>(options => options.PreviewFromNothing = true)
                .Configure<TypeFilterOptions>(options => options.Namespace = typeof(CreateTableMigration).Namespace)
                .ConfigureRunner(
                    builder => builder
                        .AddSQLite()
                        .WithGlobalConnectionString("Data Source=:memory:")
                        .WithMigrationsIn(typeof(CreateTableMigration).Assembly))
                .BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            var exception = Assert.Throws<InvalidOperationException>(() => run(runner));

            Assert.That(exception.Message, Does.Contain(nameof(RunnerOptions.PreviewFromNothing)));
        }

        private static void PostgresQuotingOff(IServiceCollection services)
        {
            services.Configure<ProcessorOptions>(options => options.ProviderSwitches = "Force Quote=false");
        }

        private static void SnowflakeQuotingOn(IServiceCollection services)
        {
            // Registered after AddSnowflake, so it replaces the options that method registers.
            services.AddScoped(_ => SnowflakeOptions.QuotingEnabled());
        }

        private static TestCaseData SchemaCase(
            string name,
            Action<IMigrationRunnerBuilder> configureProvider,
            Action<IServiceCollection> configureServices,
            string schema,
            string createVersionTable,
            string createSchema = null)
        {
            return new TestCaseData(configureProvider, configureServices, schema, createVersionTable, createSchema)
                .SetArgDisplayNames(name);
        }

        /// <summary>
        /// A live run creates the version table when the version loader is first used, after <c>BeforeAll</c>
        /// maintenance, which may create the schema the table lives in. The script keeps that order.
        /// </summary>
        [Test]
        public void EmitsTheVersionTableAfterBeforeAllMaintenance()
        {
            var script = GenerateScript(
                // WithMigrationsIn only feeds versioned migrations; maintenance comes from the type source.
                builder => builder.AddSqlServer2016()
                    .WithVersionTable(new WithBeforeAllMaintenance.AuditVersionTableMetaData())
                    .WithTypes(typeof(WithBeforeAllMaintenance.CreateAuditSchema)),
                PreviewFromNothingWithoutConnection,
                previewOnly: true,
                typeFilterNamespace: typeof(WithBeforeAllMaintenance.CreateAuditSchema).Namespace);

            var createSchema = script.IndexOf("CREATE SCHEMA [audit]", StringComparison.Ordinal);
            Assert.That(createSchema, Is.GreaterThanOrEqualTo(0), script);
            Assert.That(script.IndexOf("CREATE TABLE [audit].[VersionInfo]", StringComparison.Ordinal), Is.GreaterThan(createSchema), script);
            Assert.That(script.IndexOf("CREATE SCHEMA [audit]", createSchema + 1, StringComparison.Ordinal), Is.EqualTo(-1), script);
        }

        private static void PreviewFromNothingWithoutConnection(RunnerOptions options)
        {
            options.NoConnection = true;
            options.PreviewFromNothing = true;
        }

        private static string GenerateScript(
            Action<IMigrationRunnerBuilder> configureProvider,
            Action<RunnerOptions> configureRunner,
            bool previewOnly,
            Action<IServiceCollection> configureServices = null,
            string typeFilterNamespace = null)
        {
            var sql = new StringBuilder();
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .AddSingleton<ILoggerProvider>(new SqlCaptureLoggerProvider(sql))
                .Configure(configureRunner)
                .Configure<ProcessorOptions>(options => options.PreviewOnly = previewOnly)
                .Configure<TypeFilterOptions>(
                    options => options.Namespace = typeFilterNamespace ?? typeof(CreateTableMigration).Namespace)
                .ConfigureRunner(
                    builder =>
                    {
                        configureProvider(builder);
                        builder.WithMigrationsIn(typeof(CreateTableMigration).Assembly);
                    });
            configureServices?.Invoke(services);

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IProcessorAccessor>().Processor;

            Assert.That(processor, Is.TypeOf<ConnectionlessProcessor>());
            var connectionlessProcessor = (ConnectionlessProcessor)processor;

            scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();

            return $"""
                -- Processor: {processor.DatabaseType}
                -- Generator: {connectionlessProcessor.Generator.GetType().Name}

                {sql}
                """;
        }

        private static TestCaseData Case(string name, Action<IMigrationRunnerBuilder> configure)
        {
            return new TestCaseData(new ProviderCase(name, configure)).SetArgDisplayNames(name);
        }

        public sealed class ProviderCase
        {
            public ProviderCase(string name, Action<IMigrationRunnerBuilder> configure)
            {
                Name = name;
                Configure = configure;
            }

            public string Name { get; }

            public Action<IMigrationRunnerBuilder> Configure { get; }
        }

        private sealed class SqlCaptureLoggerProvider : ILoggerProvider
        {
            private readonly ILogger _logger;

            public SqlCaptureLoggerProvider(StringBuilder sql)
            {
                _logger = new SqlCaptureLogger(sql);
            }

            public ILogger CreateLogger(string categoryName)
            {
                return _logger;
            }

            public void Dispose()
            {
            }

            private sealed class SqlCaptureLogger : ILogger
            {
                private readonly StringBuilder _sql;

                public SqlCaptureLogger(StringBuilder sql)
                {
                    _sql = sql;
                }

                public IDisposable BeginScope<TState>(TState state)
                {
                    return null;
                }

                public bool IsEnabled(LogLevel logLevel)
                {
                    return true;
                }

                public void Log<TState>(
                    LogLevel logLevel,
                    EventId eventId,
                    TState state,
                    Exception exception,
                    Func<TState, Exception, string> formatter)
                {
                    if (eventId == RunnerEventIds.Sql)
                    {
                        _sql.AppendLine(formatter(state, exception));
                    }
                }
            }
        }

        // Nested, so it doesn't add a top-level CreateTableMigration to the test assembly. The namespace filter
        // in GenerateScript is what keeps other migrations out of these scripts.
        [Migration(1)]
        public class CreateTableMigration : Migration
        {
            public override void Up()
            {
                Create.Table("Users")
                    .WithColumn("Id").AsInt32().NotNullable().PrimaryKey()
                    .WithColumn("Name").AsString(100).NotNullable();
            }

            public override void Down()
            {
                Delete.Table("Users");
            }
        }
    }
}

namespace FluentMigrator.Tests.Unit.ConnectionlessPreview.WithBeforeAllMaintenance
{
    // A separate namespace, so the provider snapshots above don't pick these up.

    [Maintenance(MigrationStage.BeforeAll)]
    public class CreateAuditSchema : Migration
    {
        public override void Up()
        {
            Create.Schema("audit");
        }

        public override void Down()
        {
        }
    }

    // The connectionless version loader requires at least one versioned migration.
    [Migration(1)]
    public class CreateOrdersTable : Migration
    {
        public override void Up()
        {
            Create.Table("Orders")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey();
        }

        public override void Down()
        {
            Delete.Table("Orders");
        }
    }

    public sealed class AuditVersionTableMetaData : FluentMigrator.Runner.VersionTableInfo.IVersionTableMetaData
    {
        public bool OwnsSchema => false;

        public string SchemaName => "audit";

        public string TableName => "VersionInfo";

        public string ColumnName => "Version";

        public string DescriptionColumnName => "Description";

        public string UniqueIndexName => "UC_Version";

        public string AppliedOnColumnName => "AppliedOn";

        public bool CreateWithPrimaryKey => false;
    }
}

namespace FluentMigrator.Tests.Unit.ConnectionlessPreview.WithBreakingChange
{
    // A separate namespace, so the provider snapshots above don't pick it up.

    [Migration(1, BreakingChange = true)]
    public class DropLegacyTable : Migration
    {
        public override void Up()
        {
            Delete.Table("Legacy");
        }

        public override void Down()
        {
        }
    }
}

namespace FluentMigrator.Tests.Unit.ConnectionlessPreview
{
    public sealed class PrimaryKeyVersionTableMetaData : FluentMigrator.Runner.VersionTableInfo.IVersionTableMetaData
    {
        public bool OwnsSchema => false;

        public string SchemaName => null;

        public string TableName => "VersionInfo";

        public string ColumnName => "Version";

        public string DescriptionColumnName => "Description";

        public string UniqueIndexName => "UC_Version";

        public string AppliedOnColumnName => "AppliedOn";

        public bool CreateWithPrimaryKey => true;
    }
}
