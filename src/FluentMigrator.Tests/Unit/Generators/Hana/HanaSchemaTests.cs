using FluentMigrator.Runner.Generators.Hana;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Hana
{
    [TestFixture]
    public class HanaSchemaTests : BaseSchemaTests
    {
        protected HanaGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new HanaGenerator();
        }

        [Test]
        public override void CanAlterSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetAlterSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema1\".\"TestTable\" SET SCHEMA \"TestSchema2\"");
        }

        [Test]
        public override void CanCreateSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE SCHEMA \"TestSchema\"");
        }

        [Test]
        public override void CanDropSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetDeleteSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP SCHEMA \"TestSchema\"");
        }
    }
}