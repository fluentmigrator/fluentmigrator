using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2012
{
    [TestFixture]
    public class SqlServer2012SequenceTests
    {
        protected IMigrationGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqlServer2012Generator();
        }


        [Test]
        public void CanCreateSequenceWithCustomSchema()
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
                    SchemaName = "TestSchema",
                    StartWith = 2
                }
            };
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE SEQUENCE [TestSchema].[Sequence] INCREMENT BY 2 MINVALUE 0 MAXVALUE 100 START WITH 2 CACHE 10 CYCLE");
        }

        [Test]
        public void CanDropSequenceWithCustomSchema()
        {
            var expression = new DeleteSequenceExpression { SchemaName = "TestSchema", SequenceName = "Sequence" };
            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP SEQUENCE [TestSchema].[Sequence]");
        }
    }
}