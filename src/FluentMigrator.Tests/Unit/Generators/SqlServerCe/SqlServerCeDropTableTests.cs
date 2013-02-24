using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServerCe
{
    using FluentMigrator.Runner.Generators;

    public class SqlServerCeDropTableTests : GeneratorTestBase
    {
        protected SqlServerCeGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqlServerCeGenerator();
        }

        [Test]
        public void CanDropColumn()
        {
            //This does not work if column in used in constraint, index etc.
            var expression = GeneratorTestHelper.GetDeleteColumnExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] DROP COLUMN [TestColumn1];");
        }

        [Test]
        [ExpectedException(typeof(DatabaseOperationNotSupportedException))]
        public void CanNotDropMultipleColumns()
        {
            //This does not work if column in used in constraint, index etc.
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "TestColumn1", "TestColumn2" });

            generator.Generate(expression);
        }

        [Test]
        public void CanDropForeignKey()
        {
            var expression = GeneratorTestHelper.GetDeleteForeignKeyExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("ALTER TABLE [TestTable1] DROP CONSTRAINT [FK_Test]");
        }

        [Test]
        public void CanDropTable()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP TABLE [TestTable1]");
        }

        [Test]
        public void CanDeleteIndex()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP INDEX [TestTable1].[TestIndex]");
        }

        [Test]
        public void CanDropIndex()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP INDEX [TestTable1].[TestIndex]");
        }

        [Test]
        [ExpectedException(typeof(DatabaseOperationNotSupportedException))]
        public void CanDeleteSchema()
        {
            var expression = new DeleteSchemaExpression() { SchemaName = "TestSchema" };
            generator.Generate(expression);
        }
    }
}
