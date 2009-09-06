using FluentMigrator.Runner;
using FluentMigrator.Tests.Integration.Migrations;
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

					var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests).Assembly, typeof(TestMigration).Namespace);

					runner.Migrations.ShouldNotBeNull();
				});
		}

		[Test]
		public void CanLoadVersion()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					var conventions = new MigrationConventions();

					var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests).Assembly, typeof(TestMigration).Namespace);

					runner.VersionInfo.ShouldNotBeNull();
				});
		}

		[Test]
		public void CanRunMigrations()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					var conventions = new MigrationConventions();
					var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests).Assembly, typeof(TestMigration).Namespace);

					runner.MigrateUp();
					runner.VersionInfo.HasAppliedMigration(1).ShouldBeTrue();
					runner.VersionInfo.HasAppliedMigration(2).ShouldBeTrue();
					runner.VersionInfo.Latest().ShouldBe(2);

					runner.Rollback(2);
					runner.VersionInfo.HasAppliedMigration(1).ShouldBeFalse();
					runner.VersionInfo.HasAppliedMigration(2).ShouldBeFalse();
					runner.VersionInfo.Latest().ShouldBe(0);

					runner.RemoveVersionTable();
				});
		}

		private void runMigrationsInNamespace(IMigrationProcessor processor, string @namespace)
		{
			var conventions = new MigrationConventions();

			var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests).Assembly, @namespace);

			runner.MigrateUp();
		}

		[Test]
		public void CanMigratePreviousUnappliedMigrations()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					runMigrationsInNamespace(processor, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass1");
					runMigrationsInNamespace(processor, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass2");
					runMigrationsInNamespace(processor, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass3");

					processor.TableExists("UserRoles").ShouldBeTrue();
					processor.TableExists("User").ShouldBeTrue();

					var conventions = new MigrationConventions();

					var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests).Assembly, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass3");

					runner.VersionInfo.HasAppliedMigration(200909060953).ShouldBeTrue();
					runner.VersionInfo.HasAppliedMigration(200909060935).ShouldBeTrue();
					runner.VersionInfo.HasAppliedMigration(200909060930).ShouldBeTrue();

					runner.VersionInfo.Latest().ShouldBe(200909060953);

					runner.Rollback(3);

					processor.TableExists("UserRoles").ShouldBeFalse();
					processor.TableExists("User").ShouldBeFalse();

					runner.RemoveVersionTable();
				});
		}
	}
}
