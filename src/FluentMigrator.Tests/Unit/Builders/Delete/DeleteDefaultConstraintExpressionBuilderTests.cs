using FluentMigrator.Builders.Delete.DefaultConstraint;
using FluentMigrator.Expressions;
using Xunit;

namespace FluentMigrator.Tests.Unit.Builders.Delete
{
    public class DeleteDefaultConstraintExpressionBuilderTests
    {
        [SetUp]
        public void Setup()
        {
            expression = new DeleteDefaultConstraintExpression();
            builder = new DeleteDefaultConstraintExpressionBuilder(expression);
        }

        private DeleteDefaultConstraintExpressionBuilder builder;
        private DeleteDefaultConstraintExpression expression;

        [Test]
        public void OnColumnShouldSetColumnNameOnExpression()
        {
            builder.OnColumn("column");
            expression.ColumnName.ShouldBe("column");
        }

        [Test]
        public void OnSchemaShouldSetSchemaNameOnExpression()
        {
            builder.InSchema("Shema");
            expression.SchemaName.ShouldBe("Shema");
        }

        [Test]
        public void OnTableShouldSetTableNameOnExpression()
        {
            builder.OnTable("ThaTable");
            expression.TableName.ShouldBe("ThaTable");
        }
    }
}