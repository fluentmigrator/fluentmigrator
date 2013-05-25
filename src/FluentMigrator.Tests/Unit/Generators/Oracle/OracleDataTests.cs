using FluentMigrator.Runner.Generators.Oracle;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    [TestFixture]
    public class OracleDataTests
    {
        protected OracleGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new OracleGenerator();
        }

        [Test]
        public void CanDeleteDataForAllRowsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataAllRowsExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM TestTable1 WHERE 1 = 1");
        }

        [Test]
        public void CanDeleteDataForMultipleRowsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataMultipleRowsExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM TestTable1 WHERE Name = 'Just''in' AND Website IS NULL; DELETE FROM TestTable1 WHERE Website = 'github.com'");
        }

        [Test]
        public void CanDeleteDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM TestTable1 WHERE Name = 'Just''in' AND Website IS NULL");
        }

        [Test]
        public void CanInsertDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();

            var expected = "INSERT ALL INTO TestTable1 (Id, Name, Website) VALUES (1, 'Just''in', 'codethinked.com')";
            expected += " INTO TestTable1 (Id, Name, Website) VALUES (2, 'Na\\te', 'kohari.org')";
            expected += " SELECT 1 FROM DUAL";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public void CanInsertGuidDataWithDefaultSchema()
        {
            //Oracle can not insert GUID data using string representation
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(System.String.Format("INSERT ALL INTO TestTable1 (guid) VALUES ('{0}') SELECT 1 FROM DUAL", GeneratorTestHelper.TestGuid.ToString()));
        }

        [Test]
        public void CanUpdateDataForAllDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE TestTable1 SET Name = 'Just''in', Age = 25 WHERE 1 = 1");
        }

        [Test]
        public void CanUpdateDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE TestTable1 SET Name = 'Just''in', Age = 25 WHERE Id = 9 AND Homepage IS NULL");
        }
    }
}
