#region License
// Copyright (c) 2018, FluentMigrator Project
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
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.SqlServer;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.SqlServer.SqlServer2016
{
    [Category("Integration")]
    [Category("SqlServer2016")]
    public abstract class SqlServerIntegrationTests
    {
        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        protected SqlServer2016Processor Processor { get; private set; }
        protected SqlServer2008Quoter Quoter { get; private set; }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            ServiceProvider = CreateProcessorServices(initAction: null);
        }

        [OneTimeTearDown]
        public void ClassTearDown()
        {
            ServiceProvider?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            ServiceScope = ServiceProvider.CreateScope();
            Processor = ServiceScope.ServiceProvider.GetRequiredService<SqlServer2016Processor>();
            Quoter = ServiceScope.ServiceProvider.GetRequiredService<SqlServer2008Quoter>();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
        }

        protected ServiceProvider CreateProcessorServices([CanBeNull] Action<IServiceCollection> initAction)
        {
            if (!IntegrationTestOptions.SqlServer2016.IsEnabled)
                Assert.Ignore();

            var serivces = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(r => r.AddSqlServer2016())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(IntegrationTestOptions.SqlServer2016.ConnectionString));

            initAction?.Invoke(serivces);

            return serivces.BuildServiceProvider();
        }
    }
}
