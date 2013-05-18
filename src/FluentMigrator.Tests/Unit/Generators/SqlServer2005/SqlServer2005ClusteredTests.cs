using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2005
{
    [TestFixture]
    public class SqlServer2005ClusteredTests
    {
        protected SqlServer2005Generator generator;

        [SetUp]
        public void Setup()
        {
            generator = new SqlServer2005Generator();
        }

        [Test]
        public void CanCreateClusteredIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.SchemaName = "TestSchema";
            expression.Index.IsClustered = true;
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [TestSchema].[TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanCreateClusteredIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.IsClustered = true;
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanCreateClusteredMultiColumnIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            expression.Index.SchemaName = "TestSchema";
            expression.Index.IsClustered = true;
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [TestSchema].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public void CanCreateClusteredMultiColumnIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            expression.Index.IsClustered = true;
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public void CanCreateClusteredUniqueIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            expression.Index.SchemaName = "TestSchema";
            expression.Index.IsClustered = true;
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [TestSchema].[TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanCreateClusteredUniqueIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            expression.Index.IsClustered = true;
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanCreatClusteredUniqueMultiColumnIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.SchemaName = "TestSchema";
            expression.Index.IsClustered = true;
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [TestSchema].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public void CanCreatClusteredUniqueMultiColumnIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.IsClustered = true;
            var sql = generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }
    }
}
