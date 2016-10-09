using System;
using System.Data;
using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Generators.SqlServer;
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators.SqlServerCe
{
    public class SqlServerCeGeneratorTests
    {
        protected SqlServerCeGenerator Generator;

        public SqlServerCeGeneratorTests()
        {
            Generator = new SqlServerCeGenerator();
        }

        [Fact]
        public void AlterDefaultConstraintThrowsNotSupportedException()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Fact]
        public void CanCreateClusteredIndexTreatedAsNonClustered()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Fact]
        public void CanCreateMultiColumnClusteredIndexTreatedAsNonClustered()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Fact]
        public void CanCreatMultiColumnUniqueClusteredIndexTreatedAsNonClustered()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Fact]
        public void CanCreateUniqueClusteredIndexTreatedAsNonClustered()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Fact]
        [Trait("DbEngine", "SqlServerCe"), Trait("Subsystem", "Generator"), Trait("GeneratorAssert", "Table")]
        public void CanCreateTableWithNtextSizeUpTo536870911()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].Type = DbType.String;
            expression.Columns[0].Size = 536870911;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestTable1] ([TestColumn1] NTEXT NOT NULL, [TestColumn2] INT NOT NULL)");
        }

        [Fact]
        [Trait("DbEngine", "SqlServerCe"), Trait("Subsystem", "Generator"), Trait("GeneratorAssert", "Table")]
        public void CanCreateTableWithSeededIdentity()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.Columns[0].AdditionalFeatures.Add(SqlServerExtensions.IdentitySeed, 45);
            expression.Columns[0].AdditionalFeatures.Add(SqlServerExtensions.IdentityIncrement, 23);

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [TestTable1] ([TestColumn1] INT NOT NULL IDENTITY(45,23), [TestColumn2] INT NOT NULL)");
        }

        [Fact]
        public void CanCreateXmlColumn()
        {
            var expression = new CreateColumnExpression
                {
                    TableName = "TestTable1",
                    Column = new ColumnDefinition {Name = "TestColumn1", Type = DbType.Xml}
                };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD [TestColumn1] NTEXT NOT NULL");
        }

        [Fact]
        public void CanNotDropMultipleColumns()
        {
            //This does not work if column in used in constraint, index etc.
            var expression = GeneratorTestHelper.GetDeleteColumnExpression(new[] { "TestColumn1", "TestColumn2" });

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Fact]
        public void GenerateNecessaryStatementsForADeleteDefaultExpressionIsThrowsException()
        {
            var expression = new DeleteDefaultConstraintExpression { ColumnName = "Name", SchemaName = "Personalia", TableName = "Person" };

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }
    }
}
