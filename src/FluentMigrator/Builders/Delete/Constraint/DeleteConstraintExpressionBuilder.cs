using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Delete.Constraint
{
    public class DeleteConstraintExpressionBuilder : ExpressionBuilderBase<DeleteConstraintExpression>, IDeleteConstraintOnTableSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CreateConstraintExpressionBuilder"/> class.
        /// </summary>
        public DeleteConstraintExpressionBuilder(DeleteConstraintExpression expression)
            : base(expression)
        {

        }

        public void FromTable(string tableName)
        {
            Expression.Constraint.TableName = tableName;
        }

        
    }
}
