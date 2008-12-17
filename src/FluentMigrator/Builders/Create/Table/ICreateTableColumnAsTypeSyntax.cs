using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Create.Table
{
	public interface ICreateTableColumnAsTypeSyntax : IFluentSyntax
	{
		ICreateTableColumnOptionOrWithColumnSyntax AsInt16();
		ICreateTableColumnOptionOrWithColumnSyntax AsInt32();
		ICreateTableColumnOptionOrWithColumnSyntax AsInt64();
		ICreateTableColumnOptionOrWithColumnSyntax AsString();
		ICreateTableColumnOptionOrWithColumnSyntax AsFixedLengthString();
	}
}