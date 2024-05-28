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

using System.Linq;

using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.SqlServer;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2008
{
    [TestFixture]
    public class SqlServer2008IndexTests : BaseIndexTests
    {
        private SqlServer2008Generator _generator;

        [SetUp]
        public void Setup()
        {
            _generator = new SqlServer2008Generator();
        }

        [Test]
        public override void CanCreateIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE INDEX [TestIndex] ON [TestSchema].[TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public override void CanCreateIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public override void CanCreateMultiColumnIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE INDEX [TestIndex] ON [TestSchema].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public override void CanCreateMultiColumnIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [TestSchema].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public override void CanCreateMultiColumnUniqueIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public override void CanCreateUniqueIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [TestSchema].[TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public override void CanCreateUniqueIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public override void CanDropIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();
            expression.Index.SchemaName = "TestSchema";

            var result = _generator.Generate(expression);
            result.ShouldBe("DROP INDEX [TestIndex] ON [TestSchema].[TestTable1]");
        }

        [Test]
        public override void CanDropIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteIndexExpression();

            var result = _generator.Generate(expression);
            result.ShouldBe("DROP INDEX [TestIndex] ON [dbo].[TestTable1]");
        }

        [Test]
        public void CanCreateUniqueIndexWithDistinctNulls()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            expression.Index.Columns.First().SetAdditionalFeature(SqlServerExtensions.IndexColumnNullsDistinct, true);

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanCreateUniqueIndexWithNonDistinctNulls()
        {
            var expression = new CreateIndexExpression()
            {
                Index =
                {
                    Name = GeneratorTestHelper.TestIndexName,
                }
            };

            var migrationContextMock = new Mock<IMigrationContext>().Object;
            var migrationMock = new Mock<IMigration>().Object;

            var builder = new CreateIndexExpressionBuilder(expression, migrationContextMock, migrationMock);
            builder
                .OnTable(GeneratorTestHelper.TestTableName1)
                .OnColumn(GeneratorTestHelper.TestColumnName1)
                .Ascending()
                .NullsNotDistinct()
                .WithOptions().Unique();

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC) WHERE [TestColumn1] IS NOT NULL");
        }

        [Test]
        public void CanCreateUniqueIndexWithNonDistinctNullsAlternativeSyntax()
        {
            var expression = new CreateIndexExpression()
            {
                Index =
                {
                    Name = GeneratorTestHelper.TestIndexName,
                }
            };

            var migrationContextMock = new Mock<IMigrationContext>().Object;
            var migrationMock = new Mock<IMigration>().Object;

            var builder = new CreateIndexExpressionBuilder(expression, migrationContextMock, migrationMock);
            builder
                .OnTable(GeneratorTestHelper.TestTableName1)
                .OnColumn(GeneratorTestHelper.TestColumnName1)
                .Unique().NullsNotDistinct();

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC) WHERE [TestColumn1] IS NOT NULL");
        }

        [Test]
        public void CanCreateMultiColumnUniqueIndexWithNonDistinctNulls()
        {
            var expression = new CreateIndexExpression()
            {
                Index =
                {
                    Name = GeneratorTestHelper.TestIndexName,
                }
            };

            var migrationContextMock = new Mock<IMigrationContext>().Object;
            var migrationMock = new Mock<IMigration>().Object;

            var builder = new CreateIndexExpressionBuilder(expression, migrationContextMock, migrationMock);
            builder
                .OnTable(GeneratorTestHelper.TestTableName1)
                .OnColumn(GeneratorTestHelper.TestColumnName1).Ascending()
                .OnColumn(GeneratorTestHelper.TestColumnName2).Descending()
                .WithOptions().UniqueNullsNotDistinct();

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC) WHERE [TestColumn1] IS NOT NULL AND [TestColumn2] IS NOT NULL");
        }

        [Test]
        public void CanCreateMultiColumnUniqueIndexWithNonDistinctNullsWithSingleColumnNullsDistinct()
        {
            var expression = new CreateIndexExpression()
            {
                Index =
                {
                    Name = GeneratorTestHelper.TestIndexName,
                }
            };

            var migrationContextMock = new Mock<IMigrationContext>().Object;
            var migrationMock = new Mock<IMigration>().Object;

            var builder = new CreateIndexExpressionBuilder(expression, migrationContextMock, migrationMock);
            builder
                .OnTable(GeneratorTestHelper.TestTableName1)
                .OnColumn(GeneratorTestHelper.TestColumnName1).Ascending().NullsDistinct()
                .OnColumn(GeneratorTestHelper.TestColumnName2).Descending()
                .WithOptions().UniqueNullsNotDistinct();

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC) WHERE [TestColumn2] IS NOT NULL");
        }

        [Test]
        public void CanCreateMultiColumnUniqueIndexWithOneNonDistinctNulls()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.Columns.First().SetAdditionalFeature(SqlServerExtensions.IndexColumnNullsDistinct, false);

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC) WHERE [TestColumn1] IS NOT NULL");
        }

        [Test]
        public void CanCreateMultiColumnUniqueIndexWithTwoNonDistinctNulls()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();

            foreach (var c in expression.Index.Columns)
            {
                c.SetAdditionalFeature(SqlServerExtensions.IndexColumnNullsDistinct, false);
            }

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC) WHERE [TestColumn1] IS NOT NULL AND [TestColumn2] IS NOT NULL");
        }

        [Test]
        public void CanCreateIndexWithFilter()
        {
            var migrationContextMock = new Mock<IMigrationContext>().Object;
            var migrationMock = new Mock<IMigration>().Object;

            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            new CreateIndexExpressionBuilder(expression, migrationContextMock, migrationMock).Filter("TestColumn2 IS NULL");

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC) WHERE TestColumn2 IS NULL");
        }

        [Test]
        public void CanCreateIndexWithIncludedColumnAndFilter()
        {
            var migrationContextMock = new Mock<IMigrationContext>().Object;
            var migrationMock = new Mock<IMigration>().Object;

            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            var x = new CreateIndexExpressionBuilder(expression, migrationContextMock, migrationMock).Filter("TestColumn2 IS NULL").Include("TestColumn3");

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC) INCLUDE ([TestColumn3]) WHERE TestColumn2 IS NULL");
        }

        [Test]
        public void CanCreateIndexWithMultipleIncludeColumnStatements()
        {
            var migrationContextMock = new Mock<IMigrationContext>().Object;
            var migrationMock = new Mock<IMigration>().Object;

            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            var x = (new CreateIndexExpressionBuilder(expression, migrationContextMock, migrationMock) as ICreateIndexOnColumnSyntax).Include("TestColumn2").Include("TestColumn3");

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC) INCLUDE ([TestColumn2], [TestColumn3])");
        }

        [Test]
        public void CanCreateIndexWithOneIncludeStatementMultipleColumns()
        {
            var migrationContextMock = new Mock<IMigrationContext>().Object;
            var migrationMock = new Mock<IMigration>().Object;

            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            var x = (new CreateIndexExpressionBuilder(expression, migrationContextMock, migrationMock) as ICreateIndexOptionsSyntax).Include("TestColumn2").Include("TestColumn3");

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC) INCLUDE ([TestColumn2], [TestColumn3])");
        }

        [Test]
        public void CanCreateIndexWithCompression()
        {
            var migrationContextMock = new Mock<IMigrationContext>().Object;
            var migrationMock = new Mock<IMigration>().Object;

            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            new CreateIndexExpressionBuilder(expression, migrationContextMock, migrationMock).WithDataCompression(DataCompressionType.Page);

            var result = _generator.Generate(expression);
            result.ShouldBe("CREATE INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC) WITH (DATA_COMPRESSION = PAGE)");
        }
    }
}
