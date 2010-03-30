
using System;
using FluentMigrator.Runner.Initialization;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace FluentMigrator.MSBuild
{
	public class Migrate : Task
	{
	   
		[Required]
		public string Database {get;set;}

		[Required]
		public string Connection { get; set; }

		[Required]
		public string Target { get; set; }

		
		public bool LoggingEnabled { get; set; }

		
		public string Namespace { get; set; }

		public string Task { get; set; }

		public long Version { get; set; }

		public int Steps { get; set; }

		
		public string WorkingDirectory { get; set; }



		public override bool Execute()
		{
			Log.LogCommandLine(MessageImportance.Low,"Creating Context");
			var runnerContext = new RunnerContext()
									{
										Database = Database,
										Connection = Connection,
										Target = Target,
										LoggingEnabled = LoggingEnabled,
										Namespace = Namespace,
										Task = Task,
										Version = Version,
										Steps = Steps,
										WorkingDirectory = WorkingDirectory
									};

			Log.LogCommandLine(MessageImportance.Low, "Executing Migration Runner");
			new TaskExecutor(runnerContext).Execute();

			return true;
		}
	}
}

