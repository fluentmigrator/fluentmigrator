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
			Expression.ForeignTable = table;
			return this;
		}

		public ICreateForeignKeyToTableSyntax ForeignColumn(string column)
		{
			Expression.ForeignColumns.Add(column);
			return this;
		}

		public ICreateForeignKeyToTableSyntax ForeignColumns(params string[] columns)
		{
			foreach (string column in columns)
				Expression.ForeignColumns.Add(column);

			return this;
		}

		public ICreateForeignKeyPrimaryColumnSyntax ToTable(string table)
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