using System;
using System.Data.SqlClient;
using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors.Sqlite;
using FluentMigrator.Runner.Processors.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration
{
	[TestFixture]
	public class MigrationRunnerTests
	{
		[Test]
		public void CanRunMigration()
		{
			string connectionString = @"server=(local)\sqlexpress;uid=;pwd=;Trusted_Connection=yes;database=FluentMigrator";
			var conventions = new MigrationConventions();
			var connection = new SqlConnection(connectionString);
			connection.Open();
			
			var processor = new SqlServerProcessor(connection, new SqlServerGenerator());
			var runner = new MigrationRunner(conventions, processor);

			runner.Up(new TestCreateAndDropTableMigration());
			processor.TableExists("TestTable").ShouldBeTrue();
			
			runner.Down(new TestCreateAndDropTableMigration());
			processor.TableExists("TestTable").ShouldBeFalse();
		}

		[Test, Ignore("failing becacase of assertion on line 45")]
		public void CanSilentlyFail()
		{
			//need to run a known failure against sqlite and make sure it executes but only captures the exception
			var connection = new System.Data.SQLite.SQLiteConnection { ConnectionString = "Data Source=:memory:;Version=3;New=True;" };
			connection.Open();
			var conventions = new MigrationConventions();
			var processor = new SqliteProcessor(connection, new SqliteGenerator());
			var runner = new MigrationRunner(conventions, processor) { SilentlyFail = true };

			runner.Up(new TestForeignKeySilentFailure());
			runner.CaughtExceptions.Count.ShouldBeGreaterThan(0);

			runner.Down(new TestForeignKeySilentFailure());
			runner.CaughtExceptions.Count.ShouldBeGreaterThan(0);
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