using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Oracle;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.OracleWithQuotedIdentifier
{
    [TestFixture]
    public class OracleSequenceTests
    {
        protected OracleGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new OracleGenerator(useQuotedIdentifiers: true);
        }

        [Test]
        public void CanCreateSequence()
        {
            var expression = new CreateSequenceExpression
            {
                Sequence =
                {
                    Cache = 10,
                    Cycle = true,
                    Increment = 2,
                    MaxValue = 100,
                    MinValue = 0,
                    Name = "Sequence",
                    SchemaName = "Schema",
                    StartWith = 2
                }
            };
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE SEQUENCE \"Schema\".\"Sequence\" INCREMENT 2 MINVALUE 0 MAXVALUE 100 START WITH 2 CACHE 10 CYCLE");
        }

        [Test]
        public void CanCreateSequenceWithoutSchema()
        {
            var expression = new CreateSequenceExpression
            {
                Sequence =
                {
                    Cache = 10,
                    Cycle = true,
                    Increment = 2,
                    MaxValue = 100,
                    MinValue = 0,
                    Name = "Sequence",
                    StartWith = 2
                }
            };
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE SEQUENCE \"Sequence\" INCREMENT 2 MINVALUE 0 MAXVALUE 100 START WITH 2 CACHE 10 CYCLE");
        }

        [Test]
        public void CanDeleteSequence()
        {
            var expression = new DeleteSequenceExpression { SchemaName = "Schema", SequenceName = "Sequence" };
            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP SEQUENCE \"Schema\".\"Sequence\"");
        }

        [Test]
        public void CanDeleteSequenceWithoutSchemaName()
        {
            var expression = new DeleteSequenceExpression { SequenceName = "Sequence" };
            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP SEQUENCE \"Sequence\"");
        }
    }
}