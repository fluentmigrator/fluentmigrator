using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.MySql;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.MySql
{
    public class MySqlCreateTableTests : BaseTableCreateTests
    {
        private MySqlGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new MySqlGenerator();
        }

        [Test]
        public override void CanCreateTable()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255), `TestColumn2` INTEGER) ENGINE = INNODB");
        }

        [Test]
        public override void CanCreateTableWithCustomColumnType()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "[timestamp]";
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255), `TestColumn2` [timestamp], PRIMARY KEY (`TestColumn1`)) ENGINE = INNODB");
        }

        [Test]
        public override void CanCreateTableWithPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();

            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255), `TestColumn2` INTEGER, PRIMARY KEY (`TestColumn1`)) ENGINE = INNODB");
        }

        [Test]
        public override void CanCreateTableNamedPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();

            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255), `TestColumn2` INTEGER, CONSTRAINT `TestKey` PRIMARY KEY (`TestColumn1`)) ENGINE = INNODB");
        }

        [Test]
        public override void CanCreateTableNamedMultiColumnPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumNamedPrimaryKeyExpression();
            string sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255), `TestColumn2` INTEGER, CONSTRAINT `TestKey` PRIMARY KEY (`TestColumn1`, `TestColumn2`)) ENGINE = INNODB");
        }

        [Test]
        public override void CanCreateTableWithIdentity()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();

            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE `TestTable1` (`TestColumn1` INTEGER AUTO_INCREMENT, `TestColumn2` INTEGER) ENGINE = INNODB");
        }

        [Test]
        public override void CanCreateTableWithNonNullableField()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNonNullableColumn();
            expression.Columns[0].IsNullable = true;
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255), `TestColumn2` INTEGER NOT NULL) ENGINE = INNODB");
        }

        [Test]
        public override void CanCreateTableWithDefaultValue()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].DefaultValue = "Default";
            expression.Columns[1].DefaultValue = 0;
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) DEFAULT 'Default', `TestColumn2` INTEGER DEFAULT 0) ENGINE = INNODB");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNull()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;

            var result = _generator.Generate(expression);

            result.ShouldBe(
                "CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255) DEFAULT NULL, `TestColumn2` INTEGER DEFAULT 0) ENGINE = INNODB");
        }

        [Test]
        public override void CanCreateIndex()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE INDEX `TestIndex` ON `TestTable1` (`TestColumn1` ASC)");
        }

        [Test]
        public override void CanCreateMultiColumnIndex()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();

            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE INDEX `TestIndex` ON `TestTable1` (`TestColumn1` ASC, `TestColumn2` DESC)");
        }

        [Test]
        public override void CanCreateUniqueIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX `TestIndex` ON `TestTable1` (`TestColumn1` ASC)");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();

            var sql = _generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX `TestIndex` ON `TestTable1` (`TestColumn1` ASC, `TestColumn2` DESC)");
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKey()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();

            var result = _generator.Generate(expression);

            result.ShouldBe(
                "CREATE TABLE `TestTable1` (`TestColumn1` VARCHAR(255), `TestColumn2` INTEGER, PRIMARY KEY (`TestColumn1`, `TestColumn2`)) ENGINE = INNODB");
        }

        [Test]
        public override void CanCreateSchema()
        {
            var expression = new CreateSchemaExpression() { SchemaName = "TestSchema" };
            var result = _generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanCreateSchemaInStrictMode()
        {
            _generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(new CreateSchemaExpression()));
        }
    }
}