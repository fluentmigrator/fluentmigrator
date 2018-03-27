namespace FluentMigrator.Builders.Delete.Sequence
{
    using Expressions;

    public class DeleteSequenceExpressionBuilder : ExpressionBuilderBase<DeleteSequenceExpression>, IInSchemaSyntax
    {
        public DeleteSequenceExpressionBuilder(DeleteSequenceExpression expression)
            : base(expression)
        {
        }

        public void InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
        }
    }
}