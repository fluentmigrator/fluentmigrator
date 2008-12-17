using System;
using FluentMigrator.Builders.Delete.Column;
using FluentMigrator.Builders.Delete.ForeignKey;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Delete
{
	public interface IDeleteExpressionRoot : IFluentSyntax
	{
		void Table(string name);
		IDeleteColumnFromTableSyntax Column(string name);
		IDeleteForeignKeyFromTableSyntax ForeignKey();
	}
}