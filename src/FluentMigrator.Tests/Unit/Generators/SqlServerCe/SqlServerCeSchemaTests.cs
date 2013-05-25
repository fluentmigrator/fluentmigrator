using FluentMigrator.Exceptions;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.SqlServerCe
{
    [TestFixture]
    public class SqlServerCeSchemaTests : BaseSchemaTests
    {
        protected SqlServerCeGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServerCeGenerator();
        }

        [Test]
        public override void CanAlterSchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(GeneratorTestHelper.GetAlterSchemaExpression()));
        }

        [Test]
        public override void CanCreateSchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(GeneratorTestHelper.GetCreateSchemaExpression()));
        }

        [Test]
        public override void CanDropSchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(GeneratorTestHelper.GetDeleteSchemaExpression()));
        }
    }
}
