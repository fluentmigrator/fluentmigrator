using FluentMigrator.Builders.Create.Schema;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.SqlServer;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2005
{
    [TestFixture]
    public class SqlServer2005SchemaTests : BaseSchemaTests
    {
        protected SqlServer2005Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServer2005Generator();
        }

        [Test]
        public override void CanAlterSchema()
        {
            var expression = GeneratorTestHelper.GetAlterSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER SCHEMA [TestSchema2] TRANSFER [TestSchema1].[TestTable]");
        }

        [Test]
        public override void CanCreateSchema()
        {
            var expression = GeneratorTestHelper.GetCreateSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE SCHEMA [TestSchema]");
        }

        [Test]
        public override void CanDropSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP SCHEMA [TestSchema]");
        }

        [Test]
        public void CanCreateSchemaWithAuthorization()
        {
            var expression = GeneratorTestHelper.GetCreateSchemaExpression();
            var builder = new CreateSchemaExpressionBuilder(expression);
            builder.Authorization("dbo");

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE SCHEMA [TestSchema] AUTHORIZATION [dbo]");
        }
    }
}
