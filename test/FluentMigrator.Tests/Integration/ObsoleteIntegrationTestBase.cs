#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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
using System.Diagnostics;
using System.IO;
using System.Linq;

using FirebirdSql.Data.FirebirdClient;

using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.MySql;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Runner.Processors.SQLite;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Generators.MySql;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Tests.Helpers;

using Microsoft.Data.SqlClient;

using Moq;

using MySql.Data.MySqlClient;

using Npgsql;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration
{
    [Obsolete]
    public class ObsoleteIntegrationTestBase
    {
        private delegate void ExecuteTestDelegate(Action<IMigrationProcessor> test, bool tryRollback, IntegrationTestOptions.DatabaseServerOptions options);

        private readonly List<(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> getOptionsFunc, ExecuteTestDelegate executeTestDelegate)> _processors;

        private bool _isFirstExecuteForFirebird = true;

        private string _tempDataDirectory;

        protected ObsoleteIntegrationTestBase()
        {
            _processors = new List<(Type, Func<IntegrationTestOptions.DatabaseServerOptions>, ExecuteTestDelegate)>
            {
                (typeof(SqlServerProcessor), () => IntegrationTestOptions.SqlServer2005, ExecuteWithSqlServer2005),
                (typeof(SqlServerProcessor), () => IntegrationTestOptions.SqlServer2008, ExecuteWithSqlServer2008),
                (typeof(SqlServerProcessor), () => IntegrationTestOptions.SqlServer2012, ExecuteWithSqlServer2012),
                (typeof(SqlServerProcessor), () => IntegrationTestOptions.SqlServer2014, ExecuteWithSqlServer2014),
                (typeof(SqlServerProcessor), () => IntegrationTestOptions.SqlServer2016, ExecuteWithSqlServer2016),
                (typeof(SQLiteProcessor), () => IntegrationTestOptions.SQLite, ExecuteWithSqlite),
                (typeof(FirebirdProcessor), () => IntegrationTestOptions.Firebird, ExecuteWithFirebird),
                (typeof(PostgresProcessor), () => IntegrationTestOptions.Postgres, ExecuteWithPostgres),
                (typeof(MySqlProcessor), () => IntegrationTestOptions.MySql, ExecuteWithMySql),
            };
        }

        [SetUp]
        public void SetUpFirebird()
        {
            _isFirstExecuteForFirebird = true;
        }

        [SetUp]
        public void SetUpDataDirectory()
        {
            _tempDataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDataDirectory);
            AppDomain.CurrentDomain.SetData("DataDirectory", _tempDataDirectory);
        }

        [TearDown]
        public void TearDownDataDirectory()
        {
            if (!string.IsNullOrEmpty(_tempDataDirectory) && Directory.Exists(_tempDataDirectory))
            {
                Directory.Delete(_tempDataDirectory, true);
            }
        }

        protected bool IsAnyServerEnabled(params Type[] exceptProcessors)
        {
            return IsAnyServerEnabled(procType => !exceptProcessors.Any(p => p.IsAssignableFrom(procType)));
        }

        protected bool IsAnyServerEnabled(Predicate<Type> isMatch)
        {
            foreach (var (processorType, getOptionsFunc, _) in _processors)
            {
                var opt = getOptionsFunc();
                if (!opt.IsEnabled)
                    continue;

                if (!isMatch(processorType))
                    continue;

                return true;
            }

            return false;
        }

        protected void ExecuteWithSupportedProcessors(Action<IMigrationProcessor> test)
        {
            ExecuteWithSupportedProcessors(test, true);
        }

        protected void ExecuteWithSupportedProcessors(Action<IMigrationProcessor> test, bool tryRollback)
        {
            ExecuteWithSupportedProcessors(test, tryRollback, new Type[] { });
        }

        protected void ExecuteWithSupportedProcessors(Action<IMigrationProcessor> test, bool tryRollback, params Type[] exceptProcessors)
        {
            ExecuteWithSupportedProcessors(
                test,
                tryRollback,
                procType => !exceptProcessors.Any(p => p.IsAssignableFrom(procType)));
        }

        protected void ExecuteWithSupportedProcessors(Action<IMigrationProcessor> test, bool tryRollback, Predicate<Type> isMatch)
        {
            if (!IsAnyServerEnabled())
            {
                Assert.Fail(
$"No database processors are configured to run your migration tests.  This message is provided to avoid false positives.  To avoid this message enable one or more test runners in the {nameof(IntegrationTestOptions)} class.");
            }

            var executed = false;
            foreach (var (processorType, getOptionsFunc, executeFunc) in _processors)
            {
                var opt = getOptionsFunc();
                if (!opt.IsEnabled)
                    continue;

                if (!isMatch(processorType))
                    continue;

                executed = true;
                executeFunc(test, tryRollback, opt);
            }

            if (!executed)
            {
                Assert.Ignore("No processor found for the given action.");
            }
        }

        protected static void ExecuteWithSqlServer2016(Action<IMigrationProcessor> test, bool tryRollback, IntegrationTestOptions.DatabaseServerOptions serverOptions)
        {
            Debug.Assert(serverOptions.IsEnabled);

            var announcer = new TextWriterAnnouncer(TestContext.Out);
            announcer.Heading("Testing Migration against MS SQL Server 2016");
            var generator = new SqlServer2016Generator();

            ExecuteWithSqlServer(new[] { "SqlServer2016" }, serverOptions, announcer, generator, test, tryRollback);
        }

        protected static void ExecuteWithSqlServer2014(Action<IMigrationProcessor> test, bool tryRollback, IntegrationTestOptions.DatabaseServerOptions serverOptions)
        {
            Debug.Assert(serverOptions.IsEnabled);

            var announcer = new TextWriterAnnouncer(TestContext.Out);
            announcer.Heading("Testing Migration against MS SQL Server 2014");
            var generator = new SqlServer2014Generator();

            ExecuteWithSqlServer(new[] { "SqlServer2014" }, serverOptions, announcer, generator, test, tryRollback);
        }

        protected static void ExecuteWithSqlServer2012(Action<IMigrationProcessor> test, bool tryRollback, IntegrationTestOptions.DatabaseServerOptions serverOptions)
        {
            Debug.Assert(serverOptions.IsEnabled);

            var announcer = new TextWriterAnnouncer(TestContext.Out);
            announcer.Heading("Testing Migration against MS SQL Server 2012");
            var generator = new SqlServer2012Generator();

            ExecuteWithSqlServer(new[] { "SqlServer2012" }, serverOptions, announcer, generator, test, tryRollback);
        }

        protected static void ExecuteWithSqlServer2008(Action<IMigrationProcessor> test, bool tryRollback, IntegrationTestOptions.DatabaseServerOptions serverOptions)
        {
            Debug.Assert(serverOptions.IsEnabled);

            var announcer = new TextWriterAnnouncer(TestContext.Out);
            announcer.Heading("Testing Migration against MS SQL Server 2008");
            var generator = new SqlServer2008Generator();

            ExecuteWithSqlServer(new[] { "SqlServer2008" }, serverOptions, announcer, generator, test, tryRollback);
        }

        protected static void ExecuteWithSqlServer2005(Action<IMigrationProcessor> test, bool tryRollback, IntegrationTestOptions.DatabaseServerOptions serverOptions)
        {
            Debug.Assert(serverOptions.IsEnabled);

            var announcer = new TextWriterAnnouncer(TestContext.Out);
            announcer.Heading("Testing Migration against MS SQL Server 2005");
            var generator = new SqlServer2005Generator();

            ExecuteWithSqlServer(new[] { "SqlServer2005" }, serverOptions, announcer, generator, test, tryRollback);
        }

        private static void ExecuteWithSqlServer(IEnumerable<string> databaseTypes, IntegrationTestOptions.DatabaseServerOptions serverOptions, TextWriterAnnouncer announcer, SqlServer2005Generator generator, Action<IMigrationProcessor> test, bool tryRollback)
        {
            using (var connection = new SqlConnection(serverOptions.ConnectionString))
            {
                var processor = new SqlServerProcessor(databaseTypes, connection, generator, announcer, new ProcessorOptions(), new SqlServerDbFactory());
                test(processor);

                if (tryRollback && !processor.WasCommitted)
                {
                    processor.RollbackTransaction();
                }
            }
        }

        protected static void ExecuteWithSqlite(Action<IMigrationProcessor> test, bool tryRollback, IntegrationTestOptions.DatabaseServerOptions serverOptions)
        {
            if (!serverOptions.IsEnabled)
                return;

            var announcer = new TextWriterAnnouncer(TestContext.Out);
            announcer.Heading("Testing Migration against SQLite");

            var factory = new SQLiteDbFactory(serviceProvider: null);
            using (var connection = factory.Factory.CreateConnection())
            {
                Debug.Assert(connection != null, nameof(connection) + " != null");
                connection.ConnectionString = serverOptions.ConnectionString;
                connection.Open();

                var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Loose);
                var mockedConnectionStringAccessor = new Mock<IConnectionStringAccessor>(MockBehavior.Loose);
                mockedConnectionStringAccessor.SetupGet(x => x.ConnectionString).Returns(serverOptions.ConnectionString);

                using (var processor = new SQLiteProcessor(factory, new SQLiteGenerator(), LoggerHelper.Get<SQLiteProcessor>(), OptionHelper.Get<ProcessorOptions>(), mockedConnectionStringAccessor.Object, mockServiceProvider.Object, new SQLiteQuoter()))
                {
                    test(processor);

                    if (tryRollback && !processor.WasCommitted)
                    {
                        processor.RollbackTransaction();
                    }
                }
            }
        }

        protected static void ExecuteWithPostgres(Action<IMigrationProcessor> test, bool tryRollback, IntegrationTestOptions.DatabaseServerOptions serverOptions)
        {
            if (!serverOptions.IsEnabled)
                return;

            var announcer = new TextWriterAnnouncer(TestContext.Out);
            announcer.Heading("Testing Migration against Postgres");

            using (var connection = new NpgsqlConnection(serverOptions.ConnectionString))
            {
                var pgOptions = new PostgresOptions();
                var processor = new PostgresProcessor(
                    connection,
                    new PostgresGenerator(new PostgresQuoter(pgOptions)),
                    new TextWriterAnnouncer(TestContext.Out),
                    new ProcessorOptions(),
                    new PostgresDbFactory(serviceProvider: null),
                    pgOptions);

                test(processor);

                if (tryRollback && !processor.WasCommitted)
                {
                    processor.RollbackTransaction();
                }
            }
        }

        protected static void ExecuteWithMySql(Action<IMigrationProcessor> test, bool tryRollback, IntegrationTestOptions.DatabaseServerOptions serverOptions)
        {
            if (!serverOptions.IsEnabled)
                return;

            var announcer = new TextWriterAnnouncer(TestContext.Out);
            announcer.Heading("Testing Migration against MySQL Server");

            using (var connection = new MySqlConnection(serverOptions.ConnectionString))
            {
                var processor = new MySqlProcessor(connection, new MySql4Generator(), announcer, new ProcessorOptions(), new MySqlDbFactory(serviceProvider: null));
                test(processor);

                if (tryRollback && !processor.WasCommitted)
                {
                    processor.RollbackTransaction();
                }
            }
        }

        protected void ExecuteWithFirebird(Action<IMigrationProcessor> test, bool tryRollback, IntegrationTestOptions.DatabaseServerOptions serverOptions)
        {
            if (!serverOptions.IsEnabled)
                return;

            var announcer = new TextWriterAnnouncer(TestContext.Out);
            announcer.ShowSql = true;
            announcer.Heading("Testing Migration against Firebird Server");

            if (_isFirstExecuteForFirebird)
            {
                _isFirstExecuteForFirebird = false;
                FbConnection.CreateDatabase(serverOptions.ConnectionString, overwrite: true);
            }

            
            var options = FirebirdOptions.AutoCommitBehaviour();
            var processorOptions = OptionHelper.Get<ProcessorOptions>();
            var mockedConnectionStringAccessor = new Mock<IConnectionStringAccessor>(MockBehavior.Loose);
            mockedConnectionStringAccessor.SetupGet(x => x.ConnectionString).Returns(serverOptions.ConnectionString);

            var processor = new FirebirdProcessor(new FirebirdDbFactory(serviceProvider: null), new FirebirdGenerator(options), new FirebirdQuoter(options), LoggerHelper.Get<FirebirdProcessor>(), options: processorOptions, mockedConnectionStringAccessor.Object, options);

            try
            {
                test(processor);
            }
            catch (Exception)
            {
                if (tryRollback && !processor.WasCommitted)
                    processor.RollbackTransaction();
                throw;
            }
        }
    }
}
