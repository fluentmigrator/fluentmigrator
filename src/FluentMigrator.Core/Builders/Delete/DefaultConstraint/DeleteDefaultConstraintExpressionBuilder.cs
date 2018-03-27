using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Delete.DefaultConstraint
{
    public class DeleteDefaultConstraintExpressionBuilder : ExpressionBuilderBase<DeleteDefaultConstraintExpression>,
                                                            IDeleteDefaultConstraintOnTableSyntax,
                                                            IDeleteDefaultConstraintOnColumnOrInSchemaSyntax
    {
        public DeleteDefaultConstraintExpressionBuilder(DeleteDefaultConstraintExpression expression) : base(expression)
        {
        }

        public IDeleteDefaultConstraintOnColumnOrInSchemaSyntax OnTable(string tableName)
        {
            Expression.TableName = tableName;
            return this;
        }

        public void OnColumn(string columnName)
        {
            Expression.ColumnName = columnName;
        }

        public IDeleteDefaultConstraintOnColumnSyntax InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
            return this;
        }
    }
}