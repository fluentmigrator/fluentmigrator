using NUnit.Framework;
using NUnit.Should;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Expressions;

namespace FluentMigrator.Tests.Unit.Generators.Firebird
{
    [TestFixture]
    public class FirebirdSequenceTests
    {
        protected FirebirdGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new FirebirdGenerator(FirebirdOptions.StandardBehaviour());
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
            sql.ShouldBe("CREATE SEQUENCE \"Sequence\"");
        }

        [Test]
        public void CanDeleteSequence()
        {
            var expression = new DeleteSequenceExpression { SchemaName = "Schema", SequenceName = "Sequence" };
            var sql = generator.Generate(expression);
            sql.ShouldBe("DROP SEQUENCE \"Sequence\"");
        }
    }
}
