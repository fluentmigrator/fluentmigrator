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
using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Builders.Create;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Snowflake;
using FluentMigrator.Runner.Processors.Snowflake;

using Moq;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.Snowflake
{
    [TestFixture(true)]
    [TestFixture(false)]
    [Category("Generator")]
    [Category("Snowflake")]
    public class SnowflakeTableTests : BaseTableTests
    {
        protected SnowflakeGenerator Generator;
        private readonly bool _quotingEnabled;

        public SnowflakeTableTests(bool quotingEnabled)
        {
            _quotingEnabled = quotingEnabled;
        }

        [SetUp]
        public void Setup()
        {
            var sfOptions = _quotingEnabled ? SnowflakeOptions.QuotingEnabled() : SnowflakeOptions.QuotingDisabled();
            Generator = new SnowflakeGenerator(sfOptions);
        }

        /*
        [Test]
        public void CanCreateTableWithIgnoredRowGuidCol()
        {
            var expression = new CreateTableExpression
            {
                TableName = "TestTable1",
            };

            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var querySchema = new Mock<IQuerySchema>();
            new CreateTableExpressionBuilder(expression, new MigrationContext(querySchema.Object, serviceProvider))
                .WithColumn("Id").AsGuid().PrimaryKey();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([Id] UNIQUEIDENTIFIER NOT NULL ROWGUIDCOL, PRIMARY KEY ([Id]))", _quotingEnabled);
        } */

        [Test]
        public override void CanCreateTableWithCustomColumnTypeWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "timestamp";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""TestSchema"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL, ""TestColumn2"" timestamp NOT NULL, PRIMARY KEY (""TestColumn1""))", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithCustomColumnTypeWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "[timestamp]";

            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""TestSchema"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL, ""TestColumn2"" NUMBER NOT NULL)", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNullWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;

            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""TestSchema"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL DEFAULT NULL, ""TestColumn2"" NUMBER NOT NULL)", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNullWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;

            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateTableWithDefaultValueWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""TestSchema"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL DEFAULT 'Default', ""TestColumn2"" NUMBER NOT NULL DEFAULT 0)", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithDefaultValueWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateTableWithIdentityWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""TestSchema"".""TestTable1"" (""TestColumn1"" NUMBER NOT NULL IDENTITY(1,1), ""TestColumn2"" NUMBER NOT NULL)", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithIdentityWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""TestSchema"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL, ""TestColumn2"" NUMBER NOT NULL, PRIMARY KEY (""TestColumn1"", ""TestColumn2""))", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithNamedMultiColumnPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""TestSchema"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL, ""TestColumn2"" NUMBER NOT NULL, CONSTRAINT ""TestKey"" PRIMARY KEY (""TestColumn1"", ""TestColumn2""))", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithNamedMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateTableWithNamedPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""TestSchema"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL, ""TestColumn2"" NUMBER NOT NULL, CONSTRAINT ""TestKey"" PRIMARY KEY (""TestColumn1""))", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithNamedPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateTableWithNullableFieldWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";
            expression.Columns[0].IsNullable = true;

            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""TestSchema"".""TestTable1"" (""TestColumn1"" VARCHAR, ""TestColumn2"" NUMBER NOT NULL)", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithNullableFieldWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsNullable = true;
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanCreateTableWithPrimaryKeyWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"CREATE TABLE ""TestSchema"".""TestTable1"" (""TestColumn1"" VARCHAR NOT NULL, ""TestColumn2"" NUMBER NOT NULL, PRIMARY KEY (""TestColumn1""))", _quotingEnabled);
        }

        [Test]
        public override void CanCreateTableWithPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public void CanCreateTableWithForeignKeyColumnWithDefaultSchema()
        {
            var expressions = new List<IMigrationExpression>();
            var migrationContexMock = new Mock<IMigrationContext>();
            migrationContexMock.SetupGet(mc => mc.Expressions).Returns(expressions);
            var migrationContext = migrationContexMock.Object;
            new CreateExpressionRoot(migrationContext)
                .Table("FooTable")
                .WithColumn("FooColumn").AsInt32().ForeignKey("BarTable", "BarColumn");
            var createTableExpression = migrationContext.Expressions.OfType<CreateTableExpression>().First();
            var createForeignKeyExpression = migrationContext.Expressions.OfType<CreateForeignKeyExpression>().First();

            var processed = createForeignKeyExpression.Apply(ConventionSets.NoSchemaName);
            Assert.Throws<ArgumentException>(() => Generator.Generate(createTableExpression));
            Assert.Throws<ArgumentException>(() => Generator.Generate(processed));
        }

        [Test]
        public void CanCreateTableWithForeignKeyColumnWithCustomSchema()
        {
            var expressions = new List<IMigrationExpression>();
            var migrationContexMock = new Mock<IMigrationContext>();
            migrationContexMock.SetupGet(mc => mc.Expressions).Returns(expressions);
            var migrationContext = migrationContexMock.Object;
            new CreateExpressionRoot(migrationContext)
                .Table("FooTable").InSchema("FooSchema")
                .WithColumn("FooColumn").AsInt32().ForeignKey("fk_bar_foo", "BarSchema", "BarTable", "BarColumn");
            var createTableExpression = migrationContext.Expressions.OfType<CreateTableExpression>().First();
            var createForeignKeyExpression = migrationContext.Expressions.OfType<CreateForeignKeyExpression>().First();

            var createTableResult = Generator.Generate(createTableExpression);
            var createForeignKeyResult = Generator.Generate(createForeignKeyExpression);
            createTableResult.ShouldBe(@"CREATE TABLE ""FooSchema"".""FooTable"" (""FooColumn"" NUMBER NOT NULL)", _quotingEnabled);
            createForeignKeyResult.ShouldBe(@"ALTER TABLE ""FooSchema"".""FooTable"" ADD CONSTRAINT ""fk_bar_foo"" FOREIGN KEY (""FooColumn"") REFERENCES ""BarSchema"".""BarTable"" (""BarColumn"")", _quotingEnabled);
        }

        [Test]
        public override void CanDropTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"DROP TABLE ""TestSchema"".""TestTable1""", _quotingEnabled);
        }

        [Test]
        public override void CanDropTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }

        [Test]
        public override void CanRenameTableWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe(@"ALTER TABLE ""TestSchema"".""TestTable1"" RENAME TO ""TestSchema"".""TestTable2""", _quotingEnabled);
        }

        [Test]
        public override void CanRenameTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();
            Assert.Throws<ArgumentException>(() => Generator.Generate(expression));
        }
    }
}
