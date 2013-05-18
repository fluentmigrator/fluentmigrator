using System;
using FluentMigrator.Runner.Generators.SQLite;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SQLite
{
    [TestFixture]
    public class SQLiteColumnTests
    {
        protected SqliteGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new SqliteGenerator();
        }

        [Test]
        public void CanAlterColumn()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanCreateAutoIncrementColumn()
        {
            var expression = GeneratorTestHelper.GetAlterTableAutoIncrementColumnExpression();
            expression.Column.IsPrimaryKey = true;

            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"TestTable1\" ADD COLUMN \"TestColumn1\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT");
        }

        [Test]
        public void CanAddColumn()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"TestTable1\" ADD COLUMN \"TestColumn1\" TEXT NOT NULL");
        }

        [Test]
        public void CanAddDecimalColumn()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE \"TestTable1\" ADD COLUMN \"TestColumn1\" NUMERIC NOT NULL");
        }

        [Test]
        public void CanDropColumn()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe(String.Empty); //because sqlite doesnt support removing columns
        }

        [Test]
        public void CanDropMultipleColumns()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new string[] { "TestColumn1", "TestColumn2" });
            string sql = _generator.Generate(expression);
            sql.ShouldBe(String.Empty); //because sqlite doesnt support removing columns
        }

        [Test]
        public void CanRenameColumn()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }
    }
}