﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Should;
using System.Data;
using FluentMigrator.Model;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer
{
    public class SqlServer2005AlterTableTests : GeneratorTestBase
    {
        protected SqlServer2005Generator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqlServer2005Generator();
        }

        [Test]
        public   void CanAddColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD [TestColumn1] NVARCHAR(5) NOT NULL");
        }

        [Test]
        public   void CanAddDecimalColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD [TestColumn1] DECIMAL(19,2) NOT NULL");
        }

        

        [Test]
        public   void CanRenameTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("sp_rename '[dbo].[TestTable1]', 'TestTable2'");
        }

        [Test]
        public   void CanRenameColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("sp_rename '[dbo].[TestTable1].[TestColumn1]', 'TestColumn2'");
        }

        [Test]
        public   void CanAlterColumnWithDefaultSchema()
        {
            //TODO: This will fail if there are any keys attached 
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            var sql = generator.Generate(expression);

            sql.ShouldBe("ALTER TABLE [dbo].[TestTable1] ALTER COLUMN [TestColumn1] NVARCHAR(20) NOT NULL");
        }

        [Test]
        public   void CanCreateForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1]) REFERENCES [dbo].[TestTable2] ([TestColumn2])");

        }

        [Test]
        public   void CanCreateMulitColumnForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1], [TestColumn3]) REFERENCES [dbo].[TestTable2] ([TestColumn2], [TestColumn4])");

        }

        [Test]
        public void CanCreateXmlColumnWithDefaultSchema()
        {
            var expression = new CreateColumnExpression();
            expression.TableName = "TestTable1";

            expression.Column = new ColumnDefinition();
            expression.Column.Name = "TestColumn1";
            expression.Column.Type = DbType.Xml;

            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD [TestColumn1] XML NOT NULL");
        }

        public   void CanCreateAutoIncrementColumnWithDefaultSchema()
        {
            throw new NotImplementedException();
        }

        [Test]
        public   void CanAddColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD [TestColumn1] NVARCHAR(5) NOT NULL");
        }

        [Test]
        public   void CanAddDecimalColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD [TestColumn1] DECIMAL(19,2) NOT NULL");
        }



        [Test]
        public   void CanRenameTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe("sp_rename '[TestSchema].[TestTable1]', 'TestTable2'");
        }

        [Test]
        public   void CanRenameColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe("sp_rename '[TestSchema].[TestTable1].[TestColumn1]', 'TestColumn2'");
        }

        [Test]
        public   void CanAlterColumnWithCustomSchema()
        {
            //TODO: This will fail if there are any keys attached 
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            expression.SchemaName = "TestSchema";

            var sql = generator.Generate(expression);

            sql.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ALTER COLUMN [TestColumn1] NVARCHAR(20) NOT NULL");
        }

        [Test]
        public   void CanCreateForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1]) REFERENCES [TestSchema].[TestTable2] ([TestColumn2])");

        }

        [Test]
        public   void CanCreateMulitColumnForeignKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            expression.ForeignKey.ForeignTableSchema = "TestSchema";
            expression.ForeignKey.PrimaryTableSchema = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1], [TestColumn3]) REFERENCES [TestSchema].[TestTable2] ([TestColumn2], [TestColumn4])");

        }

        [Test]
        public void CanCreateXmlColumnWithCustomSchema()
        {
            var expression = new CreateColumnExpression();
            expression.TableName = "TestTable1";

            expression.Column = new ColumnDefinition();
            expression.Column.Name = "TestColumn1";
            expression.Column.Type = DbType.Xml;
            expression.SchemaName = "TestSchema";

            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD [TestColumn1] XML NOT NULL");
        }

        public   void CanCreateAutoIncrementColumnWithCustomSchema()
        {
            throw new NotImplementedException();
        }

        [Test]
        public   void CanAlterSchema()
        {
            var expression = new AlterSchemaExpression
            {
                DestinationSchemaName = "DEST",
                SourceSchemaName = "SOURCE",
                TableName = "TABLE"
            };

            var sql = generator.Generate(expression);
            sql.ShouldBe(
              "ALTER SCHEMA [DEST] TRANSFER [SOURCE].[TABLE]");
        }

        
    }
}
