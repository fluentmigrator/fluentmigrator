using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.MySql;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.MySql
{
    public class MySqlAlterTableTestscs : BaseTableAlterTests
    {
        private MySqlGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new MySqlGenerator();
        }

        [Test]
        public override void CanAddColumn()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();

            var sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` VARCHAR(5) NOT NULL");
        }

        [Test]
        public override void CanAddDecimalColumn()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();

            var sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` DECIMAL(19,2) NOT NULL");
        }

        [Test]
        public override void CanRenameColumn()
        {
            // MySql does not appear to have a way to change column without re-specifying the existing column definition
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` CHANGE `TestColumn1` `TestColumn2` ");
        }

        [Test]
        public override void CanRenameTable()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("RENAME TABLE `TestTable1` TO `TestTable2`");
        }

        [Test]
        public override void CanAlterColumn()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            var sql = _generator.Generate(expression);

            sql.ShouldBe("ALTER TABLE `TestTable1` MODIFY COLUMN `TestColumn1` VARCHAR(20) NOT NULL");
        }

        [Test]
        public override void CanCreateForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
                    var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE `TestTable1` ADD CONSTRAINT `FK_Test` FOREIGN KEY (`TestColumn1`) REFERENCES `TestTable2` (`TestColumn2`)");
        }

        [Test]
        public override void CanCreateMulitColumnForeignKey()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE `TestTable1` ADD CONSTRAINT `FK_Test` FOREIGN KEY (`TestColumn1`, `TestColumn3`) REFERENCES `TestTable2` (`TestColumn2`, `TestColumn4`)");
        }

        [Test]
        public override void CanCreateAutoIncrementColumn()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();

            var sql = _generator.Generate(expression);

            sql.ShouldBe("ALTER TABLE `TestTable1` MODIFY COLUMN `TestColumn1` INTEGER NOT NULL AUTO_INCREMENT");
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
            _generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(new CreateSchemaExpression()));
        }
    }
}