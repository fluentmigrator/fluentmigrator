using FluentMigrator.Runner.Generators.DB2;
using NUnit.Framework;
using NUnit.Should;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentMigrator.Tests.Unit.Generators.Db2
{
    [TestFixture]
    public class Db2SchemaTests : BaseSchemaTests
    {
        protected Db2Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new Db2Generator();
        }

        [Test]
        public override void CanAlterSchema()
        {
            var expression = GeneratorTestHelper.GetAlterSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Empty);
        }

        [Test]
        public override void CanCreateSchema()
        {
            var expression = GeneratorTestHelper.GetCreateSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE SCHEMA TestSchema");
        }

        [Test]
        public override void CanDropSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP SCHEMA TestSchema RESTRICT");
        }
    }
}
