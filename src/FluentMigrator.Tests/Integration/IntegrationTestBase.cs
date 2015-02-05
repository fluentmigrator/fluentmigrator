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
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.MySql;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Runner.Processors.SQLite;
using FluentMigrator.Runner.Processors.SqlServer;
using MySql.Data.MySqlClient;
using Npgsql;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Generators.MySql;
using FirebirdSql.Data.FirebirdClient;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Generators.Firebird;

namespace FluentMigrator.Tests.Integration
{
    public class IntegrationTestBase
    {
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
            if (exceptProcessors.Count(t => typeof(SqlServerProcessor).IsAssignableFrom(t)) == 0)
            {
                ExecuteWithSqlServer2005(test, tryRollback);
                ExecuteWithSqlServer2008(test, tryRollback);
                ExecuteWithSqlServer2012(test, tryRollback);
                ExecuteWithSqlServer2014(test, tryRollback);
            }
            
            if (exceptProcessors.Count(t => typeof(SQLiteProcessor).IsAssignableFrom(t)) == 0)
                ExecuteWithSqlite(test, IntegrationTestOptions.SqlLite);

            if (exceptProcessors.Count(t => typeof(MySqlProcessor).IsAssignableFrom(t)) == 0)
                ExecuteWithMySql(test, IntegrationTestOptions.MySql);

            if (exceptProcessors.Count(t => typeof(PostgresProcessor).IsAssignableFrom(t)) == 0)
                ExecuteWithPostgres(test, IntegrationTestOptions.Postgres, tryRollback);

            if (exceptProcessors.Count(t => typeof(FirebirdProcessor).IsAssignableFrom(t)) == 0)
                ExecuteWithFirebird(test, IntegrationTestOptions.Firebird);
        }

        protected static void ExecuteWithSqlServer2014(Action<IMigrationProcessor> test, bool tryRollback)
        {

            var serverOptions = IntegrationTestOptions.SqlServer2014;

            if (!serverOptions.IsEnabled)
                return;

            var announcer = new TextWriterAnnouncer(System.Console.Out);
            announcer.Heading("Testing Migration against MS SQL Server 2014");
            var generator = new SqlServer2014Generator();

            ExecuteWithSqlServer(serverOptions, announcer, generator, test, tryRollback);
        }

        protected static void ExecuteWithSqlServer2012(Action<IMigrationProcessor> test, bool tryRollback)
        {

            var serverOptions = IntegrationTestOptions.SqlServer2012;

            if (!serverOptions.IsEnabled)
                return;

            var announcer = new TextWriterAnnouncer(System.Console.Out);
            announcer.Heading("Testing Migration against MS SQL Server 2012");
            var generator = new SqlServer2012Generator();

            ExecuteWithSqlServer(serverOptions, announcer, generator, test, tryRollback);
        }

        protected static void ExecuteWithSqlServer2008(Action<IMigrationProcessor> test, bool tryRollback)
        {

            var serverOptions = IntegrationTestOptions.SqlServer2008;

            if (!serverOptions.IsEnabled)
                return;

            var announcer = new TextWriterAnnouncer(System.Console.Out);
            announcer.Heading("Testing Migration against MS SQL Server 2008");
            var generator = new SqlServer2008Generator();

            ExecuteWithSqlServer(serverOptions, announcer, generator, test, tryRollback);
        }

        protected static void ExecuteWithSqlServer2005(Action<IMigrationProcessor> test, bool tryRollback)
        {

            var serverOptions = IntegrationTestOptions.SqlServer2005;

            if (!serverOptions.IsEnabled)
                return;

            var announcer = new TextWriterAnnouncer(System.Console.Out);
            announcer.Heading("Testing Migration against MS SQL Server 2005");
            var generator = new SqlServer2005Generator();

            ExecuteWithSqlServer(serverOptions, announcer, generator, test, tryRollback);
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

        protected static void ExecuteWithSqlite(Action<IMigrationProcessor> test, IntegrationTestOptions.DatabaseServerOptions serverOptions)
        {
            if (!serverOptions.IsEnabled)
                return;

            var announcer = new TextWriterAnnouncer(System.Console.Out);
            announcer.Heading("Testing Migration against SQLite");

            var factory = new SQLiteDbFactory();
            using (var connection = factory.CreateConnection(serverOptions.ConnectionString))
            {
                var processor = new SQLiteProcessor(connection, new SQLiteGenerator(), announcer, new ProcessorOptions(), factory);
                test(processor);
            }
        }

        protected static void ExecuteWithPostgres(Action<IMigrationProcessor> test, IntegrationTestOptions.DatabaseServerOptions serverOptions, Boolean tryRollback)
        {
            if (!serverOptions.IsEnabled)
                return;

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
        }

        protected static void ExecuteWithMySql(Action<IMigrationProcessor> test, IntegrationTestOptions.DatabaseServerOptions serverOptions)
        {
            if (!serverOptions.IsEnabled)
                return;

            var announcer = new TextWriterAnnouncer(System.Console.Out);
            announcer.Heading("Testing Migration against MySQL Server");

            using (var connection = new MySqlConnection(serverOptions.ConnectionString))
            {
                var processor = new MySqlProcessor(connection, new MySqlGenerator(), announcer, new ProcessorOptions(), new MySqlDbFactory());
                test(processor);
            }
        }

        protected static void ExecuteWithFirebird(Action<IMigrationProcessor> test, IntegrationTestOptions.DatabaseServerOptions serverOptions)
        {
            if (!serverOptions.IsEnabled)
                return;

            var announcer = new TextWriterAnnouncer(System.Console.Out);
            announcer.ShowSql = true;
            announcer.Heading("Testing Migration against Firebird Server");
            
            if (!System.IO.File.Exists("fbtest.fdb"))
            {
                FbConnection.CreateDatabase(serverOptions.ConnectionString);
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

                connection.Close();
            }
        }
    }
}