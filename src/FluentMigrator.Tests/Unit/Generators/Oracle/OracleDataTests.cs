using System;
using NUnit.Framework;
using FluentMigrator.Runner.Generators.Oracle;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    public class OracleDataTests : BaseDataTests
    {
        private OracleGenerator generator;
	    private OracleGenerator quotedIdentiferGenerator;

	    [SetUp]
        public void Setup()
        {
            generator = new OracleGenerator();
			quotedIdentiferGenerator = new OracleGenerator(true);

        }
        [Test]
        public override void CanInsertData()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            string sql = quotedIdentiferGenerator.Generate(expression);

            string expected = "INSERT ALL INTO \"TestTable1\" (\"Id\", \"Name\", \"Website\") VALUES (1, 'Just''in', 'codethinked.com')";
            expected += " INTO \"TestTable1\" (\"Id\", \"Name\", \"Website\") VALUES (2, 'Na\\te', 'kohari.org')";
            expected += " SELECT 1 FROM DUAL";

            sql.ShouldBe(expected);

			sql = generator.Generate(expression);
			sql.ShouldBe(expected.Replace("\"",""));
        }

        [Test]
        public override void CanDeleteData()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("DELETE FROM TestTable1 WHERE Name = 'Just''in' AND Website IS NULL");

			sql = quotedIdentiferGenerator.Generate(expression);
			sql.ShouldBe("DELETE FROM \"TestTable1\" WHERE \"Name\" = 'Just''in' AND \"Website\" IS NULL");
        }

        [Test]
        public override void CanDeleteDataAllRows()
        {
            var expression = GeneratorTestHelper.GetDeleteDataAllRowsExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("DELETE FROM TestTable1 WHERE 1 = 1");

			sql = quotedIdentiferGenerator.Generate(expression);
			sql.ShouldBe("DELETE FROM \"TestTable1\" WHERE 1 = 1");
        }

        [Test]
        public override void CanDeleteDataMultipleRows()
        {
            var expression = GeneratorTestHelper.GetDeleteDataMultipleRowsExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("DELETE FROM TestTable1 WHERE Name = 'Just''in' AND Website IS NULL; DELETE FROM TestTable1 WHERE Website = 'github.com'");

			sql = quotedIdentiferGenerator.Generate(expression);
			sql.ShouldBe("DELETE FROM \"TestTable1\" WHERE \"Name\" = 'Just''in' AND \"Website\" IS NULL; DELETE FROM \"TestTable1\" WHERE \"Website\" = 'github.com'");
        }

        [Test]
        public override void CanInsertGuidData()
        {
            //Oracle can not insert GUID data using string representation
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();


            string sql = generator.Generate(expression);
            sql.ShouldBe(String.Format("INSERT ALL INTO TestTable1 (guid) VALUES ('{0}') SELECT 1 FROM DUAL", GeneratorTestHelper.TestGuid.ToString()));

			sql = quotedIdentiferGenerator.Generate(expression);
			sql.ShouldBe(String.Format("INSERT ALL INTO \"TestTable1\" (\"guid\") VALUES ('{0}') SELECT 1 FROM DUAL", GeneratorTestHelper.TestGuid.ToString()));
        }

        [Test]
        public override void CanUpdateData()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("UPDATE TestTable1 SET Name = 'Just''in', Age = 25 WHERE Id = 9 AND Homepage IS NULL");

			sql = quotedIdentiferGenerator.Generate(expression);
			sql.ShouldBe("UPDATE \"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE \"Id\" = 9 AND \"Homepage\" IS NULL");
        }

        [Test]
        public void CanUpdateDataForAllRows()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();

            var sql = generator.Generate(expression);
            sql.ShouldBe("UPDATE TestTable1 SET Name = 'Just''in', Age = 25 WHERE 1 = 1");

			sql = quotedIdentiferGenerator.Generate(expression);
			sql.ShouldBe("UPDATE \"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE 1 = 1");
        }
    }
}
