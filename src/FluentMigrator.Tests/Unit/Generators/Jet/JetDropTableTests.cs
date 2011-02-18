
using NUnit.Framework;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Jet;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Jet
{
    public class JetDropTableTests : BaseTableDropTests
    {
        protected JetGenerator SUT;

        [SetUp]
        public void SetUp()
        {
            SUT = new JetGenerator();

        }

        [Test]
        public override void CanDropColumn()
        {
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();

            string sql = SUT.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] DROP COLUMN [TestColumn1]");
        }

        [Test]
        public override void CanDropForeignKey()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();

            var sql = SUT.Generate(expression);

            sql.ShouldBe("ALTER TABLE [TestTable1] DROP CONSTRAINT FK_Test");
        }

        [Test]
        public override void CanDropTable()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            var sql = SUT.Generate(expression);
            sql.ShouldBe("DROP TABLE [TestTable1]");
        }

        [Test]
        public override void CanDeleteIndex()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            var sql = SUT.Generate(expression);
            sql.ShouldBe("DROP INDEX IX_TEST ON [TEST_TABLE]");
        }
    }
}
