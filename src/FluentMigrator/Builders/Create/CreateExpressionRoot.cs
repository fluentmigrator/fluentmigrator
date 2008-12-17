using System;
using FluentMigrator.Builders.Create.Column;
using FluentMigrator.Builders.Create.ForeignKey;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Create
{
	public class CreateExpressionRoot : ICreateExpressionRoot
	{
		private readonly IMigrationContext _context;

		public CreateExpressionRoot(IMigrationContext context)
		{
			_context = context;
		}

		public ICreateTableWithColumnSyntax Table(string name)
		{
			var expression = new CreateTableExpression(name);
			_context.Expressions.Add(expression);
			return new CreateTableExpressionBuilder(expression);
		}

		public ICreateColumnOnTableSyntax Column(string name)
		{
			var expression = new CreateColumnExpression(name);
			_context.Expressions.Add(expression);
			return new CreateColumnExpressionBuilder(expression);
		}

		public ICreateForeignKeyFromTableSyntax ForeignKey()
		{
			var expression = new CreateForeignKeyExpression();
			_context.Expressions.Add(expression);
			return new CreateForeignKeyExpressionBuilder(expression);
		}
	}
}