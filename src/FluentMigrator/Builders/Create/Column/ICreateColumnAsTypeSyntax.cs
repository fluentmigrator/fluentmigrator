using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Create.Column
{
	public interface ICreateColumnAsTypeSyntax : IFluentSyntax
	{
		ICreateColumnOptionSyntax AsInt16();
		ICreateColumnOptionSyntax AsInt32();
		ICreateColumnOptionSyntax AsInt64();
		ICreateColumnOptionSyntax AsString();
		ICreateColumnOptionSyntax AsFixedLengthString();
	}
}