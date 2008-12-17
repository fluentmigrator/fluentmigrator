using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Create.Column
{
	public interface ICreateColumnOptionSyntax : IFluentSyntax
	{
		ICreateColumnOptionSyntax WithSize(int size);
		ICreateColumnOptionSyntax WithDefaultValue(object value);
		ICreateColumnOptionSyntax ForeignKey();
		ICreateColumnOptionSyntax Identity();
		ICreateColumnOptionSyntax Indexed();
		ICreateColumnOptionSyntax PrimaryKey();
		ICreateColumnOptionSyntax Nullable();
		ICreateColumnOptionSyntax NotNullable();
		ICreateColumnOptionSyntax Unique();
	}
}