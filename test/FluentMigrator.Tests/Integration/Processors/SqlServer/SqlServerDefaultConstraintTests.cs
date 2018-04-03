#region License
// Copyright (c) 2018, Fluent Migrator Project
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
using System.Diagnostics;
using System.Reflection;

using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.SqlServer
{
    [TestFixture]
    [Category("Integration")]
    [Category("SqlServer2016")]
    public class SqlServerDefaultConstraintTests
    {
        [Test]
        public void Issue715()
        {
            try
            {
                var task = MakeTask(
                    "migrate",
                    typeof(Migrations.SqlServer.Issue715.Migration150).Namespace);
                task.Execute();
            }
            finally
            {
                var task = MakeTask(
                    "rollback:all",
                    typeof(Migrations.SqlServer.Issue715.Migration150).Namespace);
                task.Execute();
            }
        }

        private TaskExecutor MakeTask(string task, string migrationsNamespace, Action<RunnerContext> configureContext = null)
        {
            var consoleAnnouncer = new TextWriterAnnouncer(TestContext.Out)
            {
                ShowSql = true
            };
            var debugAnnouncer = new TextWriterAnnouncer(msg => Debug.WriteLine(msg));
            var announcer = new CompositeAnnouncer(consoleAnnouncer, debugAnnouncer);
            var runnerContext = new RunnerContext(announcer)
            {
                Database = "SqlServer2016",
                Connection = IntegrationTestOptions.SqlServer2016.ConnectionString,
                Targets = new[] { Assembly.GetExecutingAssembly().Location },
                Namespace = migrationsNamespace,
                Task = task
            };

            configureContext?.Invoke(runnerContext);
            return new TaskExecutor(runnerContext);
        }
    }
}
