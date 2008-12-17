using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Create.Column
{
	public interface ICreateColumnOnTableSyntax : IFluentSyntax
	{
		ICreateColumnAsTypeSyntax OnTable(string name);
	}
}