using FluentMigrator.Runner.Generators.Hana;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Hana
{
    [TestFixture]
    public class HanaIndexTests : BaseIndexTests
    {
        protected HanaGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new HanaGenerator();
        }

        [Test]
        public override void CanCreateIndexWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX \"TestIndex\" ON \"TestSchema\".\"TestTable1\" (\"TestColumn1\" ASC)");
        }

        [Test]
        public override void CanCreateIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX \"TestIndex\" ON \"TestTable1\" (\"TestColumn1\" ASC);");
        }

        [Test]
        public override void CanCreateMultiColumnIndexWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX \"TestIndex\" ON \"TestSchema\".\"TestTable1\" (\"TestColumn1\" ASC,\"TestColumn2\" DESC)");
        }

        [Test]
        public override void CanCreateMultiColumnIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX \"TestIndex\" ON \"TestTable1\" (\"TestColumn1\" ASC, \"TestColumn2\" DESC);");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueIndexWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX \"TestIndex\" ON \"TestSchema\".\"TestTable1\" (\"TestColumn1\" ASC,\"TestColumn2\" DESC)");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX \"TestIndex\" ON \"TestTable1\" (\"TestColumn1\" ASC, \"TestColumn2\" DESC);");
        }

        [Test]
        public override void CanCreateUniqueIndexWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX \"TestIndex\" ON \"TestSchema\".\"TestTable1\" (\"TestColumn1\" ASC)");
        }

        [Test]
        public override void CanCreateUniqueIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX \"TestIndex\" ON \"TestTable1\" (\"TestColumn1\" ASC);");
        }

        [Test]
        public override void CanDropIndexWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP INDEX \"TestSchema\".\"TestIndex\"");
        }

        [Test]
        public override void CanDropIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP INDEX \"TestIndex\";");
        }
    }
}