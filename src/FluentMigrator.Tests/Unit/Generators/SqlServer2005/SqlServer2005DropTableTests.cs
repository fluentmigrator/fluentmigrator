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

            var expectedSql =
                @"
			DECLARE @default sysname, @sql nvarchar(max);

			-- get name of default constraint
			SELECT @default = name
			FROM sys.default_constraints 
			WHERE parent_object_id = object_id('[dbo].[TestTable1]')
			AND type = 'D'
			AND parent_column_id = (
				SELECT column_id 
				FROM sys.columns 
				WHERE object_id = object_id('[dbo].[TestTable1]')
				AND name = 'TestColumn1'
			);

			-- create alter table command as string and run it
			SET @sql = N'ALTER TABLE [dbo].[TestTable1] DROP CONSTRAINT ' + @default;
			EXEC sp_executesql @sql;

			-- now we can finally drop column
			ALTER TABLE [dbo].[TestTable1] DROP COLUMN [TestColumn1];";

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

            var expectedSql =
                @"
			DECLARE @default sysname, @sql nvarchar(max);

			-- get name of default constraint
			SELECT @default = name
			FROM sys.default_constraints 
			WHERE parent_object_id = object_id('[TestSchema].[TestTable1]')
			AND type = 'D'
			AND parent_column_id = (
				SELECT column_id 
				FROM sys.columns 
				WHERE object_id = object_id('[TestSchema].[TestTable1]')
				AND name = 'TestColumn1'
			);

			-- create alter table command as string and run it
			SET @sql = N'ALTER TABLE [TestSchema].[TestTable1] DROP CONSTRAINT ' + @default;
			EXEC sp_executesql @sql;

			-- now we can finally drop column
			ALTER TABLE [TestSchema].[TestTable1] DROP COLUMN [TestColumn1];";

            sql.ShouldBe(expectedSql);
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
