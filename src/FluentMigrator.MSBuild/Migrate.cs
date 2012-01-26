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
using System;
using System.Reflection;

namespace FluentMigrator.MSBuild
{
    public class Migrate : Task
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
        private string migrationAssembly;

        [Required]
        public string Connection { get; set; }

        public string Target { get { return migrationAssembly; } set { migrationAssembly = value; } }

        public string MigrationAssembly { get { return migrationAssembly; } set { migrationAssembly = value; } }

        public string Database { get { return databaseType; } set { databaseType = value; } }

        public string DatabaseType { get { return databaseType; } set { databaseType = value; } }

        public bool Verbose { get; set; }

        public string Namespace { get; set; }

        public string Task { get; set; }

        public long Version { get; set; }

        public int Steps { get; set; }

        public string WorkingDirectory { get; set; }

        public int Timeout { get; set; }

        public string Profile { get; set; }

        public override bool Execute()
        {

            if (string.IsNullOrEmpty(databaseType))
            {
                Log.LogError("You must specific a database type. i.e. mysql or sqlserver");
                return false;
            }

            if (string.IsNullOrEmpty(migrationAssembly))
            {
                Log.LogError("You must specific a migration assembly");
                return false;
            }


            Log.LogCommandLine(MessageImportance.Low, "Creating Context");
            var announcer = new BaseAnnouncer(msg => Log.LogCommandLine(MessageImportance.Normal, msg))
            {
                ShowElapsedTime = Verbose,
                ShowSql = Verbose
            };
            var runnerContext = new RunnerContext(announcer)
            {
                Database = databaseType,
                Connection = Connection,
                Target = Target,
                PreviewOnly = false,
                Namespace = Namespace,
                Task = Task,
                Version = Version,
                Steps = Steps,
                WorkingDirectory = WorkingDirectory,
                Profile = Profile,
                Timeout = Timeout
            };

            Log.LogCommandLine(MessageImportance.Low, "Executing Migration Runner");
            try
            {
                new TaskExecutor(runnerContext).Execute();
            }
            catch (ProcessorFactoryNotFoundException ex)
            {
                announcer.Error("While executing migrations the following error was encountered: {0}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                announcer.Error("While executing migrations the following error was encountered: {0}, {1}", ex.Message, ex.StackTrace);
                return false;
            }

            return true;
        }
    }
}
