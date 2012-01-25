using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.SQLite;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SQLite
{
    public class SQLiteAlterTableTests : BaseTableAlterTests
    {
        private SqliteGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new SqliteGenerator();
        }

        [Test]
        public override void CanAddColumn()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE 'TestTable1' ADD COLUMN 'TestColumn1' TEXT NOT NULL");
        }

        [Test]
        public override void CanAddDecimalColumn()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE 'TestTable1' ADD COLUMN 'TestColumn1' NUMERIC NOT NULL");
        }

        [Test]
        public override void CanRenameColumn()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanRenameColumnInStrictMode()
        {
            _generator.CompatibilityMode = CompatibilityMode.Strict;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(GeneratorTestHelper.GetRenameColumnExpression()));
        }

        [Test]
        public override void CanRenameTable()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE 'TestTable1' RENAME TO 'TestTable2'");
        }

        [Test]
        public override void CanAlterColumn()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanAlterColumnInStrictMode()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            _generator.CompatibilityMode = CompatibilityMode.Strict;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(expression));
        }

        [Test]
        public override void CanCreateForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanCreateForeignKeyInStrictMode()
        {
            _generator.CompatibilityMode = CompatibilityMode.Strict;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(GeneratorTestHelper.GetCreateForeignKeyExpression()));
        }

        [Test]
        public override void CanCreateMulitColumnForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanCreateMulitColumnForeignKeyInStrictMode()
        {
            _generator.CompatibilityMode = CompatibilityMode.Strict;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression()));
        }

        [Test]
        public override void CanCreateAutoIncrementColumn()
        {
            var expression = GeneratorTestHelper.GetAlterTableAutoIncrementColumnExpression();
            expression.Column.IsPrimaryKey = true;

            string sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE 'TestTable1' ADD COLUMN 'TestColumn1' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT");
        }

        [Test]
        public override void CanAlterSchema()
        {
            var expression = new AlterSchemaExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanAlterSchemaInStrictMode()
        {
            _generator.CompatibilityMode = CompatibilityMode.Strict;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(new CreateSchemaExpression()));
        }
    }
}