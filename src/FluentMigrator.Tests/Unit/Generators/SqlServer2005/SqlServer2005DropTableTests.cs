using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Should;
using FluentMigrator.Runner.Generators;
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

            var expectedSql = "\r\n\t\t\tDECLARE @default sysname, @sql nvarchar(max);\r\n\r\n\t\t\t-- get name of default constraint\r\n\t\t\tSELECT @default = name\r\n\t\t\tFROM sys.default_constraints \r\n\t\t\tWHERE parent_object_id = object_id('[dbo].[TestTable1]')\r\n\t\t\tAND type = 'D'\r\n\t\t\tAND parent_column_id = (\r\n\t\t\t\tSELECT column_id \r\n\t\t\t\tFROM sys.columns \r\n\t\t\t\tWHERE object_id = object_id('[dbo].[TestTable1]')\r\n\t\t\t\tAND name = 'TestColumn1'\r\n\t\t\t);\r\n\r\n\t\t\t-- create alter table command as string and run it\r\n\t\t\tSET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + @default;\r\n\t\t\tEXEC sp_executesql @sql;\r\n\r\n\t\t\t-- now we can finally drop column\r\n\t\t\tALTER TABLE [dbo].[TestTable1] DROP COLUMN [TestColumn1];";

            sql.ShouldBe(expectedSql);
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

            var expectedSql = "\r\n\t\t\tDECLARE @default sysname, @sql nvarchar(max);\r\n\r\n\t\t\t-- get name of default constraint\r\n\t\t\tSELECT @default = name\r\n\t\t\tFROM sys.default_constraints \r\n\t\t\tWHERE parent_object_id = object_id('[TestSchema].[TestTable1]')\r\n\t\t\tAND type = 'D'\r\n\t\t\tAND parent_column_id = (\r\n\t\t\t\tSELECT column_id \r\n\t\t\t\tFROM sys.columns \r\n\t\t\t\tWHERE object_id = object_id('[TestSchema].[TestTable1]')\r\n\t\t\t\tAND name = 'TestColumn1'\r\n\t\t\t);\r\n\r\n\t\t\t-- create alter table command as string and run it\r\n\t\t\tSET @sql = N'ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT ' + @default;\r\n\t\t\tEXEC sp_executesql @sql;\r\n\r\n\t\t\t-- now we can finally drop column\r\n\t\t\tALTER TABLE [TestSchema].[TestTable1] DROP COLUMN [TestColumn1];";

            sql.ShouldBe(expectedSql);
        }

        [Test]
        public void CanDropForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT [FK_Test]");
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
