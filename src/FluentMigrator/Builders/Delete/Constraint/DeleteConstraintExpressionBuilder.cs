using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Delete.Constraint
{
    public class DeleteConstraintExpressionBuilder : ExpressionBuilderBase<DeleteConstraintExpression>, IDeleteConstraintOnTableSyntax, IDeleteConstraintColumnOrInSchemaSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CreateConstraintExpressionBuilder"/> class.
        /// </summary>
        public DeleteConstraintExpressionBuilder(DeleteConstraintExpression expression)
            : base(expression)
        {
        }

        public IDeleteConstraintColumnOrInSchemaSyntax FromTable(string tableName)
        {
            Expression.Constraint.TableName = tableName;
            return this;
        }

        public IDeleteConstraintColumnSyntax InSchema(string schemaName)
        {
            Expression.Constraint.SchemaName = schemaName;
            return this;
        }

        public void Column(string columnName)
        {
            Expression.Constraint.Columns.Add(columnName);
        }

        public void Columns(params string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                Expression.Constraint.Columns.Add(columnName);
            }
        }
    }
}
