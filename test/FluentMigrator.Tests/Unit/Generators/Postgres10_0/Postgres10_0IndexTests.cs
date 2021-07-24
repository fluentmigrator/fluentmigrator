using FluentMigrator.Builder.Create.Index;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.Postgres;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Tests.Unit.Generators.Postgres;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Postgres10_0
{
    [TestFixture]
    public class Postgres10_0IndexTests : PostgresIndexTests
    {
        /// <inheritdoc />
        protected override PostgresGenerator CreateGenerator(PostgresQuoter quoter)
        {
            return new Postgres10_0Generator(quoter);
        }

        [TestCase(GistBuffering.Auto)]
        [TestCase(GistBuffering.On)]
        [TestCase(GistBuffering.Off)]
        public override void CanCreateIndexWithBuffering(GistBuffering buffering)
        {
            var expression = GetCreateIndexWithExpression(x =>
            {
                x.Index.GetAdditionalFeature(PostgresExtensions.IndexBuffering, () => buffering);
            });

            var result = Generator.Generate(expression);
            result.ShouldBe($"CREATE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC) WITH ( BUFFERING = {buffering.ToString().ToUpper()} );");
        }

        [Test]
        public override void CanCreateIndexWithGinPendingListLimit()
        {
            var expression = GetCreateIndexWithExpression(x =>
            {
                x.Index.GetAdditionalFeature(PostgresExtensions.IndexGinPendingListLimit, () => (long)128);
            });

            var result = Generator.Generate(expression);
            result.ShouldBe($"CREATE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC) WITH ( GIN_PENDING_LIST_LIMIT = 128 );");
        }

        [Test]
        public override void CanCreateIndexWithPagesPerRange()
        {
            var expression = GetCreateIndexWithExpression(x =>
            {
                x.Index.GetAdditionalFeature(PostgresExtensions.IndexPagesPerRange, () => 128);
            });

            var result = Generator.Generate(expression);
            result.ShouldBe($"CREATE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC) WITH ( PAGES_PER_RANGE = 128 );");
        }

        [TestCase(true)]
        [TestCase(false)]
        public override void CanCreateIndexWithAutosummarize(bool autosummarize)
        {
            var expression = GetCreateIndexWithExpression(x =>
            {
                x.Index.GetAdditionalFeature(PostgresExtensions.IndexAutosummarize, () => autosummarize);
            });

            var onOff = autosummarize ? "ON" : "OFF";
            var result = Generator.Generate(expression);
            result.ShouldBe($"CREATE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC) WITH ( AUTOSUMMARIZE = {onOff} );");
        }
    }
}
