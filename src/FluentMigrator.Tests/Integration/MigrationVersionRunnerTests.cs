using System.Data.SqlClient;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Tests.Integration.Migrations;
using FluentMigrator.Tests.Integration.Migrations.Invalid;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration
{
	[TestFixture]
	public class MigrationVersionRunnerTests : IntegrationTestBase
	{
		private MigrationConventions _conventions;

		[SetUp]
		public void SetUp()
		{
			_conventions = new MigrationConventions();
		}

		[Test]
		public void CanLoadMigrations()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					var runner = new MigrationVersionRunner(_conventions, processor, new MigrationLoader(_conventions), typeof(MigrationVersionRunnerTests).Assembly, typeof(TestMigration).Namespace);

					runner.Migrations.ShouldNotBeNull();
				});
		}

		[Test]
		public void CanLoadVersion()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					var runner = new MigrationVersionRunner(_conventions, processor, new MigrationLoader(_conventions), typeof(MigrationVersionRunnerTests).Assembly, typeof(TestMigration).Namespace);

					runner.VersionInfo.ShouldNotBeNull();
				});
		}

		[Test]
		public void CanRunMigrations()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					var runner = new MigrationVersionRunner(_conventions, processor, new MigrationLoader(_conventions), typeof(MigrationVersionRunnerTests).Assembly, typeof(TestMigration).Namespace);

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
			var runner = new MigrationVersionRunner(_conventions, processor, new MigrationLoader(_conventions), typeof(MigrationVersionRunnerTests).Assembly, @namespace);

			runner.MigrateUp();
		}

		[Test]
		public void CanMigrateInterleavedMigrations()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					runMigrationsInNamespace(processor, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass1");
					runMigrationsInNamespace(processor, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass2");
					runMigrationsInNamespace(processor, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass3");

					processor.TableExists("UserRoles").ShouldBeTrue();
					processor.TableExists("User").ShouldBeTrue();

					var runner = new MigrationVersionRunner(_conventions, processor, new MigrationLoader(_conventions), typeof(MigrationVersionRunnerTests).Assembly, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass3");

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

		[Test]
		public void CanMigrateASpecificVersion()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					var runner = new MigrationVersionRunner(_conventions, processor, new MigrationLoader(_conventions), typeof(MigrationVersionRunnerTests).Assembly, "FluentMigrator.Tests.Integration.Migrations");

					runner.MigrateUp(1);

					runner.VersionInfo.HasAppliedMigration(1).ShouldBeTrue();
					processor.TableExists("Users").ShouldBeTrue();


					runner.Rollback(1);

					runner.VersionInfo.HasAppliedMigration(1).ShouldBeFalse();
					processor.TableExists("Users").ShouldBeFalse();
				});
		}

		[Test]
		public void CanMigrateASpecificVersionDown()
		{
			ExecuteWithSupportedProcessors(processor =>
			{
				var runner = new MigrationVersionRunner(_conventions, processor, new MigrationLoader(_conventions), typeof(MigrationVersionRunnerTests).Assembly, "FluentMigrator.Tests.Integration.Migrations");

				runner.MigrateUp(1);

				runner.VersionInfo.HasAppliedMigration(1).ShouldBeTrue();
				processor.TableExists("Users").ShouldBeTrue();

				runner.MigrateDown(1);

				runner.VersionInfo.HasAppliedMigration(1).ShouldBeFalse();
				processor.TableExists("Users").ShouldBeFalse();
			});
		}

		[Test]
		public void SqlServerMigrationsAreTransactional()
		{
			var connection = new SqlConnection(sqlServerConnectionString);
			connection.Open();
			var processor = new SqlServerProcessor(connection, new SqlServerGenerator());
			var runner = new MigrationVersionRunner(_conventions, processor, new MigrationLoader(_conventions), typeof(MigrationVersionRunnerTests).Assembly, typeof(InvalidMigration).Namespace);

			try
			{
				runner.MigrateUp();
			}
			catch
			{
			}

			processor.TableExists("Users").ShouldBeFalse();
		}
	}
}
