using System.Collections.Generic;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Create.Index
{
	public class CreateIndexExpressionBuilder : ExpressionBuilderBase<CreateIndexExpression>,
		ICreateIndexForTableSyntax, ICreateIndexOnColumnSyntax, ICreateIndexColumnOptionsSyntax
	{		
		private IndexColumnDefinition currentColumnDefinition;
		
		public CreateIndexExpressionBuilder(CreateIndexExpression expression) : base(expression)
		{
		}

		public ICreateIndexOnColumnSyntax OnTable(string name)
		{
			Expression.TableName = name;
			return this;
		}

		public ICreateIndexColumnOptionsSyntax OnColumn(string name)
		{
			currentColumnDefinition = new IndexColumnDefinition();
			currentColumnDefinition.ColumnName = name;
			Expression.Columns.Add(currentColumnDefinition);
			return this;
		}

		public ICreateIndexOnColumnSyntax Ascending()
		{
			currentColumnDefinition.Ascending = true;
			return this;
		}

		public ICreateIndexOnColumnSyntax Descending()
		{
			currentColumnDefinition.Ascending = false;
			return this;
		}
	}
}