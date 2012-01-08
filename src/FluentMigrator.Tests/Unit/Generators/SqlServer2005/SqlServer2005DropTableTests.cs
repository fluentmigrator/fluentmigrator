using NUnit.Framework;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Should;
using FluentMigrator.Expressions;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer
{

    public class SqlServer2005DropTableTests : GeneratorTestBase
    {
        protected SqlServer2005Generator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqlServer2005Generator();
        }

        [Test]
        public void CanDropColumnWithDefaultSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();

            var sql = generator.Generate(expression);

            const string expected = "DECLARE @default sysname, @sql nvarchar(max);\r\n\r\n" +
                        "-- get name of default constraint\r\n" +
                        "SELECT @default = name\r\n" +
                        "FROM sys.default_constraints\r\n" +
                        "WHERE parent_object_id = object_id('[dbo].[TestTable1]')\r\n" + "" +
                        "AND type = 'D'\r\n" + "" +
                        "AND parent_column_id = (\r\n" + "" +
                        "SELECT column_id\r\n" +
                        "FROM sys.columns\r\n" +
                        "WHERE object_id = object_id('[dbo].[TestTable1]')\r\n" +
                        "AND name = 'TestColumn1'\r\n" +
                        ");\r\n\r\n" +
                        "-- create alter table command to drop contraint as string and run it\r\n" +
                        "SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + @default;\r\n" +
                        "EXEC sp_executesql @sql;\r\n\r\n" +
                        "-- now we can finally drop column\r\n" +
                        "ALTER TABLE [dbo].[TestTable1] DROP COLUMN [TestColumn1];";

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanDropForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT [FK_Test]");
        }

        [Test]
        public void CanDropTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP TABLE [dbo].[TestTable1]");
        }

        [Test]
        public void CanDeleteIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP INDEX [TestIndex] ON [dbo].[TestTable1]");
        }

        [Test]
        public void CanDropColumnWithCustomSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);

            const string expected = "DECLARE @default sysname, @sql nvarchar(max);\r\n\r\n" +
                         "-- get name of default constraint\r\n" +
                         "SELECT @default = name\r\n" +
                         "FROM sys.default_constraints\r\n" +
                         "WHERE parent_object_id = object_id('[TestSchema].[TestTable1]')\r\n" + "" +
                         "AND type = 'D'\r\n" + "" +
                         "AND parent_column_id = (\r\n" + "" +
                         "SELECT column_id\r\n" +
                         "FROM sys.columns\r\n" +
                         "WHERE object_id = object_id('[TestSchema].[TestTable1]')\r\n" +
                         "AND name = 'TestColumn1'\r\n" +
                         ");\r\n\r\n" +
                         "-- create alter table command to drop contraint as string and run it\r\n" +
                         "SET @sql = N'ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT ' + @default;\r\n" +
                         "EXEC sp_executesql @sql;\r\n\r\n" +
                         "-- now we can finally drop column\r\n" +
                         "ALTER TABLE [TestSchema].[TestTable1] DROP COLUMN [TestColumn1];";

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanDropForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT [FK_Test]");
        }

        [Test]
        public void CanDropTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);

            sql.ShouldBe("DROP TABLE [TestSchema].[TestTable1]");
        }

        [Test]
        public void CanDeleteIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            expression.Index.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);

            sql.ShouldBe("DROP INDEX [TestIndex] ON [TestSchema].[TestTable1]");
        }



        [Test]
        public void CanDeleteSchema()
        {
            var expression = new DeleteSchemaExpression() { SchemaName = "TestSchema" };
            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP SCHEMA [TestSchema]");
        }
    }
}
