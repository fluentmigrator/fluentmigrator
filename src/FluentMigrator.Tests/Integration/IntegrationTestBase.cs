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
using System.Linq;
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
        private TestProcessorFactory _testProcessorFactory;
        private TestDriver _testDriver;

        [TestFixtureSetUp]
        public void BeforeAllTests()
        {
            var testConfiguration = new TestConfiguration(TestConfigFileName);
            testConfiguration.Configure();
            _testProcessorFactory = testConfiguration.GetProcessorFactory();
            _testDriver = new TestDriver(_testProcessorFactory, testConfiguration.RequestedDbEngine);
            Announcer = new TextWriterAnnouncer(System.Console.Out);
            ((TextWriterAnnouncer)Announcer).ShowSql = true;
            Announcer.Heading(string.Format("Testing Migration against {0} Server", testConfiguration.RequestedDbEngine));
        }

        [TestFixtureTearDown]
        public void AfterAllTests()
        {
            var testCleaner = _testProcessorFactory as TestCleaner;
            if (testCleaner != null)
                testCleaner.CleanUp();
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

        public void ExecuteFor(Type processorType, Action<IMigrationProcessor> test)
        {
            var excluded = AllProcessors().Except(new[] { processorType });
            ExecuteWithSupportedProcessor(test, false, excluded.ToArray());
        }

        public static readonly Type DB2 = typeof(Db2Processor);
        public static readonly Type FIREBIRD = typeof(FirebirdProcessor);
        public static readonly Type HANA = typeof(HanaProcessor);
        public static readonly Type JET = typeof(JetProcessor);
        public static readonly Type MYSQL = typeof(MySqlProcessor);
        public static readonly Type ORACLE = typeof(OracleProcessor);
        public static readonly Type POSTGRES = typeof(PostgresProcessor);
        public static readonly Type SQLITE = typeof(SQLiteProcessor);
        public static readonly Type MS_SQL_SERVER = typeof(SqlServerProcessor);
        public static readonly Type MS_SQL_CE = typeof(SqlServerCeProcessor);

        public static IEnumerable<Type> AllProcessors()
        {
            yield return DB2;
            yield return FIREBIRD;
            yield return HANA;
            yield return JET;
            yield return MYSQL;
            yield return ORACLE;
            yield return POSTGRES;
            yield return SQLITE;
            yield return MS_SQL_SERVER;
            yield return MS_SQL_CE;
        }
    }
}