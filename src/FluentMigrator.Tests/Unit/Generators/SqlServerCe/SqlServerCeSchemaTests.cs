using FluentMigrator.Exceptions;
using FluentMigrator.Runner.Generators.SqlServer;
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators.SqlServerCe
{
    public class SqlServerCeSchemaTests : BaseSchemaTests
    {
        protected SqlServerCeGenerator Generator;

        public void Setup()
        {
            Generator = new SqlServerCeGenerator();
        }

        [Fact]
        public override void CanAlterSchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(GeneratorTestHelper.GetAlterSchemaExpression()));
        }

        [Fact]
        public override void CanCreateSchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(GeneratorTestHelper.GetCreateSchemaExpression()));
        }

        [Fact]
        public override void CanDropSchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(GeneratorTestHelper.GetDeleteSchemaExpression()));
        }
    }
}
