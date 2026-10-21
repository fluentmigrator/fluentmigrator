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
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Logging;
using FluentMigrator.Runner.Processors;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

using VerifyNUnit;

namespace FluentMigrator.Tests.Integration.Connectionless
{
    [TestFixture]
    [Category("Integration")]
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
            var sql = new StringBuilder();
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .AddSingleton<ILoggerProvider>(new SqlCaptureLoggerProvider(sql))
                .Configure<RunnerOptions>(
                    options =>
                    {
                        options.NoConnection = true;
                        options.PreviewFromNothing = true;
                    })
                .Configure<ProcessorOptions>(options => options.PreviewOnly = true)
                .Configure<TypeFilterOptions>(
                    options => options.Namespace = typeof(CreateTableMigration).Namespace)
                .ConfigureRunner(
                    builder =>
                    {
                        provider.Configure(builder);
                        builder.WithMigrationsIn(typeof(CreateTableMigration).Assembly);
                    });

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IProcessorAccessor>().Processor;

            Assert.That(processor, Is.TypeOf<ConnectionlessProcessor>());
            var connectionlessProcessor = (ConnectionlessProcessor)processor;

            scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();

            var script = $"""
                -- Processor: {processor.DatabaseType}
                -- Generator: {connectionlessProcessor.Generator.GetType().Name}

                {sql}
                """;
            return Verifier.Verify(script, "sql").UseParameters(provider.Name);
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
    }

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
