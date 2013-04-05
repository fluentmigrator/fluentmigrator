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
using FluentMigrator.Runner.Initialization.AssemblyLoader;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner.Initialization
{
    public class TaskExecutor
    {
        protected IMigrationRunner Runner { get; set; }
        private IRunnerContext RunnerContext { get; set; }

        private AssemblyLoaderFactory AssemblyLoaderFactory { get; set; }
        private MigrationProcessorFactoryProvider ProcessorFactoryProvider { get; set; }

        public TaskExecutor(IRunnerContext runnerContext)
            : this(runnerContext, new AssemblyLoaderFactory(), new MigrationProcessorFactoryProvider())
        {
        }

        public TaskExecutor(IRunnerContext runnerContext, AssemblyLoaderFactory assemblyLoaderFactory, MigrationProcessorFactoryProvider processorFactoryProvider)
        {
            if (runnerContext == null) throw new ArgumentNullException("runnerContext");
            if (assemblyLoaderFactory == null) throw new ArgumentNullException("assemblyLoaderFactory");

            RunnerContext = runnerContext;
            AssemblyLoaderFactory = assemblyLoaderFactory;
            ProcessorFactoryProvider = processorFactoryProvider;
        }

        protected virtual void Initialize()
        {
            var assembly = AssemblyLoaderFactory.GetAssemblyLoader(RunnerContext.Target).Load();

            var processor = InitializeProcessor(assembly.Location);

            Runner = new MigrationRunner(assembly, RunnerContext, processor);
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

        private IMigrationProcessor InitializeProcessor(string assemblyLocation)
        {
            var manager = new ConnectionStringManager(new NetConfigManager(), RunnerContext.Announcer, RunnerContext.Connection, RunnerContext.ConnectionStringConfigPath, assemblyLocation, RunnerContext.Database);

            manager.LoadConnectionString();

            if (RunnerContext.Timeout == 0)
            {
                RunnerContext.Timeout = 30; // Set default timeout for command
            }

            var processorFactory = ProcessorFactoryProvider.GetFactory(RunnerContext.Database);
            if (processorFactory == null)
                throw new ProcessorFactoryNotFoundException(string.Format("The provider or dbtype parameter is incorrect. Available choices are {0}: ", ProcessorFactoryProvider.ListAvailableProcessorTypes()));

            var processor = processorFactory.Create(manager.ConnectionString, RunnerContext.Announcer, new ProcessorOptions
            {
                PreviewOnly = RunnerContext.PreviewOnly,
                Timeout = RunnerContext.Timeout,
                ProviderSwitches = RunnerContext.ProviderSwitches
            });

            return processor;
        }
    }
}