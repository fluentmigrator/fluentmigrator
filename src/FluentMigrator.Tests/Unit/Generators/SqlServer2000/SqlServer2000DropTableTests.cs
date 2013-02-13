using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer
{
    public class SqlServer2000DropTableTests : BaseTableDropTests
    {
        private SqlServer2000Generator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new SqlServer2000Generator();
        }

        [Test]
        public override void CanDropColumn()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            var sql = _generator.Generate(expression);

            const string expected = "DECLARE @default sysname, @sql nvarchar(4000);\r\n\r\n" +
                        "-- get name of default constraint\r\n" +
                        "SELECT @default = name\r\n" +
                        "FROM sys.default_constraints\r\n" +
                        "WHERE parent_object_id = object_id('[TestTable1]')\r\n" + "" +
                        "AND type = 'D'\r\n" + "" +
                        "AND parent_column_id = (\r\n" + "" +
                        "SELECT column_id\r\n" +
                        "FROM sys.columns\r\n" +
                        "WHERE object_id = object_id('[TestTable1]')\r\n" +
                        "AND name = 'TestColumn1'\r\n" +
                        ");\r\n\r\n" +
                        "-- create alter table command to drop contraint as string and run it\r\n" +
                        "SET @sql = N'ALTER TABLE [TestTable1] DROP CONSTRAINT ' + @default;\r\n" +
                        "EXEC sp_executesql @sql;\r\n\r\n" +
                        "-- now we can finally drop column\r\n" +
                        "ALTER TABLE [TestTable1] DROP COLUMN [TestColumn1];\r\n";

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanDropMultipleColumns()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new [] {"TestColumn1", "TestColumn2"});
            var sql = _generator.Generate(expression);

            const string expected = "DECLARE @default sysname, @sql nvarchar(4000);\r\n\r\n" +
                        "-- get name of default constraint\r\n" +
                        "SELECT @default = name\r\n" +
                        "FROM sys.default_constraints\r\n" +
                        "WHERE parent_object_id = object_id('[TestTable1]')\r\n" + "" +
                        "AND type = 'D'\r\n" + "" +
                        "AND parent_column_id = (\r\n" + "" +
                        "SELECT column_id\r\n" +
                        "FROM sys.columns\r\n" +
                        "WHERE object_id = object_id('[TestTable1]')\r\n" +
                        "AND name = 'TestColumn1'\r\n" +
                        ");\r\n\r\n" +
                        "-- create alter table command to drop contraint as string and run it\r\n" +
                        "SET @sql = N'ALTER TABLE [TestTable1] DROP CONSTRAINT ' + @default;\r\n" +
                        "EXEC sp_executesql @sql;\r\n\r\n" +
                        "-- now we can finally drop column\r\n" +
                        "ALTER TABLE [TestTable1] DROP COLUMN [TestColumn1];\r\n" +
                        "GO\r\n" +
                        "DECLARE @default sysname, @sql nvarchar(4000);\r\n\r\n" +
                        "-- get name of default constraint\r\n" +
                        "SELECT @default = name\r\n" +
                        "FROM sys.default_constraints\r\n" +
                        "WHERE parent_object_id = object_id('[TestTable1]')\r\n" + "" +
                        "AND type = 'D'\r\n" + "" +
                        "AND parent_column_id = (\r\n" + "" +
                        "SELECT column_id\r\n" +
                        "FROM sys.columns\r\n" +
                        "WHERE object_id = object_id('[TestTable1]')\r\n" +
                        "AND name = 'TestColumn2'\r\n" +
                        ");\r\n\r\n" +
                        "-- create alter table command to drop contraint as string and run it\r\n" +
                        "SET @sql = N'ALTER TABLE [TestTable1] DROP CONSTRAINT ' + @default;\r\n" +
                        "EXEC sp_executesql @sql;\r\n\r\n" +
                        "-- now we can finally drop column\r\n" +
                        "ALTER TABLE [TestTable1] DROP COLUMN [TestColumn2];\r\n";

            sql.ShouldBe(expected);
        }

        [Test]
        public override void CanDropForeignKey()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] DROP CONSTRAINT [FK_Test]");
        }

        [Test]
        public override void CanDropTable()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();

            var sql = _generator.Generate(expression);
            sql.ShouldBe("DROP TABLE [TestTable1]");
        }

        [Test]
        public override void CanDeleteIndex()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();

            var sql = _generator.Generate(expression);
            sql.ShouldBe("DROP INDEX [TestTable1].[TestIndex]");
        }

        [Test]
        public override void CanDeleteSchema()
        {
            var expression = new DeleteSchemaExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanDeleteSchemaInStrictMode()
        {
            _generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(new DeleteSchemaExpression()));
        }
    }
}