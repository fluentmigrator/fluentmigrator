using System;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.MySql;
using FluentMigrator.Runner.Generators.MySql;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.MySql8
{
    [TestFixture]
    public class MySql8IndexTests : BaseIndexTests
    {
        protected MySql8Generator Generator;

        [SetUp]
        public void Setup()
        {
            var quoter = new MySqlQuoter();
            Generator = CreateGenerator(quoter);
        }

        protected virtual MySql8Generator CreateGenerator(MySqlQuoter quoter)
        {
            return new MySql8Generator(quoter);
        }

        [Test]
        public override void CanCreateIndexWithCustomSchema()
        {

        }

        [Test]
        public override void CanCreateIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX `TestIndex` ON `TestTable1` (`TestColumn1` ASC);");
        }

        [Test]
        public override void CanCreateMultiColumnIndexWithCustomSchema()
        {
        }

        [Test]
        public override void CanCreateMultiColumnIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(
                "CREATE INDEX `TestIndex` ON `TestTable1` (`TestColumn1` ASC, `TestColumn2` DESC);");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueIndexWithCustomSchema()
        {

        }

        [Test]
        public override void CanCreateMultiColumnUniqueIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(
                "CREATE UNIQUE INDEX `TestIndex` ON `TestTable1` (`TestColumn1` ASC, `TestColumn2` DESC);");
        }

        [Test]
        public override void CanCreateUniqueIndexWithCustomSchema()
        {
        }

        [Test]
        public override void CanCreateUniqueIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX `TestIndex` ON `TestTable1` (`TestColumn1` ASC);");
        }

        [Test]
        public override void CanDropIndexWithCustomSchema()
        {
        }

        [Test]
        public override void CanDropIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP INDEX `TestIndex` ON `TestTable1`;");
        }

        // This index method doesn't support ASC/DES neither NULLS sort
        [TestCase(IndexType.Hash)]
        public void CanCreateIndexUsingIndexAlgorithm(IndexType algorithm)
        {
            var expression = GetCreateIndexWithExpression(
                x =>
                {
                    var definition = x.Index.GetAdditionalFeature(
                        MySqlExtensions.IndexType,
                        () => new MySqlIndexTypeDefinition());
                    definition.IndexType = algorithm;
                });


            var result = Generator.Generate(expression);
            result.ShouldBe(
                $"CREATE INDEX `TestIndex` USING {algorithm.ToString().ToUpper()} ON `TestTable1` (`TestColumn1`);");
        }

        protected static CreateIndexExpression GetCreateIndexWithExpression(
            Action<CreateIndexExpression> additionalFeature)
        {
            var expression = new CreateIndexExpression
            {
                Index =
                {
                    Name = GeneratorTestHelper.TestIndexName,
                    TableName = GeneratorTestHelper.TestTableName1
                }
            };

            expression.Index.Columns.Add(
                new IndexColumnDefinition
                    { Direction = Direction.Ascending, Name = GeneratorTestHelper.TestColumnName1 });

            additionalFeature(expression);

            return expression;
        }
    }
}
