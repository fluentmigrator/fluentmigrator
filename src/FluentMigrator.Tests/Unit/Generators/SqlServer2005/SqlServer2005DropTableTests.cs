using System;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

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

            string expected = "DECLARE @default sysname, @sql nvarchar(max);" + Environment.NewLine + Environment.NewLine +
                        "-- get name of default constraint" + Environment.NewLine +
                        "SELECT @default = name" + Environment.NewLine +
                        "FROM sys.default_constraints" + Environment.NewLine +
                        "WHERE parent_object_id = object_id('[dbo].[TestTable1]')" + Environment.NewLine +
                        "AND type = 'D'" + Environment.NewLine +
                        "AND parent_column_id = (" + Environment.NewLine +
                        "SELECT column_id" + Environment.NewLine +
                        "FROM sys.columns" + Environment.NewLine +
                        "WHERE object_id = object_id('[dbo].[TestTable1]')" + Environment.NewLine +
                        "AND name = 'TestColumn1'" + Environment.NewLine +
                        ");" + Environment.NewLine + Environment.NewLine +
                        "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
                        "SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + @default;" + Environment.NewLine +
                        "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
                        "-- now we can finally drop column" + Environment.NewLine +
                        "ALTER TABLE [dbo].[TestTable1] DROP COLUMN [TestColumn1];" + Environment.NewLine;

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanDropMultipleColumnsWithDefaultSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new string[] {"TestColumn1", "TestColumn2"});

            var sql = generator.Generate(expression);

            string expected = "DECLARE @default sysname, @sql nvarchar(max);" + Environment.NewLine + Environment.NewLine +
                        "-- get name of default constraint" + Environment.NewLine +
                        "SELECT @default = name" + Environment.NewLine +
                        "FROM sys.default_constraints" + Environment.NewLine +
                        "WHERE parent_object_id = object_id('[dbo].[TestTable1]')" + Environment.NewLine + "" +
                        "AND type = 'D'" + Environment.NewLine +
                        "AND parent_column_id = (" + Environment.NewLine +
                        "SELECT column_id" + Environment.NewLine +
                        "FROM sys.columns" + Environment.NewLine +
                        "WHERE object_id = object_id('[dbo].[TestTable1]')" + Environment.NewLine +
                        "AND name = 'TestColumn1'" + Environment.NewLine +
                        ");" + Environment.NewLine + Environment.NewLine +
                        "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
                        "SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + @default;" + Environment.NewLine +
                        "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
                        "-- now we can finally drop column" + Environment.NewLine +
                        "ALTER TABLE [dbo].[TestTable1] DROP COLUMN [TestColumn1];" + Environment.NewLine +
                        "GO" + Environment.NewLine +
                        "DECLARE @default sysname, @sql nvarchar(max);" + Environment.NewLine + Environment.NewLine +
                        "-- get name of default constraint" + Environment.NewLine +
                        "SELECT @default = name" + Environment.NewLine +
                        "FROM sys.default_constraints" + Environment.NewLine +
                        "WHERE parent_object_id = object_id('[dbo].[TestTable1]')" + Environment.NewLine + "" +
                        "AND type = 'D'" + Environment.NewLine +
                        "AND parent_column_id = (" + Environment.NewLine +
                        "SELECT column_id" + Environment.NewLine +
                        "FROM sys.columns" + Environment.NewLine +
                        "WHERE object_id = object_id('[dbo].[TestTable1]')" + Environment.NewLine +
                        "AND name = 'TestColumn2'" + Environment.NewLine +
                        ");" + Environment.NewLine + Environment.NewLine +
                        "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
                        "SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + @default;" + Environment.NewLine +
                        "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
                        "-- now we can finally drop column" + Environment.NewLine +
                        "ALTER TABLE [dbo].[TestTable1] DROP COLUMN [TestColumn2];" + Environment.NewLine;

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

            string expected = "DECLARE @default sysname, @sql nvarchar(max);" + Environment.NewLine + Environment.NewLine +
                         "-- get name of default constraint" + Environment.NewLine +
                         "SELECT @default = name" + Environment.NewLine +
                         "FROM sys.default_constraints" + Environment.NewLine +
                         "WHERE parent_object_id = object_id('[TestSchema].[TestTable1]')" + Environment.NewLine +
                         "AND type = 'D'" + Environment.NewLine +
                         "AND parent_column_id = (" + Environment.NewLine +
                         "SELECT column_id" + Environment.NewLine +
                         "FROM sys.columns" + Environment.NewLine +
                         "WHERE object_id = object_id('[TestSchema].[TestTable1]')" + Environment.NewLine +
                         "AND name = 'TestColumn1'" + Environment.NewLine +
                         ");" + Environment.NewLine + Environment.NewLine +
                         "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
                         "SET @sql = N'ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT ' + @default;" + Environment.NewLine +
                         "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
                         "-- now we can finally drop column" + Environment.NewLine +
                         "ALTER TABLE [TestSchema].[TestTable1] DROP COLUMN [TestColumn1];" + Environment.NewLine;

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanDropMultipleColumnsWithCustomSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new string[] {"TestColumn1", "TestColumn2"});
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);

            string expected = "DECLARE @default sysname, @sql nvarchar(max);" + Environment.NewLine + Environment.NewLine +
                         "-- get name of default constraint" + Environment.NewLine +
                         "SELECT @default = name" + Environment.NewLine +
                         "FROM sys.default_constraints" + Environment.NewLine +
                         "WHERE parent_object_id = object_id('[TestSchema].[TestTable1]')" + Environment.NewLine +
                         "AND type = 'D'" + Environment.NewLine +
                         "AND parent_column_id = (" + Environment.NewLine +
                         "SELECT column_id" + Environment.NewLine +
                         "FROM sys.columns" + Environment.NewLine +
                         "WHERE object_id = object_id('[TestSchema].[TestTable1]')" + Environment.NewLine +
                         "AND name = 'TestColumn1'" + Environment.NewLine +
                         ");" + Environment.NewLine + Environment.NewLine +
                         "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
                         "SET @sql = N'ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT ' + @default;" + Environment.NewLine +
                         "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
                         "-- now we can finally drop column" + Environment.NewLine +
                         "ALTER TABLE [TestSchema].[TestTable1] DROP COLUMN [TestColumn1];" + Environment.NewLine +
                         "GO" + Environment.NewLine +
                         "DECLARE @default sysname, @sql nvarchar(max);" + Environment.NewLine + Environment.NewLine +
                         "-- get name of default constraint" + Environment.NewLine +
                         "SELECT @default = name" + Environment.NewLine +
                         "FROM sys.default_constraints" + Environment.NewLine +
                         "WHERE parent_object_id = object_id('[TestSchema].[TestTable1]')" + Environment.NewLine +
                         "AND type = 'D'" + Environment.NewLine +
                         "AND parent_column_id = (" + Environment.NewLine +
                         "SELECT column_id" + Environment.NewLine +
                         "FROM sys.columns" + Environment.NewLine +
                         "WHERE object_id = object_id('[TestSchema].[TestTable1]')" + Environment.NewLine +
                         "AND name = 'TestColumn2'" + Environment.NewLine +
                         ");" + Environment.NewLine + Environment.NewLine +
                         "-- create alter table command to drop constraint as string and run it" + Environment.NewLine +
                         "SET @sql = N'ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT ' + @default;" + Environment.NewLine +
                         "EXEC sp_executesql @sql;" + Environment.NewLine + Environment.NewLine +
                         "-- now we can finally drop column" + Environment.NewLine +
                         "ALTER TABLE [TestSchema].[TestTable1] DROP COLUMN [TestColumn2];" + Environment.NewLine;

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
