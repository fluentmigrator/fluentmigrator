using System;
using System.Data.SqlClient;
using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
using Moq;
using Xunit;
using Xunit.Extensions;

namespace FluentMigrator.Tests.Integration
{
    public class MigrationRunnerTests
    {
        [Theory]
        [Sqlite]
        [SqlServer]
        public void CanRunMigration(IMigrationProcessor processor)
        {
            var conventions = new MigrationConventions();
            var runner = new MigrationRunner(conventions, processor);

            runner.Up(new TestCreateAndDropTableMigration());
            Assert.True(processor.TableExists("TestTable"));
            runner.Down(new TestCreateAndDropTableMigration());
            Assert.False(processor.TableExists("TestTable"));
        }

        [Fact]
        public void CanSilentlyFail()
        {
            // sqlite processor now ignores foreign keys (so i can use foreign keys on sqlserver and but ignore them in sqlite)
            // mocked instead

            var processor = new Mock<IMigrationProcessor>();
            processor.Expect(x => x.Process(It.IsAny<CreateForeignKeyExpression>())).Throws(new Exception("Error"));
            processor.Expect(x => x.Process(It.IsAny<DeleteForeignKeyExpression>())).Throws(new Exception("Error"));

            var conventions = new MigrationConventions();
            var runner = new MigrationRunner(conventions, processor.Object);

            runner.SilentlyFail = true;

            runner.Up(new TestForeignKeySilentFailure());

            Assert.True(runner.CaughtExceptions.Count > 0);

            runner.Down(new TestForeignKeySilentFailure());

            Assert.True(runner.CaughtExceptions.Count > 0);
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

    internal class TestCreateAndDropTableMigration: Migration
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