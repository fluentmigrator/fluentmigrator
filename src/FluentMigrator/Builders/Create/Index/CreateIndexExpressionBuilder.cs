using System;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Create.Index
{
	public class CreateIndexExpressionBuilder : ExpressionBuilderBase<CreateIndexExpression>,
		ICreateIndexForTableSyntax, ICreateIndexOnColumnSyntax, ICreateIndexColumnOptionsSyntax
	{
		public IndexColumnDefinition CurrentColumn { get; set; }
		
		public CreateIndexExpressionBuilder(CreateIndexExpression expression)
			: base(expression)
		{
		}

		public ICreateIndexOnColumnSyntax OnTable(string tableName)
		{
			Expression.Index.TableName = tableName;
			return this;
		}

		public ICreateIndexColumnOptionsSyntax OnColumn(string columnName)
		{
			CurrentColumn = new IndexColumnDefinition { Name = columnName };
			Expression.Index.Columns.Add(CurrentColumn);
			return this;
		}

		public ICreateIndexOnColumnSyntax Ascending()
		{
			CurrentColumn.Direction = Direction.Ascending;
			return this;
		}

		public ICreateIndexOnColumnSyntax Descending()
		{
			CurrentColumn.Direction = Direction.Descending;
			return this;
		}
	}
}