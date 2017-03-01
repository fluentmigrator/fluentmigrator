using FluentMigrator.Runner.Generators.DB2;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentMigrator.Tests.Unit.Generators.Db2
{
    public class Db2SequenceTests : BaseSequenceTests
    {
        protected Db2Generator Generator;

        public Db2SequenceTests()
        {
            Generator = new Db2Generator();
        }

        [Fact]
        public override void CanCreateSequenceWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateSequenceExpression();
            expression.Sequence.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE SEQUENCE TestSchema.Sequence INCREMENT 2 MINVALUE 0 MAXVALUE 100 START WITH 2 CACHE 10 CYCLE");
        }

        [Fact]
        public override void CanCreateSequenceWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateSequenceExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE SEQUENCE Sequence INCREMENT 2 MINVALUE 0 MAXVALUE 100 START WITH 2 CACHE 10 CYCLE");
        }

        [Fact]
        public override void CanDropSequenceWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteSequenceExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP SEQUENCE TestSchema.Sequence");
        }

        [Fact]
        public override void CanDropSequenceWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteSequenceExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP SEQUENCE Sequence");
        }
    }
}
