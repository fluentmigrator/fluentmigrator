using System;
using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Delete.ForeignKey
{
	public class DeleteForeignKeyExpressionBuilder : ExpressionBuilderBase<DeleteForeignKeyExpression>,
		IDeleteForeignKeyFromTableSyntax, IDeleteForeignKeyForeignColumnSyntax, IDeleteForeignKeyToTableSyntax, IDeleteForeignKeyPrimaryColumnSyntax
	{
		public DeleteForeignKeyExpressionBuilder(DeleteForeignKeyExpression expression)
			: base(expression)
		{
		}

		public IDeleteForeignKeyForeignColumnSyntax FromTable(string table)
		{
			Expression.ForeignTable = table;
			return this;
		}

		public IDeleteForeignKeyToTableSyntax ForeignColumn(string column)
		{
			Expression.ForeignColumns.Add(column);
			return this;
		}

		public IDeleteForeignKeyToTableSyntax ForeignColumns(params string[] columns)
		{
			foreach (string column in columns)
				Expression.ForeignColumns.Add(column);

			return this;
		}

		public IDeleteForeignKeyPrimaryColumnSyntax ToTable(string table)
		{
			Expression.PrimaryTable = table;
			return this;
		}

		public void PrimaryColumn(string column)
		{
			Expression.PrimaryColumns.Add(column);
		}

		public void PrimaryColumns(params string[] columns)
		{
			foreach (string column in columns)
				Expression.PrimaryColumns.Add(column);
		}
	}
}