using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders
{
	public interface IColumnOptionSyntax<TNext> : IFluentSyntax
		where TNext : IFluentSyntax
	{
		TNext WithDefaultValue(object value);
		TNext ForeignKey();
		TNext Identity();
		TNext Indexed();
		TNext PrimaryKey();
		TNext Nullable();
		TNext NotNullable();
		TNext Unique();
	}
}