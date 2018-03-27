using FluentMigrator.Runner.Generators.DB2;
using NUnit.Framework;
using NUnit.Should;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentMigrator.Tests.Unit.Generators.Db2
{
    [TestFixture]
    public class Db2DataTests : BaseDataTests
    {
        protected Db2Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new Db2Generator();
        }

        [Test]
        public override void CanDeleteDataForAllRowsWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataAllRowsExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM TestSchema.TestTable1");
        }

        [Test]
        public override void CanDeleteDataForAllRowsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataAllRowsExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM TestTable1");
        }

        [Test]
        public override void CanDeleteDataForMultipleRowsWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataMultipleRowsExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM TestSchema.TestTable1 WHERE Name = 'Just''in' AND Website IS NULL DELETE FROM TestSchema.TestTable1 WHERE Website = 'github.com'");
        }

        [Test]
        public override void CanDeleteDataForMultipleRowsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataMultipleRowsExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM TestTable1 WHERE Name = 'Just''in' AND Website IS NULL DELETE FROM TestTable1 WHERE Website = 'github.com'");
        }

        [Test]
        public override void CanDeleteDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM TestSchema.TestTable1 WHERE Name = 'Just''in' AND Website IS NULL");
        }

        [Test]
        public override void CanDeleteDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM TestTable1 WHERE Name = 'Just''in' AND Website IS NULL");
        }

        [Test]
        public override void CanInsertDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            expression.SchemaName = "TestSchema";

            var expected = "INSERT INTO TestSchema.TestTable1 (Id, Name, Website) VALUES (1, 'Just''in', 'codethinked.com')";
            expected += " INSERT INTO TestSchema.TestTable1 (Id, Name, Website) VALUES (2, 'Na\\te', 'kohari.org')";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanInsertDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();

            var expected = "INSERT INTO TestTable1 (Id, Name, Website) VALUES (1, 'Just''in', 'codethinked.com')";
            expected += " INSERT INTO TestTable1 (Id, Name, Website) VALUES (2, 'Na\\te', 'kohari.org')";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanInsertGuidDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(System.String.Format("INSERT INTO TestSchema.TestTable1 (guid) VALUES ('{0}')", GeneratorTestHelper.TestGuid));
        }

        [Test]
        public override void CanInsertGuidDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(System.String.Format("INSERT INTO TestTable1 (guid) VALUES ('{0}')", GeneratorTestHelper.TestGuid));
        }

        [Test]
        public override void CanUpdateDataForAllDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE TestSchema.TestTable1 SET Name = 'Just''in', Age = 25");
        }

        [Test]
        public override void CanUpdateDataForAllDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE TestTable1 SET Name = 'Just''in', Age = 25");
        }

        [Test]
        public override void CanUpdateDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE TestSchema.TestTable1 SET Name = 'Just''in', Age = 25 WHERE Id = 9 AND Homepage IS NULL");
        }

        [Test]
        public override void CanUpdateDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE TestTable1 SET Name = 'Just''in', Age = 25 WHERE Id = 9 AND Homepage IS NULL");
        }
    }
}
