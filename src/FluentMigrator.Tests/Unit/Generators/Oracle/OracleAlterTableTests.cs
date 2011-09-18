﻿using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Oracle;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    public class OracleAlterTableTests : BaseTableAlterTests
    {
        private OracleGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new OracleGenerator();
        }

        [Test]
        public override void CanAddColumn()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 NVARCHAR2(5) NOT NULL");
        }

        [Test]
        public override void CanAddDecimalColumn()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE TestTable1 ADD TestColumn1 NUMBER(19,2) NOT NULL");
        }

        [Test]
        public override void CanRenameColumn()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE TestTable1 RENAME COLUMN TestColumn1 TO TestColumn2");
        }

        [Test]
        public override void CanRenameTable()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE TestTable1 RENAME TO TestTable2");
        }

        [Test]
        public override void CanAlterColumn()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE TestTable1 MODIFY TestColumn1 NVARCHAR2(20) NOT NULL");
        }

        [Test]
        public override void CanCreateForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT FK_Test FOREIGN KEY (TestColumn1) REFERENCES TestTable2 (TestColumn2)");
        }

        [Test]
        public override void CanCreateMulitColumnForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE TestTable1 ADD CONSTRAINT FK_Test FOREIGN KEY (TestColumn1, TestColumn3) REFERENCES TestTable2 (TestColumn2, TestColumn4)");
        }

        [Test]
        public override void CanCreateAutoIncrementColumn()
        {
        }

        [Test]
        public override void CanAlterSchema()
        {
            var expression = new AlterSchemaExpression();
            var result = generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanAlterSchemaInStrictMode()
        {
            generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => generator.Generate(new CreateSchemaExpression()));
        }
    }
}