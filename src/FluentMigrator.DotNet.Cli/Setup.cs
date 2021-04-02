#region License
// Copyright (c) 2007-2018, Sean Chambers and the FluentMigrator Project
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
using System.Linq;

using AutoMapper;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Logging;
using FluentMigrator.Runner.Processors;

using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace FluentMigrator.DotNet.Cli
{
    public static class Setup
    {
        public static IServiceProvider BuildServiceProvider(MigratorOptions options, IConsole console)
        {
            var serviceCollection = new ServiceCollection();
            var serviceProvider = ConfigureServices(serviceCollection, options, console);
            Configure(serviceProvider.GetRequiredService<ILoggerFactory>());
            return serviceProvider;
        }

        private static IServiceProvider ConfigureServices(IServiceCollection services, MigratorOptions options, IConsole console)
        {
            var conventionSet = new DefaultConventionSet(defaultSchemaName: options.SchemaName, options.WorkingDirectory);

            var targetIsSqlServer = !string.IsNullOrEmpty(options.ProcessorType)
             && options.ProcessorType.StartsWith("sqlserver", StringComparison.OrdinalIgnoreCase);

            var mapper = ConfigureMapper();
            services
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                .AddOptions()
                .AddSingleton(mapper);

            if (options.Output)
            {
                services
                    .AddSingleton<ILoggerProvider, LogFileFluentMigratorLoggerProvider>()
                    .Configure<LogFileFluentMigratorLoggerOptions>(
                        opt =>
                        {
                            opt.OutputFileName = options.OutputFileName;
                            opt.OutputGoBetweenStatements = targetIsSqlServer;
                            opt.ShowSql = true;
                        });
            }

            services
                .AddFluentMigratorCore()
                .ConfigureRunner(
                    builder => builder
                        .AddDb2()
                        .AddDb2ISeries()
                        .AddDotConnectOracle()
                        .AddDotConnectOracle12C()
                        .AddFirebird()
                        .AddHana()
                        .AddMySql4()
                        .AddMySql5()
                        .AddOracle()
                        .AddOracle12C()
                        .AddOracleManaged()
                        .AddOracle12CManaged()
                        .AddPostgres()
                        .AddPostgres92()
                        .AddPostgres10_0()
                        .AddPostgres11_0()
                        .AddRedshift()
                        .AddSQLite()
                        .AddSqlServer()
                        .AddSqlServer2000()
                        .AddSqlServer2005()
                        .AddSqlServer2008()
                        .AddSqlServer2012()
                        .AddSqlServer2014()
                        .AddSqlServer2016());

            services
                .AddSingleton<IConventionSet>(conventionSet)
                .Configure<SelectingProcessorAccessorOptions>(opt => opt.ProcessorId = options.ProcessorType)
                .Configure<AssemblySourceOptions>(opt => opt.AssemblyNames = options.TargetAssemblies.ToArray())
                .Configure<TypeFilterOptions>(
                    opt =>
                    {
                        opt.Namespace = options.Namespace;
                        opt.NestedNamespaces = options.NestedNamespaces;
                    })
                .Configure<RunnerOptions>(
                    opt =>
                    {
                        opt.Task = options.Task;
                        opt.Version = options.TargetVersion ?? 0;
                        opt.StartVersion = options.StartVersion ?? 0;
                        opt.NoConnection = options.NoConnection;
                        opt.Steps = options.Steps ?? 1;
                        opt.Profile = options.Profile;
                        opt.Tags = options.Tags.ToArray();
#pragma warning disable 612
                        opt.ApplicationContext = options.Context;
#pragma warning restore 612
                        opt.TransactionPerSession = options.TransactionMode == TransactionMode.Session;
                        opt.AllowBreakingChange = options.AllowBreakingChanges;
                        opt.IncludeUntaggedMigrations = options.IncludeUntaggedMigrations;
                        opt.IncludeUntaggedMaintenances = options.IncludeUntaggedMaintenances;
                    })
                .Configure<ProcessorOptions>(
                    opt =>
                    {
                        opt.ConnectionString = options.ConnectionString;
                        opt.PreviewOnly = options.Preview;
                        opt.ProviderSwitches = options.ProcessorSwitches;
                        opt.StripComments = options.StripComments;
                        opt.Timeout = options.Timeout == null ? null : (TimeSpan?) TimeSpan.FromSeconds(options.Timeout.Value);
                    });

            services
                .Configure<MigratorOptions>(mc => mapper.Map(options, mc));

            services
                .Configure<FluentMigratorLoggerOptions>(
                    opt =>
                    {
                        opt.ShowElapsedTime = options.Verbose;
                        opt.ShowSql = options.Verbose;
                    });

            services
                .AddSingleton(console);

            return services.BuildServiceProvider();
        }

        private static void Configure(ILoggerFactory loggerFactory)
        {
            // loggerFactory
            //     .AddFluentMigratorConsole()
            //     .AddDebug(LogLevel.Trace);
        }

        private static IMapper ConfigureMapper()
        {
            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<MigratorOptions, MigratorOptions>());
            mapperConfig.AssertConfigurationIsValid();
            return new Mapper(mapperConfig);
        }
    }
}
