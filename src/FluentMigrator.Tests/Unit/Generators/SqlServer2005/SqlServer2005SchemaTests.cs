using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2005
{
    [TestFixture]
    public class SqlServer2005SchemaTests
    {
        protected SqlServer2005Generator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqlServer2005Generator();
        }

        [Test]
        public void CanAlterSchema()
        {
            var expression = new AlterSchemaExpression
            {
                DestinationSchemaName = "DEST",
                SourceSchemaName = "SOURCE",
                TableName = "TABLE"
            };

            var sql = generator.Generate(expression);
            sql.ShouldBe(
              "ALTER SCHEMA [DEST] TRANSFER [SOURCE].[TABLE]");
        }

        [Test]
        public void CanCreateSchema()
        {
            var expression = new CreateSchemaExpression
            {
                SchemaName = "TestSchema"
            };

            var sql = generator.Generate(expression);
            sql.ShouldBe(
              "CREATE SCHEMA [TestSchema]");
        }

        [Test]
        public void CanDeleteSchema()
        {
            var expression = new DeleteSchemaExpression() { SchemaName = "TestSchema" };
            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP SCHEMA [TestSchema]");
        }
    }
}
