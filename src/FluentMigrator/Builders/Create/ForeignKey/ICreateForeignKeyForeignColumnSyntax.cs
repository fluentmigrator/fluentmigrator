using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Create.ForeignKey
{
	public interface ICreateForeignKeyForeignColumnSyntax : IFluentSyntax
	{
		ICreateForeignKeyToTableSyntax ForeignColumn(string column);
		ICreateForeignKeyToTableSyntax ForeignColumns(params string[] columns);
	}
}