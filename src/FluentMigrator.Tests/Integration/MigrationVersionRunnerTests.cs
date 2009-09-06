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

					runner.Version.ShouldNotBeNull();
				});
		}

		[Test]
		public void CanRunMigration()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					var conventions = new MigrationConventions();
                    var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests).Assembly, typeof(TestMigration).Namespace);

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

                    var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests).Assembly, typeof(TestMigration).Namespace);

					runner.UpgradeToLatest(false);

					//now step down to 0
					long last;
					runner.StepDown(runner.CurrentVersion, 0, out last);
					runner.CurrentVersion.ShouldBe((long)0);
				});
		}

        private void runMigrationsInNamespace(IMigrationProcessor processor, string @namespace)
        {
            var conventions = new MigrationConventions();

            var runner = new MigrationVersionRunner(conventions, processor, new MigrationLoader(conventions), typeof(MigrationVersionRunnerTests).Assembly, @namespace);

            runner.UpgradeToLatest(false);
        }

        [Test, Ignore("Interleaved migrations not supported yet")]
        public void CanMigratePreviousUnappliedMigrations()
        {
            ExecuteWithSqlite(processor =>
               {
                   runMigrationsInNamespace(processor, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass1");
                   runMigrationsInNamespace(processor, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass2");
                   runMigrationsInNamespace(processor, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass3");

                   processor.TableExists("UserRoles").ShouldBeTrue();
                   processor.TableExists("User").ShouldBeTrue();

               });
        }
	}
}
