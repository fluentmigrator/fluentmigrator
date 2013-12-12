﻿using System;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2005
{
    [TestFixture]
    public class SqlServer2005ColumnTests
    {
        protected SqlServer2005Generator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqlServer2005Generator();
        }

        [Test]
        public void CanAlterColumnWithCustomSchema()
        {
            //TODO: This will fail if there are any keys attached 
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.SchemaName = "TestSchema";

            var sql = generator.Generate(expression);

            sql.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ALTER COLUMN [TestColumn1] NVARCHAR(20) NOT NULL");
        }

        [Test]
        public void CanAlterColumnWithDefaultSchema()
        {
            //TODO: This will fail if there are any keys attached 
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            var sql = generator.Generate(expression);

            sql.ShouldBe("ALTER TABLE [dbo].[TestTable1] ALTER COLUMN [TestColumn1] NVARCHAR(20) NOT NULL");
        }

        public void CanCreateAutoIncrementColumnWithCustomSchema()
        {
            throw new NotImplementedException();
        }

        public void CanCreateAutoIncrementColumnWithDefaultSchema()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void CanCreateColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD [TestColumn1] NVARCHAR(5) NOT NULL");
        }

        [Test]
        public void CanCreateColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD [TestColumn1] NVARCHAR(5) NOT NULL");
        }

        [Test]
        public void CanCreateDecimalColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD [TestColumn1] DECIMAL(19,2) NOT NULL");
        }

        [Test]
        public void CanCreateDecimalColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD [TestColumn1] DECIMAL(19,2) NOT NULL");
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
        public void CanDropMultipleColumnsWithCustomSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new string[] { "TestColumn1", "TestColumn2" });
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
        public void CanDropMultipleColumnsWithDefaultSchema()
        {
            //This does not work if it is a primary key
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new string[] { "TestColumn1", "TestColumn2" });

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
        public void CanRenameColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe("sp_rename '[TestSchema].[TestTable1].[TestColumn1]', 'TestColumn2'");
        }

        [Test]
        public void CanRenameColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("sp_rename '[dbo].[TestTable1].[TestColumn1]', 'TestColumn2'");
        }
    }
}
