using System.Data;
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

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public void CanCreateForeignKeyWithOnUpdateOptions(Rule rule, string output) 
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnUpdate = rule;
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                string.Format("ALTER TABLE `TestTable1` ADD CONSTRAINT `FK_Test` FOREIGN KEY (`TestColumn1`) REFERENCES `TestTable2` (`TestColumn2`) ON UPDATE {0}", output));
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public void CanCreateForeignKeyWithOnDeleteOptions(Rule rule, string output) 
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnDelete = rule;
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                string.Format("ALTER TABLE `TestTable1` ADD CONSTRAINT `FK_Test` FOREIGN KEY (`TestColumn1`) REFERENCES `TestTable2` (`TestColumn2`) ON DELETE {0}", output));
        }

        [Test]
        public void CanCreateForeignKeyWithOnDeleteAndOnUpdateOptions() 
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnDelete = Rule.Cascade;
            expression.ForeignKey.OnUpdate = Rule.SetDefault;
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE `TestTable1` ADD CONSTRAINT `FK_Test` FOREIGN KEY (`TestColumn1`) REFERENCES `TestTable2` (`TestColumn2`) ON DELETE CASCADE ON UPDATE SET DEFAULT");
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

        [Test]
        public void CanDropPrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` DROP PRIMARY KEY");
        }

        [Test]
        public void CanDropUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetDeleteUniqueConstraintExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` DROP INDEX `TESTUNIQUECONSTRAINT`");
        }

        [Test]
        public void CanCreatePrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD CONSTRAINT `PK_TestTable1_TestColumn1` PRIMARY KEY (`TestColumn1`)");
        }

        [Test]
        public void CanCreateNamedPrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD CONSTRAINT `TESTPRIMARYKEY` PRIMARY KEY (`TestColumn1`)");
        }

        [Test]
        public void CanCreateMultiColmnPrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnPrimaryKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD CONSTRAINT `PK_TestTable1_TestColumn1_TestColumn2` PRIMARY KEY (`TestColumn1`, `TestColumn2`)");
        }

        [Test]
        public void CanCreateMultiColmnNamedPrimaryKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnNamedPrimaryKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD CONSTRAINT `TESTPRIMARYKEY` PRIMARY KEY (`TestColumn1`, `TestColumn2`)");
        }

        [Test]
        public void CanCreateUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD CONSTRAINT `UC_TestTable1_TestColumn1` UNIQUE (`TestColumn1`)");
        }

        [Test]
        public void CanCreateNamedUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD CONSTRAINT `TESTUNIQUECONSTRAINT` UNIQUE (`TestColumn1`)");
        }

        [Test]
        public void CanCreateMultiColmnUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD CONSTRAINT `UC_TestTable1_TestColumn1_TestColumn2` UNIQUE (`TestColumn1`, `TestColumn2`)");
        }

        [Test]
        public void CanCreateMultiColmnNamedUniqueConstraint()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnNamedUniqueConstraintExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD CONSTRAINT `TESTUNIQUECONSTRAINT` UNIQUE (`TestColumn1`, `TestColumn2`)");
        }

        [Test]
        public void CanDropForiegnKeyConstraint()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` DROP FOREIGN KEY `FK_Test`");
        }
    }
}
