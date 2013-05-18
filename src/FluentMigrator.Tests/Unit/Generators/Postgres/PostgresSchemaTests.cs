using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Postgres;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    public class PostgresSchemaTests
    {
        protected PostgresGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new PostgresGenerator();
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
            sql.ShouldBe("ALTER TABLE \"SOURCE\".\"TABLE\" SET SCHEMA \"DEST\"");
        }

        [Test]
        public void CanCreateSchema()
        {
            var expression = new CreateSchemaExpression { SchemaName = "Schema1" };
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE SCHEMA \"Schema1\"");
        }

        [Test]
        public void CanDropSchema()
        {
            var expression = new DeleteSchemaExpression() { SchemaName = "Schema1" };
            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP SCHEMA \"Schema1\"");
        }
    }
}