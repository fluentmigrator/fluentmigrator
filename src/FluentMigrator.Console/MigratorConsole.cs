#region License

// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;

using FluentMigrator.Exceptions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;

using Mono.Options;

namespace FluentMigrator.Console
{
    public class MigratorConsole
    {
        private readonly ConsoleAnnouncer _consoleAnnouncer = new ConsoleAnnouncer();
        public string ApplicationContext;
        public string Connection;
        public string ConnectionStringConfigPath;
        public string Namespace;
        public bool NestedNamespaces;
        public bool Output;
        public string OutputFilename;
        public bool PreviewOnly;
        public string ProcessorType;
        public string Profile;
        public bool ShowHelp;
        public int Steps;
        public List<string> Tags = new List<string>();
        public string TargetAssembly;
        public string Task;
        public int? Timeout;
        public bool Verbose;
        public bool StopOnError;
        public long Version;
        public long StartVersion;
        public bool NoConnection;
        public string WorkingDirectory;
        public bool TransactionPerSession;
        public bool AllowBreakingChange;
        public string ProviderSwitches;

        public RunnerContext RunnerContext { get; private set;}

        public int Run(params string[] args)
        {
            _consoleAnnouncer.Header();

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
                                                          string.Join(", ", MigrationProcessorFactoryProvider.ProcessorTypes)),
                                            v => { ProcessorType = v; }
                                            },
                                        {
                                            "connectionString=|connection=|conn=|c=",
                                            "The name of the connection string (falls back to machine name) or the connection string itself to the server and database you want to execute your migrations against."
                                            ,
                                            v => { Connection = v; }
                                            },
                                        {
                                            "connectionStringConfigPath=|configPath=",
                                            string.Format("The path of the machine.config where the connection string named by connectionString" +
                                                          " is found. If not specified, it defaults to the machine.config used by the currently running CLR version")
                                            ,
                                            v => { ConnectionStringConfigPath = v; }
                                            },
                                        {
                                            "namespace=|ns=",
                                            "The namespace contains the migrations you want to run. Default is all migrations found within the Target Assembly will be run."
                                            ,
                                            v => { Namespace = v; }
                                            },
                                        {
                                            "nested",
                                            "Whether migrations in nested namespaces should be included. Used in conjunction with the namespace option."
                                            ,
                                            v => { NestedNamespaces = v != null; }
                                            },
                                        {
                                            "output|out|o",
                                            "Output generated SQL to a file. Default is no output. Use outputFilename to control the filename, otherwise [assemblyname].sql is the default."
                                            ,
                                            v => { Output = v != null; }
                                            },
                                        {
                                            "outputFilename=|outfile=|of=",
                                            "The name of the file to output the generated SQL to. The output option must be included for output to be saved to the file."
                                            ,
                                            v => { OutputFilename = v; }
                                            },
                                        {
                                            "preview|p",
                                            "Only output the SQL generated by the migration - do not execute it. Default is false.",
                                            v => { PreviewOnly = v != null; }
                                            },
                                        {
                                            "steps=",
                                            "The number of versions to rollback if the task is 'rollback'. Default is 1.",
                                            v => { Steps = int.Parse(v); }
                                            },
                                        {
                                            "task=|t=",
                                            "The task you want FluentMigrator to perform. Available choices are: migrate:up, migrate (same as migrate:up), migrate:down, rollback, rollback:toversion, rollback:all, validateversionorder, listmigrations. Default is 'migrate'."
                                            ,
                                            v => { Task = v; }
                                            },
                                        {
                                            "version=",
                                            "The specific version to migrate. Default is 0, which will run all migrations.",
                                            v => { Version = long.Parse(v); }
                                            },
                                         {
                                            "startVersion=",
                                            "The specific version to start migrating from. Only used when NoConnection is true. Default is 0",
                                            v => { StartVersion = long.Parse(v); }
                                            },
                                        {
                                            "noConnection",
                                            "Indicates that migrations will be generated without consulting a target database. Should only be used when generating an output file.",
                                            v => { NoConnection = v != null; }
                                            },
                                        {
                                            "verbose=",
                                            "Show the SQL statements generated and execution time in the console. Default is false.",
                                            v => { Verbose = v != null; }
                                            },
                                        {
                                            "stopOnError=",
                                            "Pauses migration execution until the user input if any error occured. Default is false.",
                                            v => { StopOnError = v != null; }
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
                                            "context=",
                                            "Set ApplicationContext to the given string.",
                                            v => { ApplicationContext = v; }
                                            },
                                        {
                                            "timeout=",
                                            "Overrides the default SqlCommand timeout of 30 seconds.",
                                            v => { Timeout = int.Parse(v); }
                                            },
                                        {
                                            "tag=",
                                            "Filters the migrations to be run by tag.",
                                            v => { Tags.Add(v); }
                                            },
                                        {
                                            "providerswitches=",
                                            "Provider specific switches",
                                            v => { ProviderSwitches = v; }
                                            },
                                        {
                                            "help|h|?",
                                            "Displays this help menu.",
                                            v => { ShowHelp = true; }
                                            },
                                        {
                                            "transaction-per-session|tps",
                                            "Overrides the transaction behavior of migrations, so that all migrations to be executed will run in one transaction.",
                                            v => { TransactionPerSession = v != null; }
                                            },
                                        {
                                            "allow-breaking-changes|abc",
                                            "Allows execution of migrations marked as breaking changes.",
                                            v => { AllowBreakingChange = v != null; }
                                            },
                                    };

                try
                {
                    optionSet.Parse(args);
                }
                catch (OptionException e)
                {
                    _consoleAnnouncer.Error(e);
                    _consoleAnnouncer.Say("Try 'migrate --help' for more information.");
                    return 2;
                }

                if (string.IsNullOrEmpty(Task))
                    Task = "migrate";

                if (!ValidateArguments(optionSet))
                {
                    return 1;
                }

                if (ShowHelp)
                {
                    DisplayHelp(optionSet);
                    return 0;
                }

                if (Output)
                {
                    return ExecuteMigrations(OutputFilename);
                }

                return ExecuteMigrations();
            }
            catch (MissingMigrationsException ex)
            {
                _consoleAnnouncer.Error(ex);
                return 6;
            }
            catch (RunnerException ex)
            {
                _consoleAnnouncer.Error(ex);
                return 5;
            }
            catch (FluentMigratorException ex)
            {
                _consoleAnnouncer.Error(ex);
                return 4;
            }
            catch (Exception ex)
            {
                _consoleAnnouncer.Error(ex);
                return 3;
            }
        }

        private bool ValidateArguments(OptionSet optionSet)
        {
            if (string.IsNullOrEmpty(TargetAssembly))
            {
                DisplayHelp(optionSet, "Please enter the path of the assembly containing migrations you want to execute.");
                return false;
            }
            if (string.IsNullOrEmpty(ProcessorType))
            {
                DisplayHelp(optionSet, "Please enter the kind of database you are migrating against.");
                return false;
            }
            return true;
        }

        private void DisplayHelp(OptionSet optionSet, string validationErrorMessage)
        {
            _consoleAnnouncer.Emphasize(validationErrorMessage);
            DisplayHelp(optionSet);
        }

        private void DisplayHelp(OptionSet p)
        {
            _consoleAnnouncer.Write("Usage:");
            _consoleAnnouncer.Write("  migrate [OPTIONS]");
            _consoleAnnouncer.Write("Example:");
            _consoleAnnouncer.Write("  migrate -a bin\\debug\\MyMigrations.dll -db SqlServer2008 -conn \"SEE_BELOW\" -profile \"Debug\"");
            _consoleAnnouncer.HorizontalRule();
            _consoleAnnouncer.Write("Example Connection Strings:");
            _consoleAnnouncer.Write("  MySql: Data Source=172.0.0.1;Database=Foo;User Id=USERNAME;Password=BLAH");
            _consoleAnnouncer.Write("  Oracle: Server=172.0.0.1;Database=Foo;Uid=USERNAME;Pwd=BLAH");
            _consoleAnnouncer.Write("  SqlLite: Data Source=:memory:");
            _consoleAnnouncer.Write("  SqlServer: server=127.0.0.1;database=Foo;user id=USERNAME;password=BLAH");
            _consoleAnnouncer.Write("             server=.\\SQLExpress;database=Foo;trusted_connection=true");
            _consoleAnnouncer.Write("   ");
            _consoleAnnouncer.Write("OR use a named connection string from the machine.config:");
            _consoleAnnouncer.Write("  migrate -a bin\\debug\\MyMigrations.dll -db SqlServer2008 -conn \"namedConnection\" -profile \"Debug\"");
            _consoleAnnouncer.HorizontalRule();
            _consoleAnnouncer.Write("Options:");
            p.WriteOptionDescriptions(System.Console.Out);
        }

        private int ExecuteMigrations()
        {
            _consoleAnnouncer.ShowElapsedTime = Verbose;
            _consoleAnnouncer.ShowSql = Verbose;

            var announcer = StopOnError
                ? (IAnnouncer)new CompositeAnnouncer(_consoleAnnouncer, new StopOnErrorAnnouncer())
                : _consoleAnnouncer;

            return ExecuteMigrations(announcer);
        }

        private int ExecuteMigrations(string outputTo)
        {
            _consoleAnnouncer.ShowElapsedTime = Verbose;
            _consoleAnnouncer.ShowSql = Verbose;

            var innerAnnouncer = StopOnError
                ? (IAnnouncer)new CompositeAnnouncer(_consoleAnnouncer, new StopOnErrorAnnouncer())
                : _consoleAnnouncer;

            using (var announcer = new LateInitAnnouncer(innerAnnouncer, ExecutingAgainstMsSql, outputTo))
            {
                return ExecuteMigrations(announcer);
            }
        }

        private bool ExecutingAgainstMsSql
        {
            get
            {
                return ProcessorType.StartsWith("SqlServer", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        private int ExecuteMigrations(IAnnouncer announcer)
        {
            RunnerContext = new RunnerContext(announcer)
            {
                Database = ProcessorType,
                Connection = Connection,
                PreviewOnly = PreviewOnly,
                Targets = new[] {TargetAssembly},
                Namespace = Namespace,
                NestedNamespaces = NestedNamespaces,
                Task = Task,
                Version = Version,
                StartVersion = StartVersion,
                NoConnection = NoConnection,
                Steps = Steps,
                WorkingDirectory = WorkingDirectory,
                Profile = Profile,
                Timeout = Timeout,
                ConnectionStringConfigPath = ConnectionStringConfigPath,
                ApplicationContext = ApplicationContext,
                Tags = Tags,
                TransactionPerSession = TransactionPerSession,
                AllowBreakingChange = AllowBreakingChange,
                ProviderSwitches = ProviderSwitches,
            };

            new LateInitTaskExecutor(RunnerContext).Execute();
            return 0;
        }
    }
}
