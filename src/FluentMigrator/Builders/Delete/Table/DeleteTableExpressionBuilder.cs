using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Delete.Table
{
    public class DeleteTableExpressionBuilder : ExpressionBuilderBase<DeleteTableExpression>, IInSchemaSyntax
    {
        public DeleteTableExpressionBuilder(DeleteTableExpression expression)
            : base(expression)
        {
        }

        public IInSchemaSyntax InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
            return this;
        }

        public IInSchemaSyntax CheckIfExists(bool enabled = true)
        {
            Expression.CheckIfExists = enabled;
            return this;
        }
    }
}