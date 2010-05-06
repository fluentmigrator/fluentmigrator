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
		public bool LoggingEnabled { get; set; }
		public string Namespace { get; set; }
		public string Task { get; set; }
		public long Version { get; set; }
		public int Steps { get; set; }
		public string WorkingDirectory { get; set; }
		public IAnnouncer Announcer { get; private set; }

		private IMigrationProcessor _processor;

		public IMigrationProcessor Processor
		{
			get
			{
				if (_processor != null)
					return _processor;

				IMigrationProcessorFactory processorFactory = ProcessorFactory.GetFactory(Database);
				_processor = processorFactory.Create(Connection, Announcer);

				return _processor;
			}
		}
	}
}