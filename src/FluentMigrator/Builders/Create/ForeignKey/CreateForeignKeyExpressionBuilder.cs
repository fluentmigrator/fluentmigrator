using System;
using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Create.ForeignKey
{
	public class CreateForeignKeyExpressionBuilder : ExpressionBuilderBase<CreateForeignKeyExpression>,
		ICreateForeignKeyFromTableSyntax, ICreateForeignKeyForeignColumnSyntax, ICreateForeignKeyToTableSyntax, ICreateForeignKeyPrimaryColumnSyntax
	{
		public CreateForeignKeyExpressionBuilder(CreateForeignKeyExpression expression)
			: base(expression)
		{
		}

		public ICreateForeignKeyForeignColumnSyntax FromTable(string table)
		{
			Expression.ForeignKey.ForeignTable = table;
			return this;
		}

		public ICreateForeignKeyToTableSyntax ForeignColumn(string column)
		{
			Expression.ForeignKey.ForeignColumns.Add(column);
			return this;
		}

		public ICreateForeignKeyToTableSyntax ForeignColumns(params string[] columns)
		{
			foreach (string column in columns)
				Expression.ForeignKey.ForeignColumns.Add(column);

			return this;
		}

		public ICreateForeignKeyPrimaryColumnSyntax ToTable(string table)
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