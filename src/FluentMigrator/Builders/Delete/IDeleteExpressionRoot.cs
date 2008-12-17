using System;
using FluentMigrator.Builders.Delete.Column;
using FluentMigrator.Builders.Delete.ForeignKey;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Delete
{
	public interface IDeleteExpressionRoot : IFluentSyntax
	{
		void Table(string tableName);
		IDeleteColumnFromTableSyntax Column(string columnName);
		IDeleteForeignKeyFromTableSyntax ForeignKey();
	}
}