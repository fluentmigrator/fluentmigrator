

namespace FluentMigrator.Tests.Unit.Generators
{
    using System;
    using System.Data;
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;
    using NUnit.Framework;
    using FluentMigrator.Runner.Generators.SqlServer;
    using NUnit.Should;
    using FluentMigrator.Runner.Generators;

    public class SqlServerCeGeneratorTests : GeneratorTestBase
    {
       
        SqlServerCeGenerator generator;

        [SetUp]
        public void SetUp()
        {
            generator = new SqlServerCeGenerator();
        }

        [Test]
        public void CannotCreateASchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedExecption>(() => generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CannotAlterASchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedExecption>(() => generator.Generate(new AlterSchemaExpression()));
        }

        [Test]
        public void CannotDeleteASchema()
        {
            Assert.Throws<DatabaseOperationNotSupportedExecption>(() => generator.Generate(new DeleteSchemaExpression()));
        }

        [Test]
        public void CreatesTheCorrectSyntaxToDropAnIndex()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            var sql = generator.Generate(expression);

            sql.ShouldBe("DROP INDEX [TestTable1].[TestIndex]");
        }
    }
}