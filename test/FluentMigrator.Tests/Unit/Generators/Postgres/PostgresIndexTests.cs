using System;
using System.Linq;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.Postgres;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors.Postgres;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    public class PostgresIndexTests : BaseIndexTests
    {
        protected PostgresGenerator Generator;

        [SetUp]
        public void Setup()
        {
            var quoter = new PostgresQuoter(new PostgresOptions());
            Generator = CreateGenerator(quoter);
        }

        protected virtual PostgresGenerator CreateGenerator(PostgresQuoter quoter)
        {
            return new PostgresGenerator(quoter);
        }

        [Test]
        public override void CanCreateIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX \"TestIndex\" ON \"TestSchema\".\"TestTable1\" (\"TestColumn1\" ASC);");
        }

        [Test]
        public override void CanCreateIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC);");
        }

        [Test]
        public override void CanCreateMultiColumnIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX \"TestIndex\" ON \"TestSchema\".\"TestTable1\" (\"TestColumn1\" ASC,\"TestColumn2\" DESC);");
        }

        [Test]
        public override void CanCreateMultiColumnIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC,\"TestColumn2\" DESC);");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX \"TestIndex\" ON \"TestSchema\".\"TestTable1\" (\"TestColumn1\" ASC,\"TestColumn2\" DESC);");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC,\"TestColumn2\" DESC);");
        }

        [Test]
        public override void CanCreateUniqueIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX \"TestIndex\" ON \"TestSchema\".\"TestTable1\" (\"TestColumn1\" ASC);");
        }

        [Test]
        public override void CanCreateUniqueIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC);");
        }

        [Test]
        public override void CanDropIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP INDEX \"TestSchema\".\"TestIndex\";");
        }

        [Test]
        public override void CanDropIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP INDEX \"public\".\"TestIndex\";");
        }

        // This index method doesn't support ASC/DES neither NULLS sort
        [TestCase(Algorithm.Brin)]
        [TestCase(Algorithm.Gin)]
        [TestCase(Algorithm.Gist)]
        [TestCase(Algorithm.Hash)]
        [TestCase(Algorithm.Spgist)]
        public void CanCreateIndexUsingIndexAlgorithm(Algorithm algorithm)
        {
            var expression = GetCreateIndexWithExpression(x =>
            {
                var definition = x.Index.GetAdditionalFeature(PostgresExtensions.IndexAlgorithm, () => new PostgresIndexAlgorithmDefinition());
                definition.Algorithm = algorithm;
            });


            var result = Generator.Generate(expression);
            result.ShouldBe($"CREATE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" USING {algorithm.ToString().ToUpper()} (\"TestColumn1\");");
        }

        [Test]
        public void CanCreateIndexWithFilter()
        {
            var expression = GetCreateIndexWithExpression(x =>
            {
                x.Index.GetAdditionalFeature(PostgresExtensions.IndexFilter, () => "\"TestColumn1\" > 100");
            });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC) WHERE \"TestColumn1\" > 100;");
        }

        protected static CreateIndexExpression GetCreateIndexWithExpression(Action<CreateIndexExpression> additionalFeature)
        {
            var expression = new CreateIndexExpression
            {
                Index =
                {
                    Name = GeneratorTestHelper.TestIndexName,
                    TableName = GeneratorTestHelper.TestTableName1
                }
            };

            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = GeneratorTestHelper.TestColumnName1 });

            additionalFeature(expression);

            return expression;
        }

        [Test]
        public void CanCreateIndexAsConcurrently()
        {
            var expression = GetCreateIndexWithExpression(
                x =>
                {
                    var definitionIsOnly = x.Index.GetAdditionalFeature(PostgresExtensions.Concurrently, () => new PostgresIndexConcurrentlyDefinition());
                    definitionIsOnly.IsConcurrently = true;
                });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX CONCURRENTLY \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC);");
        }

        [Test]
        public virtual void CanCreateIndexAsOnly()
        {
            var expression = GetCreateIndexWithExpression(
                x =>
                {
                    var definitionIsOnly = x.Index.GetAdditionalFeature(PostgresExtensions.Only, () => new PostgresIndexOnlyDefinition());
                    definitionIsOnly.IsOnly = true;
                });

            Assert.Throws<NotSupportedException>(() => Generator.Generate(expression));
        }

        [TestCase(NullSort.First)]
        [TestCase(NullSort.Last)]
        public void CanCreateIndexWithNulls(NullSort sort)
        {
            var expression = GetCreateIndexWithExpression(x =>
            {
                x.Index.Columns.First().GetAdditionalFeature(
                    PostgresExtensions.NullsSort,
                    () => new PostgresIndexNullsSort { Sort = sort });
            });

            var result = Generator.Generate(expression);
            result.ShouldBe($"CREATE INDEX \"TestIndex\" ON \"public\".\"TestTable1\" (\"TestColumn1\" ASC NULLS {sort.ToString().ToUpper()});");
        }

    }
}
