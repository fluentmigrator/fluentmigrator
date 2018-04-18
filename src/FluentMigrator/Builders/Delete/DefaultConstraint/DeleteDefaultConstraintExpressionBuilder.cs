using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Delete.DefaultConstraint
{
    /// <summary>
    /// An expression builder for a <see cref="DeleteDefaultConstraintExpression"/>
    /// </summary>
    public class DeleteDefaultConstraintExpressionBuilder : ExpressionBuilderBase<DeleteDefaultConstraintExpression>,
                                                            IDeleteDefaultConstraintOnTableSyntax,
                                                            IDeleteDefaultConstraintOnColumnOrInSchemaSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteDefaultConstraintExpressionBuilder"/> class.
        /// </summary>
        /// <param name="expression">The underlying expression</param>
        public DeleteDefaultConstraintExpressionBuilder(DeleteDefaultConstraintExpression expression) : base(expression)
        {
        }

        /// <inheritdoc />
        public IDeleteDefaultConstraintOnColumnOrInSchemaSyntax OnTable(string tableName)
        {
            Expression.TableName = tableName;
            return this;
        }

        /// <inheritdoc />
        public void OnColumn(string columnName)
        {
            Expression.ColumnName = columnName;
        }

        /// <inheritdoc />
        public IDeleteDefaultConstraintOnColumnSyntax InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
            return this;
        }
    }
}
