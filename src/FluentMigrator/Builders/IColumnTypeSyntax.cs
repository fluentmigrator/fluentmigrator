using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders
{
	public interface IColumnTypeSyntax<TNext> : IFluentSyntax
		where TNext : IFluentSyntax
	{
		TNext AsAnsiString();
		TNext AsAnsiString(int size);
		TNext AsBinary(int size);
		TNext AsBoolean();
		TNext AsByte();
		TNext AsCurrency();
		TNext AsDate();
		TNext AsDateTime();
		TNext AsDecimal();
		TNext AsDecimal(int size, int precision);
		TNext AsDouble();
		TNext AsGuid();
		TNext AsFixedLengthString(int size);
		TNext AsFixedLengthAnsiString(int size);
		TNext AsFloat();
		TNext AsInt16();
		TNext AsInt32();
		TNext AsInt64();
		TNext AsString();
		TNext AsString(int size);
		TNext AsTime();
		TNext AsXml();
		TNext AsXml(int size);
	}
}