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
using FluentMigrator.Runner.Initialization.AssemblyLoader;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner.Initialization
{
    public class TaskExecutor
    {
    	private readonly IRunnerContext runnerContext;

        public TaskExecutor(IRunnerContext runnerContext)
        {
            if (runnerContext == null)
                throw new ArgumentNullException("runnerContext", "RunnerContext cannot be null");

            this.runnerContext = runnerContext;
        }

		public void Execute()
		{
			var assembly = AssemblyLoaderFactory.GetAssemblyLoader(runnerContext.Target).Load();

			using (var processor = InitializeProcessor(assembly.Location))
			{
				ExecuteTask(new MigrationRunner(assembly, runnerContext, processor));
				runnerContext.Announcer.Say("Task completed.");
			}
		}

		private void ExecuteTask(IMigrationRunner runner)
    	{
    		switch (runnerContext.Task)
    		{
    			case null:
    			case "":
    			case "migrate":
    			case "migrate:up":
    				if (runnerContext.Version != 0)
    					runner.MigrateUp(runnerContext.Version);
    				else
    					runner.MigrateUp();
    				break;
    			case "rollback":
    				if (runnerContext.Steps == 0)
    					runnerContext.Steps = 1;
    				runner.Rollback(runnerContext.Steps);
    				break;
    			case "rollback:toversion":
    				runner.RollbackToVersion(runnerContext.Version);
    				break;
    			case "rollback:all":
    				runner.RollbackToVersion(0);
    				break;
    			case "migrate:down":
    				runner.MigrateDown(runnerContext.Version);
    				break;
    		}
    	}

    	private IMigrationProcessor InitializeProcessor(string assemblyLocation)
        {
            var manager = new ConnectionStringManager(new NetConfigManager(), runnerContext.Connection, runnerContext.ConnectionStringConfigPath, assemblyLocation, runnerContext.Database);

            manager.LoadConnectionString();

            if (runnerContext.Timeout == 0)
            {
                runnerContext.Timeout = 30; // Set default timeout for command
            }

            var processorFactory = ProcessorFactory.GetFactory(runnerContext.Database);
            var processor = processorFactory.Create(manager.ConnectionString, runnerContext.Announcer, new ProcessorOptions
            {
                PreviewOnly = runnerContext.PreviewOnly,
                Timeout = runnerContext.Timeout
            });

            return processor;
        }
    }
}