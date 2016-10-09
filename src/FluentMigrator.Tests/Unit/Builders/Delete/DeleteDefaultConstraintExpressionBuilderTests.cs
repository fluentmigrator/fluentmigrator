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

        [Fact]
        public void OnColumnShouldSetColumnNameOnExpression()
        {
            builder.OnColumn("column");
            expression.ColumnName.ShouldBe("column");
        }

        [Fact]
        public void OnSchemaShouldSetSchemaNameOnExpression()
        {
            builder.InSchema("Shema");
            expression.SchemaName.ShouldBe("Shema");
        }

        [Fact]
        public void OnTableShouldSetTableNameOnExpression()
        {
            builder.OnTable("ThaTable");
            expression.TableName.ShouldBe("ThaTable");
        }
    }
}