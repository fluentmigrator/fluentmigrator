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
using FluentMigrator.Runner.Processors.Sqlite;
using FluentMigrator.Runner.Processors.SqlServer;
using MySql.Data.MySqlClient;
using Npgsql;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Generators.MySql;

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
			}
			if (exceptProcessors.Count(t => typeof(SqliteProcessor).IsAssignableFrom(t)) == 0)
				ExecuteWithSqlite(test, IntegrationTestOptions.SqlLite);
			if (exceptProcessors.Count(t => typeof(MySqlProcessor).IsAssignableFrom(t)) == 0)
				ExecuteWithMySql(test, IntegrationTestOptions.MySql);
			if (exceptProcessors.Count(t => typeof(PostgresProcessor).IsAssignableFrom(t)) == 0)
				ExecuteWithPostgres(test, IntegrationTestOptions.Postgres, tryRollback);
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

                if (tryRollback && processor.TransactionOpened)
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

		    var factory = new SqliteDbFactory();
            using (var connection =  factory.CreateConnection(serverOptions.ConnectionString))
			{
			    var processor = new SqliteProcessor(connection, new SqliteGenerator(), announcer, new ProcessorOptions(), factory);
				test(processor);
			}
		}

		protected static void ExecuteWithPostgres(Action<IMigrationProcessor> test, IntegrationTestOptions.DatabaseServerOptions serverOptions, Boolean tryRollback)
		{
			if (!serverOptions.IsEnabled)
				return;
			var connection = new NpgsqlConnection(serverOptions.ConnectionString);
			var processor = new PostgresProcessor(connection, new PostgresGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new PostgresDbFactory());
			test(processor);

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
	}
}