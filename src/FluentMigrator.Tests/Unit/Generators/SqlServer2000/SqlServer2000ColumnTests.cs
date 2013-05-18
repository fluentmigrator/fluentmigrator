using System;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2000
{
    [TestFixture]
    public class SqlServer2000ColumnTests
    {
        protected SqlServer2000Generator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new SqlServer2000Generator();
        }

        [Test]
        public void CanAlterColumnWithDefaultSchema()
        {
            //TODO: This will fail if there are any keys attached 
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            var sql = _generator.Generate(expression);

            sql.ShouldBe("ALTER TABLE [TestTable1] ALTER COLUMN [TestColumn1] NVARCHAR(20) NOT NULL");
        }

        public void CanCreateAutoIncrementColumnWithDefaultSchema()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void CanCreateColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] ADD [TestColumn1] NVARCHAR(5) NOT NULL");
        }

        [Test]
        public void CanCreateDecimalColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] ADD [TestColumn1] DECIMAL(19,2) NOT NULL");
        }

        [Test]
        public void CanDropColumnWithDefaultSchema()
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
        public void CanDropMultipleColumnsWithDefaultSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "TestColumn1", "TestColumn2" });
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
        public void CanRenameColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("sp_rename '[TestTable1].[TestColumn1]', 'TestColumn2'");
        }
    }
}
