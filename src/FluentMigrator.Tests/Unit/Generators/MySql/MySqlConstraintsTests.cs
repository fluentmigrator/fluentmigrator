using System.Data;
using FluentMigrator.Runner.Generators.MySql;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.MySql
{
    [TestFixture]
    public class MySqlConstraintsTests
    {
        protected MySqlGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new MySqlGenerator();
        }

        [Test]
        public void CanCreateMultiColumnPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnPrimaryKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD CONSTRAINT `PK_TestTable1_TestColumn1_TestColumn2` PRIMARY KEY (`TestColumn1`, `TestColumn2`)");
        }

        [Test]
        public void CanCreateMultiColumnUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnUniqueConstraintExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD CONSTRAINT `UC_TestTable1_TestColumn1_TestColumn2` UNIQUE (`TestColumn1`, `TestColumn2`)");
        }

        [Test]
        public void CanCreateNamedForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE `TestTable1` ADD CONSTRAINT `FK_Test` FOREIGN KEY (`TestColumn1`) REFERENCES `TestTable2` (`TestColumn2`)");
        }

        [Test]
        public void CanCreateNamedForeignKeyWithOnDeleteAndOnUpdateOptions()
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnDelete = Rule.Cascade;
            expression.ForeignKey.OnUpdate = Rule.SetDefault;
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE `TestTable1` ADD CONSTRAINT `FK_Test` FOREIGN KEY (`TestColumn1`) REFERENCES `TestTable2` (`TestColumn2`) ON DELETE CASCADE ON UPDATE SET DEFAULT");
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public void CanCreateNamedForeignKeyWithOnDeleteOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnDelete = rule;
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                string.Format("ALTER TABLE `TestTable1` ADD CONSTRAINT `FK_Test` FOREIGN KEY (`TestColumn1`) REFERENCES `TestTable2` (`TestColumn2`) ON DELETE {0}", output));
        }

        [TestCase(Rule.SetDefault, "SET DEFAULT"), TestCase(Rule.SetNull, "SET NULL"), TestCase(Rule.Cascade, "CASCADE")]
        public void CanCreateNamedForeignKeyWithOnUpdateOptions(Rule rule, string output)
        {
            var expression = GeneratorTestHelper.GetCreateForeignKeyExpression();
            expression.ForeignKey.OnUpdate = rule;
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                string.Format("ALTER TABLE `TestTable1` ADD CONSTRAINT `FK_Test` FOREIGN KEY (`TestColumn1`) REFERENCES `TestTable2` (`TestColumn2`) ON UPDATE {0}", output));
        }

        [Test]
        public void CanCreateNamedMultiColumnForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnForeignKeyExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe(
                "ALTER TABLE `TestTable1` ADD CONSTRAINT `FK_Test` FOREIGN KEY (`TestColumn1`, `TestColumn3`) REFERENCES `TestTable2` (`TestColumn2`, `TestColumn4`)");
        }

        [Test]
        public void CanCreateNamedMultiColumnPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnNamedPrimaryKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD CONSTRAINT `TESTPRIMARYKEY` PRIMARY KEY (`TestColumn1`, `TestColumn2`)");
        }

        [Test]
        public void CanCreateNamedMultiColumnUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnNamedUniqueConstraintExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD CONSTRAINT `TESTUNIQUECONSTRAINT` UNIQUE (`TestColumn1`, `TestColumn2`)");
        }

        [Test]
        public void CanCreateNamedPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD CONSTRAINT `TESTPRIMARYKEY` PRIMARY KEY (`TestColumn1`)");
        }

        [Test]
        public void CanCreateNamedUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD CONSTRAINT `TESTUNIQUECONSTRAINT` UNIQUE (`TestColumn1`)");
        }

        [Test]
        public void CanCreatePrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreatePrimaryKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD CONSTRAINT `PK_TestTable1_TestColumn1` PRIMARY KEY (`TestColumn1`)");
        }

        [Test]
        public void CanCreateUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueConstraintExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD CONSTRAINT `UC_TestTable1_TestColumn1` UNIQUE (`TestColumn1`)");
        }

        [Test]
        public void CanDropForeignKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` DROP FOREIGN KEY `FK_Test`");
        }

        [Test]
        public void CanDropPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeletePrimaryKeyExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` DROP PRIMARY KEY");
        }

        [Test]
        public void CanDropUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteUniqueConstraintExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` DROP INDEX `TESTUNIQUECONSTRAINT`");
        }
    }
}
