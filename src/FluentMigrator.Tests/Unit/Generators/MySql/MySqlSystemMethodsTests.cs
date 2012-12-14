using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.MySql;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.MySql
{
    [TestFixture]
    public class MySqlSystemMethodsTests
    {
        private MySqlGenerator generator;

        [SetUp]
        public void SetUp()
        {
            generator = new MySqlGenerator();
        }

        [Test]
        public void CanUseSystemMethodCurrentDateTimeAsADefaultValueForAColumn()
        {
            var columnDefinition = new ColumnDefinition { Name = "NewColumn", Size = 15, Type = null, CustomType = "TIMESTAMP", DefaultValue = SystemMethods.CurrentDateTime };
            var expression = new CreateColumnExpression {Column = columnDefinition, TableName = "NewTable"};

            string sql = generator.Generate(expression);

            sql.ShouldBe("ALTER TABLE `NewTable` ADD COLUMN `NewColumn` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP");
        }
    }
}