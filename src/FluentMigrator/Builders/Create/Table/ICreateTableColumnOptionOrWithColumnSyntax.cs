using System;

namespace FluentMigrator.Builders.Create.Table
{
	public interface ICreateTableColumnOptionOrWithColumnSyntax : ICreateTableWithColumnSyntax
	{
		ICreateTableColumnOptionOrWithColumnSyntax WithSize(int size);
		ICreateTableColumnOptionOrWithColumnSyntax WithDefaultValue(object value);
		ICreateTableColumnOptionOrWithColumnSyntax ForeignKey();
		ICreateTableColumnOptionOrWithColumnSyntax Identity();
		ICreateTableColumnOptionOrWithColumnSyntax Indexed();
		ICreateTableColumnOptionOrWithColumnSyntax PrimaryKey();
		ICreateTableColumnOptionOrWithColumnSyntax Nullable();
		ICreateTableColumnOptionOrWithColumnSyntax NotNullable();
		ICreateTableColumnOptionOrWithColumnSyntax Unique();
	}
}