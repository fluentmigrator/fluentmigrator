using FluentMigrator.Exceptions;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.MySql;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.MySql
{
    [TestFixture]
    public class MySqlGeneratorTests
    {
        protected MySqlGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new MySqlGenerator();
        }

        [Test]
        public void CanAlterSchemaInStrictMode()
        {
            Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CanCreateSchemaInStrictMode()
        {
            Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CanDropSchemaInStrictMode()
        {
            Generator.compatabilityMode = Runner.CompatabilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new DeleteSchemaExpression()));
        }

        [Test]
        public void CanUseSystemMethodCurrentDateTimeAsADefaultValueForAColumn()
        {
            var columnDefinition = new ColumnDefinition { Name = "NewColumn", Size = 15, Type = null, CustomType = "TIMESTAMP", DefaultValue = SystemMethods.CurrentDateTime };
            var expression = new CreateColumnExpression { Column = columnDefinition, TableName = "NewTable" };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `NewTable` ADD COLUMN `NewColumn` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP");
        }

        [Test]
        public void CanAlterDefaultConstraintToCurrentTimestamp()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            expression.DefaultValue = SystemMethods.CurrentDateTime;

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ALTER `TestColumn1` SET DEFAULT CURRENT_TIMESTAMP");
        }

        [Test]
        public void CanDeleteDefaultConstraint()
        {
            var expression = new DeleteDefaultConstraintExpression
            {
                ColumnName = "TestColumn1",
                TableName = "TestTable1"
            };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE `TestTable1` ALTER `TestColumn1` DROP DEFAULT");
        }
    }
}
