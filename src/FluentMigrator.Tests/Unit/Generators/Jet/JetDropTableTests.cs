
using NUnit.Framework;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Jet;
using NUnit.Should;
using FluentMigrator.Runner.Generators;

namespace FluentMigrator.Tests.Unit.Generators.Jet
{
    public class JetDropTableTests : BaseTableDropTests
    {
        protected JetGenerator generator;

        [SetUp]
        public void SetUp()
        {
            generator = new JetGenerator();

        }

        [Test]
        public override void CanDropColumn()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();

            string sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] DROP COLUMN [TestColumn1]");
        }

        [Test]
        public override void CanDropForeignKey()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] DROP CONSTRAINT [FK_Test]");
        }

        [Test]
        public override void CanDropTable()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP TABLE [TestTable1]");
        }

        [Test]
        public override void CanDeleteIndex()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP INDEX [TestIndex] ON [TestTable1]");
        }

        [Test]
        public override void CanDeleteSchema()
        {
            var expression = new DeleteSchemaExpression();
            var result = generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanDeleteSchemaInStrictMode()
        {
            generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedExecption>(() => generator.Generate(new DeleteSchemaExpression()));
        }
    }
}
