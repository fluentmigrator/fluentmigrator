using System.Data.SqlClient;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
using Xunit;

namespace FluentMigrator.Tests.Integration
{
    public class MigrationRunnerTests
    {
        [Fact]
        public void CanRunMigration()
        {
            string connectionString = @"server=(local);uid=;pwd=;Trusted_Connection=yes;database=FluentMigrator";
            var conventions = new MigrationConventions();
            var connection = new SqlConnection(connectionString);
            connection.Open();
            var processor = new SqlServerProcessor(connection, new SqlServerGenerator());
            var runner = new MigrationRunner(conventions, processor);

            runner.Up(new TestCreateAndDropTableMigration());
            Assert.True(processor.TableExists("TestTable"));
            runner.Down(new TestCreateAndDropTableMigration());
            Assert.False(processor.TableExists("TestTable"));
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