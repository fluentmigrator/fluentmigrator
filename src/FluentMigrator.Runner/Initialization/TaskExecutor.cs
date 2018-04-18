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

using FluentMigrator.Exceptions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization.AssemblyLoader;
using FluentMigrator.Runner.Processors;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner.Initialization
{
    public class TaskExecutor
    {
        private readonly IRunnerContext _runnerContext;
        private readonly AssemblyLoaderFactory _assemblyLoaderFactory;
        private readonly IReadOnlyCollection<IMigrationProcessorFactory> _processorFactories;
        private readonly IReadOnlyCollection<IMigrationGenerator> _migrationGenerators;

        [Obsolete]
        public TaskExecutor(IRunnerContext runnerContext)
            : this(
                runnerContext,
                new DefaultConnectionStringProvider(),
                new AssemblyLoaderFactory(),
                new MigrationProcessorFactoryProvider())
        {
        }

        [Obsolete]
        public TaskExecutor(
            IRunnerContext runnerContext,
            IConnectionStringProvider connectionStringProvider,
            AssemblyLoaderFactory assemblyLoaderFactory,
            MigrationProcessorFactoryProvider _)
            : this(
                runnerContext,
                connectionStringProvider,
                assemblyLoaderFactory,
                MigrationProcessorFactoryProvider.RegisteredFactories,
                MigrationGeneratorFactory.RegisteredGenerators)
        {
        }

        public TaskExecutor(
            IRunnerContext runnerContext,
            IConnectionStringProvider connectionStringProvider,
            AssemblyLoaderFactory assemblyLoaderFactory,
            IEnumerable<IMigrationProcessorFactory> processorFactories,
            IEnumerable<IMigrationGenerator> migrationGenerators)
        {
            _runnerContext = runnerContext ?? throw new ArgumentNullException(nameof(runnerContext));
            ConnectionStringProvider = connectionStringProvider;
            _assemblyLoaderFactory = assemblyLoaderFactory ?? throw new ArgumentNullException(nameof(assemblyLoaderFactory));
            _processorFactories = processorFactories.ToList();
            _migrationGenerators = migrationGenerators.ToList();
        }

        protected IMigrationRunner Runner { get; set; }

        protected IConnectionStringProvider ConnectionStringProvider { get; }

        [Obsolete]
        protected virtual IEnumerable<Assembly> GetTargetAssemblies()
        {
            var assemblies = new HashSet<Assembly>();

            foreach (var target in _runnerContext.Targets)
            {
                var assembly = _assemblyLoaderFactory.GetAssemblyLoader(target).Load();

                if (assemblies.Add(assembly))
                {
                    yield return assembly;
                }
            }
        }

        protected virtual IServiceProvider ConfigureServices(IServiceCollection services)
        {
#pragma warning disable CS0612 // Typ oder Element ist veraltet
            var assemblies = GetTargetAssemblies().ToList();
#pragma warning restore CS0612 // Typ oder Element ist veraltet

            var connectionString = LoadConnectionString(assemblies);
            var processorFactoryType = FindProcessorFactory().GetType();

            services
                .AddFluentMigrator(
                    processorFactoryType,
                    connectionString,
                    builder =>
                    {
                        builder
                            .WithProcessorOptions(CreateProcessorOptions())
                            .WithRunnerContext(_runnerContext)
                            .WithAnnouncer(_runnerContext.Announcer);
                    });

            return services.BuildServiceProvider();
        }

        protected virtual void Initialize()
        {
#pragma warning disable CS0612 // Typ oder Element ist veraltet
            var assemblies = GetTargetAssemblies().ToList();
            var assemblyCollection = new AssemblyCollection(assemblies);
#pragma warning restore CS0612 // Typ oder Element ist veraltet

            if (!_runnerContext.NoConnection && ConnectionStringProvider == null)
            {
                _runnerContext.NoConnection = true;
            }

            var processor = _runnerContext.NoConnection
                ? InitializeConnectionlessProcessor()
                : InitializeProcessor(assemblies);

            var convSet = new DefaultConventionSet(_runnerContext);
#pragma warning disable CS0612 // Typ oder Element ist veraltet
            var migConv = assemblyCollection.GetMigrationRunnerConventions();
            var versionTableMetaData = assemblyCollection.GetVersionTableMetaData(convSet, migConv, _runnerContext);
            var maintenanceLoader = new MaintenanceLoader(assemblyCollection, _runnerContext.Tags, migConv);
            var migrationLoader = new DefaultMigrationInformationLoader(
                migConv,
                assemblyCollection,
                _runnerContext.Namespace,
                _runnerContext.NestedNamespaces,
                _runnerContext.Tags);
#pragma warning restore CS0612 // Typ oder Element ist veraltet
            var migrations = migrationLoader.LoadMigrations().Values.Select(x => x.Migration).ToList();
            Runner = new MigrationRunner(_runnerContext, processor, versionTableMetaData, migConv, maintenanceLoader, migrationLoader, migrations);
        }

        public void Execute()
        {
            Initialize();

            try
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
            finally { Runner.Processor.Dispose(); }
            _runnerContext.Announcer.Say("Task completed.");
        }

        /// <summary>
        /// Checks whether the current task will actually run any migrations.
        /// Can be used to decide whether it's necessary perform a backup before the migrations are executed.
        /// </summary>
        public bool HasMigrationsToApply()
        {
            Initialize();

            try
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
            finally
            {
                Runner.Processor.Dispose();
            }
        }

        private IMigrationProcessor InitializeConnectionlessProcessor()
        {
            var options = new ProcessorOptions
            {
                PreviewOnly = _runnerContext.PreviewOnly,
                Timeout = _runnerContext.Timeout,
                ProviderSwitches = _runnerContext.ProviderSwitches
            };

            var generatorFactory = new MigrationGeneratorFactory();
            var generator = generatorFactory.GetGenerator(_runnerContext.Database);
            if (generator == null)
                throw new ProcessorFactoryNotFoundException(string.Format("The provider or dbtype parameter is incorrect. Available choices are {0}: ", generatorFactory.ListAvailableGeneratorTypes()));

            var processor = new ConnectionlessProcessor(generator, _runnerContext, options);

            return processor;
        }

        private IMigrationProcessorFactory FindProcessorFactory()
        {
            var processorFactory = _processorFactories
                .FirstOrDefault(x => string.Equals(x.Name, _runnerContext.Database, StringComparison.OrdinalIgnoreCase));

            if (processorFactory == null)
            {
                var choices = string.Join(", ", _processorFactories.Select(x => x.Name));
                throw new ProcessorFactoryNotFoundException(
                    $"The provider or dbtype parameter is incorrect. Available choices are {choices}: ");
            }

            return processorFactory;
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
    }
}
