using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Generators.Firebird;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Firebird
{
    [TestFixture]
    public class FirebirdDataTests
    {
        protected FirebirdGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new FirebirdGenerator(FirebirdOptions.StandardBehaviour());
        }

        [Test]
        public void CanDeleteDataForAllRowsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataAllRowsExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM \"TestTable1\" WHERE 1 = 1");

        }

        [Test]
        public void CanDeleteDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM \"TestTable1\" WHERE \"Name\" = 'Just''in' AND \"Website\" IS NULL");
        }

        [Test]
        public void CanInsertDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();

            var expected = "INSERT INTO \"TestTable1\" (\"Id\", \"Name\", \"Website\") VALUES (1, 'Just''in', 'codethinked.com');";
            expected += " INSERT INTO \"TestTable1\" (\"Id\", \"Name\", \"Website\") VALUES (2, 'Na\\te', 'kohari.org')";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public void CanInsertGuidDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(System.String.Format("INSERT INTO \"TestTable1\" (\"guid\") VALUES ('{0}')", GeneratorTestHelper.TestGuid));
        }
    }
}
