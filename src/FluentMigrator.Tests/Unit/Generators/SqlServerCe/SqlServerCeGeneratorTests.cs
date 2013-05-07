using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServerCe
{
    [TestFixture]
    public class SqlServerCeGeneratorTests
    {
        protected SqlServerCeGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServerCeGenerator();
        }

        [Test]
        public void CannotAlterASchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new AlterSchemaExpression()));
        }

        [Test]
        public void CannotCreateASchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CannotDeleteASchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new DeleteSchemaExpression()));
        }

        [Test]
        public void CreatesTheCorrectSyntaxToDropAnIndex()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            var sql = Generator.Generate(expression);

            sql.ShouldBe("DROP INDEX [TestTable1].[TestIndex]");
        }

        [Test]
        [ExpectedException(typeof(DatabaseOperationNotSupportedException))]
        public void GenerateNecessaryStatementsForADeleteDefaultExpressionIsThrowsException()
        {
            var expression = new DeleteDefaultConstraintExpression {ColumnName = "Name", SchemaName = "Personalia", TableName = "Person"};
            Generator.Generate(expression);
        }
    }
}