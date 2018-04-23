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

using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.SqlServer
{
    [TestFixture]
    [Category("Integration")]
    [Category("SqlServer2016")]
    public class SqlServerDefaultConstraintTests
    {
        [SetUp]
        public void SetUp()
        {
            if (!IntegrationTestOptions.SqlServer2016.IsEnabled)
                Assert.Ignore();
        }

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

        private TaskExecutor MakeTask(string task, string migrationsNamespace)
        {
            var consoleAnnouncer = new TextWriterAnnouncer(TestContext.Out)
            {
                ShowSql = true
            };
            var debugAnnouncer = new TextWriterAnnouncer(msg => Debug.WriteLine(msg));
            var announcer = new CompositeAnnouncer(consoleAnnouncer, debugAnnouncer);

            var services = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(r => r.AddSqlServer2016())
                .AddSingleton<IAnnouncer>(announcer)
                .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader(IntegrationTestOptions.SqlServer2016.ConnectionString))
                .WithMigrationsIn(migrationsNamespace)
                .Configure<RunnerOptions>(opt => opt.Task = task)
                .AddScoped<TaskExecutor>();

            var serviceBuilder = services.BuildServiceProvider();
            return serviceBuilder.GetRequiredService<TaskExecutor>();
        }
    }
}
