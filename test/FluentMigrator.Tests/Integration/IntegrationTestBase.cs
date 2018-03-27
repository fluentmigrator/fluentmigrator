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
using System.Data.SqlClient;
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

using MySql.Data.MySqlClient;

using Npgsql;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration
{
    public class IntegrationTestBase
    {
        private bool _isFirstExecuteForFirebird = true;

        [SetUp]
        public void SetUpFirebird()
        {
            _isFirstExecuteForFirebird = true;
        }

        public void ExecuteWithSupportedProcessors(Action<IMigrationProcessor> test)
        {
            ExecuteWithSupportedProcessors(test, true);
        }

        public void ExecuteWithSupportedProcessors(Action<IMigrationProcessor> test, Boolean tryRollback)
        {
            ExecuteWithSupportedProcessors(test, tryRollback, new Type[] { });
        }

        public void ExecuteWithSupportedProcessors(Action<IMigrationProcessor> test, Boolean tryRollback, params Type[] exceptProcessors)
        {
            if (IntegrationTestOptions.AnyServerTypesEnabled == false)
            {
                Assert.Fail(
                    "No database processors are configured to run your migration tests.  This message is provided to avoid false positives.  To avoid this message enable one or more test runners in the {0} class.",
                    nameof(IntegrationTestOptions));
            }

            var executed = false;
            if (exceptProcessors.Count(t => typeof(SqlServerProcessor).IsAssignableFrom(t)) == 0)
            {
                executed |= ExecuteWithSqlServer2005(test, tryRollback);
                executed |= ExecuteWithSqlServer2008(test, tryRollback);
                executed |= ExecuteWithSqlServer2012(test, tryRollback);
                executed |= ExecuteWithSqlServer2014(test, tryRollback);
            }

            if (exceptProcessors.Count(t => typeof(SQLiteProcessor).IsAssignableFrom(t)) == 0)
                executed |= ExecuteWithSqlite(test, IntegrationTestOptions.SqlLite);

            if (exceptProcessors.Count(t => typeof(MySqlProcessor).IsAssignableFrom(t)) == 0)
                executed |= ExecuteWithMySql(test, IntegrationTestOptions.MySql);

            if (exceptProcessors.Count(t => typeof(PostgresProcessor).IsAssignableFrom(t)) == 0)
                executed |= ExecuteWithPostgres(test, IntegrationTestOptions.Postgres, tryRollback);

            if (exceptProcessors.Count(t => typeof(FirebirdProcessor).IsAssignableFrom(t)) == 0)
                executed |= ExecuteWithFirebird(test, IntegrationTestOptions.Firebird);

            if (!executed)
            {
                Assert.Ignore("No processor found for the given action.");
            }
        }

        protected static bool ExecuteWithSqlServer2014(Action<IMigrationProcessor> test, bool tryRollback)
        {

            var serverOptions = IntegrationTestOptions.SqlServer2014;

            if (!serverOptions.IsEnabled)
                return false;

            var announcer = new TextWriterAnnouncer(System.Console.Out);
            announcer.Heading("Testing Migration against MS SQL Server 2014");
            var generator = new SqlServer2014Generator();

            ExecuteWithSqlServer(serverOptions, announcer, generator, test, tryRollback);
            return true;
        }

        protected static bool ExecuteWithSqlServer2012(Action<IMigrationProcessor> test, bool tryRollback)
        {

            var serverOptions = IntegrationTestOptions.SqlServer2012;

            if (!serverOptions.IsEnabled)
                return false;

            var announcer = new TextWriterAnnouncer(System.Console.Out);
            announcer.Heading("Testing Migration against MS SQL Server 2012");
            var generator = new SqlServer2012Generator();

            ExecuteWithSqlServer(serverOptions, announcer, generator, test, tryRollback);
            return true;
        }

        protected static bool ExecuteWithSqlServer2008(Action<IMigrationProcessor> test, bool tryRollback)
        {

            var serverOptions = IntegrationTestOptions.SqlServer2008;

            if (!serverOptions.IsEnabled)
                return false;

            var announcer = new TextWriterAnnouncer(System.Console.Out);
            announcer.Heading("Testing Migration against MS SQL Server 2008");
            var generator = new SqlServer2008Generator();

            ExecuteWithSqlServer(serverOptions, announcer, generator, test, tryRollback);
            return true;
        }

        protected static bool ExecuteWithSqlServer2005(Action<IMigrationProcessor> test, bool tryRollback)
        {

            var serverOptions = IntegrationTestOptions.SqlServer2005;

            if (!serverOptions.IsEnabled)
                return false;

            var announcer = new TextWriterAnnouncer(System.Console.Out);
            announcer.Heading("Testing Migration against MS SQL Server 2005");
            var generator = new SqlServer2005Generator();

            ExecuteWithSqlServer(serverOptions, announcer, generator, test, tryRollback);
            return true;
        }

        private static void ExecuteWithSqlServer(IntegrationTestOptions.DatabaseServerOptions serverOptions, TextWriterAnnouncer announcer, SqlServer2005Generator generator, Action<IMigrationProcessor> test, bool tryRollback)
        {
            using (var connection = new SqlConnection(serverOptions.ConnectionString))
            {
                var processor = new SqlServerProcessor(connection, generator, announcer, new ProcessorOptions(), new SqlServerDbFactory());
                test(processor);

                if (tryRollback && !processor.WasCommitted)
                {
                    processor.RollbackTransaction();
                }
            }
        }

        protected static bool ExecuteWithSqlite(Action<IMigrationProcessor> test, IntegrationTestOptions.DatabaseServerOptions serverOptions)
        {
            if (!serverOptions.IsEnabled)
                return false;

            var announcer = new TextWriterAnnouncer(System.Console.Out);
            announcer.Heading("Testing Migration against SQLite");

            var factory = new SQLiteDbFactory();
            using (var connection = factory.CreateConnection(serverOptions.ConnectionString))
            {
                var processor = new SQLiteProcessor(connection, new SQLiteGenerator(), announcer, new ProcessorOptions(), factory);
                test(processor);
            }

            return true;
        }

        protected static bool ExecuteWithPostgres(Action<IMigrationProcessor> test, IntegrationTestOptions.DatabaseServerOptions serverOptions, Boolean tryRollback)
        {
            if (!serverOptions.IsEnabled)
                return false;

            var announcer = new TextWriterAnnouncer(System.Console.Out);
            announcer.Heading("Testing Migration against Postgres");

            using (var connection = new NpgsqlConnection(serverOptions.ConnectionString))
            {
                var processor = new PostgresProcessor(connection, new PostgresGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new PostgresDbFactory());

                test(processor);

                if (!processor.WasCommitted)
                {
                    processor.RollbackTransaction();
                }
            }

            return true;
        }

        protected static bool ExecuteWithMySql(Action<IMigrationProcessor> test, IntegrationTestOptions.DatabaseServerOptions serverOptions)
        {
            if (!serverOptions.IsEnabled)
                return false;

            var announcer = new TextWriterAnnouncer(System.Console.Out);
            announcer.Heading("Testing Migration against MySQL Server");

            using (var connection = new MySqlConnection(serverOptions.ConnectionString))
            {
                var processor = new MySqlProcessor(connection, new MySql4Generator(), announcer, new ProcessorOptions(), new MySqlDbFactory());
                test(processor);
            }

            return true;
        }

        protected bool ExecuteWithFirebird(Action<IMigrationProcessor> test, IntegrationTestOptions.DatabaseServerOptions serverOptions)
        {
            if (!serverOptions.IsEnabled)
                return false;

            var announcer = new TextWriterAnnouncer(System.Console.Out);
            announcer.ShowSql = true;
            announcer.Heading("Testing Migration against Firebird Server");

            if (_isFirstExecuteForFirebird)
            {
                _isFirstExecuteForFirebird = false;

                var dataDir = (string) AppDomain.CurrentDomain.GetData("DataDirectory");
                if (string.IsNullOrEmpty(dataDir))
                {
                    dataDir = AppContext.BaseDirectory;
                    AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);
                }

                FbConnection.CreateDatabase(serverOptions.ConnectionString, true);
            }

            using (var connection = new FbConnection(serverOptions.ConnectionString))
            {
                var options = FirebirdOptions.AutoCommitBehaviour();
                var processor = new FirebirdProcessor(connection, new FirebirdGenerator(options), announcer, new ProcessorOptions(), new FirebirdDbFactory(), options);

                try
                {
                    test(processor);
                }
                catch (Exception)
                {
                    if (!processor.WasCommitted)
                        processor.RollbackTransaction();
                    throw;
                }
            }

            return true;
        }
    }
}
