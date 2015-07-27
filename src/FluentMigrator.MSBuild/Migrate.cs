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
using FluentMigrator.Exceptions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Initialization;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace FluentMigrator.MSBuild
{
    public class Migrate : AppDomainIsolatedTask
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="T:Migrate"/> class.
        /// </summary>
        public Migrate()
        {
            AppDomain.CurrentDomain.ResourceResolve += new ResolveEventHandler(CurrentDomain_ResourceResolve);
        }

        private static Assembly CurrentDomain_ResourceResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine("Could Not Resolve {0}", args.Name);
            return null;
        }

        private string databaseType;

        public string ApplicationContext { get; set; }
        
        [Required]
        public string Connection { get; set; }

        public string ConnectionStringConfigPath { get; set; }

        public string Target { get { return (Targets != null && Targets.Length == 1) ? Targets[0] : string.Empty; } set { Targets = new string[] { value }; } }

        public string[] Targets { get; set; }
        public string MigrationAssembly { get { return (Targets != null && Targets.Length == 1) ? Targets[0] : string.Empty; } set { Targets = new string[] {value}; } }

        public string Database { get { return databaseType; } set { databaseType = value; } }

        public string DatabaseType { get { return databaseType; } set { databaseType = value; } }

        public bool Verbose { get; set; }

        public string Namespace { get; set; }

        public bool Nested { get; set; }

        public string Task { get; set; }

        public long Version { get; set; }

        public int Steps { get; set; }

        public string WorkingDirectory { get; set; }

        public int Timeout { get; set; }

        public string Profile { get; set; }

        public bool PreviewOnly { get; set; }

        public string Tags { get; set; }

        public bool Output { get; set; }

        public string OutputFilename { get; set; }

        public bool TransactionPerSession { get; set; }

        public override bool Execute()
        {

            if (string.IsNullOrEmpty(databaseType))
            {
                Log.LogError("You must specify a database type. i.e. mysql or sqlserver");
                return false;
            }

            if (Targets == null || Targets.Length == 0)
            {
                Log.LogError("You must specify a migration assemblies ");
                return false;
            }

            IAnnouncer announcer = new ConsoleAnnouncer
            {
                ShowElapsedTime = Verbose,
                ShowSql = Verbose
            };

            StreamWriter outputWriter = null;
            if (Output)
            {
                if (string.IsNullOrEmpty(OutputFilename))
                    OutputFilename = Path.GetFileName(Target) + ".sql";

                outputWriter = new StreamWriter(OutputFilename);
                var fileAnnouncer = new TextWriterAnnouncer(outputWriter)
                {
                    ShowElapsedTime = false,
                    ShowSql = true
                };

                announcer = new CompositeAnnouncer(announcer, fileAnnouncer);
            }

            Log.LogMessage(MessageImportance.Low, "Creating Context");
                   
            var runnerContext = new RunnerContext(announcer)
            {
                ApplicationContext = ApplicationContext,
                Database = databaseType,
                Connection = Connection,
                ConnectionStringConfigPath = ConnectionStringConfigPath,
                Targets = Targets,
                PreviewOnly = PreviewOnly,
                Namespace = Namespace,
                NestedNamespaces = Nested,
                Task = Task,
                Version = Version,
                Steps = Steps,
                WorkingDirectory = WorkingDirectory,
                Profile = Profile,
                Tags = Tags.ToTags(),
                Timeout = Timeout,
                TransactionPerSession = TransactionPerSession
            };

            Log.LogMessage(MessageImportance.Low, "Executing Migration Runner");
            try
            {
                new TaskExecutor(runnerContext).Execute();
            }
            catch (ProcessorFactoryNotFoundException ex)
            {
                Log.LogError("While executing migrations the following error was encountered: {0}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Log.LogError("While executing migrations the following error was encountered: {0}, {1}", ex.Message, ex.StackTrace);
                return false;
            }
            finally
            {
                if (outputWriter != null)
                {
                    outputWriter.Dispose();
                }
            }

            return true;
        }
    }
}
