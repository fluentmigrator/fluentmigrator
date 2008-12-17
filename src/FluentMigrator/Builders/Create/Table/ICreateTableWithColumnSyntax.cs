using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Create.Table
{
	public interface ICreateTableWithColumnSyntax : IFluentSyntax
	{
		ICreateTableColumnAsTypeSyntax WithColumn(string name);
	}
}