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

using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
    public abstract class OracleQuoterServiceTestsBase
    {
        [TestCase("")]
        [TestCase(null)]
        public void CreateProcessorWithNoProviderSwitchesShouldUseOracleQuoter(string providerSwitches)
        {
            Execute(
                services => services.Configure<ProcessorOptions>(opt => opt.ProviderSwitches = providerSwitches),
                serviceProvider =>
                {
                    var quoter = serviceProvider.GetRequiredService<OracleQuoterBase>();
                    Assert.That(quoter, Is.InstanceOf<OracleQuoter>());
                });
        }

        [TestCase("QuotedIdentifiers=true")]
        [TestCase("QuotedIdentifiers=TRUE;")]
        [TestCase("QuotedIDENTIFIERS=TRUE;")]
        [TestCase("QuotedIdentifiers=true;somethingelse=1")]
        [TestCase("somethingelse=1;QuotedIdentifiers=true")]
        [TestCase("somethingelse=1;QuotedIdentifiers=true;sometingOther='special thingy'")]
        public void CreateProcessorWithProviderSwitchIndicatingQuotedShouldUseOracleQuoterQuotedIdentifier(
            string providerSwitches)
        {
            Execute(
                services => services.Configure<ProcessorOptions>(opt => opt.ProviderSwitches = providerSwitches),
                serviceProvider =>
                {
                    var quoter = serviceProvider.GetRequiredService<OracleQuoterBase>();
                    Assert.That(quoter, Is.InstanceOf<OracleQuoterQuotedIdentifier>());
                });
        }

        private void Execute([CanBeNull] Action<IServiceCollection> initAction, [NotNull] Action<IServiceProvider> executeAction)
        {
            var serivces = AddOracleServices(ServiceCollectionExtensions.CreateServices());
            initAction?.Invoke(serivces);
            using (var serviceProvider = serivces.BuildServiceProvider())
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    executeAction(scope.ServiceProvider);
                }
            }
        }

        protected abstract IServiceCollection AddOracleServices(IServiceCollection services);
    }
}
