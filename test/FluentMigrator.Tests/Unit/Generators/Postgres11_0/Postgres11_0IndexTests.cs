using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Tests.Unit.Generators.Postgres;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Postgres11_0
{
    [TestFixture]
    public class Postgres11_0IndexTests : PostgresIndexTests
    {
        /// <inheritdoc />
        protected override PostgresGenerator CreateGenerator(PostgresQuoter quoter)
        {
            return new Postgres11_0Generator(quoter);
        }

        [Test]
        public override void CanCreateIndexAsOnly()
        {
            var expression = GetCreateIndexExpression(false, true);

            var result = Generator.Generate(expression);
            result.ShouldBe($"CREATE INDEX \"TestIndex\" ON ONLY \"public\".\"TestTable1\" (\"TestColumn1\" ASC);");
        }
    }
}
