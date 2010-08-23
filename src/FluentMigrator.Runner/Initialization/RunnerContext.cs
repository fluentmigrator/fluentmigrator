using System;
using System.Configuration;
using System.IO;
using System.Linq;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner.Initialization
{
	public class RunnerContext : IRunnerContext
	{
		public RunnerContext(IAnnouncer announcer)
		{
			Announcer = announcer;
		}

		public string Database { get; set; }
		public string Connection { get; set; }
		public string Target { get; set; }
		public bool PreviewOnly { get; set; }
		public string Namespace { get; set; }
		public string Task { get; set; }
		public long Version { get; set; }
		public int Steps { get; set; }
		public string WorkingDirectory { get; set; }
		public string Profile { get; set; }
		public int Timeout { get; set; }
		public IAnnouncer Announcer { get; set; }
		public IStopWatch StopWatch { get; set; }
	}
}