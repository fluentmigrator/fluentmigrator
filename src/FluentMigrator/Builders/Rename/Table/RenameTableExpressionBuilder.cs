using System;
using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Rename.Table
{
	public class RenameTableExpressionBuilder : ExpressionBuilderBase<RenameTableExpression>,
		IRenameTableToNameSyntax
	{
		public RenameTableExpressionBuilder(RenameTableExpression expression)
			: base(expression)
		{
		}

		public void To(string name)
		{
			Expression.NewName = name;
		}
	}
}