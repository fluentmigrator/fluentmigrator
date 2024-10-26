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

using Autofac;
using Autofac.Extensions.DependencyInjection;

using FluentMigrator.Runner.Initialization;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    public class IssueTests
    {
        [Test]
        public void Issue882WithCustomConfigReader()
        {
            var serviceProvider = ServiceCollectionExtensions.CreateServices()
                .AddSingleton<Issue882CustomConnectionStringReader>()
                .AddScoped<IConnectionStringReader>(
                    sp => sp.GetRequiredService<Issue882CustomConnectionStringReader>())
                .BuildServiceProvider(validateScopes: true);

            // Connection string is empty
            using (var scope = serviceProvider.CreateScope())
            {
                var connStringAccessor = scope.ServiceProvider.GetRequiredService<IConnectionStringAccessor>();
                Assert.That(connStringAccessor.ConnectionString, Is.Null);
            }

            // Change the connection string globally
            var reader = serviceProvider.GetRequiredService<Issue882CustomConnectionStringReader>();
            reader.ConnectionString = "abc";

            // Connection string is set
            using (var scope = serviceProvider.CreateScope())
            {
                var connStringAccessor = scope.ServiceProvider.GetRequiredService<IConnectionStringAccessor>();
                Assert.That(connStringAccessor.ConnectionString, Is.EqualTo("abc"));
            }
        }

        [Test]
        public void Issue882WithAutofac()
        {
            var services = ServiceCollectionExtensions.CreateServices()
                .AddSingleton<Issue882CustomConnectionStringReader>()
                .AddScoped<IConnectionStringReader>(
                    sp => sp.GetRequiredService<Issue882CustomConnectionStringReader>());

            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(services);
            var container = containerBuilder.Build();
            var serviceProvider = container.Resolve<IServiceProvider>();

            // Ensure that the connection string is empty
            using (var scope = serviceProvider.CreateScope())
            {
                var connStringAccessor = scope.ServiceProvider.GetRequiredService<IConnectionStringAccessor>();
                Assert.That(connStringAccessor.ConnectionString, Is.Null);
            }

            // Set the connection string for the scope only
            using (var lifetimeScope = container.BeginLifetimeScope(
                cb =>
                {
                    cb.Register(
                        cc => new Issue882CustomConnectionStringReader()
                        {
                            ConnectionString = "abc"
                        })
                        .AsSelf()
                        .SingleInstance();
                }))
            {
                var scopedServiceProvider = lifetimeScope.Resolve<IServiceProvider>();
                var connStringAccessor = scopedServiceProvider.GetRequiredService<IConnectionStringAccessor>();
                Assert.That(connStringAccessor.ConnectionString, Is.EqualTo("abc"));
            }

            // The connection string is empty again
            using (var scope = serviceProvider.CreateScope())
            {
                var connStringAccessor = scope.ServiceProvider.GetRequiredService<IConnectionStringAccessor>();
                Assert.That(connStringAccessor.ConnectionString, Is.Null);
            }
        }

        private class Issue882CustomConnectionStringReader : IConnectionStringReader
        {
            /// <summary>
            /// Gets or sets the connection string to be returned
            /// </summary>
            public string ConnectionString { get; set; }

            /// <inheritdoc />
            public int Priority { get; } = 300;

            /// <inheritdoc />
            public string GetConnectionString(string connectionStringOrName)
            {
                return string.IsNullOrEmpty(ConnectionString) ? null : ConnectionString;
            }
        }
    }
}
