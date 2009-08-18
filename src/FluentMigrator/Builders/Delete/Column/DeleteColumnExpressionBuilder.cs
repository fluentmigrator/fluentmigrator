using System;
using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Delete.Column
{
	public class DeleteColumnExpressionBuilder : ExpressionBuilderBase<DeleteColumnExpression>,
		IDeleteColumnFromTableSyntax
	{
		public DeleteColumnExpressionBuilder(DeleteColumnExpression expression)
			: base(expression)
		{
		}

		public void FromTable(string tableName)
		{
			Expression.TableName = tableName;
		}
	}
}