using System.Data.SqlClient;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Runner.Versioning;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration
{
	[TestFixture]
	public class VersionMigrationTests
	{
		[Test]
		public void CanUseVersionInfo()
		{
			string connectionString = @"server=(local)\SQLEXPRESS;uid=;pwd=;Trusted_Connection=yes;database=FluentMigrator";
			var conventions = new MigrationConventions();
			var connection = new SqlConnection(connectionString);
			connection.Open();
			var processor = new SqlServerProcessor(connection, new SqlServerGenerator());
			var runner = new MigrationRunner(conventions, processor);

			//ensure table doesn't exist
			if (processor.TableExists(VersionInfo.TABLE_NAME))
				runner.Down(new VersionMigration());

			runner.Up(new VersionMigration());
			processor.TableExists(VersionInfo.TABLE_NAME).ShouldBeTrue();

			runner.Down(new VersionMigration());
			processor.TableExists(VersionInfo.TABLE_NAME).ShouldBeFalse();
		}
	}
}
