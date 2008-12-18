using System;
using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Delete.Index
{
	public class DeleteIndexExpressionBuilder : ExpressionBuilderBase<DeleteIndexExpression>,
		IDeleteIndexForTableSyntax, IDeleteIndexOnColumnSyntax
	{
		public DeleteIndexExpressionBuilder(DeleteIndexExpression expression)
			: base(expression)
		{
		}

		public IDeleteIndexOnColumnSyntax OnTable(string tableName)
		{
			Expression.TableName = tableName;
			return this;
		}

		public void OnColumn(string columnName)
		{
			Expression.Columns.Add(columnName);
		}

		public void OnColumns(params string[] columnNames)
		{
			foreach (string columnName in columnNames)
				Expression.Columns.Add(columnName);
		}
	}
}