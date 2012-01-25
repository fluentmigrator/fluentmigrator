using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.MySql;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.MySql
{
    public class MySqlDropTableTests : BaseTableDropTests
    {
        private MySqlGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new MySqlGenerator();
        }

        [Test]
        public override void CanDropColumn()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE `TestTable1` DROP COLUMN `TestColumn1`");
        }

        [Test]
        public override void CanDropForeignKey()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE `TestTable1` DROP FOREIGN KEY `FK_Test`");
        }

        [Test]
        public override void CanDropTable()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("DROP TABLE `TestTable1`");
        }

        [Test]
        public override void CanDeleteIndex()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("DROP INDEX `TestIndex` ON `TestTable1`");
        }

        [Test]
        public override void CanDeleteSchema()
        {
            var expression = new DeleteSchemaExpression();
            var result = _generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanDeleteSchemaInStrictMode()
        {
            _generator.CompatibilityMode = CompatibilityMode.Strict;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(new DeleteSchemaExpression()));
        }
    }
}