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

using System.Data.SqlClient;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
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
					var runner = new MigrationVersionRunner(_conventions, processor, new MigrationLoader(_conventions), typeof(MigrationVersionRunnerTests).Assembly, typeof(TestMigration).Namespace, new TextWriterAnnouncer(System.Console.Out));

					runner.Migrations.ShouldNotBeNull();
				});
		}

		[Test]
		public void CanLoadVersion()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					var runner = new MigrationVersionRunner(_conventions, processor, new MigrationLoader(_conventions), typeof(MigrationVersionRunnerTests).Assembly, typeof(TestMigration).Namespace, new TextWriterAnnouncer(System.Console.Out));

					runner.VersionInfo.ShouldNotBeNull();
				});
		}

		[Test]
		public void CanRunMigrations()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					var runner = new MigrationVersionRunner(_conventions, processor, new MigrationLoader(_conventions), typeof(MigrationVersionRunnerTests).Assembly, typeof(TestMigration).Namespace, new TextWriterAnnouncer(System.Console.Out));

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
			var runner = new MigrationVersionRunner(_conventions, processor, new MigrationLoader(_conventions), typeof(MigrationVersionRunnerTests).Assembly, @namespace, new TextWriterAnnouncer(System.Console.Out));

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

					var runner = new MigrationVersionRunner(_conventions, processor, new MigrationLoader(_conventions), typeof(MigrationVersionRunnerTests).Assembly, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass3", new TextWriterAnnouncer(System.Console.Out));

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
					var runner = new MigrationVersionRunner(_conventions, processor, new MigrationLoader(_conventions), typeof(MigrationVersionRunnerTests).Assembly, "FluentMigrator.Tests.Integration.Migrations", new TextWriterAnnouncer(System.Console.Out));

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
				var runner = new MigrationVersionRunner(_conventions, processor, new MigrationLoader(_conventions), typeof(MigrationVersionRunnerTests).Assembly, "FluentMigrator.Tests.Integration.Migrations", new TextWriterAnnouncer(System.Console.Out));

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
			var connection = new SqlConnection(IntegrationTestOptions.SqlServer.ConnectionString);
			connection.Open();
			var processor = new SqlServerProcessor(connection, new SqlServer2000Generator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions());
			var runner = new MigrationVersionRunner(_conventions, processor, new MigrationLoader(_conventions), typeof(MigrationVersionRunnerTests).Assembly, typeof(InvalidMigration).Namespace, new TextWriterAnnouncer(System.Console.Out));

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
