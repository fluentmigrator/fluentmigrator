using FluentMigrator.Builders.Delete.DefaultConstraint;
using FluentMigrator.Expressions;
using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Builders.Delete
{
    [TestFixture]
    public class DeleteDefaultConstraintExpressionBuilderTests
    {
        [SetUp]
        public void Setup()
        {
            _expression = new DeleteDefaultConstraintExpression();
            _builder = new DeleteDefaultConstraintExpressionBuilder(_expression);
        }

        private DeleteDefaultConstraintExpressionBuilder _builder;
        private DeleteDefaultConstraintExpression _expression;

        [Test]
        public void OnColumnShouldSetColumnNameOnExpression()
        {
            _builder.OnColumn("column");
            _expression.ColumnName.ShouldBe("column");
        }

        [Test]
        public void OnSchemaShouldSetSchemaNameOnExpression()
        {
            _builder.InSchema("Shema");
            _expression.SchemaName.ShouldBe("Shema");
        }

        [Test]
        public void OnTableShouldSetTableNameOnExpression()
        {
            _builder.OnTable("ThaTable");
            _expression.TableName.ShouldBe("ThaTable");
        }
    }
}
