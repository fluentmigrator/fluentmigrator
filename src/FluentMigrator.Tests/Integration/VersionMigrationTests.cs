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
	public class VersionMigrationTests : IntegrationTestBase
	{
		[Test]
		public void CanUseVersionInfo()
		{
            ExecuteWithSupportedProcessors(processor =>
                {
                    var runner = new MigrationRunner(new MigrationConventions(), processor);

                    //ensure table doesn't exist
                    if (processor.TableExists(VersionInfo.TABLE_NAME))
                        runner.Down(new VersionMigration());

                    runner.Up(new VersionMigration());
                    processor.TableExists(VersionInfo.TABLE_NAME).ShouldBeTrue();

                    runner.Down(new VersionMigration());
                    processor.TableExists(VersionInfo.TABLE_NAME).ShouldBeFalse();
                });
			
		}
	}
}
