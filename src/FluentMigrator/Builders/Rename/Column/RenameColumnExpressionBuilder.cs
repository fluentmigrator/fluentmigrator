using System;
using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Rename.Column
{
	public class RenameColumnExpressionBuilder : ExpressionBuilderBase<RenameColumnExpression>,
		IRenameColumnToSyntax, IRenameColumnTableSyntax
	{
		public RenameColumnExpressionBuilder(RenameColumnExpression expression)
			: base(expression)
		{
		}

		public void To(string name)
		{
			Expression.NewName = name;
		}

	    public IRenameColumnToSyntax OnTable(string tableName)
	    {
	        Expression.TableName = tableName;
	        return this;
	    }
	}
}