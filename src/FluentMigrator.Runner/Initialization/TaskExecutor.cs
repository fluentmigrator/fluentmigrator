#region License
//
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using FluentMigrator.Exceptions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization.AssemblyLoader;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Infrastructure;
using System.Reflection;
using System.Collections.Generic;

namespace FluentMigrator.Runner.Initialization
{
    public class TaskExecutor
    {
        protected IMigrationRunner Runner { get; set; }
        private IRunnerContext RunnerContext { get; set; }

        private AssemblyLoaderFactory AssemblyLoaderFactory { get; set; }
        private MigrationProcessorFactoryProvider ProcessorFactoryProvider { get; set; }

        public TaskExecutor(IRunnerContext runnerContext)
            : this(runnerContext, new DefaultConnectionStringProvider(), new AssemblyLoaderFactory(), new MigrationProcessorFactoryProvider())
        {
        }

        public TaskExecutor(IRunnerContext runnerContext, IConnectionStringProvider connectionStringProvider, AssemblyLoaderFactory assemblyLoaderFactory, MigrationProcessorFactoryProvider processorFactoryProvider)
        {
            if (runnerContext == null) throw new ArgumentNullException("runnerContext");
            if (assemblyLoaderFactory == null) throw new ArgumentNullException("assemblyLoaderFactory");

            RunnerContext = runnerContext;
            ConnectionStringProvider = connectionStringProvider;
            AssemblyLoaderFactory = assemblyLoaderFactory;
            ProcessorFactoryProvider = processorFactoryProvider;
        }

        protected IConnectionStringProvider ConnectionStringProvider { get; }

        protected virtual IEnumerable<Assembly> GetTargetAssemblies()
        {
            var assemblies = new HashSet<Assembly>();

            foreach (var target in RunnerContext.Targets)
            {
                var assembly = AssemblyLoaderFactory.GetAssemblyLoader(target).Load();

                if (assemblies.Add(assembly))
                {
                    yield return assembly;
                }
            }
        }

        protected virtual void Initialize()
        {
            var assemblies = GetTargetAssemblies();

            var assemblyCollection = new AssemblyCollection(assemblies);

            if (!RunnerContext.NoConnection && ConnectionStringProvider == null)
            {
                RunnerContext.NoConnection = true;
            }

            var processor = RunnerContext.NoConnection? InitializeConnectionlessProcessor():InitializeProcessor(assemblyCollection);

            Runner = new MigrationRunner(assemblyCollection, RunnerContext, processor);
        }

        public void Execute()
        {
            Initialize();

            try
            {
                switch (RunnerContext.Task)
                {
                    case null:
                    case "":
                    case "migrate":
                    case "migrate:up":
                        if (RunnerContext.Version != 0)
                            Runner.MigrateUp(RunnerContext.Version);
                        else
                            Runner.MigrateUp();
                        break;
                    case "rollback":
                        if (RunnerContext.Steps == 0)
                            RunnerContext.Steps = 1;
                        Runner.Rollback(RunnerContext.Steps);
                        break;
                    case "rollback:toversion":
                        Runner.RollbackToVersion(RunnerContext.Version);
                        break;
                    case "rollback:all":
                        Runner.RollbackToVersion(0);
                        break;
                    case "migrate:down":
                        Runner.MigrateDown(RunnerContext.Version);
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
            RunnerContext.Announcer.Say("Task completed.");
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
                switch (RunnerContext.Task)
                {
                    case null:
                    case "":
                    case "migrate":
                    case "migrate:up":
                        if (RunnerContext.Version != 0)
                            return Runner.HasMigrationsToApplyUp(RunnerContext.Version);

                        return Runner.HasMigrationsToApplyUp();
                    case "rollback":
                    case "rollback:all":
                        // Number of steps doesn't matter as long as there's at least
                        // one migration applied (at least that one will be rolled back)
                        return Runner.HasMigrationsToApplyRollback();
                    case "rollback:toversion":
                    case "migrate:down":
                        return Runner.HasMigrationsToApplyDown(RunnerContext.Version);
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
                PreviewOnly = RunnerContext.PreviewOnly,
                Timeout = RunnerContext.Timeout,
                ProviderSwitches = RunnerContext.ProviderSwitches
            };

            var generator = new MigrationGeneratorFactory().GetGenerator(RunnerContext.Database);

            var processor = new ConnectionlessProcessor(generator, RunnerContext, options);

            return processor;
        }

        private IMigrationProcessor InitializeProcessor(IAssemblyCollection assemblyCollection)
        {

            if (RunnerContext.Timeout == 0)
            {
                RunnerContext.Timeout = 30; // Set default timeout for command
            }

            var connectionString = LoadConnectionString(assemblyCollection);
            var processorFactory = ProcessorFactoryProvider.GetFactory(RunnerContext.Database);

            if (processorFactory == null)
                throw new ProcessorFactoryNotFoundException(string.Format("The provider or dbtype parameter is incorrect. Available choices are {0}: ", ProcessorFactoryProvider.ListAvailableProcessorTypes()));

            var processor = processorFactory.Create(connectionString, RunnerContext.Announcer, new ProcessorOptions
            {
                PreviewOnly = RunnerContext.PreviewOnly,
                Timeout = RunnerContext.Timeout,
                ProviderSwitches = RunnerContext.ProviderSwitches
            });

            return processor;
        }

        private string LoadConnectionString(IAssemblyCollection assemblyCollection)
        {
            var singleAssembly = (assemblyCollection != null && assemblyCollection.Assemblies != null && assemblyCollection.Assemblies.Length == 1) ? assemblyCollection.Assemblies[0] : null;
            var singleAssemblyLocation = singleAssembly != null ? singleAssembly.Location : string.Empty;

            var connectionString = ConnectionStringProvider.GetConnectionString(
                RunnerContext.Announcer,
                RunnerContext.Connection,
                RunnerContext.ConnectionStringConfigPath,
                singleAssemblyLocation,
                RunnerContext.Database);

            return connectionString;
        }
    }
}
