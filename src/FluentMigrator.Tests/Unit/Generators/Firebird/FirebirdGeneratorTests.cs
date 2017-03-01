using Xunit;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Expressions;

namespace FluentMigrator.Tests.Unit.Generators.Firebird
{
    public class FirebirdGeneratorTests
    {
        protected FirebirdGenerator generator;

        public FirebirdGeneratorTests()
        {
            generator = new FirebirdGenerator(FirebirdOptions.StandardBehaviour());
        }

        [Fact]
        public void CanAlterSequence()
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

            var result = generator.GenerateAlterSequence(expression.Sequence);
            result.ShouldBe("ALTER SEQUENCE \"Sequence\" RESTART WITH 2");
        }
    }
}
