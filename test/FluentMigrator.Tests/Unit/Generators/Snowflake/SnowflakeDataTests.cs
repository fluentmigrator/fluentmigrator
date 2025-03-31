#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;

using FluentMigrator.Runner.Generators.Snowflake;
using FluentMigrator.Runner.Processors.Snowflake;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.Snowflake
{
    [TestFixture(true)]
    [TestFixture(false)]
    [Category("Generator")]
    [Category("Snowflake")]
    public class SnowflakeDataTests : BaseDataTests
    {
        protected SnowflakeGenerator Generator;
        private readonly bool _quotingEnabled;
        private const string TestSchema = "TestSchema";

        public SnowflakeDataTests(bool quotingEnabled)
        {
            _quotingEnabled = quotingEnabled;
        }

        [SetUp]
        public void Setup()
        {
            var sfOptions = _quotingEnabled ? SnowflakeOptions.QuotingEnabled() : SnowflakeOptions.QuotingDisabled();
            Generator = new SnowflakeGenerator(sfOptions);
        }

        [Test]
        public override void CanDeleteDataForAllRowsWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataAllRowsExpression();
            expression.SchemaName = TestSchema;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"DELETE FROM ""{TestSchema}"".""TestTable1"" WHERE 1 = 1;", _quotingEnabled);
        }

        [Test]
        public override void CanDeleteDataForAllRowsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataAllRowsExpression();
            var result = Generator.Generate(expression);
            result.ShouldBe(@"DELETE FROM ""PUBLIC"".""TestTable1"" WHERE 1 = 1;", _quotingEnabled);
        }

        [Test]
        public override void CanDeleteDataForMultipleRowsWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataMultipleRowsExpression();
            expression.SchemaName = TestSchema;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"DELETE FROM ""{TestSchema}"".""TestTable1"" WHERE ""Name"" = 'Just''in' AND ""Website"" IS NULL;DELETE FROM ""{TestSchema}"".""TestTable1"" WHERE ""Website"" = 'github.com';", _quotingEnabled);
        }

        [Test]
        public override void CanDeleteDataForMultipleRowsWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataMultipleRowsExpression();
            var result = Generator.Generate(expression);
            result.ShouldBe(@"DELETE FROM ""PUBLIC"".""TestTable1"" WHERE ""Name"" = 'Just''in' AND ""Website"" IS NULL;DELETE FROM ""PUBLIC"".""TestTable1"" WHERE ""Website"" = 'github.com';", _quotingEnabled);
        }

        [Test]
        public override void CanDeleteDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpression();
            expression.SchemaName = TestSchema;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"DELETE FROM ""{TestSchema}"".""TestTable1"" WHERE ""Name"" = 'Just''in' AND ""Website"" IS NULL;", _quotingEnabled);
        }

        [Test]
        public override void CanDeleteDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpression();
            var result = Generator.Generate(expression);
            result.ShouldBe(@"DELETE FROM ""PUBLIC"".""TestTable1"" WHERE ""Name"" = 'Just''in' AND ""Website"" IS NULL;", _quotingEnabled);
        }

        [Test]
        public override void CanDeleteDataWithDbNullCriteria()
        {
            var expression = GeneratorTestHelper.GetDeleteDataExpressionWithDbNullValue();
            var result = Generator.Generate(expression);
            result.ShouldBe(@"DELETE FROM ""PUBLIC"".""TestTable1"" WHERE ""Name"" = 'Just''in' AND ""Website"" IS NULL;", _quotingEnabled);
        }

        [Test]
        public override void CanInsertDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            expression.SchemaName = TestSchema;

            var expected = $@"INSERT INTO ""{TestSchema}"".""TestTable1"" (""Id"", ""Name"", ""Website"") VALUES (1, 'Just''in', 'codethinked.com');";
            expected += $@"INSERT INTO ""{TestSchema}"".""TestTable1"" (""Id"", ""Name"", ""Website"") VALUES (2, 'Na\te', 'kohari.org');";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected, _quotingEnabled);
        }

        [Test]
        public override void CanInsertDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            var expected = @"INSERT INTO ""PUBLIC"".""TestTable1"" (""Id"", ""Name"", ""Website"") VALUES (1, 'Just''in', 'codethinked.com');";
            expected += @"INSERT INTO ""PUBLIC"".""TestTable1"" (""Id"", ""Name"", ""Website"") VALUES (2, 'Na\te', 'kohari.org');";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected, _quotingEnabled);
        }

        [Test]
        public override void CanInsertGuidDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();
            expression.SchemaName = TestSchema;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"INSERT INTO ""{TestSchema}"".""TestTable1"" (""guid"") VALUES ('{GeneratorTestHelper.TestGuid}');", _quotingEnabled);
        }

        [Test]
        public override void CanInsertGuidDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetInsertGUIDExpression();
            var result = Generator.Generate(expression);
            result.ShouldBe($@"INSERT INTO ""PUBLIC"".""TestTable1"" (""guid"") VALUES ('{GeneratorTestHelper.TestGuid}');", _quotingEnabled);
        }

        [Test]
        public override void CanUpdateDataForAllDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();
            expression.SchemaName = TestSchema;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"UPDATE ""{TestSchema}"".""TestTable1"" SET ""Name"" = 'Just''in', ""Age"" = 25 WHERE 1 = 1;", _quotingEnabled);
        }

        [Test]
        public override void CanUpdateDataForAllDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();
            var result = Generator.Generate(expression);
            result.ShouldBe(@"UPDATE ""PUBLIC"".""TestTable1"" SET ""Name"" = 'Just''in', ""Age"" = 25 WHERE 1 = 1;", _quotingEnabled);
        }

        [Test]
        public override void CanUpdateDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();
            expression.SchemaName = TestSchema;

            var result = Generator.Generate(expression);
            result.ShouldBe($@"UPDATE ""{TestSchema}"".""TestTable1"" SET ""Name"" = 'Just''in', ""Age"" = 25 WHERE ""Id"" = 9 AND ""Homepage"" IS NULL;", _quotingEnabled);
        }

        [Test]
        public override void CanUpdateDataWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();
            var result = Generator.Generate(expression);
            result.ShouldBe(@"UPDATE ""PUBLIC"".""TestTable1"" SET ""Name"" = 'Just''in', ""Age"" = 25 WHERE ""Id"" = 9 AND ""Homepage"" IS NULL;", _quotingEnabled);
        }

        [Test]
        public override void CanUpdateDataWithDbNullCriteria()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithDbNullValue();
            var result = Generator.Generate(expression);
            result.ShouldBe(@"UPDATE ""PUBLIC"".""TestTable1"" SET ""Name"" = 'Just''in', ""Age"" = 25 WHERE ""Id"" = 9 AND ""Homepage"" IS NULL;", _quotingEnabled);
        }
    }
}
