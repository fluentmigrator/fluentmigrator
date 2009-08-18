using System;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Delete.Index
{
	public class DeleteIndexExpressionBuilder : ExpressionBuilderBase<DeleteIndexExpression>,
		IDeleteIndexForTableSyntax, IDeleteIndexOnColumnSyntax
	{
		public IndexColumnDefinition CurrentColumn { get; set; }

		public DeleteIndexExpressionBuilder(DeleteIndexExpression expression)
			: base(expression)
		{
		}

		public IDeleteIndexOnColumnSyntax OnTable(string tableName)
		{
			Expression.Index.TableName = tableName;
			return this;
		}

		public void OnColumn(string columnName)
		{
			var column = new IndexColumnDefinition { Name = columnName };
			Expression.Index.Columns.Add(column);
		}

		public void OnColumns(params string[] columnNames)
		{
			foreach (string columnName in columnNames)
				Expression.Index.Columns.Add(new IndexColumnDefinition { Name = columnName });
		}
	}
}