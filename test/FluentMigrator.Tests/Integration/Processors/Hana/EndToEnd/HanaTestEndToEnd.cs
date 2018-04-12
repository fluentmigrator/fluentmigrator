#region License
// Copyright (c) 2007-2018, FluentMigrator Project
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
using System.Reflection;

using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Hana;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Hana;

using NUnit.Framework;

using Sap.Data.Hana;

namespace FluentMigrator.Tests.Integration.Processors.Hana.EndToEnd
{
    [Category("Integration")]
    [Category("Hana")]
    public class HanaEndToEndFixture
    {
        protected void Migrate(string migrationsNamespace)
        {
            MakeTask("migrate", migrationsNamespace).Execute();
        }

        protected void Rollback(string migrationsNamespace)
        {
            MakeTask("rollback", migrationsNamespace).Execute();
        }

        protected TaskExecutor MakeTask(string task, string migrationsNamespace)
        {
            var announcer = new TextWriterAnnouncer(TestContext.Out);
            var runnerContext = new RunnerContext(announcer)
            {
                Database = "Hana",
                Connection = IntegrationTestOptions.Hana.ConnectionString,
                Targets = new[] { Assembly.GetExecutingAssembly().Location },
                Namespace = migrationsNamespace,
                Task = task
            };
            return new TaskExecutor(runnerContext);
        }

        protected class ScopedConnection : IDisposable
        {
            public HanaConnection Connection { get; set; }

            public HanaProcessor Processor { get; set; }

            public ScopedConnection()
            {
                Connection = new HanaConnection(IntegrationTestOptions.Hana.ConnectionString);
                Processor = new HanaProcessor(Connection, new HanaGenerator(), new TextWriterAnnouncer(TestContext.Out), new ProcessorOptions(), new HanaDbFactory());
                Connection.Open();
                Processor.BeginTransaction();
            }

            public void Dispose()
            {
                Processor?.CommitTransaction();
                Processor?.Dispose();
            }
        }
    }
}
