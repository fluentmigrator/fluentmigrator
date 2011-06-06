using System.Data.SqlServerCe;
using System.IO;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Initialization;
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
        SqlServerCe4Processor processor;
        SqlCeConnection connection;

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
            
            connection = new SqlCeConnection(ConnectionString);
            var debugAnnouncer = new ConsoleAnnouncer{ShowSql = true};
            processor = new SqlServerCe4Processor(connection, debugAnnouncer);
            runner = new MigrationRunner(new RunnerContext(debugAnnouncer),  processor);
            
        }

        [TearDown]
        public void Teardown()
        {
            processor.CommitTransaction();
            connection.Close();
            connection.Dispose();
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
            processor.TableExists(null, "TestTable1").ShouldBe(true);
            
            runner.Down(migration);
            processor.TableExists(null, "TestTable1").ShouldBe(false);
        }

        public class CanCreateTableWithCustomColumnTypeMigration : Migration
        {
            public override void Up()
            {
                Create.Table("TestTable1")
                    .WithColumn("TestColumn1").AsString()
                    .WithColumn("TestColumn2").AsCustom("[timestamp]").NotNullable();
            }

            public override void Down()
            {
                Delete.Table("TestTable1");
            }
        }

        [Test]
        public override void CanCreateTableWithCustomColumnType()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "[timestamp]";

            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] [timestamp] NOT NULL)");

            var migration = new CanCreateTableWithCustomColumnTypeMigration();
            runner.Up(migration);
            processor.TableExists(null, "TestTable1").ShouldBe(true);
            processor.ColumnExists(null, "TestTable1", "TestColumn2").ShouldBe(true);

            runner.Down(migration);
            processor.TableExists(null, "TestTable1").ShouldBe(false);
        }

        public class CanCreateTableWithPrimaryKeyMigration : Migration
        {
            public override void Up()
            {
                Create.Table("TestTable1")
                    .WithColumn("TestColumn1").AsString().PrimaryKey()
                    .WithColumn("TestColumn2").AsInt32().NotNullable();
            }

            public override void Down()
            {
                Delete.Table("TestTable1");
            }
        }

        [Test]
        public override void CanCreateTableWithPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL, PRIMARY KEY ([TestColumn1]))");

            var migration = new CanCreateTableWithPrimaryKeyMigration();
            runner.Up(migration);
            processor.TableExists(null, "TestTable1").ShouldBe(true);
            processor.ConstraintExists(null, "TestTable1", "PK_TestTable1").ShouldBe(true);

            runner.Down(migration);
            processor.TableExists(null, "TestTable1").ShouldBe(false);

        }

        public class CanCreateTableNamedPrimaryKeyMigration : Migration
        {
            public override void Up()
            {
                Create.Table("TestTable1")
                    .WithColumn("TestColumn1").AsString().PrimaryKey("TestKey")
                    .WithColumn("TestColumn2").AsInt32().NotNullable();
            }

            public override void Down()
            {
                Delete.Table("TestTable1");
            }
        }

        [Test]
        public override void CanCreateTableNamedPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL, CONSTRAINT [TestKey] PRIMARY KEY ([TestColumn1]))");

            var migration = new CanCreateTableNamedPrimaryKeyMigration();
            runner.Up(migration);
            processor.TableExists(null, "TestTable1").ShouldBe(true);
            processor.ConstraintExists(null, "TestTable1", "TestKey").ShouldBe(true);

            runner.Down(migration);
            processor.TableExists(null, "TestTable1").ShouldBe(false);

        }

        public class CanCreateTableNamedMultiColumnPrimaryKeyMigration : Migration
        {
            public override void Up()
            {
                Create.Table("TestTable1")
                    .WithColumn("TestColumn1").AsString().PrimaryKey("TestKey")
                    .WithColumn("TestColumn2").AsInt32().PrimaryKey("TestKey");
            }

            public override void Down()
            {
                Delete.Table("TestTable1");
            }
        }

        [Test]
        public override void CanCreateTableNamedMultiColumnPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumNamedPrimaryKeyExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE [TestTable1] ([TestColumn1] NVARCHAR(255) NOT NULL, [TestColumn2] INT NOT NULL, CONSTRAINT [TestKey] PRIMARY KEY ([TestColumn1], [TestColumn2]))");

            var migration = new CanCreateTableNamedMultiColumnPrimaryKeyMigration();
            runner.Up(migration);
            processor.TableExists(null, "TestTable1").ShouldBe(true);
            processor.IndexExists(null, "TestTable1", "TestKey", "TestColumn1").ShouldBe(true);
            processor.IndexExists(null, "TestTable1", "TestKey", "TestColumn2").ShouldBe(true);

            runner.Down(migration);
            processor.TableExists(null, "TestTable1").ShouldBe(false);

        }

        public class CanCreateTableWithIdentityMigration : Migration
        {
            public override void Up()
            {
                Create.Table("TestTable1")
                    .WithColumn("TestColumn1").AsInt32().Identity()
                    .WithColumn("TestColumn2").AsInt32();
            }

            public override void Down()
            {
                Delete.Table("TestTable1");
            }
        }

        [Test]
        public override void CanCreateTableWithIdentity()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE [TestTable1] ([TestColumn1] INT NOT NULL IDENTITY(1,1), [TestColumn2] INT NOT NULL)");

            var migration = new CanCreateTableWithIdentityMigration();
            runner.Up(migration);
            processor.TableExists(null, "TestTable1").ShouldBe(true);

            runner.Down(migration);
            processor.TableExists(null, "TestTable1").ShouldBe(false);

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