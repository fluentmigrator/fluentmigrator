using System;
using System.IO;
using System.Reflection;
using FluentMigrator.Console;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace FluentMigrator.NAnt
{
	[TaskName("migrate")]
	public class MigrateTask : Task
	{
		[TaskAttribute("database")]
		public string Database { get; set; }

		[TaskAttribute("connection")]
		public string Connection { get; set; }

		[TaskAttribute("target")]
		public string Target { get; set; }

		[TaskAttribute("log")]
		public bool Logging { get; set; }

		[TaskAttribute("namespace")]
		public string Namespace { get; set; }

		[TaskAttribute("task")]
		public string Task { get; set; }

		[TaskAttribute("to")]
		public long To { get; set; }

		[TaskAttribute("steps")]
		public int Steps { get; set; }

		[TaskAttribute("workingdirectory")]
		public string WorkingDirectory { get; set; }

		protected IMigrationProcessor Processor { get; set; }

		private void CreateProcessor()
		{
			IMigrationProcessorFactory processorFactory = ProcessorFactory.GetFactory(Database);
			Processor = processorFactory.Create(Connection);
		}

		protected override void ExecuteTask()
		{
			CreateProcessor();

			if (!Path.IsPathRooted(Target))
				Target = Path.GetFullPath(Target);

			var migrationConventions = new MigrationConventions();
			if (!string.IsNullOrEmpty(WorkingDirectory))
				migrationConventions.GetWorkingDirectory = () => WorkingDirectory;

//			Assembly assembly = Assembly.LoadFile(Target);
//			var runner = new MigrationVersionRunner(migrationConventions, Processor, new MigrationLoader(migrationConventions), assembly, Namespace);
//			new TaskExecutor(runner, To, Steps).Execute(Task);
			throw new NotImplementedException();
		}
	}
}
