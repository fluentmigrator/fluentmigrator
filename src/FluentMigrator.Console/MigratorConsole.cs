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
		public IMigrationProcessor Processor;
		public string Connection;
		public bool Log;
		public bool Verbose;
		public bool PreviewOnly;
		public string Namespace;
		public string Task;
		public long Version;
		public int Steps;
		private string TargetAssembly;
		private string WorkingDirectory;
        private bool ShowHelp = false;


        static void DisplayHelp(OptionSet p)
        {
            System.Console.WriteLine("Usage: FluentMigrator.Console [OPTIONS]");
            System.Console.WriteLine("Options:");
            p.WriteOptionDescriptions(System.Console.Out);
        }

	public MigratorConsole(TextWriter announcerOutput, params string[] args)
        {
			_announcerOutput = announcerOutput;
            try
            {
                var optionSet = new OptionSet()
                                    {
                                        {"db=",string.Format("Database Type is required \"/db=[db type]\". Where [db type] is one of {0}.", ProcessorFactory.ListAvailableProcessorTypes()) ,v => { ProcessorType = v; }},
                                        {"connection=","Connection String is required \"/connection\"=[connection string]", v => { Connection = v; }},
                                        {"target=", "Target Assembly is required \"/target=[assembly path]\" [path]" ,v => { TargetAssembly = v; }},
                                        {"log", v => { Log = v != null; }},
                                        {"namespace=", v => { Namespace = v; }},
                                        {"task=", v => { Task = v; }},
                                        {"version=", v => { Version = long.Parse(v); }},
                                        {"verbose", v => { Verbose = v != null; }},
                                        {"preview", v => { PreviewOnly = v != null; }},
                                        {"steps=", v => { Steps = int.Parse(v); }},
                                        {"workingdirectory=", v => { WorkingDirectory = v; }},
                                        {"help", v => { ShowHelp = v != null; }}
                                    };

                try
                {
                    optionSet.Parse(args);
                }
                catch (OptionException e)
                {
                    System.Console.WriteLine("FluentMigrator.Console: ");
                    System.Console.WriteLine(e.Message);
                    System.Console.WriteLine("Try 'FluentMigrator.Console --help' for more information.");
                    return;
                }

                if (string.IsNullOrEmpty(Task))
                    Task = "migrate";

                if (string.IsNullOrEmpty(ProcessorType) || 
                    string.IsNullOrEmpty(Connection) || 
                    string.IsNullOrEmpty(TargetAssembly))
                {
                    ShowHelp = true;
                }
                
                if (ShowHelp)
                {
                    DisplayHelp(optionSet);
                    return;
                }

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

		public MigratorConsole(params string[] args)
			: this(System.Console.Out, args)
		{
		}

		private void ExecuteMigrations()
		{
			using (var announcer = new TextWriterAnnouncer(_announcerOutput)
									{
										ShowElapsedTime = Verbose,
										ShowSql = Verbose
									})
			{
				var migrationContext = new RunnerContext(announcer)
				{
					Database = ProcessorType,
					Connection = Connection,
					Target = TargetAssembly,
					LoggingEnabled = Log,
					PreviewOnly = PreviewOnly,
					Namespace = Namespace,
					Task = Task,
					Version = Version,
					Steps = Steps,
					WorkingDirectory = WorkingDirectory
				};
				Processor = migrationContext.Processor;

				new TaskExecutor(migrationContext).Execute();
			}
		}
	}
}