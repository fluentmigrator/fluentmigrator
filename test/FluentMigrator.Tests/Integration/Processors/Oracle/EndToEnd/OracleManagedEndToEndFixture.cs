#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.Oracle.EndToEnd
{
    [Category("Integration")]
    [Category("OracleManaged")]
    public class OracleManagedEndToEndFixture
    {
        protected void Migrate(string migrationsNamespace)
        {
            using (var serviceProvider = CreateTaskServices("migrate", migrationsNamespace))
            {
                var executor = serviceProvider.GetRequiredService<TaskExecutor>();
                executor.Execute();
            }
        }

        protected void Rollback(string migrationsNamespace)
        {
            using (var serviceProvider = CreateTaskServices("rollback", migrationsNamespace))
            {
                var executor = serviceProvider.GetRequiredService<TaskExecutor>();
                executor.Execute();
            }
        }

        protected ServiceProvider CreateTaskServices(string task, string migrationsNamespace)
        {
            var serivces = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(builder => builder.AddOracleManaged())
                .Configure<RunnerOptions>(opt => opt.Task = task)
                .Configure<TypeFilterOptions>(opt =>  opt.Namespace = migrationsNamespace)
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(IntegrationTestOptions.Oracle.ConnectionString));
            return serivces.BuildServiceProvider(false);
        }
    }
}
