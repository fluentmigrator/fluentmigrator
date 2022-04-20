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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FirebirdSql.Data.FirebirdClient;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.MySql;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Runner.Processors.SQLite;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.SqlAnywhere;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration
{
    public class IntegrationTestBase
    {
        private readonly List<(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> getOptionsFunc)> _processors;

        private bool _isFirstExecuteForFirebird = true;

        private string _tempDataDirectory;

        protected IntegrationTestBase()
        {
            _processors = new List<(Type, Func<IntegrationTestOptions.DatabaseServerOptions>)>
            {
                (typeof(SqlServer2005Processor), () => IntegrationTestOptions.SqlServer2005),
                (typeof(SqlServer2008Processor), () => IntegrationTestOptions.SqlServer2008),
                (typeof(SqlServer2012Processor), () => IntegrationTestOptions.SqlServer2012),
                (typeof(SqlServer2014Processor), () => IntegrationTestOptions.SqlServer2014),
                (typeof(SqlServer2016Processor), () => IntegrationTestOptions.SqlServer2016),
                (typeof(SqlAnywhere16Processor), () => IntegrationTestOptions.SqlAnywhere16),
                (typeof(SQLiteProcessor), () => IntegrationTestOptions.SQLite),
                (typeof(FirebirdProcessor), () => IntegrationTestOptions.Firebird),
                (typeof(PostgresProcessor), () => IntegrationTestOptions.Postgres),
                (typeof(MySql4Processor), () => IntegrationTestOptions.MySql),
            };
        }

        [SetUp]
        public void SetUpFirebird()
        {
            _isFirstExecuteForFirebird = true;
        }

        [SetUp]
        public void SetUpDataDirectory()
        {
            _tempDataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDataDirectory);
            AppDomain.CurrentDomain.SetData("DataDirectory", _tempDataDirectory);
        }

        [TearDown]
        public void TearDownDataDirectory()
        {
            if (!string.IsNullOrEmpty(_tempDataDirectory) && Directory.Exists(_tempDataDirectory))
            {
                Directory.Delete(_tempDataDirectory, true);
            }
        }

        private bool IsAnyServerEnabled(params Type[] exceptProcessors)
        {
            return IsAnyServerEnabled(procType => !exceptProcessors.Any(p => p.IsAssignableFrom(procType)));
        }

        private bool IsAnyServerEnabled(Predicate<Type> isMatch)
        {
            foreach (var (processorType, getOptionsFunc) in _processors)
            {
                var opt = getOptionsFunc();
                if (!opt.IsEnabled)
                    continue;

                if (!isMatch(processorType))
                    continue;

                return true;
            }

            return false;
        }

        protected void ExecuteWithSupportedProcessors(
            Action<IServiceCollection> initAction,
            Action<IServiceProvider, ProcessorBase> testAction,
            bool tryRollback = true,
            params Type[] exceptProcessors)
        {
            ExecuteWithSupportedProcessors(
                initAction,
                testAction,
                tryRollback,
                procType => !exceptProcessors.Any(p => p.IsAssignableFrom(procType)));
        }

        private void ExecuteWithSupportedProcessors(
            Action<IServiceCollection> initAction,
            Action<IServiceProvider, ProcessorBase> testAction,
            bool tryRollback,
            Predicate<Type> isMatch)
        {
            if (!IsAnyServerEnabled())
            {
                Assert.Fail(
                    "No database processors are configured to run your migration tests.  This message is provided to avoid false positives.  To avoid this message enable one or more test runners in the {0} class.",
                    nameof(IntegrationTestOptions));
            }

            var executed = false;
            foreach (var (processorType, getOptionsFunc) in _processors)
            {
                var opt = getOptionsFunc();
                if (!opt.IsEnabled)
                    continue;

                if (!isMatch(processorType))
                    continue;

                executed = true;
                ExecuteWithProcessor(processorType, initAction, testAction, tryRollback, opt);
            }

            if (!executed)
            {
                Assert.Ignore("No processor found for the given action.");
            }
        }

        protected void ExecuteWithProcessor<TProcessor>(
            Action<IServiceCollection> initAction,
            Action<IServiceProvider, TProcessor> testAction,
            bool tryRollback,
            IntegrationTestOptions.DatabaseServerOptions serverOptions)
            where TProcessor : ProcessorBase
        {
            ExecuteWithProcessor(typeof(TProcessor), initAction, (sp, proc) => testAction(sp, (TProcessor)proc), tryRollback, serverOptions);
        }

        private void ExecuteWithProcessor(
            Type processorType,
            Action<IServiceCollection> initAction,
            Action<IServiceProvider, ProcessorBase> testAction,
            bool tryRollback,
            IntegrationTestOptions.DatabaseServerOptions serverOptions)
        {
            if (!serverOptions.IsEnabled)
                Assert.Ignore($"The configuration for {processorType.Name} is not enabled.");

            var services = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(
                    r => r
                        .AddFirebird()
                        .AddMySql4()
                        .AddPostgres()
                        .AddSQLite()
                        .AddSqlAnywhere16()
                        .AddSqlServer2005()
                        .AddSqlServer2008()
                        .AddSqlServer2012()
                        .AddSqlServer2014()
                        .AddSqlServer2016())
                .AddScoped<IProcessorAccessor>(
                    sp =>
                    {
                        var proc = (ProcessorBase)sp.GetRequiredService(processorType);
                        var opt = sp.GetRequiredService<IOptionsSnapshot<SelectingProcessorAccessorOptions>>();
                        var opt2 = sp.GetRequiredService<IOptionsSnapshot<SelectingGeneratorAccessorOptions>>();
                        return new SelectingProcessorAccessor(new[] { proc }, opt, opt2, sp);
                    })
                .AddScoped<IGeneratorAccessor>(
                    sp =>
                    {
                        var proc = (ProcessorBase)sp.GetRequiredService(processorType);
                        var opt = sp.GetRequiredService<IOptionsSnapshot<SelectingGeneratorAccessorOptions>>();
                        var opt2 = sp.GetRequiredService<IOptionsSnapshot<SelectingProcessorAccessorOptions>>();
                        return new SelectingGeneratorAccessor(new[] { proc.Generator }, opt, opt2);
                    })
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(serverOptions.ConnectionString));
            initAction?.Invoke(services);

            services
                .AddScoped(sp => sp.GetRequiredService<IProcessorAccessor>().Processor)
                .AddScoped(sp => sp.GetRequiredService<IGeneratorAccessor>().Generator);

            var serviceProvider = services
                .BuildServiceProvider(true);



            if (processorType == typeof(FirebirdProcessor) && _isFirstExecuteForFirebird)
            {
                _isFirstExecuteForFirebird = false;
                FbConnection.CreateDatabase(serverOptions.ConnectionString, true);
            }

            using (serviceProvider)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var sp = scope.ServiceProvider;
                    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger(GetType());

                    logger.LogHeader($"Testing Migration against {processorType}");

                    var processor = (ProcessorBase) sp.GetRequiredService(processorType);
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

        protected static bool LogException(Exception exception)
        {
            TestContext.WriteLine(exception);
            return false;
        }
    }
}
