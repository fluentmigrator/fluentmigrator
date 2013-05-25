using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2000
{
    [TestFixture]
    public class SqlServer2000SchemaTests
    {
        protected SqlServer2000Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServer2000Generator();
        }

        [Test]
        public void CanAlterSchema()
        {
            var expression = GeneratorTestHelper.GetAlterSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanCreateSchema()
        {
            var expression = GeneratorTestHelper.GetCreateSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public void CanDropSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }
    }
}
