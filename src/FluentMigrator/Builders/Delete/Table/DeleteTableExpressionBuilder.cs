using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Delete.Table
{
    public class DeleteTableExpressionBuilder : ExpressionBuilderBase<DeleteTableExpression>, IIfExistsOrInSchemaSynatax
    {
        public DeleteTableExpressionBuilder(DeleteTableExpression expression)
            : base(expression)
        {
            Expression.IfExists = false;
        }

        public IInSchemaSyntax IfExists()
        {
            Expression.IfExists = true;
            return this;
        }

        public void InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
        }
    }
}