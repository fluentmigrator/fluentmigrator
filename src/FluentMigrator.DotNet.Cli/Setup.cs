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

using FluentMigrator.DotNet.Cli.CustomAnnouncers;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;

using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentMigrator.DotNet.Cli
{
    public class Setup
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
            var mapper = ConfigureMapper();
            services
                .AddLogging()
                .AddOptions()
                .AddSingleton(mapper);
            services
                .Configure<MigratorOptions>(mc => mapper.Map(options, mc));
            services
                .Configure<CustomAnnouncerOptions>(
                    cao =>
                    {
                        cao.ShowElapsedTime = options.Verbose;
                        cao.ShowSql = options.Verbose;
                    });
            services
                .AddSingleton(console)
                .AddSingleton<LateInitAnnouncer>()
                .AddSingleton<LoggingAnnouncer>()
                .AddSingleton<ParserConsoleAnnouncer>()
                .AddSingleton(CreateAnnouncer);
            services
                .AddSingleton(sp => CreateRunnerContext(sp, options));
            services
                .AddTransient<TaskExecutor, LateInitTaskExecutor>();
            return services.BuildServiceProvider();
        }

        private static void Configure(ILoggerFactory loggerFactory)
        {
            loggerFactory.AddDebug(LogLevel.Trace);
        }

        private static IMapper ConfigureMapper()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MigratorOptions, MigratorOptions>();
            });
            mapperConfig.AssertConfigurationIsValid();
            return new Mapper(mapperConfig);
        }

        private static IAnnouncer CreateAnnouncer(IServiceProvider serviceProvider)
        {
            var loggingAnnouncer = serviceProvider.GetRequiredService<LoggingAnnouncer>();
            var consoleAnnouncer = serviceProvider.GetRequiredService<ParserConsoleAnnouncer>();
            var lateInitAnnouncer = serviceProvider.GetRequiredService<LateInitAnnouncer>();
            return new CompositeAnnouncer(loggingAnnouncer, consoleAnnouncer, lateInitAnnouncer);
        }

        private static IRunnerContext CreateRunnerContext(IServiceProvider serviceProvider, MigratorOptions options)
        {
            var announcer = serviceProvider.GetRequiredService<IAnnouncer>();
            return new RunnerContext(announcer)
            {
                Task = options.Task,
                Connection = options.ConnectionString,
                ConnectionStringConfigPath = options.ConnectionStringConfigPath,
                Database = options.ProcessorType,
                ProviderSwitches = options.ProcessorSwitches,
                NoConnection = options.NoConnection || string.IsNullOrEmpty(options.ConnectionString),
                Version = options.TargetVersion ?? 0,
                Targets = options.TargetAssemblies.ToArray(),
                Steps = options.Steps ?? 1,
                Namespace = options.Namespace,
                NestedNamespaces = options.NestedNamespaces,
                StartVersion = options.StartVersion ?? 0,
                WorkingDirectory = options.WorkingDirectory,
                Tags = options.Tags.ToList(),
                PreviewOnly = options.Preview,
                Profile = options.Profile,
                ApplicationContext = options.Context,
                Timeout = options.Timeout,
                TransactionPerSession = options.TransactionMode == TransactionMode.Session,
            };
        }
    }
}
