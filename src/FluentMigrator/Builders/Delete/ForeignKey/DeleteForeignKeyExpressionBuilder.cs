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
			Expression.ForeignKey.ForeignTable = table;
			return this;
		}

		public IDeleteForeignKeyToTableSyntax ForeignColumn(string column)
		{
			Expression.ForeignKey.ForeignColumns.Add(column);
			return this;
		}

		public IDeleteForeignKeyToTableSyntax ForeignColumns(params string[] columns)
		{
			foreach (string column in columns)
				Expression.ForeignKey.ForeignColumns.Add(column);

			return this;
		}

		public IDeleteForeignKeyPrimaryColumnSyntax ToTable(string table)
		{
			Expression.ForeignKey.PrimaryTable = table;
			return this;
		}

		public void PrimaryColumn(string column)
		{
			Expression.ForeignKey.PrimaryColumns.Add(column);
		}

		public void PrimaryColumns(params string[] columns)
		{
			foreach (string column in columns)
				Expression.ForeignKey.PrimaryColumns.Add(column);
		}
	}
}