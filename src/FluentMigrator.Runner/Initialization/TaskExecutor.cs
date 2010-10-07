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
using System.Configuration;
using System.IO;
using System.Linq;
using FluentMigrator.Runner.Initialization.AssemblyLoader;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner.Initialization
{
	public class TaskExecutor
	{
		private IMigrationRunner Runner { get; set; }
		private IRunnerContext RunnerContext { get; set; }
		private string ConfigFile;
		private string ConnectionString;

		private bool NotUsingConfig
		{
			get { return string.IsNullOrEmpty( ConfigFile ); }
		}

		public TaskExecutor(IRunnerContext runnerContext)
		{
			if (runnerContext == null)
				throw new ArgumentNullException("runnerContext", "RunnerContext cannot be null");

			RunnerContext = runnerContext;
		}

		private void Initialize()
		{
			var assembly = AssemblyLoaderFactory.GetAssemblyLoader(RunnerContext.Target).Load();

			var processor = InitializeProcessor();

			Runner = new MigrationRunner(assembly, RunnerContext, processor);
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

		public IMigrationProcessor InitializeProcessor()
		{
			var configFile = Path.Combine( Environment.CurrentDirectory, RunnerContext.Target );
			if ( File.Exists( configFile + ".config" ) )
			{
				var config = ConfigurationManager.OpenExeConfiguration( configFile );
				var connections = config.ConnectionStrings.ConnectionStrings;

				if ( connections.Count > 1 )
				{
					if ( string.IsNullOrEmpty( RunnerContext.Connection ) )
					{
						ReadConnectionString( connections[ Environment.MachineName ], config.FilePath );
					}
					else
					{
						ReadConnectionString( connections[ RunnerContext.Connection ], config.FilePath );
					}
				}
				else if ( connections.Count == 1 )
				{
					ReadConnectionString( connections[ 0 ], config.FilePath );
				}
			}

			if ( NotUsingConfig && !string.IsNullOrEmpty( RunnerContext.Connection ) )
			{
				ConnectionString = RunnerContext.Connection;
			}

			if ( string.IsNullOrEmpty( ConnectionString ) )
			{
				throw new ArgumentException( "Connection String or Name is required \"/connection\"" );
			}

			if ( string.IsNullOrEmpty( RunnerContext.Database ) )
			{
				throw new ArgumentException(
					"Database Type is required \"/db [db type]\". Available db types is [sqlserver], [sqlite]" );
			}

			if ( NotUsingConfig )
			{
				Console.WriteLine( "Using Database {0} and Connection String {1}", RunnerContext.Database, ConnectionString );
			}
			else
			{
				Console.WriteLine( "Using Connection {0} from Configuration file {1}", RunnerContext.Connection, ConfigFile );
			}

			if ( RunnerContext.Timeout == 0 )
			{
				RunnerContext.Timeout = 30; // Set default timeout for command
			}

			var processorFactory = ProcessorFactory.GetFactory( RunnerContext.Database );
			var processor = processorFactory.Create( ConnectionString, RunnerContext.Announcer, new ProcessorOptions
			{
				PreviewOnly = RunnerContext.PreviewOnly,
				Timeout = RunnerContext.Timeout
			} );

			return processor;
		}

		private void ReadConnectionString( ConnectionStringSettings connection, string configurationFile )
		{
			if ( connection != null )
			{
				var factory = ProcessorFactory.Factories.Where( f => f.IsForProvider( connection.ProviderName ) ).FirstOrDefault();
				if ( factory != null )
				{
					RunnerContext.Database = factory.Name;
					RunnerContext.Connection = connection.Name;
					ConnectionString = connection.ConnectionString;
					ConfigFile = configurationFile;
				}
			}
			else
			{
				Console.WriteLine( "connection is null!" );
			}
		}
	}
}