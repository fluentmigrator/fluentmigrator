#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using System.Collections.Generic;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.DB2;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Processors.Hana;
using FluentMigrator.Runner.Processors.Jet;
using FluentMigrator.Runner.Processors.MySql;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Runner.Processors.SQLite;
using FluentMigrator.Runner.Processors.SqlServer;
using NUnit.Framework;

namespace FluentMigrator.Tests.Integration
{
    public class IntegrationTestBase
    {
		private const string TestConfigFileName = "TestConfig.xml";
        private TestDriver _testDriver;

        [TestFixtureSetUp]
        public void BeforeAllTests()
        {
            var testConfiguration = new TestConfiguration(TestConfigFileName);
            testConfiguration.Configure();
            _testDriver = new TestDriver(testConfiguration.GetProcessorFactory(), testConfiguration.RequestedDbEngine);
            Announcer = new TextWriterAnnouncer(System.Console.Out);
            ((TextWriterAnnouncer)Announcer).ShowSql = true;
            Announcer.Heading(string.Format("Testing Migration against {0} Server", testConfiguration.RequestedDbEngine));
        }

        protected ProcessorOptions ProcessorOptions { get; set; }

        protected IAnnouncer Announcer { get; set; }

        public void ExecuteWithSupportedProcessor(Action<IMigrationProcessor> test)
        {
            ExecuteWithSupportedProcessor(test, true);
        }

        public void ExecuteWithSupportedProcessor(Action<IMigrationProcessor> test, Boolean tryRollback)
        {
            ExecuteWithSupportedProcessor(test, tryRollback, new Type[] { });
        }

        public void ExecuteWithSupportedProcessor(Action<IMigrationProcessor> test, Boolean tryRollback, params Type[] excludedProcessors)
        {
			_testDriver.Run(test, Announcer, ProcessorOptions, tryRollback, excludedProcessors);
        }

        public static IEnumerable<Type> AllProcessors()
        {
            yield return typeof(Db2Processor);
            yield return typeof(FirebirdProcessor);
            yield return typeof(HanaProcessor);
            yield return typeof(JetProcessor);
            yield return typeof(MySqlProcessor);
            yield return typeof(OracleProcessor);
            yield return typeof(PostgresProcessor);
            yield return typeof(SQLiteProcessor);
            yield return typeof(SqlServerProcessor);

        }
    }
}