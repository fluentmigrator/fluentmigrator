using System;
using FluentMigrator.Builders.Create.Column;
using FluentMigrator.Builders.Create.ForeignKey;
using FluentMigrator.Builders.Create.Index;
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

		public ICreateTableWithColumnSyntax Table(string tableName)
		{
			var expression = new CreateTableExpression { TableName = tableName };
			_context.Expressions.Add(expression);
			return new CreateTableExpressionBuilder(expression);
		}

		public ICreateColumnOnTableSyntax Column(string columnName)
		{
			var expression = new CreateColumnExpression { Column = { Name = columnName } };
			_context.Expressions.Add(expression);
			return new CreateColumnExpressionBuilder(expression);
		}

		public ICreateForeignKeyFromTableSyntax ForeignKey()
		{
			var expression = new CreateForeignKeyExpression();
			_context.Expressions.Add(expression);
			return new CreateForeignKeyExpressionBuilder(expression);
		}

		public ICreateIndexForTableSyntax Index()
		{
			var expression = new CreateIndexExpression();
			_context.Expressions.Add(expression);
			return new CreateIndexExpressionBuilder(expression);
		}
	}
}