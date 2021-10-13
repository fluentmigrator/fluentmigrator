using System;

using FluentMigrator.Postgres;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors.Postgres;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    public class PostgresDataTests : BaseDataTests
    {
        protected PostgresGenerator Generator;

        [SetUp]
        public void Setup()
        {
            var quoter = new PostgresQuoter(new PostgresOptions());
            Generator = CreateGenerator(quoter);
        }

        [Test]
        public override void CanDeleteDataForAllRowsWithCustomSchema()
        {
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
            result.ShouldBe("DELETE FROM \"public\".\"TestTable1\";");
        }

        [Test]
        public override void CanDeleteDataForMultipleRowsWithCustomSchema()
        {
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
            result.ShouldBe("DELETE FROM \"public\".\"TestTable1\" WHERE \"Name\" = 'Just''in' AND \"Website\" IS NULL;DELETE FROM \"public\".\"TestTable1\" WHERE \"Website\" = 'github.com';");
        }

        [Test]
        public override void CanDeleteDataWithCustomSchema()
        {
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
            result.ShouldBe("DELETE FROM \"public\".\"TestTable1\" WHERE \"Name\" = 'Just''in' AND \"Website\" IS NULL;");
        }

        [Test]
        public override void CanDeleteDataWithDbNullCriteria()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpressionWithDbNullValue();
            var result = Generator.Generate(expression);
            result.ShouldBe("DELETE FROM \"public\".\"TestTable1\" WHERE \"Name\" = 'Just''in' AND \"Website\" IS NULL;");
        }

        [Test]
        public override void CanInsertDataWithCustomSchema()
        {
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

            var expected = "INSERT INTO \"public\".\"TestTable1\" (\"Id\",\"Name\",\"Website\") VALUES (1,'Just''in','codethinked.com');";
            expected += "INSERT INTO \"public\".\"TestTable1\" (\"Id\",\"Name\",\"Website\") VALUES (2,'Na\\te','kohari.org');";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanInsertGuidDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Format("INSERT INTO \"TestSchema\".\"TestTable1\" (\"guid\") VALUES ('{0}');", GeneratorTestHelper.TestGuid));
        }

        [Test]
        public override void CanInsertGuidDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe(string.Format("INSERT INTO \"public\".\"TestTable1\" (\"guid\") VALUES ('{0}');", GeneratorTestHelper.TestGuid));
        }

        [Test]
        public virtual void CanInsertWithOverridingSystemValue()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            expression.AdditionalFeatures[PostgresExtensions.OverridingIdentityValues] = PostgresOverridingIdentityValuesType.System;

            Should.Throw<NotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public virtual void CanInsertWithOverridingUserValue()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            expression.AdditionalFeatures[PostgresExtensions.OverridingIdentityValues] = PostgresOverridingIdentityValuesType.User;

            Should.Throw<NotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanUpdateDataForAllDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE \"TestSchema\".\"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE 1 = 1;");
        }

        [Test]
        public override void CanUpdateDataForAllDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE \"public\".\"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE 1 = 1;");
        }

        [Test]
        public override void CanUpdateDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE \"TestSchema\".\"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE \"Id\" = 9 AND \"Homepage\" IS NULL;");
        }

        [Test]
        public override void CanUpdateDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE \"public\".\"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE \"Id\" = 9 AND \"Homepage\" IS NULL;");
        }

        [Test]
        public override void CanUpdateDataWithDbNullCriteria()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithDbNullValue();

            var result = Generator.Generate(expression);
            result.ShouldBe("UPDATE \"public\".\"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE \"Id\" = 9 AND \"Homepage\" IS NULL;");
        }

        protected virtual PostgresGenerator CreateGenerator(PostgresQuoter quoter)
        {
            return new PostgresGenerator(quoter);
        }
    }
}
