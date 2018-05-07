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

using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.Oracle
{
    [Category("Integration")]
    [Obsolete]
    public abstract class OracleProcessorFactoryTestsBase
    {
        private IMigrationProcessorFactory _factory;
        private string _connectionString;
        private IAnnouncer _announcer;
        private ProcessorOptions _options;

        protected void SetUp(IMigrationProcessorFactory processorFactory)
        {
            if (!IntegrationTestOptions.Oracle.IsEnabled)
            {
                Assert.Ignore();
            }

            _factory = processorFactory;
            _connectionString = "Data Source=localhost/XE;User Id=Something;Password=Something";
            _announcer = new NullAnnouncer();
            _options = new ProcessorOptions();
        }

        [TestCase("")]
        [TestCase(null)]
        public void CreateProcessorWithNoProviderSwitchesShouldUseOracleQuoter(string providerSwitches)
        {
            _options.ProviderSwitches = providerSwitches;
            var processor = _factory.Create(_connectionString, _announcer, _options);
            Assert.That(((OracleProcessor) processor).Quoter, Is.InstanceOf<OracleQuoter>());
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
            _options.ProviderSwitches = providerSwitches;
            var processor = _factory.Create(_connectionString, _announcer, _options);
            Assert.That(((OracleProcessor) processor).Quoter, Is.InstanceOf<OracleQuoterQuotedIdentifier>());
        }
    }
}
