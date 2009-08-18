using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Create.ForeignKey
{
	public interface ICreateForeignKeyToTableSyntax : IFluentSyntax
	{
		ICreateForeignKeyPrimaryColumnSyntax ToTable(string table);
	}
}