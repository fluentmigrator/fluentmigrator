using FluentMigrator.Runner;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration
{
	[TestFixture]
	public class MigrationVersionRunnerTests : IntegrationTestBase
	{
		[Test]
		public void CanLoadMigrations()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					var conventions = new MigrationConventions();

					var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests));

					runner.Migrations.ShouldNotBeNull();
				});
		}

		[Test]
		public void CanLoadVersion()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					var conventions = new MigrationConventions();

					var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests));

					runner.Version.ShouldNotBeNull();
				});
		}

		[Test]
		public void CanRunMigration()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					var conventions = new MigrationConventions();
					var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests));

					runner.UpgradeToVersion(2, false);
					runner.Version.CurrentVersion.ShouldBe((long)2);

					//now step down to 0
					long last = 0;
					runner.StepDown(runner.CurrentVersion, 0, out last);
					runner.CurrentVersion.ShouldBe((long)0);
				});
		}

		[Test]
		public void CanUpdgradeToLatest()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					var conventions = new MigrationConventions();

					var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests));

					runner.UpgradeToLatest(false);

					//now step down to 0
					long last;
					runner.StepDown(runner.CurrentVersion, 0, out last);
					runner.CurrentVersion.ShouldBe((long)0);
				});
		}
	}
}
