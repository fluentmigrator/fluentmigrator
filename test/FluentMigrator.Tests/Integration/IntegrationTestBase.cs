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
using System.IO;

using FirebirdSql.Data.FirebirdClient;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Initialization;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration
{
    [Category("Integration")]
    public abstract class IntegrationTestBase
    {
        private bool _isFirstExecuteForFirebird = true;

        private string _tempDataDirectory;

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

        protected void ExecuteWithProcessor(
            Type processorType,
            Action<IServiceCollection> initAction,
            Action<IServiceProvider, ProcessorBase> testAction,
            Func<IntegrationTestOptions.DatabaseServerOptions> serverOptionsGetter,
            bool tryRollback = false)
        {
            var serverOptions = serverOptionsGetter();

            serverOptions.IgnoreIfNotEnabled();

            var services = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(
                    r => r
                        .AddFirebird()
                        .AddMySql4()
                        .AddOracle12CManaged()
                        .AddPostgres()
                        .AddSnowflake()
                        .AddSQLite()
                        .AddSqlServer2005()
                        .AddSqlServer2008()
                        .AddSqlServer2012()
                        .AddSqlServer2014()
                        .AddSqlServer2016()
                    )
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
                FbConnection.CreateDatabase(serverOptions.ConnectionString, pageSize:16384, overwrite: true);
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
            TestContext.Out.WriteLine(exception);
            return false;
        }
    }
}
