using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders
{
	public interface IColumnTypeSyntax<TNext> : IFluentSyntax
		where TNext : IFluentSyntax
	{
		TNext AsAnsiString();
		TNext AsBinary();
		TNext AsBoolean();
		TNext AsByte();
		TNext AsCurrency();
		TNext AsDate();
		TNext AsDateTime();
		TNext AsDecimal();
		TNext AsDouble();
		TNext AsFixedLengthString();
		TNext AsFixedLengthAnsiString();
		TNext AsFloat();
		TNext AsInt16();
		TNext AsInt32();
		TNext AsInt64();
		TNext AsString();
		TNext AsTime();
		TNext AsXml();
	}
}