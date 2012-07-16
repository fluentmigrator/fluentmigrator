#region License

// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Initialization;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace FluentMigrator.NAnt
{
    [TaskName("migrate")]
    public class MigrateTask : Task
    {
        [TaskAttribute("context")]
        public string ApplicationContext { get; set; }

        [TaskAttribute("database")]
        public string Database { get; set; }

        [TaskAttribute("connection")]
        public string Connection { get; set; }

        [TaskAttribute("target")]
        public string Target { get; set; }

        [TaskAttribute("namespace")]
        public string Namespace { get; set; }

        [TaskAttribute("task")]
        public string Task { get; set; }

        [TaskAttribute("to")]
        public long Version { get; set; }

        [TaskAttribute("steps")]
        public int Steps { get; set; }

        [TaskAttribute("workingdirectory")]
        public string WorkingDirectory { get; set; }

        [TaskAttribute("profile")]
        public string Profile { get; set; }

        [TaskAttribute("tags")]
        public string Tags { get; set; }

        [TaskAttribute("timeout")]
        public int Timeout { get; set; }

        protected override void ExecuteTask()
        {
            var announcer = new ConsoleAnnouncer
                                {
                                    ShowElapsedTime = Verbose,
                                    ShowSql = Verbose
                                };
            var runnerContext = new RunnerContext(announcer)
                                    {
                                        ApplicationContext = ApplicationContext,
                                        Database = Database,
                                        Connection = Connection,
                                        Target = Target,
                                        PreviewOnly = false,
                                        Namespace = Namespace,
                                        Task = Task,
                                        Version = Version,
                                        Steps = Steps,
                                        WorkingDirectory = WorkingDirectory,
                                        Profile = Profile,
                                        Tags = Tags.ToTags(),
                                        Timeout = Timeout
                                    };

            try
            {
                new TaskExecutor(runnerContext).Execute();
            }
            catch (ProcessorFactoryNotFoundException ex)
            {
                announcer.Error("While executing migrations the following error was encountered: {0}", ex.Message);
            }
            catch (Exception e)
            {
                announcer.Error("While executing migrations the following error was encountered: {0}, {1}", e.Message, e.StackTrace);
            }
        }
    }
}