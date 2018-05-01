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

using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.SqlServer.SqlServer2016
{
    [TestFixture]
    public class SqlServerDefaultConstraintTests : SqlServerIntegrationTests
    {
        [Test]
        public void Issue715()
        {
            try
            {
                Execute(
                    services => services
                        .Configure<RunnerOptions>(opt => opt.Task = "migrate")
                        .WithMigrationsIn(typeof(Migrations.SqlServer.Issue715.Migration150).Namespace),
                    sp =>
                    {
                        var task = sp.GetRequiredService<TaskExecutor>();
                        task.Execute();
                    });
            }
            finally
            {
                Execute(
                    services => services.AddScoped<TaskExecutor>()
                        .Configure<RunnerOptions>(opt => opt.Task = "rollback:all")
                        .WithMigrationsIn(typeof(Migrations.SqlServer.Issue715.Migration150).Namespace),
                    sp =>
                    {
                        var task = sp.GetRequiredService<TaskExecutor>();
                        task.Execute();
                    });
            }
        }

        private void Execute(
            [CanBeNull] Action<IServiceCollection> initAction,
            [NotNull] Action<IServiceProvider> executeAction)
        {
            using (var serviceProvider = CreateProcessorServices(initAction))
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    executeAction(scope.ServiceProvider);
                }
            }
        }
    }
}
