using System;
using System.Data.SqlClient;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors.SqlServer;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration
{
	[TestFixture]
	public class MigrationRunnerTests : IntegrationTestBase
	{
		[Test]
		public void CanRunMigration()
		{
			ExecuteWithSupportedProcessors(processor =>
				{
					var conventions = new MigrationConventions();

					var runner = new MigrationRunner(conventions, processor);

					runner.Up(new TestCreateAndDropTableMigration());
					processor.TableExists("TestTable").ShouldBeTrue();

					runner.Down(new TestCreateAndDropTableMigration());
					processor.TableExists("TestTable").ShouldBeFalse();
				});
		}

		[Test]
		public void CanSilentlyFail()
		{
			var processor = new Mock<IMigrationProcessor>();
			processor.Setup(x => x.Process(It.IsAny<CreateForeignKeyExpression>())).Throws(new Exception("Error"));
			processor.Setup(x => x.Process(It.IsAny<DeleteForeignKeyExpression>())).Throws(new Exception("Error"));

			var conventions = new MigrationConventions();
			
			var runner = new MigrationRunner(conventions, processor.Object) { SilentlyFail = true };

			runner.Up(new TestForeignKeySilentFailure());
			runner.CaughtExceptions.Count.ShouldBeGreaterThan(0);

			runner.Down(new TestForeignKeySilentFailure());
			runner.CaughtExceptions.Count.ShouldBeGreaterThan(0);
		}

      [Test]
      public void CanApplyForeignKeyConvention()
      {
         var connection = new SqlConnection(sqlServerConnectionString);
			connection.Open();
			var processor = new SqlServerProcessor(connection, new SqlServerGenerator());

         var conventions = new MigrationConventions();
         var runner = new MigrationRunner(conventions, processor);

         runner.Up(new TestForeignKeyNamingConvention());
         processor.TableExists("Users").ShouldBeTrue();
         processor.ConstraintExists( "Users", "FK_Users_GroupId_Groups_GroupId").ShouldBeTrue();

         runner.Down(new TestForeignKeyNamingConvention());
         processor.TableExists("Users").ShouldBeFalse();
      }

      [Test]
      public void CanApplyIndexConvention()
      {
         var connection = new SqlConnection(sqlServerConnectionString);
         connection.Open();
         var processor = new SqlServerProcessor(connection, new SqlServerGenerator());

         var conventions = new MigrationConventions();
         var runner = new MigrationRunner(conventions, processor);

         runner.Up(new TestIndexNamingConvention());
         processor.TableExists("Users").ShouldBeTrue();

         runner.Down(new TestIndexNamingConvention());
         processor.TableExists("Users").ShouldBeFalse();
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

         Create.Index().OnTable( "Users" ).OnColumn( "GroupId" ).Ascending();
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