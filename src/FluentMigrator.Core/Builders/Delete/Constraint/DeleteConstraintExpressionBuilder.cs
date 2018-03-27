using System.Collections.Generic;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Delete.Constraint
{
    public class DeleteConstraintExpressionBuilder
        : ExpressionBuilderBase<DeleteConstraintExpression>,
          IDeleteConstraintOnTableSyntax,
          IDeleteConstraintInSchemaOptionsSyntax,
          ISupportAdditionalFeatures
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CreateConstraintExpressionBuilder"/> class.
        /// </summary>
        public DeleteConstraintExpressionBuilder(DeleteConstraintExpression expression)
            : base(expression)
        {
        }

        public IDictionary<string, object> AdditionalFeatures => Expression.AdditionalFeatures;

        public IDeleteConstraintInSchemaOptionsSyntax FromTable(string tableName)
        {
            Expression.Constraint.TableName = tableName;
            return this;
        }

        public IDeleteConstraintInSchemaOptionsSyntax InSchema(string schemaName)
        {
            Expression.Constraint.SchemaName = schemaName;
            return this;
        }
    }
}
