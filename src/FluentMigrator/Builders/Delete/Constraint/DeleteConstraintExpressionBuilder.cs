using System;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Delete.Constraint
{
    public class DeleteConstraintExpressionBuilder : ExpressionBuilderBase<DeleteConstraintExpression>, 
        IDeleteConstraintOnTableSyntax, 
        IDeleteConstraintInSchemaOptionsSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CreateConstraintExpressionBuilder"/> class.
        /// </summary>
        public DeleteConstraintExpressionBuilder(DeleteConstraintExpression expression)
            : base(expression)
        {
        }

        public IDeleteConstraintInSchemaOptionsSyntax ApplyOnline(OnlineMode mode = OnlineMode.On)
        {
            Expression.Constraint.ApplyOnline = mode;
            return this;
        }

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
