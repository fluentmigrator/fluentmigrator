using System;
using System.IO;
using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Console
{
	public class MigratorConsole
	{
		public string ProcessorType;
		public IMigrationProcessor Processor;
		public string Connection;
		public bool Log;
		public string Namespace;
		public string Task;
		public long Version;
		public int Steps;
      private string TargetAssembly;

		public MigratorConsole(string[] args)
		{
			ParseArguments(args);
			CreateProcessor();
			ExecuteMigrations();
		}

		private void ParseArguments(string[] args)
		{
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].Contains("/db"))
					ProcessorType = args[i + 1];

				if (args[i].Contains("/connection"))
					Connection = args[i + 1];

				if (args[i].Contains("/target"))
					TargetAssembly = args[i + 1];

				if (args[i].Contains("/log"))
					Log = true;

				if (args[i].Contains("/namespace"))
					Namespace = args[i + 1];

				if (args[i].Contains("/task"))
					Task = args[i + 1];

				if (args[i].Contains("/version"))
					Version = long.Parse(args[i + 1]);

				if (args[i].Contains("/steps"))
					Steps = int.Parse(args[i + 1]);
			}

			if (string.IsNullOrEmpty(ProcessorType))
				throw new ArgumentException("Database Type is required \"/db [db type]\". Available db types is [sqlserver], [sqlite]");
			if (string.IsNullOrEmpty(Connection))
				throw new ArgumentException("Connection String is required \"/connection\"");
         if (string.IsNullOrEmpty(TargetAssembly))
            throw new ArgumentException("Target Assembly is required \"/target [assembly path]\"");
			if (string.IsNullOrEmpty(Task))
				Task = "migrate";
		}

		private void CreateProcessor()
		{
			IMigrationProcessorFactory processorFactory = ProcessorFactory.GetFactory(ProcessorType);
			Processor = processorFactory.Create(Connection);
		}

		private void ExecuteMigrations()
		{
			if (!Path.IsPathRooted(TargetAssembly))
				TargetAssembly = Path.GetFullPath(TargetAssembly);

			Assembly assembly = Assembly.LoadFile(TargetAssembly);
			var runner = new MigrationVersionRunner(new MigrationConventions(), Processor, new MigrationLoader(new MigrationConventions()), assembly, Namespace);
			new TaskExecutor(runner, Version, Steps).Execute(Task);
		}
	}
}