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
				CreateProcessor();
				ExecuteMigrations();
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("!! An error has occurred.  The error is:");
				System.Console.WriteLine(ex);
				//set Exit code to failure
				System.Environment.ExitCode = 1;
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