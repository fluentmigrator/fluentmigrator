using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServerCe
{
    [TestFixture]
    public class SqlServerCeColumnTests
    {
        protected SqlServerCeGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqlServerCeGenerator();
        }

        [Test]
        public void CanAlterColumnWithDefaultSchema()
        {
            //TODO: This will fail if there are any keys attached 
            var expression = GeneratorTestHelper.GetAlterColumnExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] ALTER COLUMN [TestColumn1] NVARCHAR(20) NOT NULL");
        }

        [Test]
        public void CanCreateColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateColumnExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] ADD [TestColumn1] NVARCHAR(5) NOT NULL");
        }

        [Test]
        public void CanCreateDecimalColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateDecimalColumnExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] ADD [TestColumn1] NUMERIC(19,2) NOT NULL");
        }

        [Test]
        public void CanDropColumnWithDefaultSchema()
        {
            //This does not work if column in used in constraint, index etc.
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] DROP COLUMN [TestColumn1];");
        }
    }
}
