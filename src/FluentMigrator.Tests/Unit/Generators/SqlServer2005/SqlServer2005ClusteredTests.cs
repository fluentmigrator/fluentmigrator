using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2005
{
    [TestFixture]
    public class SqlServer2005ClusteredTests
    {
        protected SqlServer2005Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServer2005Generator();
        }

        [Test]
        public void CanCreateClusteredIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.SchemaName = "TestSchema";
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [TestSchema].[TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanCreateClusteredIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanCreateMultiColumnClusteredIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            expression.Index.SchemaName = "TestSchema";
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [TestSchema].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public void CanCreateMultiColumnClusteredIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public void CanCreateUniqueClusteredIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            expression.Index.SchemaName = "TestSchema";
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [TestSchema].[TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanCreateUniqueClusteredIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanCreateUniqueClusteredMultiColumnIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.SchemaName = "TestSchema";
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [TestSchema].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public void CanCreateUniqueClusteredMultiColumnIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }
    }
}
