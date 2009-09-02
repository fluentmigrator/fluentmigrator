using System.Data.SqlClient;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration
{
	[TestFixture]
	public class MigrationVersionRunnerTests
	{
		private string connectionString = @"server=(local)\SQLEXPRESS;uid=;pwd=;Trusted_Connection=yes;database=FluentMigrator";

		[Test]
		public void CanLoadMigrations()
		{
			var conventions = new MigrationConventions();
			var connection = new SqlConnection(connectionString);
			connection.Open();
			var processor = new SqlServerProcessor(connection, new SqlServerGenerator());

			var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests));

			runner.Migrations.ShouldNotBeNull();
		}

		[Test]
		public void CanLoadVersion()
		{
			var conventions = new MigrationConventions();
			var connection = new SqlConnection(connectionString);
			connection.Open();
			var processor = new SqlServerProcessor(connection, new SqlServerGenerator());

			var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests));

			runner.Version.ShouldNotBeNull();
		}

		[Test]
		public void CanRunMigration()
		{
			var conventions = new MigrationConventions();
			var connection = new SqlConnection(connectionString);
			connection.Open();
			var processor = new SqlServerProcessor(connection, new SqlServerGenerator());

			var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests));

			runner.UpgradeToVersion(2, false);
			runner.Version.CurrentVersion.ShouldBe((long)2);

			//now step down to 0
			long last = 0;
			runner.StepDown(runner.CurrentVersion, 0, out last);
			runner.CurrentVersion.ShouldBe((long) 0);
		}

		[Test]
		public void CanUpdgradeToLatest()
		{
			var conventions = new MigrationConventions();
			var connection = new SqlConnection(connectionString);
			connection.Open();
			var processor = new SqlServerProcessor(connection, new SqlServerGenerator());

			var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests));

			runner.UpgradeToLatest(false);

			//now step down to 0
			long last;
			runner.StepDown(runner.CurrentVersion, 0, out last);
			runner.CurrentVersion.ShouldBe((long)0);
		}
	}
}
