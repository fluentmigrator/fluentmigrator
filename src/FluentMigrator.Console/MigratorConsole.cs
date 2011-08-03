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
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using Mono.Options;

namespace FluentMigrator.Console
{
	public class MigratorConsole
	{
		private readonly TextWriter _announcerOutput;
		public string ProcessorType;
		public string Connection;
		public string ConnectionStringName;
		public bool Verbose;
		public bool PreviewOnly;
		public string Namespace;
		public string Task;
		public bool Output;
	    public string OutputFilename;
		public long Version;
		public int Steps;
		public string TargetAssembly;
		public string WorkingDirectory;
		public string Profile;
		public int Timeout;
		public bool ShowHelp;
		public string ConnectionStringConfigPath;

		static void DisplayHelp( OptionSet p )
		{
			const string hr = "-------------------------------------------------------------------------------";
			System.Console.WriteLine( hr );
			System.Console.WriteLine( "=============================== FluentMigrator ================================" );
			System.Console.WriteLine( hr );
			System.Console.WriteLine( "Source Code:" );
			System.Console.WriteLine( "  http://github.com/schambers/fluentmigrator/network" );
			System.Console.WriteLine( "Ask For Help:" );
			System.Console.WriteLine( "  http://groups.google.com/group/fluentmigrator-google-group" );
			System.Console.WriteLine( hr );
			System.Console.WriteLine( "Usage:" );
			System.Console.WriteLine( "  migrate [OPTIONS]" );
			System.Console.WriteLine( "Example:" );
			System.Console.WriteLine( "  migrate -a bin\\debug\\MyMigrations.dll -db SqlServer2008 -conn \"SEE_BELOW\" -profile \"Debug\"" );
			System.Console.WriteLine( hr );
			System.Console.WriteLine( "Example Connection Strings:" );
			System.Console.WriteLine( "  MySql: Data Source=172.0.0.1;Database=Foo;User Id=USERNAME;Password=BLAH" );
			System.Console.WriteLine( "  Oracle: Server=172.0.0.1;Database=Foo;Uid=USERNAME;Pwd=BLAH" );
			System.Console.WriteLine( "  SqlLite: Data Source=:memory:;Version=3;New=True" );
			System.Console.WriteLine( "  SqlServer: server=127.0.0.1;database=Foo;user id=USERNAME;password=BLAH" );
			System.Console.WriteLine("             server=.\\SQLExpress;database=Foo;trusted_connection=true");
			System.Console.WriteLine("   ");
			System.Console.WriteLine("OR use a named connection string from the machine.config:");
			System.Console.WriteLine("  migrate -a bin\\debug\\MyMigrations.dll -db SqlServer2008 -connectionName \"namedConnection\" -profile \"Debug\"");
			System.Console.WriteLine(hr);
			System.Console.WriteLine( "Options:" );
			p.WriteOptionDescriptions( System.Console.Out );
		}

		public MigratorConsole( params string[] args )
			: this( System.Console.Out, args )
		{
		}

		public MigratorConsole( TextWriter announcerOutput, params string[] args )
		{
			_announcerOutput = announcerOutput;
			try
			{
				var optionSet = new OptionSet
				{
					{
						"assembly=|a=|target=",
						"REQUIRED. The assembly containing the migrations you want to execute.",
						v => { TargetAssembly = v; }
					},
					{
						"provider=|dbType=|db=",
						string.Format("REQUIRED. The kind of database you are migrating against. Available choices are: {0}.",
						ProcessorFactory.ListAvailableProcessorTypes()),
						v => { ProcessorType = v; }
					},
					{
						"connectionString=|connection=|conn=|c=",
						"REQUIRED. The connection string to the server and database you want to execute your migrations against.",
						v => { Connection = v; }
					},
					{
						"connectionStringName=|connectionName=|C=",
						"The name of the connection string named in machine config. Use --connectionStringConfigPath to specify where the machine.config lives",
						v => { ConnectionStringName = v; }
					},
					{
						"connectionStringConfigPath=|configPath=",
						string.Format("The path of the machine.config where the connection string named by connectionStringName"+
							" is found. If not specified, it defaults to the machine.config used by the currently running CLR version"),
						v => { ConnectionStringConfigPath = v; }
					},
					{
						"namespace=|ns=",
						"The namespace contains the migrations you want to run. Default is all migrations found within the Target Assembly will be run.",
						v => { Namespace = v; }
					},
					{
						"output|out|o",
						"Output generated SQL to a file. Default is no output. Use outputFilename to control the filename, otherwise [assemblyname].sql is the default.",
						v => { Output = true; }
					},
					{
						"outputFilename=|outfile=|of=",
						"The name of the file to output the generated SQL to. The output option must be included for output to be saved to the file.",
						v => { OutputFilename = v; }
					},
					{
						"preview|p",
						"Only output the SQL generated by the migration - do not execute it. Default is false.",
						v => { PreviewOnly = true; }
					},
					{
						"steps=",
						"The number of versions to rollback if the task is 'rollback'. Default is 1.",
						v => { Steps = int.Parse(v); }
					},
					{
						"task=|t=",
						"The task you want FluentMigrator to perform. Available choices are: migrate:up, migrate (same as migrate:up), migrate:down, rollback, rollback:toversion, rollback:all. Default is 'migrate'.",
						v => { Task = v; }
					},
					{
						"version=",
						"The specific version to migrate. Default is 0, which will run all migrations.",
						v => { Version = long.Parse(v); }
					},
					{
						"verbose=",
						"Show the SQL statements generated and execution time in the console. Default is false.",
						v => { Verbose = true; }
					},
					{
						"workingdirectory=|wd=",
						"The directory to load SQL scripts specified by migrations from.",
						v => { WorkingDirectory = v; }
					},
					{
						"profile=",
						"The profile to run after executing migrations.",
						v => { Profile = v; }
					},
					{
						"timeout=",
						"Overrides the default SqlCommand timeout of 30 seconds.",
						v => { Timeout = int.Parse(v); }
					},
					{
						"help|h|?",
						"Displays this help menu.",
						v => { ShowHelp = true; }
					}
				};

				try
				{
					optionSet.Parse( args );
				}
				catch ( OptionException e )
				{
					System.Console.WriteLine( "FluentMigrator.Console:" );
					System.Console.WriteLine( e.Message );
					System.Console.WriteLine( "Try 'migrate --help' for more information." );
					return;
				}

				if ( string.IsNullOrEmpty( Task ) )
					Task = "migrate";

				if ( string.IsNullOrEmpty( ProcessorType ) ||
					(string.IsNullOrEmpty( Connection ) && string.IsNullOrEmpty( ConnectionStringName )) ||
					string.IsNullOrEmpty( TargetAssembly ) )
				{
					DisplayHelp( optionSet );
					Environment.ExitCode = 1;
					return;
				}

				if ( ShowHelp )
				{
					DisplayHelp( optionSet );
					return;
				}

				if ( Output )
				{
				    if (string.IsNullOrEmpty(OutputFilename))
				        OutputFilename = TargetAssembly + ".sql";

				    ExecuteMigrations(OutputFilename);
				}
				else
					ExecuteMigrations();
			}
			catch ( Exception ex )
			{
				System.Console.WriteLine( "!! An error has occurred.  The error is:" );
				System.Console.WriteLine( ex );
				Environment.ExitCode = 1;
			}
		}

		private void ExecuteMigrations()
		{
			var consoleAnnouncer = new TextWriterAnnouncer( _announcerOutput )
			{
				ShowElapsedTime = Verbose,
				ShowSql = Verbose
			};
			ExecuteMigrations( consoleAnnouncer );
		}

		private void ExecuteMigrations( string outputTo )
		{
			using ( var sw = new StreamWriter( outputTo ) )
			{
				var fileAnnouncer = new TextWriterAnnouncer( sw )
									{
										ShowElapsedTime = false,
										ShowSql = true
									};
				var consoleAnnouncer = new TextWriterAnnouncer( _announcerOutput )
										{
											ShowElapsedTime = Verbose,
											ShowSql = Verbose
										};
				var announcer = new CompositeAnnouncer( new[]
								{
									consoleAnnouncer,
									fileAnnouncer
								});

				ExecuteMigrations( announcer );
			}
		}

		private void ExecuteMigrations( IAnnouncer announcer )
		{
			var runnerContext = new RunnerContext( announcer )
			{
				Database = ProcessorType,
				Connection = Connection,
				Target = TargetAssembly,
				PreviewOnly = PreviewOnly,
				Namespace = Namespace,
				Task = Task,
				Version = Version,
				Steps = Steps,
				WorkingDirectory = WorkingDirectory,
				Profile = Profile,
				Timeout = Timeout,
				ConnectionStringConfigPath = ConnectionStringConfigPath,
			};
			runnerContext.UseConnectionName(ConnectionStringName);

			new TaskExecutor( runnerContext ).Execute();
		}
	}
}