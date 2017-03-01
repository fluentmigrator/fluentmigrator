﻿using FluentMigrator.Runner.Generators.Hana;
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators.Hana
{
    public class HanaSchemaTests : BaseSchemaTests
    {
        protected HanaGenerator Generator;

        public HanaSchemaTests()
        {
            Generator = new HanaGenerator();
        }

        [Fact]
        public override void CanAlterSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetAlterSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema1\".\"TestTable\" SET SCHEMA \"TestSchema2\"");
        }

        [Fact]
        public override void CanCreateSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE SCHEMA \"TestSchema\"");
        }

        [Fact]
        public override void CanDropSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetDeleteSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP SCHEMA \"TestSchema\"");
        }
    }
}