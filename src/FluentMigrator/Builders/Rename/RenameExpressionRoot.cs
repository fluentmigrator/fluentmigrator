using System;
using FluentMigrator.Builders.Rename.Column;
using FluentMigrator.Builders.Rename.Table;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Rename
{
	public class RenameExpressionRoot : IRenameExpressionRoot
	{
		private readonly IMigrationContext _context;

		public RenameExpressionRoot(IMigrationContext context)
		{
			_context = context;
		}

		public IRenameTableToSyntax Table(string oldName)
		{
			var expression = new RenameTableExpression { OldName = oldName };
			_context.Expressions.Add(expression);
			return new RenameTableExpressionBuilder(expression);
		}

		public IRenameColumnTableSyntax Column(string oldName)
		{
			var expression = new RenameColumnExpression { OldName = oldName };
			_context.Expressions.Add(expression);
			return new RenameColumnExpressionBuilder(expression);
		}
	}
}