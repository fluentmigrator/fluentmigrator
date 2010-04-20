using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
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
		private string WorkingDirectory;

		public MigratorConsole(string[] args)
		{
		    try {
		        ParseArguments(args);
		        ExecuteMigrations();
		    }
		    catch (Exception ex) {
                System.Console.WriteLine("!! An error has occurred.  The error is:");
		        System.Console.WriteLine(ex);
                Environment.ExitCode = 1;
		    }
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

				if (args[i].Contains("/workingdirectory"))
					WorkingDirectory = args[i + 1];
			}
            if (string.IsNullOrEmpty(TargetAssembly))
                throw new ArgumentException("Target Assembly is required \"/target [assembly path]\"");
			
			if (string.IsNullOrEmpty(Task))
				Task = "migrate";
		}

		private void ExecuteMigrations()
		{
			var migrationContext = new RunnerContext
			{
				Database = ProcessorType,
				Connection = Connection,
				Target = TargetAssembly,
				LoggingEnabled = Log,
				Namespace = Namespace,
				Task = Task,
				Version = Version,
				Steps = Steps,
				WorkingDirectory = WorkingDirectory
			};

			new TaskExecutor(migrationContext).Execute();
		}
	}
}