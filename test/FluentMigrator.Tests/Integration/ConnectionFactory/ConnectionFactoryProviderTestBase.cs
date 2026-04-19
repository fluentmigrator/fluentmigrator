#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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
using System.Data;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Tests.Integration.ConnectionFactory.Migrations;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Tests.Integration.ConnectionFactory
{
    public abstract class ConnectionFactoryProviderTestBase
    {
        protected const string MigrationNamespace =
            "FluentMigrator.Tests.Integration.ConnectionFactory.Migrations";

        protected static ServiceProvider CreateServiceProvider(
            Action<IMigrationRunnerBuilder> addProvider,
            Func<IServiceProvider, IDbConnection> connectionFactory,
            Action<IServiceCollection> configureServices = null)
        {
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(runnerBuilder =>
                {
                    addProvider(runnerBuilder);

                    runnerBuilder
                        .WithConnectionFactory(connectionFactory)
                        .ScanIn(typeof(CreateFactoryConnectionTable).Assembly)
                        .For.Migrations();
                })
                .Configure<TypeFilterOptions>(options =>
                {
                    options.Namespace = MigrationNamespace;
                    options.NestedNamespaces = true;
                });

            configureServices?.Invoke(services);

            return services.BuildServiceProvider();
        }

        protected static string GetRequiredEnvironmentVariableOrIgnore(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);

            if (string.IsNullOrWhiteSpace(value))
            {
                NUnit.Framework.Assert.Ignore($"Set {name} to run this provider integration test.");
            }

            return value;
        }
    }
}
