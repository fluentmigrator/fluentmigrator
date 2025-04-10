#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using FluentMigrator.Runner.Generators.Redshift;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Redshift
{
    [TestFixture]
    [Category("Redshift")]
    public sealed class RedshiftDataTests : BaseDataTests
    {
        private RedshiftGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new RedshiftGenerator();
        }

        [Test]
        public override void CanDeleteDataForAllRowsWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataAllRowsExpression();
            expression.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe("DELETE FROM \"TestSchema\".\"TestTable1\" WHERE 1 = 1;");
        }

        [Test]
        public override void CanDeleteDataForAllRowsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataAllRowsExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe("DELETE FROM \"public\".\"TestTable1\" WHERE 1 = 1;");
        }

        [Test]
        public override void CanDeleteDataForMultipleRowsWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataMultipleRowsExpression();
            expression.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "DELETE FROM \"TestSchema\".\"TestTable1\" WHERE \"Name\" = 'Just''in' AND \"Website\" IS NULL;DELETE FROM \"TestSchema\".\"TestTable1\" WHERE \"Website\" = 'github.com';");
        }

        [Test]
        public override void CanDeleteDataForMultipleRowsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataMultipleRowsExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "DELETE FROM \"public\".\"TestTable1\" WHERE \"Name\" = 'Just''in' AND \"Website\" IS NULL;DELETE FROM \"public\".\"TestTable1\" WHERE \"Website\" = 'github.com';");
        }

        [Test]
        public override void CanDeleteDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpression();
            expression.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "DELETE FROM \"TestSchema\".\"TestTable1\" WHERE \"Name\" = 'Just''in' AND \"Website\" IS NULL;");
        }

        [Test]
        public override void CanDeleteDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "DELETE FROM \"public\".\"TestTable1\" WHERE \"Name\" = 'Just''in' AND \"Website\" IS NULL;");
        }

        public override void CanDeleteDataWithDbNullCriteria()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpressionWithDbNullValue();
            var result = _generator.Generate(expression);
            result.ShouldBe(
                "DELETE FROM \"public\".\"TestTable1\" WHERE \"Name\" = 'Just''in' AND \"Website\" IS NULL;");
        }

        [Test]
        public override void CanInsertDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            expression.SchemaName = "TestSchema";

            var expected =
                "INSERT INTO \"TestSchema\".\"TestTable1\" (\"Id\",\"Name\",\"Website\") VALUES (1,'Just''in','codethinked.com');";
            expected +=
                "INSERT INTO \"TestSchema\".\"TestTable1\" (\"Id\",\"Name\",\"Website\") VALUES (2,'Na\\te','kohari.org');";

            var result = _generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanInsertDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();

            var expected =
                "INSERT INTO \"public\".\"TestTable1\" (\"Id\",\"Name\",\"Website\") VALUES (1,'Just''in','codethinked.com');";
            expected +=
                "INSERT INTO \"public\".\"TestTable1\" (\"Id\",\"Name\",\"Website\") VALUES (2,'Na\\te','kohari.org');";

            var result = _generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanInsertGuidDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();
            expression.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(string.Format("INSERT INTO \"TestSchema\".\"TestTable1\" (\"guid\") VALUES ('{0}');",
                                                 GeneratorTestHelper.TestGuid));
        }

        [Test]
        public override void CanInsertGuidDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe(string.Format("INSERT INTO \"public\".\"TestTable1\" (\"guid\") VALUES ('{0}');",
                                                 GeneratorTestHelper.TestGuid));
        }

        [Test]
        public override void CanUpdateDataForAllDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();
            expression.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe("UPDATE \"TestSchema\".\"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE 1 = 1;");
        }

        [Test]
        public override void CanUpdateDataForAllDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();

            var result = _generator.Generate(expression);
            result.ShouldBe("UPDATE \"public\".\"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE 1 = 1;");
        }

        [Test]
        public override void CanUpdateDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();
            expression.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "UPDATE \"TestSchema\".\"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE \"Id\" = 9 AND \"Homepage\" IS NULL;");
        }

        [Test]
        public override void CanUpdateDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "UPDATE \"public\".\"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE \"Id\" = 9 AND \"Homepage\" IS NULL;");
        }

        public override void CanUpdateDataWithDbNullCriteria()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithDbNullValue();

            var result = _generator.Generate(expression);
            result.ShouldBe(
                "UPDATE \"public\".\"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE \"Id\" = 9 AND \"Homepage\" IS NULL;");
        }
    }
}
