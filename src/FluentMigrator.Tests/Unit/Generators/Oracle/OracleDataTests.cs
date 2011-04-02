using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using FluentMigrator.Runner.Generators.Oracle;
using NUnit.Should;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    public class OracleDataTests : BaseDataTests
    {
        private OracleGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new OracleGenerator();

        }
        [Test]
        public override void CanInsertData()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            string sql = generator.Generate(expression);

            string expected = "INSERT ALL INTO TestTable1 (Id, Name, Website) VALUES (1, 'Just''in', 'codethinked.com')";
            expected += @" INTO TestTable1 (Id, Name, Website) VALUES (2, 'Na\te', 'kohari.org')";
            expected += " SELECT 1 FROM DUAL";

            sql.ShouldBe(expected);
        }

        [Test]
        public override void CanDeleteData()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpression();

            var sql = generator.Generate(expression);

            sql.ShouldBe("DELETE FROM TestTable1 WHERE Name = 'Just''in' AND Website IS NULL");
        }

        [Test]
        public override void CanDeleteDataAllRows()
        {
            var expression = GeneratorTestHelper.GetDeleteDataAllRowsExpression();

            var sql = generator.Generate(expression);

            sql.ShouldBe("DELETE FROM TestTable1 WHERE 1 = 1");
        }

        [Test]
        public override void CanDeleteDataMultipleRows()
        {
            var expression = GeneratorTestHelper.GetDeleteDataMultipleRowsExpression();

            var sql = generator.Generate(expression);

            sql.ShouldBe("DELETE FROM TestTable1 WHERE Name = 'Just''in' AND Website IS NULL; DELETE FROM TestTable1 WHERE Website = 'github.com'");
        }

        [Test]
        public override void CanInsertGuidData()
        {
            //Oracle can not insert GUID data using string representation
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();


            string sql = generator.Generate(expression);

            string expected = String.Format("INSERT ALL INTO TestTable1 (guid) VALUES ('{0}') SELECT 1 FROM DUAL", GeneratorTestHelper.TestGuid.ToString());

            sql.ShouldBe(expected);
        }

        [Test]
        public override void CanUpdateData()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("UPDATE TestTable1 SET Name = 'Just''in', Age = 25 WHERE Id = 9 AND Homepage IS NULL");
        }
    }
}
