using FluentMigrator.Runner.Initialization;
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
		public bool LoggingEnabled { get; set; }

		[TaskAttribute("namespace")]
		public string Namespace { get; set; }

		[TaskAttribute("task")]
		public string Task { get; set; }

		[TaskAttribute("to")]
		public long Version { get; set; }

		[TaskAttribute("steps")]
		public int Steps { get; set; }

		[TaskAttribute("workingdirectory")]
		public string WorkingDirectory { get; set; }

		protected override void ExecuteTask()
		{
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

			new TaskExecutor(runnerContext).Execute();
		}
	}
}
