using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Create.ForeignKey
{
	public interface ICreateForeignKeyFromTableSyntax : IFluentSyntax
	{
		ICreateForeignKeyForeignColumnSyntax FromTable(string table);
	}
}