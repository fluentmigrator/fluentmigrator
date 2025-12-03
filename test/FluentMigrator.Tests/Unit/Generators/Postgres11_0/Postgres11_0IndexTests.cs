using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.Postgres;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Tests.Unit.Generators.Postgres10_0;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Postgres11_0
{
    [TestFixture]
    public abstract class Postgres11_0IndexTests : Postgres10_0IndexTests
    {
        /// <inheritdoc />
        protected override PostgresGenerator CreateGenerator(PostgresQuoter quoter)
        {
            return new Postgres11_0Generator(quoter);
        }

        [Test]
        public override void CanCreateIndexAsOnly()
        {
            var expression = GetCreateIndexWithExpression(
                x =>
                {
                    var definitionIsOnly = x.Index.GetAdditionalFeature(PostgresExtensions.Only, () => new PostgresIndexOnlyDefinition());
                    definitionIsOnly.IsOnly = true;
                });;

            var result = Generator.Generate(expression);
            result.ShouldBe($"CREATE INDEX \"TestIndex\" ON ONLY \"public\".\"TestTable1\" (\"TestColumn1\" ASC);");
        }

        [Test]
        public override void CanCreateIndexWithVacuumCleanupIndexScaleFactor()
        {
            var expression = GetCreateIndexWithExpression(x =>
            {
                x.Index.GetAdditionalFeature(PostgresExtensions.IndexVacuumCleanupIndexScaleFactor, () => (float)0.1);
            });

            var result = Generator.Generate(expression);
            result.ShouldBe($"CREATE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC) WITH ( VACUUM_CLEANUP_INDEX_SCALE_FACTOR = 0.1 );");
        }
    }
}
