using System;
using FluentMigrator.Builders.Delete.Column;
using FluentMigrator.Builders.Delete.ForeignKey;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Delete
{
	public class DeleteExpressionRoot : IDeleteExpressionRoot
	{
		private readonly IMigrationContext _context;

		public DeleteExpressionRoot(IMigrationContext context)
		{
			_context = context;
		}

		public void Table(string name)
		{
			var expression = new DeleteTableExpression(name);
			_context.Expressions.Add(expression);
		}

		public IDeleteColumnFromTableSyntax Column(string name)
		{
			var expression = new DeleteColumnExpression(name);
			_context.Expressions.Add(expression);
			return new DeleteColumnExpressionBuilder(expression);
		}

		public IDeleteForeignKeyFromTableSyntax ForeignKey()
		{
			var expression = new DeleteForeignKeyExpression();
			_context.Expressions.Add(expression);
			return new DeleteForeignKeyExpressionBuilder(expression);
		}
	}
}