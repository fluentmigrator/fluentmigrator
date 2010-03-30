using System;
using System.IO;
using System.Reflection;
using FluentMigrator.Runner.Initialization.AssemblyLoader;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner.Initialization
{
	public class TaskExecutor
	{
		private IMigrationVersionRunner Runner { get; set; }
		private IRunnerContext RunnerContext { get; set; }

		public TaskExecutor(IRunnerContext runnerContext)
		{
			if (runnerContext == null)
				throw new ArgumentNullException("runnerContext", "RunnerContext cannot be null");

			RunnerContext = runnerContext;
		}

		private void Initialize()
		{
			var migrationConventions = new MigrationConventions();
			if (!string.IsNullOrEmpty(RunnerContext.WorkingDirectory))
				migrationConventions.GetWorkingDirectory = () => RunnerContext.WorkingDirectory;

			Assembly assembly = AssemblyLoaderFactory.GetAssemblyLoader(RunnerContext.Target).Load();

			Runner = new MigrationVersionRunner(migrationConventions, RunnerContext.Processor, new MigrationLoader(migrationConventions), assembly, RunnerContext.Namespace);
		}

		public void Execute()
		{
			Initialize();

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
			}
		}
	}
}