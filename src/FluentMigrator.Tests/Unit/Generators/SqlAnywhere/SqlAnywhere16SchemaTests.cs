using FluentMigrator.Exceptions;
using FluentMigrator.Runner.Generators.SqlAnywhere;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2005
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
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Schema")]
        public override void CanAlterSchema()
        {
            var expression = GeneratorTestHelper.GetAlterSchemaExpression();
            var currentCompatabilityMode = Generator.compatabilityMode;

            try
            {
                Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
                Shouldly.Should.Throw<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
            }
            finally
            {
                Generator.compatabilityMode = currentCompatabilityMode;
            }
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Schema")]
        public override void CanCreateSchema()
        {
            var expression = GeneratorTestHelper.GetCreateSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE SCHEMA AUTHORIZATION [TestSchema]");
        }

        [Test]
        [Category("SQLAnywhere"), Category("SQLAnywhere16"), Category("Generator"), Category("Schema")]
        public override void CanDropSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteSchemaExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP USER [TestSchema]");
        }
    }
}