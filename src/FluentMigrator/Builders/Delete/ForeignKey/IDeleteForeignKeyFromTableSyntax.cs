using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Delete.ForeignKey
{
	public interface IDeleteForeignKeyFromTableSyntax : IFluentSyntax
	{
		IDeleteForeignKeyForeignColumnSyntax FromTable(string table);
	}
}