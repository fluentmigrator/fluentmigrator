using System;
using System.Data.SqlClient;
using System.Data.SQLite;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors.Sqlite;
using FluentMigrator.Runner.Processors.SqlServer;

namespace FluentMigrator.Tests.Integration
{
	public class IntegrationTestBase
	{
		protected string sqlServerConnectionString = @"server=(local)\SQLEXPRESS;uid=;pwd=;Trusted_Connection=yes;database=FluentMigrator";
		protected string sqliteConnectionString = @"Data Source=:memory:;Version=3;New=True;";

		public void ExecuteWithSupportedProcessors(Action<IMigrationProcessor> test)
		{
			ExecuteWithSqlServer(test);
			ExecuteWithSqlite(test);
		}

		public void ExecuteWithSqlServer(Action<IMigrationProcessor> test)
		{
			var connection = new SqlConnection(sqlServerConnectionString);
			connection.Open();
			var processor = new SqlServerProcessor(connection, new SqlServerGenerator());
			test(processor);
		}

		public void ExecuteWithSqlite(Action<IMigrationProcessor> test)
		{
			var connection = new SQLiteConnection(sqliteConnectionString);
			var processor = new SqliteProcessor(connection, new SqliteGenerator());
			test(processor);
		}
	}
}