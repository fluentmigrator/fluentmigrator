using System.Data.SqlClient;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.VersionTableInfo;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration
{
	[TestFixture]
	public class VersionMigrationTests : IntegrationTestBase
	{
		[Test]
		public void CanUseVersionInfo()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					var runner = new MigrationRunner(new MigrationConventions(), processor);

					IVersionTableMetaData tableMetaData = new DefaultVersionTableMetaData();

					//ensure table doesn't exist
					if (processor.TableExists(tableMetaData.TableName))
						runner.Down(new VersionMigration(tableMetaData));

					runner.Up(new VersionMigration(tableMetaData));
					processor.TableExists(tableMetaData.TableName).ShouldBeTrue();

					runner.Down(new VersionMigration(tableMetaData));
					processor.TableExists(tableMetaData.TableName).ShouldBeFalse();
				});
			
		}
	}
}
