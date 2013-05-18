using System;
using FluentMigrator.Runner.Generators.Oracle;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.OracleWithQuotedIdentifier
{
    [TestFixture]
    public class OracleColumnTests
    {
        protected OracleGenerator _generator;
        protected OracleGenerator generator;

        [SetUp]
        public void Setup()
        {
            _generator = new OracleGenerator(useQuotedIdentifiers: true);
            generator = new OracleGenerator(useQuotedIdentifiers: true);
        }

        [Test]
        public void CanAlterColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"TestTable1\" MODIFY \"TestColumn1\" NVARCHAR2(20) NOT NULL");
        }

        [Test]
        public void CanCreateAutoIncrementColumnWithDefaultSchema()
        {
        }

        [Test]
        public void CanCreateColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"TestTable1\" ADD \"TestColumn1\" NVARCHAR2(5) NOT NULL");
        }

        [Test]
        public void CanCreateDecimalColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"TestTable1\" ADD \"TestColumn1\" NUMBER(19,2) NOT NULL");
        }

        [Test]
        public void CanDropColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"TestTable1\" DROP COLUMN \"TestColumn1\"");
        }

        [Test]
        public void CanDropMultipleColumnsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new string[] { "\"TestColumn1\"", "\"TestColumn2\"" });
            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"TestTable1\" DROP COLUMN \"TestColumn1\";" + Environment.NewLine + "ALTER TABLE \"TestTable1\" DROP COLUMN \"TestColumn2\"");
        }

        [Test]
        public void CanRenameColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"TestTable1\" RENAME COLUMN \"TestColumn1\" TO \"TestColumn2\"");
        }
    }
}