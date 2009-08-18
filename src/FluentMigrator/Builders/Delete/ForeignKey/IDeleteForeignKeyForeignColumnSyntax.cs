using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Delete.ForeignKey
{
	public interface IDeleteForeignKeyForeignColumnSyntax : IFluentSyntax
	{
		IDeleteForeignKeyToTableSyntax ForeignColumn(string column);
		IDeleteForeignKeyToTableSyntax ForeignColumns(params string[] columns);
	}
}