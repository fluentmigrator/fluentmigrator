using System.Linq;

using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Postgres;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Tests.Unit.Generators.Postgres11_0;
using FluentMigrator.Tests.Unit.Generators.Snowflake;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Postgres15_0
{
    [TestFixture]
    public class Postgres15_0IndexTests : Postgres11_0IndexTests
    {
        /// <inheritdoc />
        protected override PostgresGenerator CreateGenerator(PostgresQuoter quoter)
        {
            return new Postgres15_0Generator(quoter);
        }

        [Test]
        public override void CanCreateMultiColumnUniqueIndexWithOneNonDistinctNulls()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.Columns.First().SetAdditionalFeature(PostgresExtensions.IndexColumnNullsDistinct, false);

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC,\"TestColumn2\" DESC) NULLS NOT DISTINCT;");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueIndexWithTwoNonDistinctNulls()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();

            foreach (var c in expression.Index.Columns)
            {
                c.SetAdditionalFeature(PostgresExtensions.IndexColumnNullsDistinct, false);
            }

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC,\"TestColumn2\" DESC) NULLS NOT DISTINCT;");
        }
    }
}
