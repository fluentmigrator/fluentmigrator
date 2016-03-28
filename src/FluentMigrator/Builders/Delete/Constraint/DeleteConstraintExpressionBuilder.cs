using System;
using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Delete.Constraint
{
    public class DeleteConstraintExpressionBuilder : ExpressionBuilderBase<DeleteConstraintExpression>, IDeleteConstraintOnTableSyntax, IInSchemaSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CreateConstraintExpressionBuilder"/> class.
        /// </summary>
        public DeleteConstraintExpressionBuilder(DeleteConstraintExpression expression)
            : base(expression)
        {
        }

        public IInSchemaSyntax FromTable(string tableName)
        {
            Expression.Constraint.TableName = tableName;
            return this;
        }

        public IDeleteConstraintOnTableSyntax CheckIfExists(bool enabled = true)
        {
            Expression.CheckIfExists = enabled;
            return this;
        }

        public IInSchemaSyntax InSchema(string schemaName)
        {
            Expression.Constraint.SchemaName = schemaName;
            return this;
        }

        IInSchemaSyntax IInSchemaSyntax.CheckIfExists(bool enabled = true)
        {
            Expression.CheckIfExists = enabled;
            return this;
        }
    }
}
