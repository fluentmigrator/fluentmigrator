using FluentMigrator.Runner.Generators.SqlServer;
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2012
{
    public class SqlServer2012SequenceTests : BaseSequenceTests
    {
        protected SqlServer2012Generator Generator;

        public SqlServer2012SequenceTests()
        {
            Generator = new SqlServer2012Generator();
        }


        [Fact]
        public override void CanCreateSequenceWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateSequenceExpression();
            expression.Sequence.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE SEQUENCE [TestSchema].[Sequence] INCREMENT BY 2 MINVALUE 0 MAXVALUE 100 START WITH 2 CACHE 10 CYCLE");
        }

        [Fact]
        public override void CanCreateSequenceWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateSequenceExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE SEQUENCE [dbo].[Sequence] INCREMENT BY 2 MINVALUE 0 MAXVALUE 100 START WITH 2 CACHE 10 CYCLE");
        }

        [Fact]
        public override void CanDropSequenceWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteSequenceExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP SEQUENCE [TestSchema].[Sequence]");
        }

        [Fact]
        public override void CanDropSequenceWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteSequenceExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP SEQUENCE [dbo].[Sequence]");
        }
    }
}
