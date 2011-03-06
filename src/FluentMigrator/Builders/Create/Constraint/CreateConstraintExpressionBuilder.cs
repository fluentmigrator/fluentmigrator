using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Create.Constraint
{

    public class CreateConstraintExpressionBuilder : ExpressionBuilderBase<CreateConstraintExpression>,ICreateConstraintOnTableSyntax,ICreateConstraintColumnsSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CreateConstraintExpressionBuilder"/> class.
        /// </summary>
        public CreateConstraintExpressionBuilder(CreateConstraintExpression expression) : base(expression)
        {

        }

        public ICreateConstraintColumnsSyntax OnTable(string tableName)
        {
            Expression.Constraint.TableName = tableName;
            return this;
        }

        public void Column(string columnName)
        {
            Expression.Constraint.Columns.Add(columnName);
        }

        public void Columns(string[] columnNames)
        {
            foreach(var columnName in columnNames){
                Expression.Constraint.Columns.Add(columnName);
            }
        }


    }
}
