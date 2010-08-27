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
using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization;
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
		[Test]
		public void CanLoadMigrations()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
										{
											Namespace = typeof(TestMigration).Namespace,
										};

					var runner = new MigrationRunner(typeof(MigrationVersionRunnerTests).Assembly, runnerContext, processor);

					runner.Processor.CommitTransaction();

					runner.MigrationLoader.Migrations.ShouldNotBeNull();
				});
		}

		[Test]
		public void CanLoadVersion()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
					{
						Namespace = typeof(TestMigration).Namespace,
					};

					var runner = new MigrationRunner(typeof(TestMigration).Assembly, runnerContext, processor);

					runner.Processor.CommitTransaction();
					runner.VersionLoader.VersionInfo.ShouldNotBeNull();
				});
		}

		[Test]
		public void CanRunMigrations()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					Assembly asm = typeof (MigrationVersionRunnerTests).Assembly;
					var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
											{
												Namespace = "FluentMigrator.Tests.Integration.Migrations"
											};
					var runner = new MigrationRunner(asm, runnerContext, processor);

					runner.MigrateUp();

					runner.VersionLoader.VersionInfo.HasAppliedMigration(1).ShouldBeTrue();
					runner.VersionLoader.VersionInfo.HasAppliedMigration(2).ShouldBeTrue();
					runner.VersionLoader.VersionInfo.Latest().ShouldBe(2);

					runner.Processor.BeginTransaction();
					runner.Rollback( 3 );
				});
		}

		private void runMigrationsInNamespace(IMigrationProcessor processor, string @namespace)
		{
			Assembly asm = typeof (MigrationVersionRunnerTests).Assembly;
			var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
									{
										Namespace = "FluentMigrator.Tests.Integration.Migrations"
									};
			runnerContext.Namespace = @namespace;
			var runner = new MigrationRunner( asm, runnerContext, processor );

			runner.MigrateUp();
		}

		[Test]
		public void CanMigrateASpecificVersion()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					Assembly asm = typeof(MigrationVersionRunnerTests).Assembly;
					var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out));
					runnerContext.Namespace = "FluentMigrator.Tests.Integration.Migrations";
					var runner = new MigrationRunner(asm, runnerContext, processor);

					runner.MigrateUp(1);

					runner.VersionLoader.VersionInfo.HasAppliedMigration(1).ShouldBeTrue();
					processor.TableExists("Users").ShouldBeTrue();


					runner.Rollback(1);

					runner.VersionLoader.VersionInfo.HasAppliedMigration(1).ShouldBeFalse();
					processor.TableExists("Users").ShouldBeFalse();
				});
		}

		[Test]
		public void CanMigrateASpecificVersionDown()
		{
			ExecuteWithSupportedProcessors(processor =>
			{
				Assembly asm = typeof(MigrationVersionRunnerTests).Assembly;
				var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out));
				runnerContext.Namespace = "FluentMigrator.Tests.Integration.Migrations";
				var runner = new MigrationRunner(asm, runnerContext, processor);

				runner.MigrateUp(1);

				runner.VersionLoader.VersionInfo.HasAppliedMigration(1).ShouldBeTrue();
				processor.TableExists("Users").ShouldBeTrue();

				runner.MigrateDown(1);

				runner.VersionLoader.VersionInfo.HasAppliedMigration(1).ShouldBeFalse();
				processor.TableExists("Users").ShouldBeFalse();
			});
		}

		[Test]
		public void SqlServerMigrationsAreTransactional()
		{
			var connection = new SqlConnection(IntegrationTestOptions.SqlServer.ConnectionString);
			var processor = new SqlServerProcessor(connection, new SqlServer2000Generator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions());

			Assembly asm = typeof(MigrationVersionRunnerTests).Assembly;
			var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out));
			runnerContext.Namespace = typeof(InvalidMigration).Namespace;
			var runner = new MigrationRunner(asm, runnerContext, processor);

			try
			{
				runner.MigrateUp();
				processor.RollbackTransaction();
			}
			catch
			{
			}

			processor.TableExists("Users").ShouldBeFalse();
		}
	}
}
