using System.Data.SqlServerCe;
using System.IO;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServerCe4
{
    public class SqlServerCe4CreateTableTests : BaseTableCreateTests
    {
        const string DbFile = "Ce4Test.sdf";
        const string ConnectionString = "Data Source=" + DbFile;
        SqlServerCe4Generator generator;
        MigrationRunner runner;

        [SetUp]
        public void Setup()
        {
            generator = new SqlServerCe4Generator();
            if(File.Exists(DbFile))
                File.Delete(DbFile);
            using (var engine = new SqlCeEngine(ConnectionString))
            {
                engine.CreateDatabase();
            }
            
            var connection = new SqlCeConnection(ConnectionString);
            var processor = new SqlServerCe4Processor(connection);
            runner = new MigrationRunner(processor);
        }

        public class CanCreateTableMigration : Migration
        {
            public override void Up()
            {
                Create.Table("TestTable1").WithColumn("TestColumn1").AsString();
            }

            public override void Down()
            {
                Delete.Table("TestTable1");
            }
        }

        [Test]
        public override void CanCreateTable()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL)");

            var migration = new CanCreateTableMigration();
            runner.Up(migration);
            runner.Down(migration);
        }

        public override void CanCreateTableWithCustomColumnType()
        {
            throw new System.NotImplementedException();
        }

        public override void CanCreateTableWithPrimaryKey()
        {
            throw new System.NotImplementedException();
        }

        public override void CanCreateTableNamedPrimaryKey()
        {
            throw new System.NotImplementedException();
        }

        public override void CanCreateTableNamedMultiColumnPrimaryKey()
        {
            throw new System.NotImplementedException();
        }

        public override void CanCreateTableWithIdentity()
        {
            throw new System.NotImplementedException();
        }

        public override void CanCreateTableWithNullableField()
        {
            throw new System.NotImplementedException();
        }

        public override void CanCreateTableWithDefaultValue()
        {
            throw new System.NotImplementedException();
        }

        public override void CanCreateTableWithDefaultValueExplicitlySetToNull()
        {
            throw new System.NotImplementedException();
        }

        public override void CanCreateTableWithMultiColumnPrimaryKey()
        {
            throw new System.NotImplementedException();
        }

        public override void CanCreateIndex()
        {
            throw new System.NotImplementedException();
        }

        public override void CanCreateMultiColumnIndex()
        {
            throw new System.NotImplementedException();
        }

        public override void CanCreateUniqueIndex()
        {
            throw new System.NotImplementedException();
        }

        public override void CanCreateMultiColumnUniqueIndex()
        {
            throw new System.NotImplementedException();
        }

        public override void CanCreateSchema()
        {
            throw new System.NotImplementedException();
        }
    }

}