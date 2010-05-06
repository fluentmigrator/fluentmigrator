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

using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace FluentMigrator.MSBuild
{
	public class Migrate : Task
	{

		[Required]
		public string Database { get; set; }

		[Required]
		public string Connection { get; set; }

		[Required]
		public string Target { get; set; }

		public bool LoggingEnabled { get; set; }

		public bool Verbose { get; set; }

		public string Namespace { get; set; }

		public string Task { get; set; }

		public long Version { get; set; }

		public int Steps { get; set; }

		public string WorkingDirectory { get; set; }

		public override bool Execute()
		{
			Log.LogCommandLine(MessageImportance.Low, "Creating Context");
			var announcer = new BaseAnnouncer(msg => Log.LogCommandLine(MessageImportance.Normal, msg))
								{
									ShowElapsedTime = Verbose,
									ShowSql = Verbose
								};
			var runnerContext = new RunnerContext(announcer)
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
