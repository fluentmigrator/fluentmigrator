using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Should;
using System.Data;
using FluentMigrator.Model;
using FluentMigrator.Expressions;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer
{
    public class SqlServer2000AlterTableTests : BaseTableAlterTests
    {
        protected SqlServer2000Generator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqlServer2000Generator();


        }

        [Test]
        public override void CanAddColumn()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] ADD [TestColumn1] NVARCHAR(5) NOT NULL");
        }

        [Test]
        public override void CanAddDecimalColumn()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] ADD [TestColumn1] DECIMAL(19,2) NOT NULL");
        }

        [Test]
        public override void CanRenameColumn()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("sp_rename '[Schema1].[TestTable1].[TestColumn1]', 'TestColumn2'");
        }

        [Test]
        public override void CanRenameTable()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("sp_rename '[TestTable1]', 'TestTable2'");
        }

        [Test]
        public override void CanAlterColumn()
        {
            throw new NotImplementedException();
        }

        [Test]
        public override void CanCreateForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE [TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([TestColumn1]) REFERENCES [TestTable2] ([TestColumn1])");

        }

        [Test]
        public override void CanCreateMulitColumnForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE [TestTable1] ADD CONSTRAINT [FK_Test] FOREIGN KEY ([Column3],[Column4]) REFERENCES [TestPrimaryTable] ([Column1],[Column2])");

        }

        [Test]
        public void CanCreateXmlColumn()
        {
            var expression = new CreateColumnExpression();
            expression.TableName = "Table1";

            expression.Column = new ColumnDefinition();
            expression.Column.Name = "MyXmlColumn";
            expression.Column.Type = DbType.Xml;

            var sql = generator.Generate(expression);
            sql.ShouldNotBeNull();
        }

        public override void CanCreateAutoIncrementColumn()
        {
            throw new NotImplementedException();
        }
    }
}
