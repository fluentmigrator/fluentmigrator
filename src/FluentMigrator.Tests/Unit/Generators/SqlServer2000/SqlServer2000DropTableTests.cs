using System;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;
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

            string expected = "DECLARE @default sysname, @sql nvarchar(4000);" + Environment.NewLine + Environment.NewLine +
                        "-- get name of default constraint" + Environment.NewLine +
                        "SELECT @default = name" + Environment.NewLine +
                        "FROM sys.default_constraints" + Environment.NewLine +
                        "WHERE parent_object_id = object_id('[TestTable1]')" + Environment.NewLine +
                        "AND type = 'D'" + Environment.NewLine +
                        "AND parent_column_id = (" + Environment.NewLine +
                        "SELECT column_id" + Environment.NewLine +
                        "FROM sys.columns" + Environment.NewLine +
                        "WHERE object_id = object_id('[TestTable1]')" + Environment.NewLine +
                        "AND name = 'TestColumn1'" + Environment.NewLine +
                        ");" + Environment.NewLine + Environment.NewLine +
                        "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
                        "SET @sql = N'ALTER TABLE [TestTable1] DROP CONSTRAINT ' + @default;" + Environment.NewLine +
                        "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
                        "-- now we can finally drop column" + Environment.NewLine +
                        "ALTER TABLE [TestTable1] DROP COLUMN [TestColumn1];" + Environment.NewLine;

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanDropMultipleColumns()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new [] {"TestColumn1", "TestColumn2"});
            var sql = _generator.Generate(expression);

            string expected = "DECLARE @default sysname, @sql nvarchar(4000);" + Environment.NewLine + Environment.NewLine +
                        "-- get name of default constraint" + Environment.NewLine +
                        "SELECT @default = name" + Environment.NewLine +
                        "FROM sys.default_constraints" + Environment.NewLine +
                        "WHERE parent_object_id = object_id('[TestTable1]')" + Environment.NewLine + "" +
                        "AND type = 'D'" + Environment.NewLine +
                        "AND parent_column_id = (" + Environment.NewLine +
                        "SELECT column_id" + Environment.NewLine +
                        "FROM sys.columns" + Environment.NewLine +
                        "WHERE object_id = object_id('[TestTable1]')" + Environment.NewLine +
                        "AND name = 'TestColumn1'" + Environment.NewLine +
                        ");" + Environment.NewLine + Environment.NewLine +
                        "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
                        "SET @sql = N'ALTER TABLE [TestTable1] DROP CONSTRAINT ' + @default;" + Environment.NewLine +
                        "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
                        "-- now we can finally drop column" + Environment.NewLine +
                        "ALTER TABLE [TestTable1] DROP COLUMN [TestColumn1];" + Environment.NewLine +
                        "GO" + Environment.NewLine +
                        "DECLARE @default sysname, @sql nvarchar(4000);" + Environment.NewLine + Environment.NewLine +
                        "-- get name of default constraint" + Environment.NewLine +
                        "SELECT @default = name" + Environment.NewLine +
                        "FROM sys.default_constraints" + Environment.NewLine +
                        "WHERE parent_object_id = object_id('[TestTable1]')" + Environment.NewLine +
                        "AND type = 'D'" + Environment.NewLine +
                        "AND parent_column_id = (" + Environment.NewLine +
                        "SELECT column_id" + Environment.NewLine +
                        "FROM sys.columns" + Environment.NewLine +
                        "WHERE object_id = object_id('[TestTable1]')" + Environment.NewLine +
                        "AND name = 'TestColumn2'" + Environment.NewLine +
                        ");" + Environment.NewLine + Environment.NewLine +
                        "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
                        "SET @sql = N'ALTER TABLE [TestTable1] DROP CONSTRAINT ' + @default;" + Environment.NewLine +
                        "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
                        "-- now we can finally drop column" + Environment.NewLine +
                        "ALTER TABLE [TestTable1] DROP COLUMN [TestColumn2];" + Environment.NewLine;

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