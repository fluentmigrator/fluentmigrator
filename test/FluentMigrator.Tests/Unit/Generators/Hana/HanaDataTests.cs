using FluentMigrator.Runner.Generators.Hana;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Hana
{
    [TestFixture]
    public class HanaDataTests : BaseDataTests
    {
        protected HanaGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new HanaGenerator();
        }

        [Test]
        public override void CanDeleteDataForAllRowsWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");
            var expression = GeneratorTestHelper.GetDeleteDataAllRowsExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM \"TestSchema\".\"TestTable1\";");
        }

        [Test]
        public override void CanDeleteDataForAllRowsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataAllRowsExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM \"TestTable1\" WHERE 1 = 1;");
        }

        [Test]
        public override void CanDeleteDataForMultipleRowsWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetDeleteDataMultipleRowsExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM \"TestSchema\".\"TestTable1\" WHERE \"Name\" = 'Just''in' AND \"Website\" IS NULL;DELETE FROM \"TestSchema\".\"TestTable1\" WHERE \"Website\" = 'github.com';");
        }

        [Test]
        public override void CanDeleteDataForMultipleRowsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataMultipleRowsExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM \"TestTable1\" WHERE \"Name\" = 'Just''in' AND \"Website\" IS NULL; " +
                            "DELETE FROM \"TestTable1\" WHERE \"Website\" = 'github.com';");
        }

        [Test]
        public override void CanDeleteDataWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetDeleteDataExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM \"TestSchema\".\"TestTable1\" WHERE \"Name\" = 'Just''in' AND \"Website\" IS NULL;");
        }

        [Test]
        public override void CanDeleteDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM \"TestTable1\" WHERE \"Name\" = 'Just''in' AND \"Website\" IS NULL;");
        }

        [Test]
        public override void CanInsertDataWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetInsertDataExpression();
            expression.SchemaName = "TestSchema";

            var expected = "INSERT INTO \"TestSchema\".\"TestTable1\" (\"Id\",\"Name\",\"Website\") VALUES (1,'Just''in','codethinked.com');";
            expected += "INSERT INTO \"TestSchema\".\"TestTable1\" (\"Id\",\"Name\",\"Website\") VALUES (2,'Na\\te','kohari.org');";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanInsertDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();

            var expected = "INSERT INTO \"TestTable1\" (\"Id\", \"Name\", \"Website\") VALUES (1, 'Just''in', 'codethinked.com');";
            expected += " INSERT INTO \"TestTable1\" (\"Id\", \"Name\", \"Website\") VALUES (2, 'Na\\te', 'kohari.org');";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanInsertGuidDataWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetInsertGUIDExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(System.String.Format("INSERT INTO \"TestSchema\".\"TestTable1\" (\"guid\") VALUES ('{0}')", GeneratorTestHelper.TestGuid));
        }

        [Test]
        public override void CanInsertGuidDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(System.String.Format("INSERT INTO \"TestTable1\" (\"guid\") VALUES ('{0}');", GeneratorTestHelper.TestGuid));
        }

        [Test]
        public override void CanUpdateDataForAllDataWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");


            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE \"TestSchema\".\"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE 1 = 1");
        }

        [Test]
        public override void CanUpdateDataForAllDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE \"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE 1 = 1;");
        }

        [Test]
        public override void CanUpdateDataWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetUpdateDataExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE \"TestSchema\".\"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE \"Id\" = 9 AND \"Homepage\" IS NULL");
        }

        [Test]
        public override void CanUpdateDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE \"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE \"Id\" = 9 AND \"Homepage\" IS NULL;");
        }
    }
}