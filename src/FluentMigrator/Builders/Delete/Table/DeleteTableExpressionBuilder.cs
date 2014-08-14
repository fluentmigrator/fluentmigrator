using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Delete.Table
{
    public class DeleteTableExpressionBuilder : ExpressionBuilderBase<DeleteTableExpression>, IInSchemaSyntax
    {
        public DeleteTableExpressionBuilder(DeleteTableExpression expression)
            : base(expression)
        {
        }

        public void InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
        }
    }
}