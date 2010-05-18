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
using System.Data.SQLite;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.MySql;
using FluentMigrator.Runner.Processors.Sqlite;
using FluentMigrator.Runner.Processors.SqlServer;
using MySql.Data.MySqlClient;

namespace FluentMigrator.Tests.Integration
{
	public class IntegrationTestBase
	{
		public void ExecuteWithSupportedProcessors(Action<IMigrationProcessor> test)
		{
			ExecuteWithSqlServer(test, IntegrationTestOptions.SqlServer);
			ExecuteWithSqlite(test, IntegrationTestOptions.SqlLite);
			ExecuteWithMySql(test, IntegrationTestOptions.MySql);
		}

		private static void ExecuteWithSqlServer(Action<IMigrationProcessor> test, IntegrationTestOptions.DatabaseServerOptions serverOptions)
		{
			if (!serverOptions.IsEnabled)
				return;

			var connection = new SqlConnection(serverOptions.ConnectionString);
			connection.Open();
			var processor = new SqlServerProcessor(connection, new SqlServer2000Generator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions());
			test(processor);
		}

		private static void ExecuteWithSqlite(Action<IMigrationProcessor> test, IntegrationTestOptions.DatabaseServerOptions serverOptions)
		{
			if (!serverOptions.IsEnabled)
				return;

			var connection = new SQLiteConnection(serverOptions.ConnectionString);
			var processor = new SqliteProcessor(connection, new SqliteGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions());
			test(processor);
		}

		private static void ExecuteWithMySql(Action<IMigrationProcessor> test, IntegrationTestOptions.DatabaseServerOptions serverOptions)
		{
			if (!serverOptions.IsEnabled)
				return;

			var connection = new MySqlConnection(serverOptions.ConnectionString);
			var processor = new MySqlProcessor(connection, new MySqlGenerator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions());
			test(processor);
		}
	}
}