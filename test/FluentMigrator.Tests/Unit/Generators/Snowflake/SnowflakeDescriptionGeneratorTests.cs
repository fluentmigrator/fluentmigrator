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
using System.Linq;

using FluentMigrator.Runner.Generators.Snowflake;
using FluentMigrator.Runner.Processors.Snowflake;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Snowflake
{
    [TestFixture(true)]
    [TestFixture(false)]
    [Category("Generator")]
    [Category("Snowflake")]
    public class SnowflakeDescriptionGeneratorTests : BaseDescriptionGeneratorTests
    {
        protected SnowflakeGenerator Generator;
        private readonly bool _quotingEnabled;
        private const string TestSchema = "TestSchema";

        public SnowflakeDescriptionGeneratorTests(bool quotingEnabled)
        {
            _quotingEnabled = quotingEnabled;
        }

        [SetUp]
        public void Setup()
        {
            var sfOptions = _quotingEnabled ? SnowflakeOptions.QuotingEnabled() : SnowflakeOptions.QuotingDisabled();
            Generator = new SnowflakeGenerator(sfOptions);
            DescriptionGenerator = new SnowflakeDescriptionGenerator((SnowflakeQuoter)Generator.Quoter);
        }

        [Test]
        public override void GenerateDescriptionStatementsForCreateTableReturnTableDescriptionStatement()
        {
            var createTableExpression = GeneratorTestHelper.GetCreateTableWithTableDescription();
            createTableExpression.SchemaName = TestSchema;
            var statements = DescriptionGenerator.GenerateDescriptionStatements(createTableExpression);

            var result = statements.First();
            result.ShouldBe($@"COMMENT ON TABLE ""{TestSchema}"".""TestTable1"" IS 'TestDescription';", _quotingEnabled);
        }

        [Test]
        public override void
            GenerateDescriptionStatementsForCreateTableReturnTableDescriptionAndColumnDescriptionsStatements()
        {
            var createTableExpression = GeneratorTestHelper.GetCreateTableWithTableDescriptionAndColumnDescriptions();
            createTableExpression.SchemaName = TestSchema;
            var statements = DescriptionGenerator.GenerateDescriptionStatements(createTableExpression).ToArray();

            var result = string.Join(string.Empty, statements);
            result.ShouldBe(
                $@"COMMENT ON TABLE ""{TestSchema}"".""TestTable1"" IS 'TestDescription';COMMENT ON COLUMN ""{TestSchema}"".""TestTable1"".""TestColumn1"" IS 'Description:TestColumn1Description';COMMENT ON COLUMN ""{TestSchema}"".""TestTable1"".""TestColumn2"" IS 'Description:TestColumn2Description';", _quotingEnabled);
        }

        [Test]
        public override void GenerateDescriptionStatementsForCreateTableReturnTableDescriptionAndColumnDescriptionsWithAdditionalDescriptionsStatements()
        {
            var createTableExpression = GeneratorTestHelper.GetCreateTableWithTableDescriptionAndColumnDescriptionsAndAdditionalDescriptions();
            createTableExpression.SchemaName = TestSchema;
            var statements = DescriptionGenerator.GenerateDescriptionStatements(createTableExpression).ToArray();

            var result = string.Join("", statements);
            result.ShouldBe($@"COMMENT ON TABLE ""{TestSchema}"".""TestTable1"" IS 'TestDescription';COMMENT ON COLUMN ""{TestSchema}"".""TestTable1"".""TestColumn1"" IS 'Description:TestColumn1Description" + Environment.NewLine +
                $@"AdditionalColumnDescriptionKey1:AdditionalColumnDescriptionValue1';COMMENT ON COLUMN ""{TestSchema}"".""TestTable1"".""TestColumn2"" IS 'Description:TestColumn2Description" + Environment.NewLine +
                "AdditionalColumnDescriptionKey2:AdditionalColumnDescriptionValue2';", _quotingEnabled);
        }

        [Test]
        public override void GenerateDescriptionStatementForAlterTableReturnTableDescriptionStatement()
        {
            var alterTableExpression = GeneratorTestHelper.GetAlterTableWithDescriptionExpression();
            alterTableExpression.SchemaName = TestSchema;
            var statement = DescriptionGenerator.GenerateDescriptionStatement(alterTableExpression);

            statement.ShouldBe($@"COMMENT ON TABLE ""{TestSchema}"".""TestTable1"" IS 'TestDescription';", _quotingEnabled);
        }

        [Test]
        public override void GenerateDescriptionStatementForCreateColumnReturnColumnDescriptionStatement()
        {
            var createColumnExpression = GeneratorTestHelper.GetCreateColumnExpressionWithDescription();
            createColumnExpression.SchemaName = TestSchema;
            var statement = DescriptionGenerator.GenerateDescriptionStatement(createColumnExpression);

            statement.ShouldBe($@"COMMENT ON COLUMN ""{TestSchema}"".""TestTable1"".""TestColumn1"" IS 'Description:TestColumn1Description';", _quotingEnabled);
        }

        [Test]
        public override void GenerateDescriptionStatementForCreateColumnReturnColumnDescriptionStatementWithAdditionalDescriptions()
        {
            var createColumnExpression = GeneratorTestHelper.GetCreateColumnExpressionWithDescriptionWithAdditionalDescriptions();
            createColumnExpression.SchemaName = TestSchema;
            var statement = DescriptionGenerator.GenerateDescriptionStatement(createColumnExpression);

            statement.ShouldBe($@"COMMENT ON COLUMN ""{TestSchema}"".""TestTable1"".""TestColumn1"" IS 'Description:TestColumn1Description" + Environment.NewLine +
                "AdditionalColumnDescriptionKey1:AdditionalColumnDescriptionValue1';", _quotingEnabled);
        }

        [Test]
        public override void GenerateDescriptionStatementForAlterColumnReturnColumnDescriptionStatement()
        {
            var alterColumnExpression = GeneratorTestHelper.GetAlterColumnExpressionWithDescription();
            alterColumnExpression.SchemaName = TestSchema;
            var statement = DescriptionGenerator.GenerateDescriptionStatement(alterColumnExpression);

            statement.ShouldBe($@"COMMENT ON COLUMN ""{TestSchema}"".""TestTable1"".""TestColumn1"" IS 'Description:TestColumn1Description';", _quotingEnabled);
        }

        [Test]
        public override void GenerateDescriptionStatementForAlterColumnReturnColumnDescriptionStatementWithAdditionalDescriptions()
        {
            var alterColumnExpression = GeneratorTestHelper.GetAlterColumnExpressionWithDescriptionWithAdditionalDescriptions();
            alterColumnExpression.SchemaName = TestSchema;
            var statement = DescriptionGenerator.GenerateDescriptionStatement(alterColumnExpression);

            statement.ShouldBe($@"COMMENT ON COLUMN ""{TestSchema}"".""TestTable1"".""TestColumn1"" IS 'Description:TestColumn1Description" + Environment.NewLine +
                "AdditionalColumnDescriptionKey1:AdditionalColumnDescriptionValue1';", _quotingEnabled);
        }

        [Test]
        public override void GenerateDescriptionStatementsHaveSingleStatementForDescriptionOnCreate()
        {
            var createTableExpression = GeneratorTestHelper.GetCreateTableWithTableDescription();
            createTableExpression.SchemaName = TestSchema;
            var result = DescriptionGenerator.GenerateDescriptionStatements(createTableExpression);

            result.Count().ShouldBe(1);
        }
    }
}
