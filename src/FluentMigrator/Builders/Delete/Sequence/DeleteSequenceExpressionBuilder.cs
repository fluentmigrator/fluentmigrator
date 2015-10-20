using System;

namespace FluentMigrator.Builders.Delete.Sequence
{
    using Expressions;

    public class DeleteSequenceExpressionBuilder : ExpressionBuilderBase<DeleteSequenceExpression>, IInSchemaSyntax
    {
        public DeleteSequenceExpressionBuilder(DeleteSequenceExpression expression)
            : base(expression)
        {
        }

        public IInSchemaSyntax InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
            return this;
        }

        public IInSchemaSyntax CheckIfExists()
        {
            throw new NotImplementedException();
        }
    }
}