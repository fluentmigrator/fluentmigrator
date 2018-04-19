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
using System.Linq;
using System.Reflection;

using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization.AssemblyLoader;
using FluentMigrator.Runner.Processors;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner.Initialization
{
    [CLSCompliant(false)]
    public class TaskExecutor
    {
        private readonly IRunnerContext _runnerContext;
        private readonly AssemblyLoaderFactory _assemblyLoaderFactory;

        [CanBeNull]
        private readonly IServiceProvider _serviceProvider;

        public TaskExecutor(IRunnerContext runnerContext)
            : this(
                runnerContext,
                new AssemblyLoaderFactory(),
                new DefaultConnectionStringProvider())
        {
        }

        public TaskExecutor(IRunnerContext runnerContext, IServiceProvider serviceProvider)
        {
            _runnerContext = runnerContext ?? throw new ArgumentNullException(nameof(runnerContext));
            ConnectionStringProvider = serviceProvider.GetService<IConnectionStringProvider>();
            _assemblyLoaderFactory = serviceProvider.GetRequiredService<AssemblyLoaderFactory>();
            _serviceProvider = serviceProvider;
        }

        [Obsolete("Ony the statically provided factories are accessed")]
        public TaskExecutor(
            IRunnerContext runnerContext,
            [CanBeNull] IConnectionStringProvider connectionStringProvider,
            AssemblyLoaderFactory assemblyLoaderFactory,
            MigrationProcessorFactoryProvider _)
            : this(
                runnerContext,
                assemblyLoaderFactory,
                connectionStringProvider)
        {
        }

        public TaskExecutor(
            IRunnerContext runnerContext,
            AssemblyLoaderFactory assemblyLoaderFactory,
            [CanBeNull] IConnectionStringProvider connectionStringProvider = null)
        {
            _runnerContext = runnerContext ?? throw new ArgumentNullException(nameof(runnerContext));
            ConnectionStringProvider = connectionStringProvider;
            _assemblyLoaderFactory = assemblyLoaderFactory ?? throw new ArgumentNullException(nameof(assemblyLoaderFactory));
        }

        protected IMigrationRunner Runner { get; set; }

        protected IConnectionStringProvider ConnectionStringProvider { get; }

        protected virtual IEnumerable<Assembly> GetTargetAssemblies()
        {
            return _assemblyLoaderFactory.GetTargetAssemblies(_runnerContext.Targets);
        }

        private ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            var assemblies = GetTargetAssemblies().ToList();
#pragma warning disable 612
            var assemblyCollection = new AssemblyCollection(assemblies);
#pragma warning restore 612

            if (!_runnerContext.NoConnection && ConnectionStringProvider == null)
            {
                _runnerContext.NoConnection = true;
            }

            // Configure without the processor and migrations
            services
                .AddFluentMigratorCore();

            // Configure the processor
            if (_runnerContext.NoConnection)
            {
                var processorFactoryType = typeof(ConnectionlessProcessorFactory);
                services
                    .AddMigrationGenerators(MigrationGeneratorFactory.RegisteredGenerators)
                    .ConfigureProcessorFactory(processorFactoryType, string.Empty);
            }
            else
            {
                var connectionString = LoadConnectionString(assemblies);
                services
                    .AddMigrationProcessorFactories(MigrationProcessorFactoryProvider.RegisteredFactories)
                    .ConfigureProcessor(connectionString);
            }

            // Configure other options
            services
                .ConfigureRunner(
                    builder =>
                    {
                        builder
                            .WithProcessorOptions(CreateProcessorOptions())
                            .WithRunnerContext(_runnerContext)
                            .WithAnnouncer(_runnerContext.Announcer)
#pragma warning disable 612
                            .WithRunnerConventions(assemblyCollection.GetMigrationRunnerConventions())
#pragma warning restore 612
                            .AddMigrations(assemblies, _runnerContext.Namespace, _runnerContext.NestedNamespaces);
                    });

            // Configure the version table
            using (var sp = services.BuildServiceProvider(false))
            {
                var migConv = sp.GetRequiredService<IMigrationRunnerConventions>();
                var convSet = sp.GetRequiredService<IConventionSet>();
#pragma warning disable 612
                var versionTableMetaData = assemblyCollection.GetVersionTableMetaData(convSet, migConv, _runnerContext);
#pragma warning restore 612
                services.ConfigureRunner(builder => builder.ConfigureVersionTable(versionTableMetaData));
            }

            return services.BuildServiceProvider();
        }

        [Obsolete("This is not functional any more")]
        protected virtual void Initialize()
        {
        }

        public void Execute()
        {
            using (new RunnerScope(_serviceProvider, this))
            {
                switch (_runnerContext.Task)
                {
                    case null:
                    case "":
                    case "migrate":
                    case "migrate:up":
                        if (_runnerContext.Version != 0)
                            Runner.MigrateUp(_runnerContext.Version);
                        else
                            Runner.MigrateUp();
                        break;
                    case "rollback":
                        if (_runnerContext.Steps == 0)
                            _runnerContext.Steps = 1;
                        Runner.Rollback(_runnerContext.Steps);
                        break;
                    case "rollback:toversion":
                        Runner.RollbackToVersion(_runnerContext.Version);
                        break;
                    case "rollback:all":
                        Runner.RollbackToVersion(0);
                        break;
                    case "migrate:down":
                        Runner.MigrateDown(_runnerContext.Version);
                        break;
                    case "validateversionorder":
                        Runner.ValidateVersionOrder();
                        break;
                    case "listmigrations":
                        Runner.ListMigrations();
                        break;
                }
            }

            _runnerContext.Announcer.Say("Task completed.");
        }

        /// <summary>
        /// Checks whether the current task will actually run any migrations.
        /// Can be used to decide whether it's necessary perform a backup before the migrations are executed.
        /// </summary>
        public bool HasMigrationsToApply()
        {
            using (new RunnerScope(_serviceProvider, this))
            {
                switch (_runnerContext.Task)
                {
                    case null:
                    case "":
                    case "migrate":
                    case "migrate:up":
                        if (_runnerContext.Version != 0)
                            return Runner.HasMigrationsToApplyUp(_runnerContext.Version);

                        return Runner.HasMigrationsToApplyUp();
                    case "rollback":
                    case "rollback:all":
                        // Number of steps doesn't matter as long as there's at least
                        // one migration applied (at least that one will be rolled back)
                        return Runner.HasMigrationsToApplyRollback();
                    case "rollback:toversion":
                    case "migrate:down":
                        return Runner.HasMigrationsToApplyDown(_runnerContext.Version);
                    default:
                        return false;
                }
            }
        }

        private IMigrationProcessorOptions CreateProcessorOptions()
        {
            return new ProcessorOptions
            {
                PreviewOnly = _runnerContext.PreviewOnly,
                Timeout = _runnerContext.Timeout,
                ProviderSwitches = _runnerContext.ProviderSwitches
            };
        }

        private string LoadConnectionString(IReadOnlyCollection<Assembly> assemblies)
        {
            var singleAssembly = assemblies.Count == 1 ? assemblies.Single() : null;
            var singleAssemblyLocation = singleAssembly != null ? singleAssembly.Location : string.Empty;

            var connectionString = ConnectionStringProvider.GetConnectionString(
                _runnerContext.Announcer,
                _runnerContext.Connection,
                _runnerContext.ConnectionStringConfigPath,
                singleAssemblyLocation,
                _runnerContext.Database);

            return connectionString;
        }

        private class RunnerScope : IDisposable
        {
            [CanBeNull]
            private readonly ServiceProvider _temporaryServiceProvider;

            [NotNull]
            private readonly TaskExecutor _executor;

            [CanBeNull]
            private readonly IServiceScope _serviceScope;

            private readonly bool _hasCustomRunner;

            public RunnerScope([CanBeNull] IServiceProvider serviceProvider, [NotNull] TaskExecutor executor)
            {
                _executor = executor;

#pragma warning disable 618
                executor.Initialize();
#pragma warning restore 618

                if (executor.Runner != null)
                {
                    _hasCustomRunner = true;
                }
                else
                {
                    IServiceScope serviceScope;
                    if (serviceProvider == null)
                    {
                        _temporaryServiceProvider = executor.ConfigureServices();
                        serviceScope = _temporaryServiceProvider.CreateScope();
                    }
                    else
                    {
                        serviceScope = serviceProvider.CreateScope();
                    }

                    _serviceScope = serviceScope;
                    _executor.Runner = serviceScope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                }
            }

            public void Dispose()
            {
                if (_hasCustomRunner)
                {
                    _executor.Runner.Processor?.Dispose();
                }
                else
                {
                    _executor.Runner = null;
                    _serviceScope?.Dispose();
                    _temporaryServiceProvider?.Dispose();
                }
            }
        }
    }
}
