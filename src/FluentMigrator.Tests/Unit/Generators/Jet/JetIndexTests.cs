using FluentMigrator.Runner.Generators.Jet;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Jet
{
    [TestFixture]
    public class JetIndexTests
    {
        protected JetGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new JetGenerator();
        }

        [Test]
        public void CanCreateIndex()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();

            var sql = _generator.Generate(expression);

            sql.ShouldBe("CREATE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanCreateMultiColumnIndex()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();

            var sql = _generator.Generate(expression);

            sql.ShouldBe("CREATE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public void CanCreateMultiColumnUniqueIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();

            var sql = _generator.Generate(expression);

            sql.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public void CanCreateUniqueIndex()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();

            var sql = _generator.Generate(expression);

            sql.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanDeleteIndex()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            var sql = _generator.Generate(expression);
            sql.ShouldBe("DROP INDEX [TestIndex] ON [TestTable1]");
        }
    }
}
