using FluentMigrator.Exceptions;
using FluentMigrator.Runner.Generators.SqlAnywhere;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SqlAnywhere
{
    [TestFixture]
    public class SqlAnywhere16SchemaTests : BaseSchemaTests
    {
        protected SqlAnywhere16Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlAnywhere16Generator();
        }

        [Test]
        [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("Schema")]
        public override void CanAlterSchema()
        {
            var expression = GeneratorTestHelper.GetAlterSchemaExpression();
            var currentCompatabilityMode = Generator.CompatibilityMode;

            try
            {
                Generator.CompatibilityMode = Runner.CompatibilityMode.STRICT;
                Should.Throw<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
            }
            finally
            {
                Generator.CompatibilityMode = currentCompatabilityMode;
            }
        }

        [Test]
        [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("Schema")]
        public override void CanCreateSchema()
        {
            var expression = GeneratorTestHelper.GetCreateSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE SCHEMA AUTHORIZATION [TestSchema]");
        }

        [Test]
        [Category("SqlAnywhere"), Category("SqlAnywhere16"), Category("Generator"), Category("Schema")]
        public override void CanDropSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP USER [TestSchema]");
        }
    }
}
