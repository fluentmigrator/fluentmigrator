using FluentMigrator.Runner.Generators.MySql;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.MySql
{
    [TestFixture]
    public class MySqlColumnTests
    {
        protected MySqlGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new MySqlGenerator();
        }

        [Test]
        public void CanAlterColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` MODIFY COLUMN `TestColumn1` VARCHAR(20) NOT NULL");
        }

        [Test]
        public void CanCreateAutoIncrementColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` MODIFY COLUMN `TestColumn1` INTEGER NOT NULL AUTO_INCREMENT");
        }

        [Test]
        public void CanCreateColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` VARCHAR(5) NOT NULL");
        }

        [Test]
        public void CanCreateDecimalColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ADD COLUMN `TestColumn1` DECIMAL(19,2) NOT NULL");
        }

        [Test]
        public void CanDropColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` DROP COLUMN `TestColumn1`");
        }

        [Test]
        public void CanDropMultipleColumnsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "TestColumn1", "TestColumn2" });

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` DROP COLUMN `TestColumn1`;" + System.Environment.NewLine + "ALTER TABLE `TestTable1` DROP COLUMN `TestColumn2`");
        }

        [Test]
        public void CanRenameColumnWithDefaultSchema()
        {
            // MySql does not appear to have a way to change column without re-specifying the existing column definition
            var expression = GeneratorTestHelper.GetRenameColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` CHANGE `TestColumn1` `TestColumn2` ");
        }
    }
}
