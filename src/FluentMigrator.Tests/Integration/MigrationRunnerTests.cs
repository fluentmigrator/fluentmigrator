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
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Sqlite;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Tests.Integration.Migrations;
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
			_runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
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

					processor.TableExists(null, "TestTable").ShouldBeTrue();

					// This is a hack until MigrationVersionRunner and MigrationRunner are refactored and merged together
					//processor.CommitTransaction();

					runner.Down(new TestCreateAndDropTableMigration());
					processor.TableExists(null, "TestTable").ShouldBeFalse();
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

			var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor.Object) { SilentlyFail = true };

			runner.Up(new TestForeignKeySilentFailure());

			runner.CaughtExceptions.Count.ShouldBeGreaterThan(0);

			runner.Down(new TestForeignKeySilentFailure());
			runner.CaughtExceptions.Count.ShouldBeGreaterThan(0);
		}

		[Test]
		public void CanApplyForeignKeyConvention()
		{
			ExecuteWithSupportedProcessors(
				processor =>
				{
					var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

					runner.Up(new TestForeignKeyNamingConvention());

					processor.ConstraintExists(null, "Users", "FK_Users_GroupId_Groups_GroupId").ShouldBeTrue();
					runner.Down(new TestForeignKeyNamingConvention());
				}, false, typeof(SqliteProcessor));
		}

        [Test]
        public void CanApplyForeignKeyConventionWithSchema()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestForeignKeyNamingConventionWithSchema());

                    processor.ConstraintExists("TestSchema", "Users", "FK_Users_GroupId_Groups_GroupId").ShouldBeTrue();
                    runner.Down(new TestForeignKeyNamingConvention());
                }, false, typeof(SqliteProcessor));
        }

		[Test]
		public void CanApplyIndexConvention()
		{
			ExecuteWithSupportedProcessors(
				processor =>
				{
					var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

					runner.Up(new TestIndexNamingConvention());
					processor.IndexExists(null, "Users", "IX_Users_GroupId").ShouldBeTrue();
					processor.TableExists(null, "Users").ShouldBeTrue();

					runner.Down(new TestIndexNamingConvention());
					processor.IndexExists(null, "Users", "IX_Users_GroupId").ShouldBeFalse();
					processor.TableExists(null, "Users").ShouldBeFalse();
				});
		}

        [Test]
        public void CanApplyIndexConventionWithSchema()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestIndexNamingConventionWithSchema());
                    processor.IndexExists("TestSchema", "Users", "IX_Users_GroupId").ShouldBeTrue();
                    processor.TableExists("TestSchema", "Users").ShouldBeTrue();

                    runner.Down(new TestIndexNamingConvention());
                    processor.IndexExists("TestSchema", "Users", "IX_Users_GroupId").ShouldBeFalse();
                    processor.TableExists("TestSchema", "Users").ShouldBeFalse();
                });
        }

		[Test]
		public void CanCreateAndDropIndex()
		{
			ExecuteWithSupportedProcessors(
				processor =>
				{
					var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

					runner.Up(new TestCreateAndDropTableMigration());
					processor.IndexExists(null, "TestTable", "IX_TestTable_Name").ShouldBeFalse();

					runner.Up(new TestCreateAndDropIndexMigration());
					processor.IndexExists(null, "TestTable", "IX_TestTable_Name").ShouldBeTrue();

					runner.Down(new TestCreateAndDropIndexMigration());
					processor.IndexExists(null, "TestTable", "IX_TestTable_Name").ShouldBeFalse();

					runner.Down(new TestCreateAndDropTableMigration());
					processor.IndexExists(null, "TestTable", "IX_TestTable_Name").ShouldBeFalse();

					//processor.CommitTransaction();
				});
		}

        [Test]
        public void CanCreateAndDropIndexWithSchema()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.IndexExists("TestSchema", "TestTable", "IX_TestTable_Name").ShouldBeFalse();

                    runner.Up(new TestCreateAndDropIndexMigration());
                    processor.IndexExists("TestSchema", "TestTable", "IX_TestTable_Name").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropIndexMigration());
                    processor.IndexExists("TestSchema", "TestTable", "IX_TestTable_Name").ShouldBeFalse();

                    runner.Down(new TestCreateAndDropTableMigration());
                    processor.IndexExists("TestSchema", "TestTable", "IX_TestTable_Name").ShouldBeFalse();

                    //processor.CommitTransaction();
                });
        }

		[Test]
		public void CanRenameTable()
		{
			ExecuteWithSupportedProcessors(
				processor =>
				{
					var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

					runner.Up(new TestCreateAndDropTableMigration());
					processor.TableExists(null, "TestTable2").ShouldBeTrue();

					runner.Up(new TestRenameTableMigration());
					processor.TableExists(null, "TestTable2").ShouldBeFalse();
					processor.TableExists(null, "TestTable'3").ShouldBeTrue();

					runner.Down(new TestRenameTableMigration());
					processor.TableExists(null, "TestTable'3").ShouldBeFalse();
					processor.TableExists(null, "TestTable2").ShouldBeTrue();

					runner.Down(new TestCreateAndDropTableMigration());
					processor.TableExists(null, "TestTable2").ShouldBeFalse();

					//processor.CommitTransaction();
				});
		}

        [Test]
        public void CanRenameTableWithSchema()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.TableExists("TestSchema", "TestTable2").ShouldBeTrue();

                    runner.Up(new TestRenameTableMigration());
                    processor.TableExists("TestSchema", "TestTable2").ShouldBeFalse();
                    processor.TableExists("TestSchema", "TestTable'3").ShouldBeTrue();

                    runner.Down(new TestRenameTableMigration());
                    processor.TableExists("TestSchema", "TestTable'3").ShouldBeFalse();
                    processor.TableExists("TestSchema", "TestTable2").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropTableMigration());
                    processor.TableExists("TestSchema", "TestTable2").ShouldBeFalse();

                    //processor.CommitTransaction();
                });
        }

		[Test]
		public void CanRenameColumn()
		{
			ExecuteWithSupportedProcessors(
				processor =>
				{
					var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

					runner.Up(new TestCreateAndDropTableMigration());
					processor.ColumnExists(null, "TestTable2", "Name").ShouldBeTrue();

					runner.Up(new TestRenameColumnMigration());
					processor.ColumnExists(null, "TestTable2", "Name").ShouldBeFalse();
					processor.ColumnExists(null, "TestTable2", "Name'3").ShouldBeTrue();

					runner.Down(new TestRenameColumnMigration());
					processor.ColumnExists(null, "TestTable2", "Name'3").ShouldBeFalse();
					processor.ColumnExists(null, "TestTable2", "Name").ShouldBeTrue();

					runner.Down(new TestCreateAndDropTableMigration());
					processor.ColumnExists(null, "TestTable2", "Name").ShouldBeFalse();
				}, true, typeof(SqliteProcessor));
		}

        [Test]
        public void CanRenameColumnWithSchema()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "Name").ShouldBeTrue();

                    runner.Up(new TestRenameColumnMigration());
                    processor.ColumnExists("TestSchema", "TestTable2", "Name").ShouldBeFalse();
                    processor.ColumnExists("TestSchema", "TestTable2", "Name'3").ShouldBeTrue();

                    runner.Down(new TestRenameColumnMigration());
                    processor.ColumnExists("TestSchema", "TestTable2", "Name'3").ShouldBeFalse();
                    processor.ColumnExists("TestSchema", "TestTable2", "Name").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropTableMigration());
                    processor.ColumnExists("TestSchema", "TestTable2", "Name").ShouldBeFalse();
				}, true, typeof(SqliteProcessor));
        }

		[Test]
		public void CanLoadMigrations()
		{
			ExecuteWithSupportedProcessors(processor =>
			{
				var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
				{
					Namespace = typeof(TestMigration).Namespace,
				};

				var runner = new MigrationRunner(typeof(MigrationRunnerTests).Assembly, runnerContext, processor);

				//runner.Processor.CommitTransaction();

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

				//runner.Processor.CommitTransaction();
				runner.VersionLoader.VersionInfo.ShouldNotBeNull();
			});
		}

		[Test]
		public void CanRunMigrations()
		{
			ExecuteWithSupportedProcessors(processor =>
			{
				MigrationRunner runner = SetupMigrationRunner(processor);

				runner.MigrateUp();

				runner.VersionLoader.VersionInfo.HasAppliedMigration(1).ShouldBeTrue();
				runner.VersionLoader.VersionInfo.HasAppliedMigration(2).ShouldBeTrue();
				runner.VersionLoader.VersionInfo.Latest().ShouldBe(2);
			});
		}

		[Test]
		public void CanMigrateASpecificVersion()
		{
			ExecuteWithSupportedProcessors(processor =>
			{
				MigrationRunner runner = SetupMigrationRunner(processor);

				runner.MigrateUp(1);

				runner.VersionLoader.VersionInfo.HasAppliedMigration(1).ShouldBeTrue();
				processor.TableExists(null, "Users").ShouldBeTrue();
			});
		}

		[Test]
		public void CanMigrateASpecificVersionDown()
		{
			try
			{
				ExecuteWithSupportedProcessors(processor =>
				{
					MigrationRunner runner = SetupMigrationRunner(processor);

					runner.MigrateUp(1);

					runner.VersionLoader.VersionInfo.HasAppliedMigration(1).ShouldBeTrue();
					processor.TableExists(null, "Users").ShouldBeTrue();
				}, false, typeof(SqliteProcessor));

				ExecuteWithSupportedProcessors(processor =>
				{
					MigrationRunner testRunner = SetupMigrationRunner(processor);
					testRunner.MigrateDown(1);

					testRunner.VersionLoader.VersionInfo.HasAppliedMigration(1).ShouldBeFalse();
					processor.TableExists(null, "Users").ShouldBeFalse();
				}, false, typeof(SqliteProcessor));

			}
			finally
			{
				ExecuteWithSupportedProcessors(processor =>
				{
					MigrationRunner testRunner = SetupMigrationRunner(processor);
					testRunner.RollbackToVersion(0);
				}, false);
			}
		}

		[Test]
		public void RollbackAllShouldRemoveVersionInfoTable()
		{
			ExecuteWithSupportedProcessors(processor =>
			{
				MigrationRunner runner = SetupMigrationRunner(processor);

				runner.MigrateUp(2);

                processor.TableExists(runner.VersionLoader.VersionTableMetaData.SchemaName, runner.VersionLoader.VersionTableMetaData.TableName).ShouldBeTrue();
			});

			ExecuteWithSupportedProcessors(processor =>
			{
				MigrationRunner runner = SetupMigrationRunner(processor);
				runner.RollbackToVersion(0);

                processor.TableExists(runner.VersionLoader.VersionTableMetaData.SchemaName, runner.VersionLoader.VersionTableMetaData.TableName).ShouldBeFalse();
			});
		}

		[Test]
		public void MigrateUpWithSqlServerProcessorShouldCommitItsTransaction()
		{
			if (!IntegrationTestOptions.SqlServer.IsEnabled)
				return;

			var connection = new SqlConnection(IntegrationTestOptions.SqlServer.ConnectionString);
			var processor = new SqlServerProcessor(connection, new SqlServer2000Generator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions());

			MigrationRunner runner = SetupMigrationRunner(processor);
			runner.MigrateUp();

			try
			{
				processor.WasCommitted.ShouldBeTrue();

			}
			finally
			{
				CleanupTestSqlServerDatabase(connection, processor);
			}
		}

		[Test]
		public void MigrateUpSpecificVersionWithSqlServerProcessorShouldCommitItsTransaction()
		{
			if (!IntegrationTestOptions.SqlServer.IsEnabled)
				return;

			var connection = new SqlConnection(IntegrationTestOptions.SqlServer.ConnectionString);
			var processor = new SqlServerProcessor(connection, new SqlServer2000Generator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions());

			MigrationRunner runner = SetupMigrationRunner(processor);
			runner.MigrateUp(1);

			try
			{
				processor.WasCommitted.ShouldBeTrue();

			}
			finally
			{
				CleanupTestSqlServerDatabase(connection, processor);
			}
		}


		private static MigrationRunner SetupMigrationRunner(IMigrationProcessor processor)
		{
			Assembly asm = typeof(MigrationRunnerTests).Assembly;
			var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
			{
				Namespace = "FluentMigrator.Tests.Integration.Migrations"
			};

			return new MigrationRunner(asm, runnerContext, processor);
		}

		private static void CleanupTestSqlServerDatabase(SqlConnection connection, SqlServerProcessor origProcessor)
		{
			if (origProcessor.WasCommitted)
			{
				connection.Close();

				var cleanupProcessor = new SqlServerProcessor(connection, new SqlServer2000Generator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions());
				MigrationRunner cleanupRunner = SetupMigrationRunner(cleanupProcessor);
				cleanupRunner.RollbackToVersion(0);

			}
			else
			{
				origProcessor.RollbackTransaction();
			}
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
			Delete.Index("IX_Users_GroupId").OnTable("Users").OnColumn("GroupId");
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

	internal class TestRenameTableMigration : AutoReversingMigration
	{
		public override void Up()
		{
			Rename.Table("TestTable2").To("TestTable'3");
		}
	}

	internal class TestRenameColumnMigration : AutoReversingMigration
	{
		public override void Up()
		{
			Rename.Column("Name").OnTable("TestTable2").To("Name'3");
		}
	}

	internal class TestCreateAndDropIndexMigration : Migration
	{
		public override void Up()
		{
			Create.Index("IX_TestTable_Name").OnTable("TestTable").OnColumn("Name");
		}

		public override void Down()
		{
			Delete.Index("IX_TestTable_Name").OnTable("TestTable");
		}
	}


    internal class TestForeignKeyNamingConventionWithSchema : Migration
    {
        public override void Up()
        {
            Create.Table("Users")
                .InSchema("TestSchema")
                .WithColumn("UserId").AsInt32().Identity().PrimaryKey()
                .WithColumn("GroupId").AsInt32().NotNullable()
                .WithColumn("UserName").AsString(32).NotNullable()
                .WithColumn("Password").AsString(32).NotNullable();

            Create.Table("Groups")
                .InSchema("TestSchema")
                .WithColumn("GroupId").AsInt32().Identity().PrimaryKey()
                .WithColumn("Name").AsString(32).NotNullable();

            Create.ForeignKey().FromTable("Users").InSchema("TestSchema").ForeignColumn("GroupId").ToTable("Groups").InSchema("TestSchema").PrimaryColumn("GroupId");
        }

        public override void Down()
        {
            Delete.Table("Users");
            Delete.Table("Groups");
        }
    }

    internal class TestIndexNamingConventionWithSchema : Migration
    {
        public override void Up()
        {
            Create.Table("Users")
                .InSchema("TestSchema")
                .WithColumn("UserId").AsInt32().Identity().PrimaryKey()
                .WithColumn("GroupId").AsInt32().NotNullable()
                .WithColumn("UserName").AsString(32).NotNullable()
                .WithColumn("Password").AsString(32).NotNullable();

            Create.Index().OnTable("Users").InSchema("TestSchema").OnColumn("GroupId").Ascending();
        }

        public override void Down()
        {
            Delete.Index("IX_Users_GroupId").OnTable("Users").OnColumn("GroupId");
            Delete.Table("Users");
        }
    }

    internal class TestCreateAndDropTableMigrationWithSchema : Migration
    {
        public override void Up()
        {
            Create.Table("TestTable")
                .InSchema("TestSchema")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Name").AsString(255).NotNullable().WithDefaultValue("Anonymous");

            Create.Table("TestTable2")
                .InSchema("TestSchema")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Name").AsString(255).Nullable()
                .WithColumn("TestTableId").AsInt32().NotNullable();

            Create.Index("ix_Name").OnTable("TestTable2").InSchema("TestSchema").OnColumn("Name").Ascending()
                .WithOptions().NonClustered();

            Create.Column("Name2").OnTable("TestTable2").InSchema("TestSchema").AsBoolean().Nullable();

            Create.ForeignKey("fk_TestTable2_TestTableId_TestTable_Id")
                .FromTable("TestTable2").InSchema("TestSchema").ForeignColumn("TestTableId")
                .ToTable("TestTable").InSchema("TestSchema").PrimaryColumn("Id");

            Insert.IntoTable("TestTable").InSchema("TestSchema").Row(new { Name = "Test" });
        }

        public override void Down()
        {
            Delete.Table("TestTable2").InSchema("TestSchema");
            Delete.Table("TestTable").InSchema("TestSchema");
        }
    }

    internal class TestRenameTableMigrationWithSchema : AutoReversingMigration
    {
        public override void Up()
        {
            Rename.Table("TestTable2").InSchema("TestSchema").To("TestTable'3");
        }
    }

    internal class TestRenameColumnMigrationWithSchema : AutoReversingMigration
    {
        public override void Up()
        {
            Rename.Column("Name").OnTable("TestTable2").InSchema("TestSchema").To("Name'3");
        }
    }

    internal class TestCreateAndDropIndexMigrationWithSchema : Migration
    {
        public override void Up()
        {
            Create.Index("IX_TestTable_Name").OnTable("TestTable").InSchema("TestSchema").OnColumn("Name");
        }

        public override void Down()
        {
            Delete.Index("IX_TestTable_Name").OnTable("TestTable").InSchema("TestSchema");
        }
    }


}