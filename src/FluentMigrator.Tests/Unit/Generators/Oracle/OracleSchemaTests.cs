using FluentMigrator.Runner.Generators.Oracle;
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    public class OracleSchemaTests : BaseSchemaTests
    {
        protected OracleGenerator Generator;

        public OracleSchemaTests()
        {
            Generator = new OracleGenerator();
        }

        [Fact]
        public override void CanAlterSchema()
        {
            var expression = GeneratorTestHelper.GetAlterSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Fact]
        public override void CanCreateSchema()
        {
            var expression = GeneratorTestHelper.GetCreateSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Fact]
        public override void CanDropSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }
    }
}
