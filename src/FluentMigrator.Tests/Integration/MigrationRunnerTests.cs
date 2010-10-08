#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// Copyright (c) 2010, Nathan Brown
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

using System;
using System.Data.SqlClient;
using System.Reflection;
using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Tests.Integration.Migrations;
using FluentMigrator.Tests.Integration.Migrations.Invalid;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration
{
	[TestFixture]
	public class MigrationRunnerTests : IntegrationTestBase
	{
		private IRunnerContext _runnerContext;

		[SetUp]
		public void SetUp()
		{
			_runnerContext = new RunnerContext( new TextWriterAnnouncer( System.Console.Out ) )
										{
											Database = "sqlserver",
											Target = GetType().Assembly.Location,
											Connection = IntegrationTestOptions.SqlServer.ConnectionString,
											Namespace = "FluentMigrator.Tests.Integration.Migrations"
										};
		}

		[Test]
		public void CanRunMigration()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

					runner.Up(new TestCreateAndDropTableMigration());

					processor.TableExists("TestTable").ShouldBeTrue();

					// This is a hack until MigrationVersionRunner and MigrationRunner are refactored and merged together
					//processor.CommitTransaction();

					runner.Down(new TestCreateAndDropTableMigration());
					processor.TableExists("TestTable").ShouldBeFalse();
				});
		}

		[Test]
		public void CanSilentlyFail()
		{
			var processorOptions = new Mock<IMigrationProcessorOptions>();
			processorOptions.SetupGet(x => x.PreviewOnly).Returns(false);

			var processor = new Mock<IMigrationProcessor>();
			processor.Setup(x => x.Process(It.IsAny<CreateForeignKeyExpression>())).Throws(new Exception("Error"));
			processor.Setup(x => x.Process(It.IsAny<DeleteForeignKeyExpression>())).Throws(new Exception("Error"));
			processor.Setup(x => x.Options).Returns(processorOptions.Object);

			var runner = new MigrationRunner( Assembly.GetExecutingAssembly(), _runnerContext, processor.Object ) { SilentlyFail = true };

			runner.Up(new TestForeignKeySilentFailure());
	
			runner.CaughtExceptions.Count.ShouldBeGreaterThan(0);

			runner.Down(new TestForeignKeySilentFailure());
			runner.CaughtExceptions.Count.ShouldBeGreaterThan(0);
		}

		//BUG: the Users table is not getting properly dropped. this test will fail on the second execution
		[Test]
		public void CanApplyForeignKeyConvention()
		{
			ExecuteWithSupportedProcessors(
				processor =>
				{
					var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

					runner.Up(new TestForeignKeyNamingConvention());

					// This is a hack until MigrationVersionRunner and MigrationRunner are refactored and merged together
					//processor.CommitTransaction();

					processor.ConstraintExists( "Users", "FK_Users_GroupId_Groups_GroupId" ).ShouldBeTrue();
					runner.Down( new TestForeignKeyNamingConvention() );
				});
		}

		//BUG: the Users table is not getting properly dropped. this test will fail on the second execution
		[Test]
		public void CanApplyIndexConvention()
		{
			ExecuteWithSupportedProcessors(
				processor =>
				{
					var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

					runner.Up(new TestIndexNamingConvention());
					processor.TableExists("Users").ShouldBeTrue();

					runner.Down(new TestIndexNamingConvention());
					processor.TableExists("Users").ShouldBeFalse();

					//processor.CommitTransaction();
				});
		}

		[Test]
		public void CanLoadMigrations()
		{
			ExecuteWithSupportedProcessors( processor =>
			{
				var runnerContext = new RunnerContext( new TextWriterAnnouncer( System.Console.Out ) )
				{
					Namespace = typeof( TestMigration ).Namespace,
				};

				var runner = new MigrationRunner( typeof( MigrationRunnerTests ).Assembly, runnerContext, processor );

				//runner.Processor.CommitTransaction();

				runner.MigrationLoader.Migrations.ShouldNotBeNull();
			} );
		}

		[Test]
		public void CanLoadVersion()
		{
			ExecuteWithSupportedProcessors( processor =>
			{
				var runnerContext = new RunnerContext( new TextWriterAnnouncer( System.Console.Out ) )
				{
					Namespace = typeof( TestMigration ).Namespace,
				};

				var runner = new MigrationRunner( typeof( TestMigration ).Assembly, runnerContext, processor );

				//runner.Processor.CommitTransaction();
				runner.VersionLoader.VersionInfo.ShouldNotBeNull();
			} );
		}

		[Test]
		public void CanRunMigrations()
		{
			ExecuteWithSupportedProcessors( processor =>
			{
				Assembly asm = typeof( MigrationRunnerTests ).Assembly;
				var runnerContext = new RunnerContext( new TextWriterAnnouncer( System.Console.Out ) )
				{
					Namespace = "FluentMigrator.Tests.Integration.Migrations"
				};
				var runner = new MigrationRunner( asm, runnerContext, processor );

				runner.MigrateUp();

				runner.VersionLoader.VersionInfo.HasAppliedMigration( 1 ).ShouldBeTrue();
				runner.VersionLoader.VersionInfo.HasAppliedMigration( 2 ).ShouldBeTrue();
				runner.VersionLoader.VersionInfo.Latest().ShouldBe( 2 );
			});
		}

		[Test]
		public void CanMigrateASpecificVersion()
		{
			ExecuteWithSupportedProcessors( processor =>
			{
				Assembly asm = typeof( MigrationRunnerTests ).Assembly;
				var runnerContext = new RunnerContext( new TextWriterAnnouncer( System.Console.Out ) );
				runnerContext.Namespace = "FluentMigrator.Tests.Integration.Migrations";
				var runner = new MigrationRunner( asm, runnerContext, processor );

				runner.MigrateUp( 1 );

				runner.VersionLoader.VersionInfo.HasAppliedMigration( 1 ).ShouldBeTrue();
				processor.TableExists( "Users" ).ShouldBeTrue();
			});
		}

		[Test]
		public void CanMigrateASpecificVersionDown()
		{
			ExecuteWithSupportedProcessors( processor =>
			{
				Assembly asm = typeof( MigrationRunnerTests ).Assembly;
				var runnerContext = new RunnerContext( new TextWriterAnnouncer( System.Console.Out ) );
				runnerContext.Namespace = "FluentMigrator.Tests.Integration.Migrations";
				var runner = new MigrationRunner( asm, runnerContext, processor );

				runner.MigrateUp( 1 );

				runner.VersionLoader.VersionInfo.HasAppliedMigration( 1 ).ShouldBeTrue();
				processor.TableExists( "Users" ).ShouldBeTrue();

				runner.MigrateDown( 1 );

				runner.VersionLoader.VersionInfo.HasAppliedMigration( 1 ).ShouldBeFalse();
				processor.TableExists( "Users" ).ShouldBeFalse();
			} );
		}
	}

	internal class TestForeignKeyNamingConvention : Migration
	{
		public override void Up()
		{
			Create.Table("Users")
				.WithColumn("UserId").AsInt32().Identity().PrimaryKey()
				.WithColumn("GroupId").AsInt32().NotNullable()
				.WithColumn("UserName").AsString(32).NotNullable()
				.WithColumn("Password").AsString(32).NotNullable();

			Create.Table("Groups")
				.WithColumn("GroupId").AsInt32().Identity().PrimaryKey()
				.WithColumn("Name").AsString(32).NotNullable();

			Create.ForeignKey().FromTable("Users").ForeignColumn("GroupId").ToTable("Groups").PrimaryColumn("GroupId");
		}

		public override void Down()
		{
			Delete.Table("Users");
			Delete.Table("Groups");
		}
	}

	internal class TestIndexNamingConvention : Migration
	{
		public override void Up()
		{
			Create.Table("Users")
				.WithColumn("UserId").AsInt32().Identity().PrimaryKey()
				.WithColumn("GroupId").AsInt32().NotNullable()
				.WithColumn("UserName").AsString(32).NotNullable()
				.WithColumn("Password").AsString(32).NotNullable();

			Create.Index().OnTable("Users").OnColumn("GroupId").Ascending();
		}

		public override void Down()
		{
			Delete.Table("Users");
		}
	}

	internal class TestForeignKeySilentFailure : Migration
	{
		public override void Up()
		{
			Create.Table("Users")
				.WithColumn("UserId").AsInt32().Identity().PrimaryKey()
				.WithColumn("GroupId").AsInt32().NotNullable()
				.WithColumn("UserName").AsString(32).NotNullable()
				.WithColumn("Password").AsString(32).NotNullable();

			Create.Table("Groups")
				.WithColumn("GroupId").AsInt32().Identity().PrimaryKey()
				.WithColumn("Name").AsString(32).NotNullable();

			Create.ForeignKey("FK_Foo").FromTable("Users").ForeignColumn("GroupId").ToTable("Groups").PrimaryColumn("GroupId");
		}

		public override void Down()
		{
			Delete.ForeignKey("FK_Foo").OnTable("Users");
			Delete.Table("Users");
			Delete.Table("Groups");
		}
	}

	internal class TestCreateAndDropTableMigration : Migration
	{
		public override void Up()
		{
			Create.Table("TestTable")
				.WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
				.WithColumn("Name").AsString(255).NotNullable().WithDefaultValue("Anonymous");

			Create.Table("TestTable2")
				.WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
				.WithColumn("Name").AsString(255).Nullable()
				.WithColumn("TestTableId").AsInt32().NotNullable();

			Create.Index("ix_Name").OnTable("TestTable2").OnColumn("Name").Ascending()
				.WithOptions().NonClustered();

			Create.Column("Name2").OnTable("TestTable2").AsBoolean().Nullable();

			Create.ForeignKey("fk_TestTable2_TestTableId_TestTable_Id")
				.FromTable("TestTable2").ForeignColumn("TestTableId")
				.ToTable("TestTable").PrimaryColumn("Id");

			Insert.IntoTable("TestTable").Row(new { Name = "Test" });
		}

		public override void Down()
		{
			Delete.Table("TestTable2");
			Delete.Table("TestTable");
		}
	}
}