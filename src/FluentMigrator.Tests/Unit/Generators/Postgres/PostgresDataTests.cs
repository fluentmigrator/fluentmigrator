using FluentMigrator.Runner.Generators.Postgres;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    public class PostgresDataTests
    {
        private PostgresGenerator generator;

        [SetUp]
        public void SetUp()
        {
            generator = new PostgresGenerator();
        }

        [Test]
        public void CanUpdateData()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("UPDATE \"public\".\"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE \"Id\" = 9 AND \"Homepage\" IS NULL");
        }

        [Test]
        public void CanUpdateDataForAllRows()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();

            var sql = generator.Generate(expression);
            sql.ShouldBe("UPDATE \"public\".\"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE 1 = 1");
        }

        [Test]
        public void CanUpdateDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe("UPDATE \"TestSchema\".\"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE \"Id\" = 9 AND \"Homepage\" IS NULL");
        }
    }
}