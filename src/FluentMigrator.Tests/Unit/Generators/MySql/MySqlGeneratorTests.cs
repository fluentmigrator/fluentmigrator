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
        protected MySqlGenerator _generator;
        protected MySqlGenerator generator;

        [SetUp]
        public void Setup()
        {
            _generator = new MySqlGenerator();
            generator = new MySqlGenerator();
        }

        [Test]
        public void CanAlterSchemaInStrictMode()
        {
            _generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CanCreateSchemaInStrictMode()
        {
            _generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CanDropSchemaInStrictMode()
        {
            _generator.compatabilityMode = Runner.CompatabilityMode.STRICT;
            Assert.Throws<DatabaseOperationNotSupportedException>(() => _generator.Generate(new DeleteSchemaExpression()));
        }

        [Test]
        public void CanUseSystemMethodCurrentDateTimeAsADefaultValueForAColumn()
        {
            var columnDefinition = new ColumnDefinition { Name = "NewColumn", Size = 15, Type = null, CustomType = "TIMESTAMP", DefaultValue = SystemMethods.CurrentDateTime };
            var expression = new CreateColumnExpression { Column = columnDefinition, TableName = "NewTable" };

            string sql = generator.Generate(expression);

            sql.ShouldBe("ALTER TABLE `NewTable` ADD COLUMN `NewColumn` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP");
        }
    }
}
